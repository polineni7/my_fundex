# Architecture

## V1 deployment
A modular monolith with one ASP.NET Core host and one PostgreSQL database. Every business module is an independent .NET project and owns one PostgreSQL schema. Redis is reserved for hot cache/rate-limit/realtime expansion. External Upstox calls are behind `IBrokerOrderGateway`.

## Scale path
- <10K active: 2+ stateless API replicas, PostgreSQL HA, Redis, dedicated workers as traffic grows.
- 10K-50K: separate MarketData and BrokerExecution processes, read replicas, managed WebSocket/SignalR, message broker when queue pressure justifies it.
- 50K-100K+: extract Trading/Risk/MarketData/Wallet/Portfolio/Notification/Intelligence modules into services. Contracts remain stable.

## DB-lock discipline
- No broker/network calls inside DB transactions.
- Short financial transactions only.
- Optimistic concurrency through `Version`.
- `ExecuteUpdateAsync` for atomic capital reservations.
- Read-only queries use `AsNoTracking` and projection.
- Correct composite/partial indexes should be added from production query patterns.
- Event/history tables are append-oriented.

## Security
- Internal PKs are never exposed by normal APIs.
- Public IDs are UUIDv7 (`Guid.CreateVersion7()` in .NET 9+; on .NET 8 use a UUIDv7 library or database generation). NOTE: this source targets net8; replace `Guid.CreateVersion7()` with the supplied UUID helper/library if your SDK lacks it.
- JWT + role/permission claims.
- Secrets should ultimately move to a vault; DB setting rows are for runtime non-secret configuration or encrypted references.
- UAT and PROD must be separate deployments/databases.
