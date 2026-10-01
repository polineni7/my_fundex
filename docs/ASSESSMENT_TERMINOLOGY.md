# Assessment terminology and flow

Business description: applicant enrols in a paid simulated trading assessment; a successful applicant becomes eligible for contract review, not automatically an employee. Contracted traders may work with firm capital only under an approved corporate/broker arrangement.

Current storage rename (data preserved):
- fundex_subscription.Subscriptions → AssessmentEnrollments
- SubscriptionId → EnrollmentId
- PreviousSubscriptionId → PreviousEnrollmentId
- SubscribedAt → EnrolledAt
- PlanVersions.RegistrationFee → AssessmentFee

The fundex_subscription module/schema and legacy C# and JSON names remain for compatibility. They are implementation names, not a claim of recurring billing. Historical migration SQL retains historical names. The latest default-data script uses AssessmentFee. Stop old API instances before migration and deploy updated binaries together.

User wording now uses assessment plan, assessment fee and assessment enrolment. An assessment purchase creates a new simulated account, pass advances stages, fail preserves history and a new purchase starts a fresh attempt. Completion allows review; it does not establish a signed contract, employee status or live broker permission. Contract signing, employment verification and shared corporate-broker accounting remain unimplemented and must not be implied by the wording.

Public crawlable information page: /assessment.html. Static HTML includes meaningful title, description, language, headings and readable process content. Account portals retain noindex/nofollow. Canonical URLs and sitemap submission require the actual production domain, which is not configured; no domain or SEO ranking promise was invented.

Before public launch, supply the actual legal entity/contact, approved assessment/refund terms, privacy notice, contract and broker access arrangement. No legal policy was fabricated. Renaming a fee does not determine its tax or securities-law treatment. See PROFIT_ALLOCATION_AND_INDIA.md for official references and unresolved operational requirements.
