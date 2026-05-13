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
2. Frontend calls `POST /api/v1/account/login` with `withCredentials: true`.
3. Backend authenticates against **FreeIPA** (`ipa.lapikud.ee`) via JSON-RPC, syncs the user's IPA groups into the local `AppUserRole` table, and sets two **HttpOnly cookies** on the response: `hd_jwt` (path `/api`) and `hd_rt` (path `/api/v1/account`). Both are `SameSite=Strict` and `Secure` when the request is HTTPS. The response body returns only the user identity (id, username, roles).
4. The frontend cannot read the JWT (HttpOnly), so on app mount `layout.tsx` calls `GET /api/v1/account/me`, which validates the cookie server-side and returns the identity used to hydrate `AccountContext`.
5. All subsequent API calls go through axios with `withCredentials: true`; the browser attaches `hd_jwt` automatically. The backend's `JwtBearerEvents.OnMessageReceived` handler in `Program.cs` pulls the token out of the cookie when no `Authorization` header is present — the SPA never sends a `Bearer` header.
6. On 401, the axios interceptor calls `POST /api/v1/account/renewRefreshToken`, which rotates the refresh token, sets new cookies, and returns the new identity.

Roles used throughout both layers: `admins`, `helpdesk_db_admins`, `members`, `pixels`.

## CORS and deployment

- In dev, Next.js `rewrites()` proxies `/api/*` to the backend, so all browser requests are same-origin and CORS never fires.
- In production with the frontend calling the backend directly, the backend uses the `FrontendOnly` CORS policy (registered in `Program.cs`) which reads the allowed origins from the `AllowedOrigins` configuration array. Set this via the `AllowedOrigins__0`, `AllowedOrigins__1`, … keys — loaded from `HelpdeskDb/.env` in dev via `DotNetEnv`, or set directly on the process in production. `appsettings.json` ships an empty placeholder.
- `AllowCredentials()` is required for the auth cookies to cross origins, which means `AllowAnyOrigin()` cannot be used — the allowlist must be explicit.
- If the deployed frontend and backend live on **different registrable domains** (not just different subdomains of the same domain), the auth cookies must switch from `SameSite=Strict` to `SameSite=None` + `Secure=true` in `AccountController.SetAuthCookies` or the browser will drop them.

## API contract

- All REST endpoints are versioned: `api/v{version}/...` (currently `v1`).
- Request/response shapes are defined in `HelpdeskDb/App.DTO/v1/`. The frontend mirrors these in `helpdeskdb-frontend/src/types/domain/DomainTypes.ts`.
- The aggregated asset view model used by the overview page is `AssetViewModel` — defined in `App.DAL.DTO/ViewModels/` on the backend and `IAssetViewModels.ts` on the frontend.

## Localization

Both layers support English (`en`), Estonian (`et`), and Russian (`ru`).

- **Backend** — `.resx` files in `HelpdeskDb/App.Resources/`. MVC views use `IStringLocalizer<T>`. When adding a new entity, create `Entity.resx`, `Entity.et.resx`, and `Entity.ru.resx` under `App.Resources/Domain/`.
- **Frontend** — JSON files under `helpdeskdb-frontend/public/locales/{lang}/{namespace}.json`, loaded via `react-i18next`. Each domain entity has its own namespace. When adding translations, update all three language directories.
