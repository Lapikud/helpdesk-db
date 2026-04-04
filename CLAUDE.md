# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository structure

```
project/
  HelpdeskDb/               — ASP.NET Core 10 backend (REST API + MVC admin)
  helpdeskdb-frontend/      — Next.js 15 frontend (App Router)
```

Each subdirectory has its own `CLAUDE.md` with full detail. This file provides the cross-cutting context needed to work across both.

## System overview

**HelpdeskDb** is an IT asset management system for tracking physical assets (laptops, peripherals, etc.) stored in cupboards and rooms. Users can browse, reserve, and manage assets. Admins manage the physical location hierarchy and ownership records.

## Running the full stack

```bash
# Terminal 1 — backend (from HelpdeskDb/)
dotnet run --project WebApp/WebApp.csproj
# Listens on http://localhost:5018

# Terminal 2 — frontend (from helpdeskdb-frontend/)
npm run dev
# Listens on http://localhost:3000
```

Or run only the backend + database via Docker (from `HelpdeskDb/`):

```bash
# First-time: copy the example env file and fill in your values
cp .env.example .env
docker-compose up
```

The frontend uses relative `/api/*` URLs — Next.js proxies them to the backend via a `rewrites()` rule in `next.config.ts`. The proxy destination is `${NEXT_PUBLIC_BACKEND_URL}/api/*`, configured in `helpdeskdb-frontend/.env.local` (default `http://localhost:5018`). Copy `.env.local.example` → `.env.local` before running the frontend.

## Authentication flow (end-to-end)

1. User submits credentials on the frontend `/login` page.
2. Frontend calls `POST /api/v1/account/login` on the backend.
3. Backend authenticates against **FreeIPA** (`ipa.lapikud.ee`) via JSON-RPC, syncs the user's IPA groups into the local `AppUserRole` table, and returns a JWT + refresh token.
4. Frontend stores both in `localStorage` (`_jwt`, `_refreshToken`) and decodes the JWT via `JwtHelper.ts` to populate `AccountContext`.
5. All subsequent API calls include the JWT as a `Bearer` token. On 401, the axios interceptor auto-refreshes via `POST /api/v1/account/renewRefreshToken`.

Roles used throughout both layers: `admins`, `helpdesk_db_admins`, `members`, `pixels`.

## API contract

- All REST endpoints are versioned: `api/v{version}/...` (currently `v1`).
- Request/response shapes are defined in `HelpdeskDb/App.DTO/v1/`. The frontend mirrors these in `helpdeskdb-frontend/src/types/domain/DomainTypes.ts`.
- The aggregated asset view model used by the overview page is `AssetViewModel` — defined in `App.DAL.DTO/ViewModels/` on the backend and `IAssetViewModels.ts` on the frontend.

## Localization

Both layers support English (`en`), Estonian (`et`), and Russian (`ru`).

- **Backend** — `.resx` files in `HelpdeskDb/App.Resources/`. MVC views use `IStringLocalizer<T>`. When adding a new entity, create `Entity.resx`, `Entity.et.resx`, and `Entity.ru.resx` under `App.Resources/Domain/`.
- **Frontend** — JSON files under `helpdeskdb-frontend/public/locales/{lang}/{namespace}.json`, loaded via `react-i18next`. Each domain entity has its own namespace. When adding translations, update all three language directories.
