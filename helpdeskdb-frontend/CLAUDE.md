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

## Architecture

This is a **Next.js 15** frontend (App Router, React 19, Tailwind CSS 3) for the HelpdeskDb asset management system. All pages use `"use client"` — there are no server components.

### Proxy and middleware

- `next.config.ts` `rewrites()` proxies `/api/:path*` to `${NEXT_PUBLIC_BACKEND_URL}/api/:path*` (set in `.env.local`; default `http://localhost:5018`), so all browser requests stay same-origin and CORS never fires in dev.
- **`src/middleware.ts`** (matcher `/api/:path*`) sets `x-forwarded-proto` to the browser's real scheme on every proxied API request. The Next proxy doesn't add this header itself; the backend's `UseForwardedHeaders` reads it, so when running `npm run dev:https` the backend sees HTTPS and marks the auth cookies `Secure`. Over plain HTTP it forwards `http` — a no-op.
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
- Services are instantiated with `useMemo` in page components and **must have `injectSetAccountInfo(setAccountInfo)` called on them** before use — this lets the 401 interceptor push the refreshed identity back into React context.
- `OverviewService` extends `BaseService` directly (not `EntityService`). Its endpoints live under `home/overview/*`: `home/overview` (returns `IAssetsOverviewViewModel` — `{ availableAssets, assetsReservedByUser }`), plus `createNewAsset`, `edit/{id}`, `remove/{id}`, `reserve/{id}`, `changeReservationTime/{id}`, `remove-reservation/{id}`, and `return/{id}` (mark a reserved asset returned).
- All service methods return `IResultObject<T>` (`{ data?, errors?, statusCode? }`). Check `errors` or `statusCode >= 400` for failures.

### Authentication

- The JWT and refresh token live in **HttpOnly cookies** (`hd_jwt`, `hd_rt`) set by the backend. The frontend never sees the token strings — it only knows whether the cookies are valid by asking the server.
- On app mount (`layout.tsx`), the frontend calls `GET /api/v1/account/me`, which validates the `hd_jwt` cookie server-side and returns the user identity (id, username, roles). That identity is used to hydrate `AccountContext`.
- `AccountContext` (`src/context/AccountContext.ts`) is the single source of truth for auth state in the UI (`id`, `name`, `roles`). It does not hold token strings.
- Roles checked in the UI: `admins` (gates create/edit/delete actions on most pages), `members` and `pixels` (together with `admins`, gate the Actions column on the reservations page).
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

**Hydration guard** — every page initializes `const [hydrated, setHydrated] = useState(false)` set via `useEffect`. Data fetches are gated on `hydrated`. Return `<Spinner>` until true — this avoids running browser-only code (localStorage reads for i18n / UI state, cookie-dependent fetches) during SSR, and lets `layout.tsx`'s `/me` call settle before pages decide what to render.

**Client-side enrichment** — pages needing related names (e.g. `IAssetReservationWithNames`) fetch all resources in parallel via `Promise.all` and join client-side. No server-side join endpoints exist for enriched types.

**Reservation action display** (`assetReservations/page.tsx`): the whole Actions column renders only when the user is `admins`, `members`, or `pixels`. Per row, priority order — `isRemoved` → "Removed" label; else `reservationTo < now` → "Expired" label; else `userId === accountInfo.id` → Edit/Delete dialog buttons; else nothing. The Create button is admin-only.

### Entity dialog system

All entity create/edit/delete flows use two generic dialogs driven by declarative per-entity configs:

- **`src/components/dialogs/common/entityDialogTypes.ts`** — the config types: `FormDialogConfig<TForm>` (`namespace`, `singularKey`, `fields`, `defaultValues`), the `FieldSpec` union (`text` | `number` | `select` | `display` | `readonly`), `ValidationSpec` (required/min/max/length — translated via the `validationerrors` namespace), `SelectOption`, and `DeleteSummaryField<TEntity>`. Label keys resolve in the config's entity namespace by default; prefix with `ns:` (e.g. `common:Comment`) to target another namespace.
- **`src/components/dialogs/entityConfigs/*.ts`** — one file per entity (`category`, `owner`, `room`, `cupboard`, `cupboardInRoom`, `location`, `locationInCupboard`, `role`, `categoryAsset`, `locationAsset`, `ownerAsset`, `dbAsset`, `removedAsset`), each exporting `xFormConfig` (the `FormDialogConfig`), `xToForm` (entity → form values mapper for edit), and `xDeleteSummary` (`DeleteSummaryField[]` for the delete confirmation).
- **`src/components/dialogs/common/EntityFormDialog.tsx`** — generic create/edit modal built on `react-hook-form`. Props: `open`, `mode` (`create`/`edit`), `config`, `initialValues`, `options` (dynamic select option lists keyed by each select field's `optionsKey`), `staticValues` (values for `display`/`readonly` fields), `onClose`, `onConfirm`, `isLoading`.
- **`src/components/dialogs/common/EntityDeleteDialog.tsx`** — generic delete confirmation. Props: `open`, `entity`, `namespace`, `singularKey`, `summaryFields`, `onClose`, `onConfirm`, `isLoading`.
- **Per-entity thin wrappers** in `src/components/dialogs/{entity}Dialogs/` (e.g. `categoryDialogs/CreateCategoryDialog.tsx`) just bind the config into the generic dialog so pages import named dialogs.
- `onConfirm` handlers return `ConfirmResult` — resolve `{ error: string }` to show an error inside the dialog, or `void` on success.

**Adding CRUD for a new entity:** create the `entityConfigs/{entity}.ts` config, add thin `Create/Edit/Delete{Entity}Dialog` wrappers, then wire them into the list page.

**Typical list page anatomy** (see `src/app/categories/page.tsx` as the reference): `"use client"`; service via `useMemo` + `injectSetAccountInfo`; `hydrated` guard; `fetchData` with `getAllAsync`; `isAdmin` gates the Actions column and Create button; `showCreate/showEdit/showDelete` booleans plus `xToEdit`/`xToDelete` state; handlers call the service, re-fetch on success, and return `{ error }` on failure; render is `ListPageWrapper` → `DataTable` → the three dialogs.

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
- Other shared components: `AssetList.tsx`, `DateTimePicker.tsx`, `LanguageSwitcher.tsx`, `LoadingSpinner.tsx`, `Header.tsx`, `Footer.tsx`; `src/hooks/useBarcodeScanner.ts`

### Types

- `src/types/domain/DomainTypes.ts` — all domain interfaces (`IAsset`, `ICategory`, `IRoom`, etc., plus `Add` and `WithNames` variants)
- `src/types/domain/IAssetViewModels.ts` — overview view models (`IAssetViewModel`, `IAssetsOverviewViewModel`, and Create/Update/Remove variants)
- `src/types/IResultObject.ts` — generic API response wrapper
- `src/types/IDomainId.ts` — base `{ id }` interface; `src/types/IIdentityResponse.ts` — identity payload returned by login/me/renew
