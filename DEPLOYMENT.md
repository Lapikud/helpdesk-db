# Deploying HelpdeskDb on Proxmox VE

This guide walks through deploying the full stack (PostgreSQL + ASP.NET Core backend + Next.js frontend) with Docker Compose inside a VM on a Proxmox VE host.

**Why a VM and not an LXC container?** Proxmox officially recommends running Docker inside a full VM. Docker inside LXC requires nesting/keyctl workarounds, conflicts with unprivileged containers, and tends to break on PVE kernel upgrades. Use a VM.

## Architecture

```
Internet
   │ :80 / :443
   ▼
┌──────────────────────────────────────────── Proxmox VM ──┐
│  Caddy (TLS, Let's Encrypt)          ← only public port  │
│    │ http                                                │
│    ▼                                                     │
│  frontend  (Next.js standalone, :3000)                   │
│    │ /api/* proxied via rewrites()                       │
│    ▼                                                     │
│  app       (ASP.NET Core, :8080, MVC admin on loopback)  │
│    │                                                     │
│    ▼                                                     │
│  db        (PostgreSQL 17, loopback :5433 on the host)   │
└───────────────────────────────────────────────────────────┘
        │ outbound :443
        ▼
  FreeIPA (ipa.lapikud.ee) — authentication
```

