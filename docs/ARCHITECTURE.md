# Current architecture

V1 is one ASP.NET Core deployment plus an optional instrument-sync worker. Business modules remain separate class libraries. The API is the composition root and currently still contains several endpoint orchestration handlers; complete extraction of those handlers into module application layers is pending.

Each business library now has Domain, Application, Infrastructure and Contracts directories, plus DependencyInjection.cs. Existing public namespaces were retained to preserve callers while moving implementations. Shared cross-module interfaces live in MyFundex.Contracts; business projects reference Contracts/BuildingBlocks instead of each other's DbContexts.

Each module defaults to `fundex_<module>`; accounts/master/market/notification/withdrawal retain their established singular short names. The host's deployment model combines explicit module mappings. EF history lives in fundex_integration. It is a fresh-install migration, not a legacy-data migration.

EntityBase supplies internal long keys, audit fields, soft deletion and optimistic Version. Public entities retain domain-specific UUIDs. Monetary properties default to numeric(20,4), with quantity/price overrides retained. Immutable financial/audit records reject updates/deletion through SaveChanges.

External broker/payment calls happen outside local DB transactions. Provisioning uses idempotent receivers across module boundaries; a paid-payment polling worker retries partial initial activation. This is not yet the full durable financial outbox/reconciliation architecture.

Account route selection and payment environment are independent. Evaluation always targets sandbox; Funded targets live and requires a broker credential reference. Live activation and broker account opening/funding remain separate unfinished workflows.

See SETUP_AND_VALIDATION.md for limitations, key management, migration and verification status.
