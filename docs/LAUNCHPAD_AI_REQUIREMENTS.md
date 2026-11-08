# Launchpad AI: requirements

| | |
|---|---|
| Version | 0.1 (draft for review) |
| Date | 2026-11-08 |
| Status | Draft. To be strengthened before a spec (design and tasks) is written. |
| Owner | Forge platform team |

Requirement IDs are stable; keep them when editing. Priority: **Must** (first release), **Should** (soon after), **Could** (later).

---

## 1. Purpose

Launchpad AI turns a client's requirements document into a reviewed delivery plan on the Forge platform:
which services to generate and how to configure them, which AWS resources they need, what it will cost and take,
and what remains to be built by the domain team or added to the platform.

## 2. Background

- Forge generates services ("prints") from a manifest built in the Launchpad wizard. Many client needs are already
  covered by the platform (authentication, tenancy, observability, Genesis modules).
- Today a person reads each client document and maps it to Forge by hand. Coverage, cost and effort are estimated
  case by case, and platform gaps found in one client project are not captured for others.
- There is no up-to-date, machine-readable list of what Forge offers. `docs/FORGE_CAPABILITY_SUMMARY.md` is
  hand-written and already out of date.

## 3. Goals and non-goals

**Goals**

- G1. Show, per client requirement, whether the platform covers it, configuration covers it, or it is a gap.
- G2. Propose the services, their Launchpad configuration and the AWS resources, ready for review.
- G3. Estimate AWS cost and delivery time with visible assumptions.
- G4. Feed platform gaps into the Forge backlog so the platform grows from real demand.
- G5. Keep a person in control: nothing is generated or provisioned without approval.

**Non-goals (first release)**

- Writing domain business logic.
- Provisioning AWS resources without the existing Launchpad generation and pipeline steps.
- Binding commercial quotes. Estimates are planning ranges, not prices.
- Clouds other than AWS.

## 4. Users

| User | Needs |
|---|---|
| Solution architect / pre-sales | Fast, defensible fit-gap and estimate for a client proposal |
| Delivery lead | Service plan, effort and gap list to plan the team |
| Platform owner | Platform gaps with demand signals, to prioritise the roadmap |
| Domain developer | A pre-filled, valid Launchpad configuration to generate from |

## 5. Glossary

| Term | Meaning |
|---|---|
| Catalog | The machine-readable list of everything Forge can generate or include (section 7.1) |
| Requirement list | The normalised, structured version of the client document (section 7.2) |
| Fit-gap report | Per requirement: covered by platform, by configuration, or a gap (section 7.3) |
| Proposal | Services, draft manifests and AWS resource plan derived from the report (sections 7.4, 7.5) |
| Analysis | One run of Launchpad AI over one client document, with its report, proposal and estimate |

## 6. Workflow

1. Upload the client document.
2. Launchpad AI normalises it into a requirement list and asks clarifying questions.
3. The user answers. Launchpad AI produces the fit-gap report.
4. Launchpad AI produces the proposal (services, configuration, AWS resources) and the estimate.
5. The user reviews and edits, then approves.
6. Approved services pre-fill the Launchpad wizard. Generation follows the existing flow.
7. Approved platform gaps are filed in the platform backlog.

---

## 7. Functional requirements

### 7.1 Forge Catalog (prerequisite)

| ID | Pri | Requirement |
|---|---|---|
| CAT-1 | Must | The catalog lists every service type, always-included capability, add-on module (Genesis and Canvas), and configuration choice (database, Cognito, single or multi-tenant, background jobs, utilities). |
| CAT-2 | Must | Each item has: name, one-line description, category, status (available, preview, coming soon), cloud (agnostic or AWS), package and version where relevant, and a link to its developer documentation. |
| CAT-3 | Must | Each item declares the AWS resources it needs (for example Caching: ElastiCache; File Storage: S3; Database: RDS PostgreSQL; Cognito: user pool and app clients; Messaging: SQS/SNS) and which properties of them are configurable. |
| CAT-4 | Must | Dependencies and conflicts ("requires", "excludes") come from the same rules the Engine enforces (`ManifestValidator`), not from a separate hand-written list. |
| CAT-5 | Must | The catalog is kept as data in the Forge Engine and exposed through the Forge API. |
| CAT-6 | Should | Launchpad has a Catalog page to browse the catalog without starting a generation. |
| CAT-7 | Should | `docs/FORGE_CATALOG.md` is generated from the catalog in CI and replaces `FORGE_CAPABILITY_SUMMARY.md`. |
| CAT-8 | Must | A build check fails when a module or option exists in the Engine but not in the catalog, or the reverse. |

Acceptance: for every module in the Engine registry there is exactly one catalog item; the generated document matches the API output.

### 7.2 Intake and normalisation

