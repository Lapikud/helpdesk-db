# HelpdeskDb

IT asset management system for tracking physical assets (laptops, peripherals, etc.) stored in cupboards and rooms. Users browse and reserve assets; admins manage the location hierarchy, ownership records, and reservation history.

## Tech stack

| Layer | Stack |
|---|---|
| Frontend | Next.js, React, TypeScript, Tailwind CSS |
| Backend | ASP.NET Core 10, Entity Framework Core 10, C#, PostgreSQL |
| Auth | FreeIPA (JSON-RPC) — JWT delivered to the browser via HttpOnly cookies |
| Infra | Docker Compose |

For the full per-layer dependency lists, see [`HelpdeskDb/CLAUDE.md`](HelpdeskDb/CLAUDE.md) and [`helpdeskdb-frontend/CLAUDE.md`](helpdeskdb-frontend/CLAUDE.md).

## Repository layout

```
project/
  HelpdeskDb/               — ASP.NET Core 10 backend (REST API + MVC admin)
  helpdeskdb-frontend/      — Next.js frontend (App Router)
  CLAUDE.md                 — cross-cutting architecture / notes
```

Each subfolder has its own `CLAUDE.md` with deeper detail on commands, layering, mappers, services, and patterns.

## Prerequisites

- **.NET 10 SDK** — verify with `dotnet --list-sdks`
- **Node.js 20+** and **npm** — verify with `node --version`
- **PostgreSQL** — the backend will not start without a reachable DB. Either:
  - install Postgres natively (typical: `localhost:5432`), or
  - use the bundled `docker-compose` Postgres (requires **Docker** / Docker Desktop)
- **FreeIPA credentials** for `ipa.lapikud.ee` — needed to log in to the running app and to populate the `IpaServiceAccount__*` env vars

## First-time setup

Copy the two example env files and fill in real values:

```bash
# Backend env (from HelpdeskDb/)
cp .env.example .env

# Frontend env (from helpdeskdb-frontend/)
cp .env.local.example .env.local
```

Backend `.env` keys you must set:

- `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` — credentials for the Postgres instance
- `ConnectionStrings__DefaultConnection` — used by `dotnet run` (host = `localhost`)
- `DOCKER_DB_CONNECTION` — used by the backend container (host = `lapikudhelpdesk-db-postgres`)
- `JWTSecurity__Key` — long random string (e.g. `openssl rand -base64 64`)
- `AllowedOrigins__0` — CORS allowlist entry, e.g. `http://localhost:3000`
- `IpaServiceAccount__Username` / `IpaServiceAccount__Password` — IPA service account used to re-sync roles on `/renewRefreshToken`

Frontend `.env.local` keys:

- `NEXT_PUBLIC_BACKEND_URL` — defaults to `http://localhost:5018`; this is the rewrite destination for `/api/*`

## Connection strings — read this before running

`HelpdeskDb/.env` contains two connection strings, and **both must point at the same Postgres instance with matching credentials**:

| Variable | When it's used | Host | Port |
|---|---|---|---|
| `ConnectionStrings__DefaultConnection` | Local `dotnet run` | `localhost` | match your Postgres |
| `DOCKER_DB_CONNECTION` | Backend running inside `docker-compose` | `lapikudhelpdesk-db-postgres` (docker service name) | `5432` (container-internal) |

Pick a Postgres source and align the local port:

- **Your own Postgres on `localhost:5432`** — set both connection strings to `Port=5432`. You do not need to run `docker-compose up db`.
- **Bundled `docker-compose` Postgres** — `docker-compose.yml` publishes the container on host port **`5433`** (`"5433:5432"`). Set `ConnectionStrings__DefaultConnection` to `Port=5433` for local `dotnet run`. (If `5433` is taken on your machine, edit the mapping in `docker-compose.yml` and keep the connection string in sync.)

`POSTGRES_USER` / `POSTGRES_PASSWORD` / `POSTGRES_DB` in `.env` must match the `Username=` / `Password=` / `Database=` segments inside both connection strings — otherwise the db container initializes with one set of credentials while the backend tries to connect with another.

## Run the app

### Step 1 — make sure Postgres is running

Either your own local Postgres, or bring up the bundled one:

```bash
# from HelpdeskDb/
docker-compose up db
```

### Step 2 — pick one of two run modes

**Option A — backend and frontend both run locally (most common for dev):**

```bash
# Terminal 1 (from HelpdeskDb/)
dotnet run --project WebApp/WebApp.csproj
# → http://localhost:5018

# Terminal 2 (from helpdeskdb-frontend/)
npm install         # first time only
npm run dev
# → http://localhost:3000
```

**Option B — backend in Docker, frontend local:**

```bash
# from HelpdeskDb/
docker-compose up
# Postgres + backend come up together; backend on host port 80

# from helpdeskdb-frontend/ (separate terminal)
npm run dev
```

In dev, Next.js proxies `/api/*` to the backend (`rewrites()` in `next.config.ts`), so the browser stays same-origin and CORS never fires. On startup, the backend's `DataSeeder` applies migrations and seeds roles + sample data if the tables are empty.

Open `http://localhost:3000`, log in with FreeIPA credentials, and you're in.

## Tests

```bash
# from HelpdeskDb/
dotnet test HelpdeskDb.sln
```

Tests are self-contained — they use **SQLite in-memory** and a `FakeIpaAuthClient`, so they do not need Postgres or network access to FreeIPA.

## Authentication (one-paragraph summary)

Login goes to `POST /api/v1/account/login`. The backend authenticates against FreeIPA, syncs IPA group membership into the local `AppUserRole` table, and sets two HttpOnly cookies (`hd_jwt`, `hd_rt`). The frontend hydrates its identity by calling `GET /api/v1/account/me` on app mount — the JWT itself is never exposed to JavaScript. Full flow in [`CLAUDE.md`](CLAUDE.md).

## Localization

Three languages: English (`en`), Estonian (`et`), Russian (`ru`).

- Backend strings: `.resx` files under `HelpdeskDb/App.Resources/`
- Frontend strings: JSON under `helpdeskdb-frontend/public/locales/{lang}/{namespace}.json`

When adding a translation key, update all three language sets.

## Further reading

- [`CLAUDE.md`](CLAUDE.md) — cross-cutting architecture, full auth flow, CORS deployment notes
- [`HelpdeskDb/CLAUDE.md`](HelpdeskDb/CLAUDE.md) — backend layering, manual mapper chain, entity-addition checklist, testing strategy
- [`helpdeskdb-frontend/CLAUDE.md`](helpdeskdb-frontend/CLAUDE.md) — service layer, AuthGuard, page patterns, list-page component stack
