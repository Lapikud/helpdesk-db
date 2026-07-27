# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from `helpdeskdb-frontend/`:

```bash
# Install dependencies
npm install

# Run dev server (Turbopack)
npm run dev

# Run dev server over HTTPS (self-signed cert; for testing Secure-cookie behavior)
npm run dev:https

# Build for production
npm run build

# Lint
npm run lint
```

The dev server runs on `http://localhost:3000`. The backend API must be running on `http://localhost:5018`.

First-time setup — copy the example env file:

```bash
cp .env.local.example .env.local
```

### Docker

The frontend is containerized (`Dockerfile` + `.dockerignore` in this directory) and runs as the `frontend` service in `../HelpdeskDb/docker-compose.yml`. Notes:

- `next.config.ts` sets `output: "standalone"`; the runtime image copies `.next/standalone` + `.next/static` + `public` and runs `node server.js` as the non-root `node` user on port 3000.
- `rewrites()` is resolved during `next build`, so the backend URL is a **build arg** (`NEXT_PUBLIC_BACKEND_URL`, compose passes `FRONTEND_BACKEND_URL`, default `http://lapikudhelpdesk-db-backend:8080`) — not a runtime env var. Changing it requires an image rebuild.
- Deployment requires a TLS proxy in front (bundled Caddy overlay or an external proxy) — see the root `README.md`.

## Architecture

This is a **Next.js 15** frontend (App Router, React 19, Tailwind CSS 3) for the HelpdeskDb asset management system. All pages use `"use client"` — there are no server components. Data fetching and caching are owned by **TanStack Query v5** (`@tanstack/react-query`; the devtools package is a devDependency mounted in `providers.tsx` — v5 strips it from production bundles).

### Proxy and middleware

- `next.config.ts` `rewrites()` proxies `/api/:path*` to `${NEXT_PUBLIC_BACKEND_URL}/api/:path*` (set in `.env.local`; default `http://localhost:5018`), so all browser requests stay same-origin and CORS never fires in dev.
- **`src/middleware.ts`** (matcher `/api/:path*`) sets `x-forwarded-proto` to the browser's real scheme on every proxied API request — unless an upstream TLS-terminating proxy (Caddy, nginx) already set the header, in which case that value is preserved. The Next proxy doesn't add this header itself; the backend's `UseForwardedHeaders` reads it, so when running `npm run dev:https` (or deployed behind an HTTPS proxy) the backend sees HTTPS and marks the auth cookies `Secure`. Over plain HTTP it forwards `http` — a no-op.
- `next.config.ts` also sets `trailingSlash: true` + `skipTrailingSlashRedirect: true` and attaches a **security headers** block to every response: CSP (`'unsafe-inline'` for Next bootstrap scripts; dev additionally gets `'unsafe-eval'` and localhost websockets for Turbopack HMR), `X-Frame-Options: DENY`, `X-Content-Type-Options: nosniff`, `Referrer-Policy`, `Permissions-Policy`, and HSTS.

### Service layer

All API calls go through a class hierarchy in `src/services/`:

```
BaseService         — axios instance with withCredentials: true, auto token-refresh interceptor
  └── EntityService<TEntity, TAddEntity>  — generic CRUD: getAllAsync, getAsync, addAsync, updateAsync, deleteAsync
        └── SpecificService (e.g., AssetService, CategoryService, ...)
```

- `BaseService` uses the relative base URL `/api/v1/` — requests go through the Next.js proxy described above.
- The axios instance is created with `withCredentials: true` so the browser sends and receives the HttpOnly auth cookies (`hd_jwt`, `hd_rt`) on every request. The frontend never reads or writes those cookies directly.
- The axios response interceptor catches 401s, calls `POST /account/renewRefreshToken` (which rotates the cookies on the backend and returns the refreshed identity), updates `AccountContext` with that identity, and replays the original request. Concurrent 401s are coalesced into a **single shared refresh promise** (module-level in `BaseService.ts`); if the refresh itself fails, the interceptor clears the account info and hard-redirects to `/login` via `window.location.href`.
- **Services are module-level singletons** exported from `src/services/index.ts` (which also exports the `allServices` array). Pages import them directly (`import { categoryService } from "@/services"`). Do not instantiate services in components — the old per-page `useMemo(() => new XService(), [])` + render-body `injectSetAccountInfo` pattern is gone. `ServiceAuthBinder` (see "Authentication") wires `setAccountInfo` into every service once.
- `OverviewService` extends `BaseService` directly (not `EntityService`). Its endpoints live under `home/overview/*`: `home/overview` (returns `IAssetsOverviewViewModel` — `{ availableAssets, assetsReservedByUser }`), plus `createNewAsset`, `edit/{id}`, `remove/{id}`, `reserve/{id}`, `changeReservationTime/{id}`, `remove-reservation/{id}`, and `return/{id}` (mark a reserved asset returned).
- All service methods return `IResultObject<T>` (`{ data?, errors?, statusCode? }`) and never throw. `unwrap()` from `src/services/errors.ts` converts that into data-or-thrown-`ApiError` (carrying `statusCode` and `errors`) — React Query decides success/failure by promise rejection, so every queryFn/mutationFn wraps the service call in `unwrap`. Components don't check `result.errors` by hand anymore.

