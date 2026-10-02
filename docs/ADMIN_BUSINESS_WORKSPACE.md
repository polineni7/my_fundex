# Admin business workspace — 2026-10-02

## Delivered

Existing modular structure retained: coupons in Payments; audiences, members, campaigns, delivery history and content in Administration. Entities use the common audited/soft-delete/concurrency base. Indexed UUID lookups and bounded queries are used. External SMTP/payment calls stay outside database transactions.

- Coupon CRUD and archive; fixed/percentage discounts, dates, plan/user eligibility, minimum fee, per-user and total limits. Trader checkout accepts a coupon. Server computes and snapshots the discounted price. Optimistic concurrency protects reservations. Minimum INR 1 remains payable. Once reserved, pricing and eligibility are immutable.
- Audience create/edit/archive, member creation and consent editing, registration-date imports without inferred consent.
- Campaign draft CRUD, saved audience selection, scheduling, explicit queue/cancel and delivery history. Consent is rechecked at delivery. Ambiguous SMTP outcomes require manual review and are not automatically resent. In-progress sends can finish after cancellation.
- CMS/event records: pages, banners, FAQs, events; draft/publish; visibility dates; public content API. Content is plain text.
- Broker purpose tables and create/edit/verify; archive only disabled connections without linked trading accounts.
- Business report and displayed-trader CSV export; contextual back navigation; dropdown trading controls.

## Database

Migration `20261002113846_BusinessManagement` adds six tables and payment discount snapshots. SQL: `database/012_business_management.sql`. Tested on isolated PostgreSQL, then applied to UAT without resetting existing data.

## Sending

Set up existing SMTP configuration and `Campaigns:PublicBaseUrl` (HTTPS API origin), then explicitly enable `Campaigns:SendingEnabled`. Sending is disabled by default. Unsubscribe links use random tokens and confirmation POST; prior unsubscribe blocks administrative re-import with marketing consent. A verified resubscribe flow remains unimplemented.

## Verification

69 backend tests passed with PostgreSQL enabled. Backend build passed with zero warnings/errors. Both frontends passed lint/build. Isolated API smoke covered coupon create/archive, audience/member/consent, campaign draft/archive, disabled send gate, public content and archive visibility. No real email, payment or trade was sent.

## Remaining limits

- Razorpay discounted capture/refund and SMTP delivery require provider integration testing. Uncertain coupon reservations remain consumed; refund/redemption release tools are not implemented.
- Bounded lists (500 records, 1000 members, latest 200 traders) need pagination for larger datasets. Saved dynamic assessment segments and audience CSV workflows remain.
- Public CMS API exists; public-site route rendering, SEO metadata, media and event registration are not implemented.
- Finance reports fees and allocations, not company cash/net income. Expense/tax ledgers, bank reconciliation and refund workflows remain.
- Angel One/Groww execution, shared corporate brokerage, and fully database-managed payment/SMTP secrets remain incomplete.
- No browser visual/accessibility audit was performed in this pass.

This delivery is not a claim that all production business requirements are complete.
