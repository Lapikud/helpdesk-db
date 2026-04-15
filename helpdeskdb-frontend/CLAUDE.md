# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

All commands run from `helpdeskdb-frontend/`:

```bash
# Install dependencies
npm install

# Run dev server (Turbopack)
npm run dev

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

This is a **Next.js 15** frontend (App Router) for the HelpdeskDb asset management system. All pages use `"use client"` — there are no server components.

### Service layer

All API calls go through a class hierarchy:

```
BaseService         — axios instance, JWT injection into headers, auto token-refresh interceptor
  └── EntityService<TEntity, TAddEntity>  — generic CRUD: getAllAsync, getAsync, addAsync, updateAsync, deleteAsync
        └── SpecificService (e.g., AssetService, CategoryService, ...)
```

- `BaseService` uses the relative base URL `/api/v1/` — requests go to the Next.js dev server, which proxies `/api/*` to the backend via the `rewrites()` rule in `next.config.ts`. The proxy destination is `${NEXT_PUBLIC_BACKEND_URL}/api/*` (set in `.env.local`; default `http://localhost:5018`).
- The axios response interceptor auto-refreshes the JWT on 401 responses via `POST /account/renewRefreshToken`.
- Services are instantiated with `useMemo` in page components and **must have `injectSetAccountInfo(setAccountInfo)` called on them** before use — this wires the token-refresh callback back to React context so the UI reflects the new token.
- `OverviewService` fetches the aggregated `IAssetViewModel[]` used by the `/overview` page (wraps the backend's overview endpoint, not the generic CRUD base).
- All service methods return `IResultObject<T>` (`{ data?, errors?, statusCode? }`). Check `errors` or `statusCode >= 400` for failures.

### Authentication

- JWT and refresh token are stored in `localStorage` under keys `_jwt` and `_refreshToken`.
- On app mount (`layout.tsx`), tokens are read from localStorage and decoded using `JwtHelper` to populate `AccountContext`.
- `AccountContext` (`src/context/AccountContext.ts`) is the single source of truth for auth state (`jwt`, `refreshToken`, `roles`, `name`, `id`).
- Roles checked in the UI: `admins`, `members` (from the backend's FreeIPA sync).
- JWT claims use full Microsoft/XMLSOAP URIs — `JwtHelper.ts` handles extracting roles, username, and user ID.

### Internationalization

- Uses `react-i18next` with `i18next-http-backend` (translations loaded from `/public/locales/{lang}/{namespace}.json`).
- Supported languages: `en`, `et`, `ru`. Language detected from `localStorage` key `i18nextLng`, then browser.
- i18n is initialized in `i18n.ts` (root of `src/`) and imported once in `layout.tsx`.
- Each domain entity has its own namespace (e.g., `asset`, `category`, `room`). Common strings use the `common` namespace.
- When adding translations, add the key to all three language files: `public/locales/en/`, `et/`, and `ru/`.

### Page structure

**Hydration guard** — every page initializes `const [hydrated, setHydrated] = useState(false)` set via `useEffect`. Auth checks and data fetches are gated on `hydrated`. Return `<Spinner>` until true — prevents SSR/localStorage access errors.

**Client-side enrichment** — pages needing related names (e.g. `IAssetReservationWithNames`) fetch all resources in parallel via `Promise.all` and join client-side. No server-side join endpoints exist for enriched types.

**Reservation action display** (`assetReservations/page.tsx`): priority order — `isRemoved` → "Removed" label; else `reservationTo < now` → "Expired" label; else `userId === accountInfo.id` → Edit/Delete links; else nothing.

Pages follow the Next.js App Router convention under `src/app/`:

- `/overview` — the primary user-facing page; asset list with inline dialogs for create/edit/remove/reserve
- `/login` — FreeIPA login form
- `/{entity}/` — list page (use `ListPageWrapper` + `DataTable` + `TableActions`)
- `/{entity}/create` — create form (being migrated to inline dialogs)
- `/{entity}/edit/[id]` — edit form (being migrated to inline dialogs)
- `/{entity}/delete/[id]` — delete confirmation (being migrated to inline dialogs)
- `/{entity}/details/[id]` — **being removed** (redundant, do not add new details pages)

Implemented entity pages: `categories`, `categoryAssets`, `owners`, `ownerAssets`, `rooms`, `cupboards`, `cupboardsInRooms`, `locations`, `locationsInCupboards`, `locationAssets`, `assetReservations`, `removedAssets`, `users`, `userAssets`, `userManagement`, `userRoles`, `roles`, `refreshTokens`.

### Layout pattern for list pages

All entity list pages use a shared three-component stack:

1. **`ListPageWrapper`** (`src/components/ListPageWrapper.tsx`) — full-bleed gray `#efefef` background, title, and optional `createButton` ReactNode slot. The slot accepts a `<Link>` today and will accept a `<button onClick>` when create actions move to inline dialogs.
2. **`DataTable`** (`src/components/DataTable.tsx`) — white outer card → gray inner area → dark `#424242` pill header row → scrollable white row cards. Pass `columns: string[]` and `rows: { id, cells: ReactNode[] }[]`. Use the `minWidth` prop to control mobile horizontal-scroll breakpoint.
3. **`TableActions`** (`src/components/TableActions.tsx`) — exports `ActionCell` (flex wrapper), `EditButton`, and `DeleteButton`. Both buttons support `href` (renders `<Link>`) and `onClick` (renders `<button>`) so they are ready for the dialog migration. For complex action states (Expired, Removed, conditional), put custom JSX directly inside `<ActionCell>`.

Pages currently using this stack: `categories`, `owners`, `removedAssets`, `assetReservations`.

**Planned migration:** create/edit/delete actions will move from separate routes (`/categories/create`, etc.) to inline dialogs (same pattern as the `/overview` page). Details pages (`/{entity}/details/[id]`) are being removed as redundant — do not add Details links to new pages.

### Components

- `src/components/dialogs/overviewDialogs/` — modal dialogs used on the overview page (Create/Edit/Remove asset, Reserve/ChangeReservation/RemoveReservation)
- `src/components/dialogs/common/Modal.tsx` — base modal wrapper
- `src/components/ui/` — shadcn/ui-style primitives (Button, Calendar, Popover, ScrollArea, Select) built on Radix UI
- `src/components/AssetLineDetails.tsx` and `AssetCardDetails.tsx` — two display modes for assets on the overview

### Types

- `src/types/domain/DomainTypes.ts` — all domain interfaces (`IAsset`, `ICategory`, `IRoom`, etc., plus `Add` and `WithNames` variants)
- `src/types/domain/IAssetViewModels.ts` — view models used specifically for the overview page (aggregated asset data)
- `src/types/IResultObject.ts` — generic API response wrapper