### Data fetching (React Query)

- **`src/app/providers.tsx`** — creates the `QueryClient` in a `useState` lazy initializer and mounts `QueryClientProvider` (+ `ReactQueryDevtools`) in `layout.tsx` wrapping the app. Defaults: `staleTime` 30s, `gcTime` 5min, `refetchOnWindowFocus: false`; queries never retry on 4xx `ApiError` (401 is owned by the axios refresh interceptor — retrying would fight it) and retry at most twice otherwise; mutations never retry.
- **`src/lib/queryKeys.ts`** — the central `qk` key factory. Never inline query keys. **Rule: any parameter that changes the response goes in the key** (`qk.assets(includeRemoved)`, `qk.overview(searchTerm)`). `qk.assetsRoot()` / `qk.assetReservationsRoot()` / `qk.overviewRoot()` are prefix keys for invalidation (they match every variant and per-id entry in one call); the reservations *list* key carries a `"list"` discriminator so invalidating the list doesn't prefix-match and refetch per-id entries.
- **`src/hooks/queries/entityQueries.ts`** — shared `useXxx()` list hooks, one per entity. Defining each query once is what makes the cache shared across pages — use these hooks, don't inline `useQuery` in pages. Reference data (categories, owners, rooms, cupboards, locations, roles, users) gets a 5-min `staleTime`; transactional data uses the 30s default. Per-id hooks (`useAsset`, `useAssetReservation`, `useCategoryAssetByAsset`, `useLocationAssetByAsset`, `useOwnerAssetByAsset`) take `id | null` and stay `enabled: !!id`. The `…ByAsset` hooks resolve `null` only when the asset genuinely has no mapping; a failed fetch throws and marks the query errored.
- **`src/hooks/queries/overviewQueries.ts`** — `useOverview(searchTerm)` with `placeholderData: keepPreviousData`, so the previous list stays on screen while a new search term loads (`isLoading` is true only for the very first fetch).

### Authentication

- The JWT and refresh token live in **HttpOnly cookies** (`hd_jwt`, `hd_rt`) set by the backend. The frontend never sees the token strings — it only knows whether the cookies are valid by asking the server.
- On app mount (`layout.tsx`), the frontend calls `GET /api/v1/account/me`, which validates the `hd_jwt` cookie server-side and returns the user identity (id, username, roles). That identity is used to hydrate `AccountContext`.
- `AccountContext` (`src/context/AccountContext.ts`) is the single source of truth for auth state in the UI (`id`, `name`, `roles`). It does not hold token strings.
- **`ServiceAuthBinder`** (`src/components/ServiceAuthBinder.tsx`) is rendered once in `layout.tsx`, *above* the hydration gate; a single effect wires `setAccountInfo` into every service in `allServices` — this is how the 401 interceptor pushes a refreshed identity back into React context. Child effects run before the parent's, so services are wired before layout's `/me` call fires.
- **`QueryCacheReset`** (`src/components/QueryCacheReset.tsx`) is rendered inside both `Providers` and `AccountContext.Provider`; it clears the whole query cache whenever `accountInfo.id` changes (login, logout, user switch, failed refresh), so one user never sees another's cached data. The logout handler in `Header.tsx` deliberately does **not** call `queryClient.clear()` — the timing rationale is in the component's comment; don't move the clear.
- Roles checked in the UI: `admins` **or** `helpdesk_db_admins` (usually a `canManage` const) gate create/edit/delete actions, admin-only pages, and the Header admin dropdown — this matches the backend's `[Authorize(Roles = "admins,helpdesk_db_admins")]` on write endpoints. `members` and `pixels` (together with the two admin roles) gate the Actions columns on the reservations page and the overview `AssetList`.
- **`AuthGuard`** (`src/components/AuthGuard.tsx`) is mounted once inside `layout.tsx` and wraps every route. It treats `/login` as public, reads `AccountContext`, redirects unauthenticated users to `/login`, and shows a spinner while `accountInfo` is still hydrating. Individual pages must not check auth themselves — do not add per-page redirects.
- **Logout** (`Header.tsx` → `AccountService.logoutAsync`) calls `POST /api/v1/account/logout` so the backend deletes the refresh token from the DB and clears both cookies, then clears `AccountContext`. Do not try to clear cookies from the client — they are HttpOnly.

### Internationalization

