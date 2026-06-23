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
├── docker-compose.yml          Local PostgreSQL
├── Dockerfile.api              Mobile API container
├── Dockerfile.admin            Admin API container
└── src/
    ├── BareqAlNaqool.Domain/          Entities and constants
    ├── BareqAlNaqool.Application/     DTOs and service interfaces
    ├── BareqAlNaqool.Infrastructure/  EF Core, Identity, JWT, services, seed data
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

**Swagger UI** (Development only):

| API | URL |
|-----|-----|
| Mobile | http://localhost:5000/swagger |
| Admin | http://localhost:5001/swagger |

Use the **Authorize** button to paste a JWT from `POST /api/auth/login` (mobile) or `POST /api/admin/auth/login` (admin).

On first startup the mobile API applies EF Core migrations and seeds demo data from `Infrastructure/Seed/en.json` and `ar.json`. Both hosts share the same PostgreSQL database.

### Connection string

Default (see `appsettings.json`):

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

Migrations run automatically on startup (`MigrateAsync`). Seeding runs only from the mobile API host.

## Railway deployment

Deploy **two services** from this repo, both connected to the same Railway PostgreSQL instance.

### 1. Add PostgreSQL

In your Railway project, add a **PostgreSQL** plugin. Railway injects `DATABASE_URL` automatically.

### 2. Mobile API service

| Setting | Value |
|---------|-------|
| Dockerfile | `Dockerfile.api` |
| Root directory | repository root |

**Environment variables:**

| Variable | Notes |
|----------|-------|
| `DATABASE_URL` | Set automatically when Postgres is linked |
| `Jwt__Key` | Long random secret (required in production) |
| `Jwt__Issuer` | `BareqAlNaqool.Api` |
| `Jwt__Audience` | `BareqAlNaqool.Clients` |

### 3. Admin API service

| Setting | Value |
|---------|-------|
| Dockerfile | `Dockerfile.admin` |
| Root directory | repository root |

Link the **same** PostgreSQL instance. Override JWT settings for admin:

| Variable | Example |
|----------|---------|
| `Jwt__Key` | Separate long random secret |
| `Jwt__Issuer` | `BareqAlNaqool.Admin` |
| `Jwt__Audience` | `BareqAlNaqool.AdminClients` |

Railway sets `PORT`; both Dockerfiles bind to `0.0.0.0:8080` and `ConfigureRailwayPort()` reads `PORT` at runtime.

### First deploy

1. Deploy the mobile API first (runs migrations + seed).
2. Deploy the admin API second (applies migrations only).

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

`appsettings.json` in each host project:

- `ConnectionStrings:DefaultConnection` — PostgreSQL (overridden by `DATABASE_URL` on Railway)
- `Jwt` — `Key`, `Issuer`, `Audience`, `ExpiryMinutes`

Use double-underscore env vars in production, e.g. `Jwt__Key`.

## Notes

- Entity primary keys are `int`; API DTOs expose string `id` values matching the Flutter app.
- `isMine` on events and `isMe` on messages are computed from the authenticated user.
- `timeAgo` is computed server-side from stored `DateTime` values.
