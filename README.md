# MyFundex V1

Partial modular-monolith implementation using .NET 8, PostgreSQL and React/Vite. **Not production-ready.** See [implementation status](docs/IMPLEMENTATION_STATUS.md) for verified changes, missing features and explicit execution/payment gates.

## Local setup

1. Install .NET 8 SDK, Node.js 22.12+ (Node 24 was used for verification), and Docker or PostgreSQL.
2. Run `docker compose up -d postgres redis` if Docker is available.
3. Run `dotnet restore backend/MyFundex.sln`.
4. Run `dotnet run --project backend/src/MyFundex.Api`.
5. In each UI directory (`ui/agent-ui`, `ui/admin-manager-ui`), run `npm ci` then `npm run dev`.

API: `http://localhost:50901`. Trader UI: `http://localhost:5173`. Admin UI: `http://localhost:5174`.

The Development launch profile creates a **fresh** database using the combined module model. `EnsureCreated` does not upgrade existing databases. Do not drop existing data to apply these changes; prepare a reviewed migration. Production migrations have not been supplied, and `dotnet ef database update` is not currently a valid installation procedure.

Development-only bootstrap account: `admin@myfundex.local` / `ChangeMe!123`. This is seeded only when the identity user table is empty. Production does not auto-create the schema or seed this account.

## Configuration

Use environment variables or a secret store. Do not commit credentials or enter secrets in the runtime settings UI.

- `ConnectionStrings__Postgres`: PostgreSQL connection string
- `Jwt__Key`: strong signing key of at least 32 characters (required outside Development)
- `Cors__Origins__0`, `Cors__Origins__1`: allowed frontend origins
- `Razorpay__KeyId`, `Razorpay__KeySecret`, `Razorpay__WebhookSecret`
- `Razorpay__CheckoutEnabled`: false by default; keep disabled until challenge fulfilment is complete
- `Broker__Upstox__Sandbox__AccessToken`
- `Broker__Upstox__Production__AccessToken`
- `Broker__Upstox__MarketData__AccessToken`
- `VITE_API_URL`: optional frontend API base URL, including `/api/v1`

The non-secret setting `Broker.Upstox.Environment` selects SANDBOX/PRODUCTION. Market quotes require supported market-data credentials; sandbox order credentials do not imply sandbox quote availability.

For payment integration testing, configure Razorpay's `payment.captured` webhook at `/api/v1/webhooks/razorpay`. Verification checks the signature and independently fetches the captured payment and expected amount. Payment-to-challenge fulfilment is not implemented; do not enable checkout for customers yet.

## Checks

```text
dotnet test backend/MyFundex.sln
npm --prefix ui/agent-ui run build
npm --prefix ui/admin-manager-ui run build
```

Regression tests do not substitute for PostgreSQL concurrency tests, provider sandbox tests or end-to-end financial lifecycle validation.