- Uses `react-i18next` with `i18next-http-backend` (translations loaded from `/public/locales/{lang}/{namespace}.json`).
- Supported languages: `en`, `et`, `ru`. Language detected from `localStorage` key `i18nextLng`, then browser.
- i18n is initialized in `i18n.ts` (root of `src/`) and imported once in `layout.tsx`.
- Each domain entity has its own namespace (e.g., `asset`, `category`, `room`). Common strings use the `common` namespace; form validation messages use `validationerrors`.
- When adding translations, add the key to all three language files: `public/locales/en/`, `et/`, and `ru/`.

### Page structure

Pages follow the Next.js App Router convention under `src/app/`. **All entity CRUD happens through inline dialogs on the list page — there are no `/create`, `/edit/[id]`, `/delete/[id]`, or `/details/[id]` subroutes. Do not add new subroutes; add dialogs instead.**

- `/overview` — the primary user-facing page; asset list with inline dialogs for create/edit/remove/reserve/return
- `/login` — FreeIPA login form
- `/dbassets` — admin asset list (the "Assets" admin link)
- `/{entity}/` — list page with inline CRUD dialogs (see "Entity dialog system" below)

Implemented entity list pages: `dbassets`, `categories`, `categoryAssets`, `owners`, `ownerAssets`, `rooms`, `cupboards`, `cupboardsInRooms`, `locations`, `locationAssets`, `assetReservations`, `removedAssets`, `users`, `roles`, `refreshTokens`. Locations-in-cupboards have no page of their own — they are managed through the nested `CupboardLocationsDialog` opened from the cupboards page. (The old `userAssets` pages and `UserAssetsService` were removed — do not re-add them.)

**Hydration** — `layout.tsx` holds the single hydration gate: it renders a spinner until the `/me` call settles, so pages never render before the identity is known. The old per-page `hydrated`/`setHydrated` guard is gone — do not add per-page hydration guards.

**Admin-only pages** redirect non-managers via `useEffect(() => { if (accountInfo && !canManage) router.push("/") })`: `dbassets`, `categoryAssets`, `locationAssets`, `ownerAssets`, `locations`, `users`, `roles`, `refreshTokens`, `cupboardsInRooms`, `rooms`.

**Client-side enrichment** — pages needing related names (e.g. `IAssetReservationWithNames`) run several shared query hooks and join the cached results in `useMemo`. No server-side join endpoints exist for enriched types; each list is cached under its own key, so lookup lists are shared with every other page that needs them.

**Reservation action display** (`assetReservations/page.tsx`): the whole Actions column renders only when the user is `admins`, `helpdesk_db_admins`, `members`, or `pixels`. Per row, priority order — `isRemoved` → "Removed" label; else `reservationTo < now` → "Expired" label; else `userId === accountInfo.id` → Edit/Delete dialog buttons; else nothing. The Create button is `admins`/`helpdesk_db_admins` only.

**Overview page dialogs** are driven by selection state + the per-id query hooks: a dialog opens when its queries reach `isSuccess`, and re-clicking the same row is the retry gesture for an errored query. Overview mutations invalidate `qk.overviewRoot()` plus the related entity keys (asset mappings, reservations).

### Entity dialog system

All entity create/edit/delete flows use two generic dialogs driven by declarative per-entity configs:

