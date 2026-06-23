# Bareq Al Naqool API

.NET 10 backend for the Bareq Al Naqool family mobile app. API-only — no admin UI.

## Stack

| Layer | Technology |
|-------|------------|
| Database | PostgreSQL |
| ORM | Entity Framework Core 10 |
| Deployment | [Railway](https://railway.app) |

## Solution structure

```
bareq_alnaqool_api/
├── BareqAlNaqool.slnx
├── docker-compose.yml          Local PostgreSQL + optional API containers
├── Dockerfile.api              Mobile API container
├── Dockerfile.admin            Admin API container
├── Dockerfile.migrate          One-off database migration job
└── src/
    ├── BareqAlNaqool.Domain/          Entities and constants
    ├── BareqAlNaqool.Application/     DTOs and service interfaces
    ├── BareqAlNaqool.Infrastructure/  EF Core, Identity, JWT, services, seed data
    ├── BareqAlNaqool.Migrator/        CLI migration + seed runner
    ├── BareqAlNaqool.Api/             Mobile REST API (port 5000)
    └── BareqAlNaqool.Admin/           Admin REST API (port 5001)
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (for local PostgreSQL)

## Local development

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Build and run

```bash
dotnet restore
dotnet build
```

**Mobile API** (port 5000):

```bash
dotnet run --project src/BareqAlNaqool.Api
```

**Admin API** (port 5001):

```bash
dotnet run --project src/BareqAlNaqool.Admin
```

**Swagger UI** — visit the service root URL or `/swagger`:

| API | URL |
|-----|-----|
| Mobile | http://localhost:5000/swagger |
| Admin | http://localhost:5001/swagger |

Use the **Authorize** button to paste a JWT from `POST /api/auth/login` (mobile) or `POST /api/admin/auth/login` (admin).

On first startup in **Development**, the mobile API applies EF Core migrations and seeds demo data from `Infrastructure/Seed/en.json` and `ar.json`. Both hosts share the same PostgreSQL database.

**Health checks:** `GET /health` (liveness) and `GET /health/ready` (includes database).

### Connection string

Local default (see `appsettings.Development.json`):

```
Host=localhost;Port=5432;Database=bareq_alnaqool;Username=postgres;Password=postgres
```

Copy `.env.example` to `.env` and adjust values if needed.

### EF Core migrations

```bash
dotnet ef migrations add <Name> \
  --project src/BareqAlNaqool.Infrastructure \
  --startup-project src/BareqAlNaqool.Api

dotnet ef database update \
  --project src/BareqAlNaqool.Infrastructure \
  --startup-project src/BareqAlNaqool.Api
```

Or run the migrator directly:

```bash
dotnet run --project src/BareqAlNaqool.Migrator              # migrate only
dotnet run --project src/BareqAlNaqool.Migrator -- --seed  # migrate + seed
```

In Development, the mobile API still migrates/seeds on startup by default (`Database:MigrateOnStartup` / `Database:SeedOnStartup` in `appsettings.Development.json`). Production containers do **not** migrate on startup.

## Railway deployment

Deploy **three services** from this repo: a one-off **Migrator**, then **Mobile API** and **Admin API** — all connected to the same Railway PostgreSQL instance.

### 1. Add PostgreSQL

In your Railway project, add a **PostgreSQL** plugin.

### 2. Database migrator (run before API deploys)

| Setting | Value |
|---------|-------|
| Dockerfile | `Dockerfile.migrate` |
| Root directory | repository root |

**Environment variables:**

| Variable | Value |
|----------|-------|
| `DATABASE_URL` | `${{ Postgres.DATABASE_URL }}` (use your Postgres service name) |

**Start command** (first deploy only, to seed demo data):

```
--seed
```

After the first successful run, remove `--seed` from the start command (or delete the migrator service). Re-run the migrator only when you add new EF migrations.

### 3. Mobile API service

| Setting | Value |
|---------|-------|
| Dockerfile | `Dockerfile.api` |
| Root directory | repository root |

**Required environment variables:**

| Variable | Notes |
|----------|-------|
| `DATABASE_URL` | `${{ Postgres.DATABASE_URL }}` — required on **both** API services |
| `Jwt__Key` | Long random secret, 32+ characters (required) |
| `Jwt__Issuer` | `BareqAlNaqool.Api` |
| `Jwt__Audience` | `BareqAlNaqool.Clients` |

Containers run with `ASPNETCORE_ENVIRONMENT=Production`. The app refuses to start if `DATABASE_URL` or `Jwt__Key` is missing or uses a dev default.

### 4. Admin API service

| Setting | Value |
|---------|-------|
| Dockerfile | `Dockerfile.admin` |
| Root directory | repository root |

Link the **same** PostgreSQL instance:

| Variable | Example |
|----------|---------|
| `DATABASE_URL` | `${{ Postgres.DATABASE_URL }}` |
| `Jwt__Key` | Separate long random secret (32+ chars) |
| `Jwt__Issuer` | `BareqAlNaqool.Admin` |
| `Jwt__Audience` | `BareqAlNaqool.AdminClients` |

Railway sets `PORT`; Dockerfiles bind to `0.0.0.0:8080` and `ConfigureRailwayPort()` reads `PORT` at runtime.

### Deploy order

1. Run the **Migrator** service once (`--seed` on first deploy).
2. Deploy **Mobile API**.
3. Deploy **Admin API**.

Point the Flutter app `API_BASE_URL` at the mobile API Railway URL.

## Localization

Pass `?lang=en` (default) or `?lang=ar` on any mobile endpoint to receive localized strings.

## Demo accounts

| Role   | Username | Email                     | Password    |
|--------|----------|---------------------------|-------------|
| Member | ahmed    | ahmed.alnaqool@email.com  | password123 |
| Admin  | admin    | admin@alnaqool.com        | Admin123!   |

Guest access: `POST /api/auth/guest` (mobile API).

## Mobile API endpoints

| Area | Endpoints |
|------|-----------|
| Auth | `POST /api/auth/login`, `register`, `guest`, `logout`, `forgot-password`; `GET /api/auth/session` |
| Home | `GET /api/home/stats`, `/api/home/latest-news` |
| Landing | `GET /api/landing/slides` |
| News | `GET /api/news`, `/api/news/categories`, `/api/news/{id}` |
| Events | `GET /api/events`, `/api/events/{id}`, `/api/events/{id}/registered`; `POST /api/events/{id}/register` |
| Family tree | `GET /api/family-tree/branches`, `/branches/{id}`, `/branches/{id}/members`, `/founder-lineage` |
| Messages | `GET /api/messages/filters`, `/conversations`, `/conversations/{id}`, `/conversations/{id}/messages`; `POST /api/messages/conversations/{id}/messages` |
| Gallery | `GET /api/gallery/types`, `/albums`, `/albums/{id}`, `/albums/{id}/photos` |
| Documents | `GET /api/documents/categories`, `/api/documents`, `/api/documents/{id}` |
| Council | `GET /api/council/modules`, `/latest-meeting`, `/president`, `/{moduleId}/items` |
| Profile | `GET/PUT /api/profile` |
| Directory | `GET /api/directory/branches`, `/api/directory`, `/api/directory/{id}` |
| Notifications | `GET /api/notifications`, `/unread-count`; `PUT /api/notifications/{id}/read`, `/read-all` |
| Contact | `POST /api/contact` |

Protected routes require `Authorization: Bearer <token>` from login, register, or guest.

## Admin API endpoints

Login: `POST /api/admin/auth/login`

All other routes require admin JWT (`Authorization: Bearer <token>`).

| Resource | Base route |
|----------|------------|
| Dashboard | `GET /api/admin/dashboard/stats` |
| News | `/api/admin/news` |
| Events | `/api/admin/events` |
| Branches | `/api/admin/branches` |
| Albums | `/api/admin/albums` |
| Documents | `/api/admin/documents` |
| Notifications | `/api/admin/notifications` |
| Council items | `/api/admin/council-items` |
| Directory members | `/api/admin/directory-members` |
| Users | `/api/admin/users` |

Each CRUD resource supports `GET`, `GET/{id}`, `POST`, `PUT/{id}`, `DELETE/{id}`.

## Configuration

| Setting | Local (Development) | Production (Railway) |
|---------|---------------------|----------------------|
| Database | `ConnectionStrings__DefaultConnection` or localhost default | `DATABASE_URL=${{ Postgres.DATABASE_URL }}` |
| JWT | `Jwt__Key` in `.env` or `appsettings.Development.json` | `Jwt__Key` (32+ chars, unique per service) |
| Migrations | Auto on mobile API startup | Run `Dockerfile.migrate` job |
| Seed data | Auto on mobile API startup (Development) | Migrator with `--seed` (first deploy only) |

Use double-underscore env vars, e.g. `Jwt__Key`, `Database__MigrateOnStartup`.

## Notes

- Entity primary keys are `int`; API DTOs expose string `id` values matching the Flutter app.
- `isMine` on events and `isMe` on messages are computed from the authenticated user.
- `timeAgo` is computed server-side from stored `DateTime` values.
