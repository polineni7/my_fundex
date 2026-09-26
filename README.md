# MyFundex V1

Production-oriented modular-monolith starter for a funded-equity-account platform.

## Stack
- .NET 8 ASP.NET Core Web API
- EF Core 8 + PostgreSQL
- Redis-ready caching
- JWT/RBAC
- Upstox Sandbox/Production gateway abstraction
- React + Vite + Zustand + Tailwind (Trader and Admin/Manager apps)
- Outbox-ready module contracts

## Architectural rules
1. One business module = one .NET project.
2. One PostgreSQL database in V1, one schema per module.
3. No module directly reads/writes another module's DbContext/tables.
4. Cross-module synchronous interactions use interfaces in `MyFundex.Contracts`.
5. Cross-module asynchronous interactions use integration-event contracts/outbox.
6. `Id` is BIGINT internal PK; domain-specific IDs (`AccountId`, `OrderId`, etc.) are UUID/Guid public identifiers.
7. All standard entities inherit `EntityBase`: created/updated/deleted audit + soft-delete + version.
8. External broker calls never occur while holding a DB transaction.
9. Money uses `decimal` / PostgreSQL `numeric`, never float/double.
10. Production and UAT/Sandbox use separate databases and deployment settings.

## Important Upstox note
Sandbox support is provider-defined. The implementation uses the real Upstox HTTP endpoints configured in the database; there is no fake broker implementation. Configure valid sandbox/live access tokens before order placement.

## Run backend
1. Start PostgreSQL and Redis with `docker compose up -d postgres redis`.
2. Install .NET 8 SDK.
3. `dotnet restore backend/MyFundex.sln`
4. `dotnet ef database update --project backend/src/MyFundex.Api` (or use the included migration bootstrap pattern after adding migrations).
5. `dotnet run --project backend/src/MyFundex.Api`

Default dev admin seeded by the API bootstrap:
- Email: `admin@myfundex.local`
- Password: `ChangeMe!123`

Change this immediately outside local development.

## Run UIs
From `ui/agent-ui` and `ui/admin-manager-ui`:
```
npm install
npm run dev
```

## Environment
API expects PostgreSQL/Redis/JWT bootstrap values in `appsettings*.json` or environment variables. Dynamic business/provider settings live in `configuration.settings` and are cached.
