# MyFundex

Modular-monolith funded-equity platform under development. INR/equities, Google trader sign-in, versioned two-/three-step challenge plans, Razorpay payments, and account-level Upstox sandbox/live routing.

**The production MVP is not complete.** See [setup and current validation](docs/SETUP_AND_VALIDATION.md) for implemented work, required secrets, database installation, and the remaining execution/financial lifecycle.

## Local checks

```text
dotnet tool restore
dotnet restore backend/MyFundex.sln
dotnet build backend/MyFundex.sln
dotnet test backend/MyFundex.sln
npm --prefix ui/agent-ui ci
npm --prefix ui/agent-ui run lint
npm --prefix ui/agent-ui run build
npm --prefix ui/admin-manager-ui ci
npm --prefix ui/admin-manager-ui run lint
npm --prefix ui/admin-manager-ui run build
```

Start PostgreSQL using the development docker-compose.yml or your own installation. Apply the initial migration to a fresh database, then run `dotnet run --project backend/src/MyFundex.Api`. The original development profile listens on HTTP 50901 and HTTPS 50900. Frontends use ports 5173 and 5174. Google login requires HTTPS and provider configuration.

SQL installation and optional demonstration inserts are in `database/`. Existing old-schema databases need a separate reviewed data migration; the fresh-install script does not move their data.

Production configuration names are in `.env.example`; inject real values securely. SMTP and SMS adapters are in `MyFundex.Messaging`, with SMS disabled until a provider is selected. Checkout is disabled by default.