- **`src/components/dialogs/common/entityDialogTypes.ts`** — the config types: `FormDialogConfig<TForm>` (`namespace`, `singularKey`, `fields`, `defaultValues`), the `FieldSpec` union (`text` | `number` | `select` | `display` | `readonly`), `ValidationSpec` (required/min/max/length — translated via the `validationerrors` namespace), `SelectOption`, and `DeleteSummaryField<TEntity>`. Label keys resolve in the config's entity namespace by default; prefix with `ns:` (e.g. `common:Comment`) to target another namespace.
- **`src/components/dialogs/entityConfigs/*.ts`** — one file per entity (`category`, `owner`, `room`, `cupboard`, `cupboardInRoom`, `location`, `locationInCupboard`, `role`, `categoryAsset`, `locationAsset`, `ownerAsset`, `dbAsset`, `removedAsset`, `editAssetViewModel`), each exporting `xFormConfig` (the `FormDialogConfig`), `xToForm` (entity → form values mapper for edit), and `xDeleteSummary` (`DeleteSummaryField[]` for the delete confirmation). `editAssetViewModel` backs the overview's `EditAssetDialog`, which is a thin wrapper over `EntityFormDialog`.
- **`src/components/dialogs/common/EntityFormDialog.tsx`** — generic create/edit modal built on `react-hook-form`. Props: `open`, `mode` (`create`/`edit`), `config`, `initialValues`, `options` (dynamic select option lists keyed by each select field's `optionsKey`), `staticValues` (values for `display`/`readonly` fields), `onClose`, `onConfirm`, `isLoading`.
- **`src/components/dialogs/common/EntityDeleteDialog.tsx`** — generic delete confirmation. Props: `open`, `entity`, `namespace`, `singularKey`, `summaryFields`, `onClose`, `onConfirm`, `isLoading`.
- **Per-entity thin wrappers** in `src/components/dialogs/{entity}Dialogs/` (e.g. `categoryDialogs/CreateCategoryDialog.tsx`) just bind the config into the generic dialog so pages import named dialogs.
- `onConfirm` handlers return `ConfirmResult` — resolve `{ error: string }` to show an error inside the dialog, or `void` on success.

**Adding CRUD for a new entity:** create the `entityConfigs/{entity}.ts` config, add thin `Create/Edit/Delete{Entity}Dialog` wrappers, then wire them into the list page.

**Typical list page anatomy** (see `src/app/categories/page.tsx` as the reference): `"use client"`; singleton service import from `@/services`; `canManage` (`admins || helpdesk_db_admins`) gates the Actions column and Create button; `useQueryClient` + the shared query hook (`const { data = [], isError, error } = useCategories()`); an `invalidate()` helper (`queryClient.invalidateQueries({ queryKey: qk.categories() })`); three `useMutation`s (`mutationFn` wraps the service call in `unwrap`, `onSuccess: invalidate`); `showCreate/showEdit/showDelete` booleans plus `xToEdit`/`xToDelete` state; handlers `await mutation.mutateAsync(...)` in try/catch — close the dialog on success, return `{ error: (error as Error).message }` on failure (the ConfirmResult contract); dialogs get `isLoading={mutation.isPending}`; an inline error banner renders when `isError`; render is `ListPageWrapper` → `DataTable` → the three dialogs.

### Layout pattern for list pages

All entity list pages use a shared three-component stack:

1. **`ListPageWrapper`** (`src/components/ListPageWrapper.tsx`) — full-bleed gray `#efefef` background, title, and optional `createButton` ReactNode slot (pages pass a `<button onClick>` that opens the create dialog).
2. **`DataTable`** (`src/components/DataTable.tsx`) — white outer card → gray inner area → dark `#424242` pill header row → scrollable white row cards. Pass `columns: string[]` and `rows: { id, cells: ReactNode[] }[]`. Use the `minWidth` prop to control the mobile horizontal-scroll breakpoint, and `emptyMessage` for the no-rows state.
3. **`TableActions`** (`src/components/TableActions.tsx`) — exports `ActionCell` (flex wrapper), `EditButton`, and `DeleteButton`. Both buttons support `href` (renders `<Link>`) and `onClick` (renders `<button>`); pages use `onClick` to open dialogs. For complex action states (Expired, Removed, conditional), put custom JSX directly inside `<ActionCell>`.

### Components

- `src/components/dialogs/overviewDialogs/` — modal dialogs used on the overview page (Create/Edit/Remove asset, Reserve/ChangeReservationTime/RemoveReservation)
- `src/components/dialogs/common/` — `Modal.tsx` (base modal wrapper), `EntityFormDialog.tsx`, `EntityDeleteDialog.tsx`, `entityDialogTypes.ts`
- `src/components/dialogs/entityConfigs/` and `src/components/dialogs/{entity}Dialogs/` — see "Entity dialog system"
- `src/components/dialogs/locationInCupboardDialogs/CupboardLocationsDialog.tsx` — nested dialog for managing a cupboard's locations
- `src/components/ui/` — shadcn/ui-style primitives (Button, Calendar, Popover, ScrollArea, Select) built on Radix UI
- `src/components/AssetLineDetails.tsx` and `AssetCardDetails.tsx` — two display modes for assets on the overview
- `src/components/ServiceAuthBinder.tsx` and `QueryCacheReset.tsx` — auth/cache wiring rendered once in `layout.tsx` (see "Authentication")
- Other shared components: `AssetList.tsx`, `DateTimePicker.tsx`, `LanguageSwitcher.tsx`, `LoadingSpinner.tsx`, `Header.tsx`, `Footer.tsx`; `src/hooks/useBarcodeScanner.ts`; `src/hooks/queries/` (see "Data fetching")

### Types

- `src/types/domain/DomainTypes.ts` — all domain interfaces (`IAsset`, `ICategory`, `IRoom`, etc., plus `Add` and `WithNames` variants)
- `src/types/domain/IAssetViewModels.ts` — overview view models (`IAssetViewModel`, `IAssetsOverviewViewModel`, and Create/Update/Remove variants)
- `src/types/IResultObject.ts` — generic API response wrapper
- `src/types/IDomainId.ts` — base `{ id }` interface; `src/types/IIdentityResponse.ts` — identity payload returned by login/me/renew
