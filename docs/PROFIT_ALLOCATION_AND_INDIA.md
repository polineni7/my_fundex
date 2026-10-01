# Plan profit allocation and Indian operating model

## Implemented

ADMIN can create ADMIN, MANAGER and TRADER users through Users and access. Passwords are BCrypt cost-12 hashes; creation, roles and audit are committed transactionally. Creating a trader does not bypass purchase/evaluation eligibility. This is password-account creation; Google linking to an existing identity remains deliberately blocked pending a verified linking flow.

Plan-version creation now accepts TaxWithholdingPercent and OtherDeductionPercent alongside RewardSharePercent. Rates are immutable through the existing published-version workflow; change terms by creating a new version. Both deduction rates default to zero and must be nonnegative with their sum below 100%. These defaults do not mean an exemption from Indian tax.

Allocation order:
1. For final settlement, remove verified broker charges from realized account surplus after losses are covered and all orders/positions are resolved.
2. Calculate trader gross share and platform share.
3. Calculate configured tax withholding and other deductions on the trader gross share.
4. Credit only net trader reward to withdrawable funds; record TaxWithheld and OtherDeductions separately in the balanced wallet journal.

Example: INR 1,000 net distributable profit, 80% trader share, 10% withholding and 5% other deductions yields INR 200 platform share, INR 80 withheld, INR 40 other deductions and INR 680 trader net. This is arithmetic illustration, NOT a statutory Indian tax rate recommendation.

New live sell executions persist realized P&L and plan-rate snapshots. The trader Trade earnings screen shows the provisional split per execution and separate actual verified settlements. Estimates exclude broker charges and account-wide losses and are not automatically paid per winning trade. Historical executions lacking captured rates/P&L are displayed as unavailable, not fabricated. Evaluation profits are simulated, not withdrawable.

Withheld tax is a payable/reserve, not evidence of government remittance, a tax certificate or a completed tax return. Platform share is persisted in settlement records; statutory tax reporting/remittance and corporate general-ledger export remain unimplemented.

## Corporate proprietary account requirement

The owner describes traders as working for the firm through one backend broker account. This differs from the current one-broker-account-per-funded-allocation design. Shared-broker live execution is NOT implemented or enabled by these changes. The existing unique broker-account restriction remains in place.

Before implementing/enabling that route, establish the corporate account holder, broker-authorized dealer/API access model, employment/contractor classification, and treatment of evaluation purchase fees. A shared account needs a central reconciled capital pool, unique per-trader reservations, aggregate limits, broker execution allocation, corporate-action handling, external-order detection and recovery across all trader books. Removing the existing uniqueness restriction alone would permit double allocation of money and is not a solution.

## Indian regulatory scope

SEBI's 4 November 2024 advisory specifically addresses unauthorized virtual/paper trading offerings based on listed-stock data and refers to securities-market competitions/prize distributions. The proposed paid evaluation/public access model requires Indian securities counsel and broker review; describing participants as employees does not settle that question.

Source: https://www.sebi.gov.in/sebi_data/attachdocs/nov-2024/1730717924773.pdf

Indian tax withholding depends on the legally determined nature of payment, payer/payee, thresholds and applicable law. Employee remuneration and contractor/business payments cannot be assigned the same arbitrary per-trade rate by default. A CA must specify the withholding/payroll treatment, PAN/KYC requirements, GST/invoicing treatment for plan fees, deposit/reporting/certificate obligations and effective dates. Current generic percentage fields do not implement payroll, threshold aggregation, tax returns, STT/GST schedules or statutory certificates.

Source: https://www.incometax.gov.in/iec/foportal/help/all-topics/e-filing-services/tax-payments

Live trading, checkout and payout flags remain disabled in the UAT configuration. There is no claim that this software or business model is SEBI-approved or fully compliant.

## Validation

Backend build passes with zero warnings/errors. All 41 backend tests pass, including real PostgreSQL migrations and deduction-ledger idempotency/balancing tests. Both frontend lint/build checks pass. Migration SQL is database/007_plan_profit_deductions.sql.

UAT migration applied successfully. API smoke passed: ADMIN creates ADMIN/MANAGER/TRADER identities (201), duplicate emails rejected (409), each identity logs in with its assigned role (200), and TRADER creation attempts are forbidden (403). Test identities were confined to fundx_identity_tests.