| ID | Pri | Requirement |
|---|---|---|
| INT-1 | Must | Accept RFP, BRD, SRS and user-story documents as PDF, DOCX or Markdown, and pasted text. |
| INT-2 | Must | Produce a requirement list. Each item has an id, a category (functional, integration, non-functional, compliance, tenancy, data) and a reference to its source (document, section, page or line). |
| INT-3 | Must | Ask clarifying questions when information needed for a decision is missing: for example number of client customers (tenancy), sign-in provider, expected users and data volume, environments, data residency. |
| INT-4 | Must | The user can edit, add, merge or delete requirements before analysis continues. |
| INT-5 | Should | Detect duplicate and conflicting requirements and point them out. |
| INT-6 | Could | Accept several documents for one client (for example an RFP plus an answers sheet). |

Acceptance: on the golden set (NFR-Q1), at least 90% of the requirements a person extracts are present in the list, each with a correct source reference.

### 7.3 Fit-gap report

| ID | Pri | Requirement |
|---|---|---|
| FIT-1 | Must | Classify each requirement as one of: **Platform** (an always-included capability), **Configuration** (a catalog option or module), **Domain gap** (business logic to build), **Platform gap** (reusable capability Forge does not have), **Out of scope**. |
| FIT-2 | Must | Every classification cites the catalog item(s) it relies on and gives a one-line reason. |
| FIT-3 | Must | Only catalog items with status "available" (or "preview", marked as such) may be used to cover a requirement. "Coming soon" items are reported as "planned", not as covered. |
| FIT-4 | Must | Each classification has a confidence level (high, medium, low). Low-confidence items are flagged for review. |
| FIT-5 | Must | Show a summary: share of requirements per class, and the list of gaps. |
| FIT-6 | Should | Map compliance requirements (data residency, encryption, retention, audit) to the platform controls that meet them, or mark them as gaps. |

Acceptance: on the golden set, classification agrees with the expected answer for at least 85% of requirements; no requirement is marked as covered by an item that is not in the catalog.

### 7.4 Service proposal

| ID | Pri | Requirement |
|---|---|---|
| PRO-1 | Must | Propose the services to generate (type, name, purpose) and which requirements each one serves. |
| PRO-2 | Must | Produce a draft Launchpad configuration (manifest) for each service: modules, database, authentication, tenancy mode, background jobs, utilities. |
| PRO-3 | Must | Every draft manifest passes `ManifestValidator`. Invalid drafts are corrected or shown with the validation errors; they are never offered as ready. |
| PRO-4 | Must | Respect the vertical's existing setup and defaults (environments, database engine, Cognito pool) and reuse existing shared resources instead of proposing duplicates. |
| PRO-5 | Should | The user can apply an approved proposal to the Launchpad wizard in one step, then generate through the normal flow. |

Acceptance: every proposed manifest validates; applying a proposal produces the same wizard state a person would enter by hand for the golden-set cases.

### 7.5 AWS resource plan

| ID | Pri | Requirement |
|---|---|---|
| AWS-1 | Must | List every AWS resource the proposal needs, per environment, derived from the catalog items chosen (CAT-3). The AI does not add resources that no catalog item declares. |
| AWS-2 | Must | For each resource, propose its configurable properties (size or class, capacity, retention, backups, encryption, region) with the requirement or assumption that drives each value. |
| AWS-3 | Must | Mark resources that already exist for the vertical and will be reused. |
| AWS-4 | Must | The user can edit every property before approval. |
| AWS-5 | Should | Approved properties pre-fill Launchpad so the generated print and its infrastructure code (Terraform or CDK) use them. |
| AWS-6 | Should | Every planned resource carries cost-allocation tags (client, vertical, service, environment). |

Acceptance: the resource list for a golden-set case equals the union of the resources declared by its chosen catalog items, per environment.

### 7.6 Estimation

| ID | Pri | Requirement |
|---|---|---|
| EST-1 | Must | Estimate monthly AWS cost per environment from the resource plan, using AWS pricing data for the chosen region. |
| EST-2 | Must | Show cost as a range (low, expected, high) with the sizing assumptions listed (users, requests, data volume, retention). Changing an assumption recalculates the estimate. |
| EST-3 | Must | Estimate delivery time from the gap list (domain gaps), integration and testing, and show the effort saved by platform coverage separately. |
| EST-4 | Must | Every estimate states that it is a planning range, not a quote, and lists what is excluded. |
| EST-5 | Should | Record actual effort and actual AWS cost for delivered projects and use them to calibrate future estimates. |

Acceptance: cost figures can be reproduced from the listed resources, properties and pricing data; time estimates list every gap they include.

### 7.7 Platform backlog loop

| ID | Pri | Requirement |
|---|---|---|
| BKL-1 | Must | For each platform gap, propose a backlog item with a title, description, the client requirement it came from, and why it is reusable. |
| BKL-2 | Must | Check the existing backlog first. When a matching item exists, add the client as an additional requester instead of creating a duplicate. |
| BKL-3 | Must | Items are created only after the platform owner accepts them. |
| BKL-4 | Should | Accepted items appear in the catalog as "coming soon", so later analyses report them as planned. |

