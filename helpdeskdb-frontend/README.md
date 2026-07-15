# HelpdeskDb frontend

Next.js 15 (App Router) frontend for **HelpdeskDb**, an IT asset management system — browsing, reserving, and administering physical assets stored in cupboards and rooms. Talks to the ASP.NET Core backend in [`../HelpdeskDb`](../HelpdeskDb).

## Prerequisites

- **Node.js 20+** and npm
- The backend running on `http://localhost:5018` (see the [root README](../README.md) for full-stack setup)

## Setup

```bash
cp .env.local.example .env.local   # sets NEXT_PUBLIC_BACKEND_URL (default http://localhost:5018)
npm install
```

## Commands

```bash
npm run dev         # dev server (Turbopack) → http://localhost:3000
npm run dev:https   # dev server over HTTPS with a self-signed cert (tests Secure-cookie behavior)
npm run build       # production build
npm run lint        # ESLint
```

## How the API proxy works

The app calls relative `/api/*` URLs. A `rewrites()` rule in `next.config.ts` proxies them to `${NEXT_PUBLIC_BACKEND_URL}/api/*`, so browser requests stay same-origin and CORS never fires in dev. Auth uses HttpOnly cookies set by the backend (`hd_jwt`, `hd_rt`) — the frontend never sees the tokens. `src/middleware.ts` forwards the browser's real scheme (`X-Forwarded-Proto`) on proxied requests so the backend marks the cookies `Secure` when you run over HTTPS.

## Further reading

- [`CLAUDE.md`](CLAUDE.md) — architecture: service layer, auth flow, entity dialog system, list-page components, i18n
- [Root `CLAUDE.md`](../CLAUDE.md) — cross-cutting auth/CORS/deployment notes
