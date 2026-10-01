# Neon UAT setup — 2026-10-01

The workspace is linked to project `aged-cloud-91446800` (`fundx`), branch `production` (`br-calm-truth-b41mpzf2`), region `aws-us-east-2`. The branch name is production; MyFundex uses the explicitly supplied database `fundx_uat` for UAT.

Neon CLI 7.0.1, project skills and MCP were installed. `neon mcp -y` configured detected clients globally, including Codex, using an account-scoped API key. Restart/reconnect the client to load the MCP configuration if necessary. The key is stored by the CLI/client configuration, not in this repository.

`neon.ts` and `hello.ts` contain the requested configuration and handler. Deployment succeeded. The `data` bucket is private. The public hello function returned HTTP 200 and `Hello from Neon Functions`:

https://br-calm-truth-b41mpzf2-api.compute.c-6.us-east-2.aws.neon.tech/

This function is a deployment smoke test. It does not host the ASP.NET MyFundex application.

## MyFundex database

The supplied connection is stored only in git-ignored `.env.uat`, with certificate verification and channel binding required. `.env.local`, written by Neon, and `.neon` are also ignored. Do not copy their credentials into tracked configuration or frontend files. The Neon-generated default DATABASE_URL is separate from the supplied MyFundex UAT connection.

All four MyFundex migrations applied successfully to `fundx_uat`. The local API returned HTTP 200 with `{"status":"Ready"}` from `/health/ready`. The readiness smoke ran with financial workers and automatic seeding disabled; it does not establish provider or full trading lifecycle acceptance.

Run from the repository root:

```powershell
# Apply outstanding migrations using the direct endpoint.
./scripts/Invoke-Uat.ps1 -Action Migrate

# Start MyFundex with the private UAT environment settings.
./scripts/Invoke-Uat.ps1 -Action Run
```

Configure the normal application environment/JWT/encryption settings described in SETUP_AND_VALIDATION.md before starting. No administrator account was seeded by this setup. Real trading, checkout and payouts remain disabled in the local UAT configuration. Configure provider credentials independently when ready for acceptance tests.

Future function changes: `neon config plan`, then `neon deploy`. The CLI accepts the requested `preview` wrapper but warns that functions/buckets are now GA and may be moved to top level. The requested configuration was preserved.