Acceptance: running the same document twice creates no duplicate backlog items.

### 7.8 Review, approval and history

| ID | Pri | Requirement |
|---|---|---|
| REV-1 | Must | Nothing is generated, provisioned or filed without explicit user approval. |
| REV-2 | Must | Every item in the report, proposal and estimate links back to the requirement(s) it came from. |
| REV-3 | Must | Each analysis is saved with its input, answers, outputs, catalog version and model version. |
| REV-4 | Should | An analysis can be re-run when the catalog changes, showing what changed (for example "this gap is now covered"). |
| REV-5 | Should | When the client sends a revised document, show the change in scope, cost and time compared with the previous analysis. |

### 7.9 Output

| ID | Pri | Requirement |
|---|---|---|
| OUT-1 | Must | Show the fit-gap summary, proposal, resource plan and estimate in Launchpad. |
| OUT-2 | Should | Export them as Word or PDF for a client proposal, without internal notes. |

---

## 8. Non-functional requirements

### 8.1 Security and data governance

| ID | Pri | Requirement |
|---|---|---|
| NFR-S1 | Must | Client documents and AI processing stay in the Clarivex AWS account (Amazon Bedrock through the Genesis AI module). No document is sent to a third-party AI service. |
| NFR-S2 | Must | Only authorised Launchpad roles can upload documents or view analyses; access is per client. |
| NFR-S3 | Must | Documents and analyses are kept for a configurable period, then deleted. |
| NFR-S4 | Should | Personal data in documents is detected and redacted before analysis. |
| NFR-S5 | Must | Uploads, analyses, approvals and backlog submissions are audit-logged (who, when, what). |

### 8.2 Quality and evaluation

| ID | Pri | Requirement |
|---|---|---|
| NFR-Q1 | Must | A golden set of 5 to 10 past client documents with expected requirement lists, classifications and proposals is maintained. |
| NFR-Q2 | Must | The golden set is run after every change to the catalog, prompts or model, and results are compared with the targets in section 7. |
| NFR-Q3 | Must | The AI never presents a capability as available unless the catalog says so (no invented features). |

### 8.3 Operations

| ID | Pri | Requirement |
|---|---|---|
| NFR-O1 | Should | A typical RFP (up to 50 pages) is analysed in under 10 minutes, with progress shown. |
| NFR-O2 | Must | AI usage cost per analysis is recorded, with a configurable monthly budget and alert. |
| NFR-O3 | Should | A failed or partial analysis can be resumed without re-uploading. |

---

## 9. Dependencies

- Forge Catalog (section 7.1) is a prerequisite for everything else.
- Genesis AI module (Amazon Bedrock) for document understanding and analysis.
- `ManifestValidator` and the Engine module registry (catalog rules and proposal validation).
- Launchpad wizard and generation flow, including infrastructure-code generation (Terraform/CDK).
- AWS Pricing API for cost estimation.
- Platform backlog location (GitHub issues or `docs/BACKLOG.md`), to be decided.

## 10. Phasing

| Phase | Scope |
|---|---|
| 0 | Forge Catalog with AWS resource declarations, API, CI document and sync check (CAT-1 to CAT-5, CAT-7, CAT-8). Refresh `FORGE_CAPABILITY_SUMMARY.md` meanwhile. |
| 1 (MVP) | Intake, fit-gap report, service proposal with validated manifests, AWS resource list, review and audit (INT, FIT, PRO-1 to PRO-4, AWS-1 to AWS-4, REV-1 to REV-3, OUT-1, NFR Musts). |
| 2 | Estimation (EST), backlog loop (BKL), apply proposal to wizard (PRO-5), pre-fill infrastructure properties (AWS-5). |
| 3 | Re-run and revision comparison (REV-4, REV-5), export (OUT-2), calibration (EST-5), Catalog page (CAT-6), multiple documents (INT-6). |

## 11. Success measures

- Time from client document to reviewed fit-gap report: from days to under one hour of a person's time.
- Golden-set targets in section 7 are met.
- At least 80% of proposed configurations are approved with no more than minor edits.
- Estimation error against actuals narrows release by release.
- Platform backlog items carry requester counts from real client documents.

## 12. Open questions

1. Who are the first users: pre-sales only, or delivery and domain teams too?
2. Where does the platform backlog live: GitHub issues, `docs/BACKLOG.md`, or both?
3. Which golden-set documents can be used, and who owns the expected answers?
4. Effort model: what unit (person-days, story points) and which rates, if any, are shown?
5. Should estimates include environments beyond the vertical defaults (for example a performance environment)?
6. Retention period for client documents, and any client contracts that forbid AI processing?
7. Is Bedrock available in every region where clients require data residency?
8. Should Launchpad AI also review a design or generated print against the requirements later (traceability to delivery)?