- All browser traffic goes through the frontend; it proxies `/api/*` to the backend over the compose network.
- Without the Caddy overlay, everything is bound to **loopback only** — you must put your own TLS-terminating reverse proxy in front (see [Option B](#option-b--deploy-behind-an-existing-reverse-proxy)).
- The backend runs with `ASPNETCORE_ENVIRONMENT=Production`, which makes the auth cookies always `Secure`. **The browser-facing side must be HTTPS or login will not work.**

## Prerequisites

- A Proxmox VE host (tested against PVE 8.x) with ~2 vCPU / 4 GB RAM / 32 GB disk to spare.
- **Option A (bundled Caddy):** a public DNS record for your domain pointing at the VM (or at a router that forwards 80/443 to it), and ports 80 + 443 reachable from the internet (Let's Encrypt HTTP challenge).
- **Option B (own reverse proxy):** an existing HTTPS-terminating proxy (Nginx Proxy Manager, nginx, Traefik, …) that can reach the VM.
- FreeIPA credentials for `ipa.lapikud.ee`, including a **service account** for the role re-sync (any authenticated IPA user typically qualifies).
- The VM must have **outbound HTTPS (443) access to `ipa.lapikud.ee`** — logins fail without it.

## Step 1 — Create the VM

Via the PVE web UI: **Create VM** → Debian 12 (or Ubuntu 24.04 LTS) ISO → 2 cores, 4096 MB RAM, 32 GB disk (VirtIO SCSI), VirtIO network, enable **QEMU Guest Agent**. Or from the PVE shell:

```bash
# Adjust VMID, storage name, and bridge to your environment
qm create 200 --name helpdeskdb --memory 4096 --cores 2 \
  --net0 virtio,bridge=vmbr0 \
  --scsihw virtio-scsi-single --scsi0 local-lvm:32 \
  --ide2 local:iso/debian-12.x.x-amd64-netinst.iso,media=cdrom \
  --boot order=scsi0;ide2 --agent enabled=1
qm start 200
```

Install the OS, then inside the VM:

```bash
sudo apt update && sudo apt -y upgrade
sudo apt -y install qemu-guest-agent git curl
sudo systemctl enable --now qemu-guest-agent
```

Give the VM a static IP (or a DHCP reservation) — the reverse proxy / DNS record needs a stable target.

## Step 2 — Install Docker

Use Docker's official apt repository — the distro packages are too old. The Caddy overlay uses `!override` tags, which need **docker compose v2.24.4+**; the official repo always satisfies this.

```bash
curl -fsSL https://get.docker.com | sudo sh
sudo usermod -aG docker $USER
# Log out and back in for the group change, then verify:
docker --version
docker compose version   # must be >= 2.24.4
```

## Step 3 — Clone the repository

Clone the **whole repo** — `docker-compose.yml` builds the frontend image from `../helpdeskdb-frontend`, so both directories must be present:

```bash
git clone <your-repo-url> helpdesk-db
cd helpdesk-db/HelpdeskDb
```

All `docker compose` commands below run from `helpdesk-db/HelpdeskDb/` (compose reads `.env` from the directory containing the compose file).

## Step 4 — Configure `.env`

```bash
cp .env.example .env
nano .env
```

| Variable | Required | Production value |
|---|---|---|
| `POSTGRES_DB` | yes | e.g. `laphelpdeskdb` |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` | yes | strong, unique credentials for the db container |
| `DOCKER_DB_CONNECTION` | yes | `Host=lapikudhelpdesk-db-postgres;Port=5432;Database=<POSTGRES_DB>;Username=<POSTGRES_USER>;Password=<POSTGRES_PASSWORD>;Include Error Detail=true` — the `Database=`/`Username=`/`Password=` segments **must match** the three `POSTGRES_*` values, or the db initializes with one set of credentials and the backend connects with another |
| `JWTSecurity__Key` | yes | long random secret: `openssl rand -base64 64` |
| `IpaServiceAccount__Username` / `IpaServiceAccount__Password` | yes | FreeIPA service account (used to re-sync roles on token renewal — without it, IPA role revocations don't propagate for up to 7 days) |
| `AllowedOrigins__0` | yes | `https://<your-domain>` (no trailing slash). CORS never actually fires in this deployment (the frontend proxies `/api/*` same-origin), but don't leave the dev value `http://localhost:3000` in production |
| `DOMAIN` | Option A only | public FQDN, e.g. `helpdesk.example.com` — used by the bundled Caddy |
| `ALLOWED_HOSTS` | optional | Host-header allowlist, e.g. `<your-domain>;lapikudhelpdesk-db-backend;localhost`. Must include `lapikudhelpdesk-db-backend` (the frontend proxy forwards that Host). Default `*` |
| `FRONTEND_BACKEND_URL` | leave default | Baked into the frontend image **at build time** (Next.js `rewrites()` target). The default (`http://lapikudhelpdesk-db-backend:8080`) is correct for compose; changing it requires `docker compose build frontend` |
| `ConnectionStrings__DefaultConnection` | not used in Docker | only for local `dotnet run` on a dev machine; can stay as-is |

## Step 5, Option A — Deploy with the bundled Caddy (recommended)

Caddy terminates TLS with an auto-provisioned Let's Encrypt certificate, redirects HTTP→HTTPS, and forwards `X-Forwarded-Proto: https` so the Secure auth cookies work.

1. Create a DNS **A record** for `DOMAIN` pointing at the VM's public IP (or your router's, with 80/443 port-forwarded to the VM).
2. Open ports 80 and 443. If you use the Proxmox firewall on the VM: **VM → Firewall → Add** rules allowing TCP 80 and 443 in. Inside the VM, if `ufw` is active: `sudo ufw allow 80,443/tcp`.
3. Bring the stack up:

```bash
docker compose -f docker-compose.yml -f docker-compose.caddy.yml up -d --build
```

The first build takes several minutes (the backend image runs `dotnet test` during the build — this is deliberate; a failing test aborts the deploy).

4. Verify:

```bash
docker compose -f docker-compose.yml -f docker-compose.caddy.yml ps      # all services Up, db healthy
docker compose -f docker-compose.yml -f docker-compose.caddy.yml logs -f caddy   # watch cert issuance
```

Open `https://<your-domain>` and log in with FreeIPA credentials. On first startup the backend applies EF migrations and seeds roles + sample data automatically.

In this mode the backend's MVC admin surface is on `127.0.0.1:8080` **inside the VM only**. Reach it from your workstation via SSH tunnel:

```bash
ssh -L 8080:127.0.0.1:8080 user@<vm-ip>   # then browse http://localhost:8080
```

## Step 5, Option B — Deploy behind an existing reverse proxy

```bash
docker compose up -d --build
```

Everything binds to loopback: frontend `127.0.0.1:3000`, backend MVC admin `127.0.0.1:80`, Postgres `127.0.0.1:5433`.

- **Proxy on the same VM** (e.g. host nginx): point the HTTPS vhost at `http://127.0.0.1:3000`.
- **Proxy on another host/VM** (e.g. Nginx Proxy Manager elsewhere on the Proxmox network): the loopback binding is not reachable. Edit the frontend port mapping in `docker-compose.yml` from `"127.0.0.1:3000:3000"` to a LAN-reachable bind such as `"<vm-lan-ip>:3000:3000"`, and firewall port 3000 so **only the proxy host** can reach it (plain HTTP crosses that hop).

The proxy **must** forward `X-Forwarded-Proto` (all mainstream proxies do by default) — the frontend's middleware passes it through to the backend so the auth cookies are issued `Secure` and accepted by the browser. nginx example:

```nginx
server {
    listen 443 ssl;
    server_name helpdesk.example.com;
    # ssl_certificate / ssl_certificate_key ...

    location / {
        proxy_pass http://127.0.0.1:3000;
        proxy_set_header Host $host;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    }
}
```

In Nginx Proxy Manager: create a Proxy Host → forward to the VM IP, port 3000, scheme `http`, and enable **Websockets Support**; NPM sends the forwarded headers automatically.

## Updating a running deployment

```bash
cd helpdesk-db
git pull
cd HelpdeskDb
docker compose -f docker-compose.yml -f docker-compose.caddy.yml up -d --build   # Option A
# or: docker compose up -d --build                                              # Option B
```

Compose rebuilds only what changed and recreates the affected containers. Database data survives — it lives in the named volume `db-volume`. Remember: changing `FRONTEND_BACKEND_URL` requires a frontend image rebuild (it's a build arg, not a runtime env var).

## Backups

Two complementary layers:

1. **Proxmox vzdump** — schedule VM backups in the PVE UI (**Datacenter → Backup**). This captures everything, including the Docker volumes.
2. **Logical Postgres dumps** — cheap, restore-anywhere:

```bash
# Backup (from HelpdeskDb/; reads credentials from .env)
docker compose exec db sh -c 'pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB"' > helpdeskdb-$(date +%F).sql

# Restore into a fresh db container
docker compose exec -T db sh -c 'psql -U "$POSTGRES_USER" "$POSTGRES_DB"' < helpdeskdb-2026-08-02.sql
```

Consider a daily cron for the dump. Named volumes to be aware of: `db-volume` (Postgres data), `caddy-data` (Let's Encrypt certs/account — losing it forces re-issuance and can hit rate limits), `caddy-config`.

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| Login silently fails (no cookie set) | The browser-facing side is plain HTTP. The backend runs as Production so cookies are always `Secure` — HTTPS in front is mandatory. |
| Login returns 401 with valid credentials | VM can't reach `ipa.lapikud.ee:443` (check outbound firewall/DNS), or the IPA credentials are wrong. |
| Caddy has no certificate / TLS errors | DNS record doesn't point at this host, or 80/443 are blocked. Check `docker compose ... logs caddy`. Certs persist in `caddy-data` — don't delete that volume, re-issuing too often hits Let's Encrypt rate limits. |
| Backend restarts on first boot | Should no longer happen — `app` waits for the db healthcheck. If the db never becomes healthy, check `POSTGRES_*` values and `docker compose logs db`. |
| `!override` YAML error on `docker compose ... caddy.yml` | docker compose is older than v2.24.4 — install from the official Docker repo (Step 2). |
| Role changes in FreeIPA don't apply | `IpaServiceAccount__*` misconfigured — the re-sync on token renewal silently no-ops, so revocations wait for the 7-day refresh-token expiry. Check backend logs for the sync warning. |
| Admin MVC pages log you out after redeploys | ASP.NET Data Protection keys aren't persisted, so cookie sessions/antiforgery tokens are invalidated whenever the backend container is recreated. Harmless for the SPA (JWT); log in to the admin again. Persist keys to a volume if it becomes annoying. |

### Known quirks (by design)

- The backend image build runs the full test suite (`dotnet test App.Tests`) — slow but intentional: a red test blocks the deploy.
- The JWT lifetime is 120 s (`JWTSecurity:ExpiresInSeconds`). This is intentional — the SPA refreshes transparently via the refresh-token cookie; don't "fix" it by raising the lifetime.
- Refresh-token lifetime is hardcoded to 7 days and is not configurable via `.env`.
