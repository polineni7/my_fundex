# Risk policy management

Open Admin > Risk policies to create a named policy group and view its saved rules. Creation produces an active version for new account assignments. Choose the group in an assessment stage. Existing accounts retain their assigned version.

Supported rules:
- Equity only (required; instrument eligibility is enforced by the trading services).
- Maximum estimated order value in INR (required; rejects values above the limit, allows equality).
- Optional entry window in IST (both HH:mm values required when enabled; same-day start inclusive and end exclusive). New buy orders outside the window are rejected and audited. Sell orders are exempt from this window so positions can be reduced. Other risk checks and market-session restrictions still apply.

Previously accepted/resting orders are not cancelled by the entry-window rule. This is an order-submission window, not a square-off schedule. Profit targets and daily/total loss limits remain assessment-plan settings. Rules are supported explicitly; there is no arbitrary expression/script execution.

Policy creation and viewing use policies.write and policies.read permissions respectively (administrators retain full access). The UI exposes creation and version/rule viewing; editing existing versions, manual reassignment, and custom rule types are not included. No database migration is required: existing PolicySets, PolicyVersions, Rules, Assignments, and Violations tables are used.
