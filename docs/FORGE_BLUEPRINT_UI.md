# Pervaxis Forge — Frontend Blueprint
**Launchpad UI & Angular Template Implementation Plan**

**Version:** 1.1  
**Date:** May 5, 2026  
**Project Start:** May 6, 2026  
**Projected Completion:** June 18, 2026 (6 weeks)  
**Team:** Pervaxis Platform Team — Frontend  
**Status:** Pre-Implementation

> **Parallel Execution:** UI and BFF teams run independently from Day 1.  
> Week 1: UI builds Vertical Enrollment wizard against a mock API while BFF builds the real one.  
> End of Week 1: swap mock for real API — teams integrate.  
> From Week 2 onwards: UI continues building Generation Wizard while BFF completes Engine + Templates.  
> Final integration test: Week 6.
>
> **Solution Structure:** See [FORGE_SOLUTION_STRUCTURE.md](FORGE_SOLUTION_STRUCTURE.md) for the full repo layout, Angular app structure, folder conventions, and `package.json` contents. Read this before writing a single file on Day 1.

---

## Table of Contents
1. [Timeline & Milestones](#1-timeline--milestones)
2. [Phase 0: Vertical Enrollment UI](#2-phase-0-vertical-enrollment-ui)
3. [Phase 1: Vertical Dashboard & Workspace](#3-phase-1-vertical-dashboard--workspace)
4. [Phase 2: Service Generation Wizard](#4-phase-2-service-generation-wizard)
5. [Phase 3: Angular Shell & MFE Templates](#5-phase-3-angular-shell--mfe-templates)
6. [UI/UX Specifications](#6-uiux-specifications)
7. [Mock API Strategy](#7-mock-api-strategy)
8. [Dependencies & Blockers](#8-dependencies--blockers)
9. [Quality Gates](#9-quality-gates)
10. [Testing Strategy](#10-testing-strategy)
11. [Risk Mitigation](#11-risk-mitigation)
12. [Definition of Done](#12-definition-of-done)

---

## 1. Timeline & Milestones

### 1.1 Schedule

```
Week 1 (May 6-10):   Phase 0 — Vertical Enrollment Wizard  [parallel with BFF team]
                     (against mock API — swap to real by May 10)
Week 2 (May 13-17):  Phase 1 — Vertical Dashboard + Workspace
Week 3 (May 20-24):  Phase 2 — Service Generation Wizard (Steps 1-3)
Week 4 (May 27-31):  Phase 2 — Service Generation Wizard (Steps 4-6)
Week 5 (Jun 3-7):    Phase 3 — Angular Shell + MFE Templates
Week 6 (Jun 10-14):  Integration Testing + Polish + Production Readiness
```

### 1.2 Milestones

| Milestone | Target Date | Status | Deliverable |
|---|---|---|---|
| **M0: Enrollment Wizard Live (mock)** | May 8, 2026 | 🔴 Not Started | 5-step wizard functional against mock API |
| **M0b: Enrollment Wizard Integrated** | May 10, 2026 | 🔴 Not Started | Wizard working against real BFF API |
| **M1: Dashboard + Workspace Complete** | May 17, 2026 | 🔴 Not Started | Vertical cards, workspace view, navigation |
| **M2: Generation Wizard Complete** | May 31, 2026 | 🔴 Not Started | Full 6-step generation wizard |
| **M3: Angular Templates Complete** | June 7, 2026 | 🔴 Not Started | Shell + MFE Scriban templates |
| **M4: Production Ready** | June 14, 2026 | 🔴 Not Started | All quality gates passed |

### 1.3 Critical Path

```
Phase 0: Enrollment Wizard (Week 1) ← swap mock → real end of Week 1
    ↓
Phase 1: Dashboard + Workspace (Week 2)
    ↓
Phase 2: Generation Wizard (Weeks 3-4)   ← CRITICAL PATH
    ↓
Phase 3: Angular Templates (Week 5)
    ↓
Integration + Polish (Week 6)
```

---

## 2. Phase 0: Vertical Enrollment UI

**Duration:** 1 week (May 6 - May 10)  
**Goal:** Fully functional Vertical Enrollment wizard; mock API first, real API by May 10  
**Team:** 1-2 frontend engineers  
**Runs in parallel with:** BFF Phase 0 (enrollment API)

> The BFF team will share the API contract (request/response shapes) by May 8.  
> Until then, build all HTTP calls against `HttpClientTestingModule` mocks — the contract is stable enough from the spec to start immediately.

### 2.1 Day 1: Project Setup

**Tasks:**
- [ ] Create `Pervaxis.Forge.Launchpad` Angular 18 project
- [ ] Configure Nx workspace
- [ ] Add Angular Material + Angular CDK
- [ ] Set up routing:
  - `/` → `VerticalDashboardComponent` (placeholder for now)
  - `/verticals/enroll` → `VerticalEnrollmentComponent`
  - `/verticals/:slug` → `VerticalWorkspaceComponent` (placeholder)
  - `/verticals/:slug/generate` → `GenerationWizardComponent` (placeholder)
  - `/unauthorized` → `UnauthorizedComponent`
- [ ] Configure `HttpClient` with environment-based `apiBaseUrl`
- [ ] Create `AuthService` stub (hardcode `forge-admin` role for week 1)
- [ ] Create `forgeAuthGuard` protecting all routes
- [ ] Create `VerticalApiService` — all enrollment HTTP calls (pointing to mock)
- [ ] Set up `HttpClientTestingModule` mock infrastructure

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

### 2.2 Day 1: Mock API Setup

**Tasks:**
- [ ] Create `MockVerticalApiService` implementing same interface as `VerticalApiService`
- [ ] Mock responses for:
  - `POST /api/v1/verticals` → return created vertical
  - `GET /api/v1/verticals` → return `[clarivolt, clarifrost]`
  - `GET /api/v1/verticals/:slug` → return single vertical
  - `POST /api/v1/verticals/:slug/validate` → return `{ awsConnectivity: ok, githubConnectivity: ok }`
- [ ] Use Angular environment flag to toggle mock vs real: `useMockApi: true` in `environment.ts`
- [ ] Swap to real API by changing `useMockApi: false` in environment — zero code change

**Owner:** Frontend Engineer B  
**Effort:** 4 hours

---

### 2.3 Days 1-2: Enrollment Wizard Shell

**Tasks:**
- [ ] Create `VerticalEnrollmentComponent` — Material Stepper wrapper
- [ ] 5 step definitions: Identity, Cloud Provider, Source Control, Tech Defaults, Review
- [ ] Wizard state via Angular Signals: `enrollmentState` signal holds all step data
- [ ] Back / Next navigation with per-step validity guard
- [ ] Unsaved changes warning on browser back / route leave
- [ ] Persist wizard state to `sessionStorage` (survives accidental refresh)
- [ ] Cancel button → confirm dialog → back to Dashboard

**Owner:** Frontend Engineer A  
**Effort:** 8 hours

---

### 2.4 Days 2-3: Step 1 — Vertical Identity

**Tasks:**
- [ ] Create `VerticalIdentityStepComponent`
- [ ] Fields: `slug`, `displayName`, `description`, `ownerTeam`, `ownerEmail`
- [ ] `slug` validation: kebab-case regex, max 50 chars
- [ ] `slug` uniqueness: debounced `GET /api/v1/verticals/:slug` — show green tick or "already taken"
- [ ] Slug prefix preview: `clarivolt / intake-service` — updates live as user types
- [ ] `ownerEmail` format validation
- [ ] All fields required, inline error messages

**Owner:** Frontend Engineer B  
**Effort:** 8 hours

---

### 2.5 Days 2-3: Step 2 — Cloud Provider

**Tasks:**
- [ ] Create `CloudProviderStepComponent`
- [ ] Provider selection cards: AWS (active), Azure (disabled + "Coming Soon" chip), GCP (disabled + "Coming Soon" chip)
- [ ] AWS pre-selected, cannot deselect
- [ ] AWS fields panel (revealed on AWS selection):
  - `awsAccountId` — 12-digit numeric validation
  - `iamRoleArn` — ARN format validation (`arn:aws:iam::...`)
  - `defaultRegion` — Material Select with full AWS region list
- [ ] Info tooltip on IAM Role ARN: "Forge uses this role to create AWS resources on your behalf. No access keys stored."

**Owner:** Frontend Engineer A  
**Effort:** 8 hours

---

### 2.6 Days 3-4: Step 3 — Source Control

**Tasks:**
- [ ] Create `SourceControlStepComponent`
- [ ] Platform cards: GitHub (active + pre-selected), GitLab (disabled), Azure DevOps (disabled)
- [ ] GitHub fields panel:
  - `githubOrg` — free text, required
  - `accessToken` — password input, show/hide toggle, never re-displayed after save
  - `defaultVisibility` — radio: Private (default) / Public
  - `defaultBranchProtection` — slide toggle, default on
- [ ] "Verify Access" button — calls `POST /api/v1/verticals/{slug}/validate` (mock in week 1)
- [ ] Verification result: success badge (org confirmed) or error chip (message from API)
- [ ] Step cannot advance without successful verification

**Owner:** Frontend Engineer B  
**Effort:** 10 hours

---

### 2.7 Day 4: Step 4 — Technical Defaults

**Tasks:**
- [ ] Create `TechDefaultsStepComponent`
- [ ] `environments` — Material chip list: add/remove, drag to reorder, default `[test, accp, prod]`
- [ ] `defaultEnvironment` — select from the chip list (reactive, updates when list changes)
- [ ] IaC checkboxes: Terraform (checked), CDK (checked)
- [ ] `defaultDbEngine` — select: None (default) / PostgreSQL

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

### 2.8 Day 4-5: Step 5 — Review & Enroll

**Tasks:**
- [ ] Create `ReviewEnrollStepComponent`
- [ ] Read-only summary: Identity section, Cloud section (IAM ARN masked `arn:aws:iam::***`), Source Control (token shown as `ghp_***...***`), Defaults
- [ ] "Edit" links per section — navigate to that step
- [ ] "Enroll Vertical" button → `POST /api/v1/verticals`
- [ ] Loading state with spinner during API call
- [ ] Success: redirect to `/verticals/:slug` (Workspace)
- [ ] Error: show server validation errors inline per section

**Owner:** Frontend Engineer B  
**Effort:** 6 hours

---

### 2.9 Day 5: Integration Swap

**Tasks:**
- [ ] Receive Swagger JSON from BFF team (due May 8)
- [ ] Verify mock response shapes match real API contract
- [ ] Fix any shape mismatches in mock
- [ ] Set `useMockApi: false` in `environment.ts`
- [ ] Run enrollment wizard end-to-end against real BFF API in dev environment
- [ ] Fix any integration issues

**Owner:** Both  
**Effort:** 4 hours

---

### 2.10 Phase 0 Deliverables

- Complete 5-step Enrollment Wizard functional against mock (Day 3)
- Integrated against real BFF API in dev environment (Day 5 / May 10)
- Wizard state persisted in sessionStorage
- All step validations working
- "Verify Access" button calls real connectivity endpoint

---

## 3. Phase 1: Vertical Dashboard & Workspace

**Duration:** 1 week (May 13 - May 17)  
**Goal:** Landing page and per-vertical workspace  
**Team:** 2 frontend engineers

### 3.1 Vertical Dashboard (Landing Page)

Route: `/`

**Tasks:**
- [ ] Create `VerticalDashboardComponent`
- [ ] Fetch enrolled verticals on load (`GET /api/v1/verticals`)
- [ ] Vertical cards: slug badge, display name, cloud provider chip (AWS), GitHub org, service count, "Open" button
- [ ] Empty state: full-screen illustration + "Enroll Your First Vertical" CTA
- [ ] Loading skeleton (3 ghost cards) while fetching
- [ ] "Enroll New Vertical" button top-right — always visible
- [ ] Error state: retry button if API fails

**Owner:** Frontend Engineer A  
**Effort:** 10 hours

---

### 3.2 Vertical Workspace

Route: `/verticals/:slug`

**Tasks:**
- [ ] Create `VerticalWorkspaceComponent`
- [ ] Load vertical details on init (`GET /api/v1/verticals/:slug`)
- [ ] Header: display name, AWS badge, GitHub org, "Settings" icon button
- [ ] Stats row: total services generated, last generation timestamp
- [ ] "Generate New Services" primary button → `/verticals/:slug/generate`
- [ ] Recent generations table (from audit log endpoint): date, service names, operator, status chips
- [ ] Vertical settings slide-out panel:
  - View/edit: `displayName`, `description`, `ownerTeam`, `ownerEmail`
  - Token rotation: re-enter GitHub token, "Update Token" button
  - Danger zone: "Unenroll Vertical" with confirmation dialog

**Owner:** Frontend Engineer B  
**Effort:** 12 hours

---

### 3.3 Navigation + Shell

**Tasks:**
- [ ] App shell: top nav with Forge logo, current vertical breadsocrumb, user role chip
- [ ] Breadcrumb: `Forge > Clarivolt > Generate` — clickable
- [ ] Route guards tested: unauthenticated → login, non-admin → `/unauthorized`
- [ ] 404 page for unknown routes

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

### 3.4 Phase 1 Deliverables

- Vertical Dashboard with enrolled verticals
- Vertical Workspace with stats + recent generations
- App shell navigation
- All routes guarded

---

## 4. Phase 2: Service Generation Wizard

**Duration:** 2 weeks (May 20 - May 31)  
**Goal:** Full 6-step service generation wizard within a vertical  
**Team:** 2 frontend engineers

> At this point the BFF team has the Engine + REST templates complete.  
> Generation API endpoints (`POST /api/v1/generate/batch`, `POST /api/v1/validate`) available for integration.

### 4.1 Wizard Architecture

Route: `/verticals/:slug/generate`

- Vertical context (slug, cloud account, GitHub org, environments, IaC defaults) loaded once from `GET /api/v1/verticals/:slug` — never re-entered
- Wizard state in typed Angular Signals, persisted to `sessionStorage`
- 6 steps, validated before advancing

---

### 4.2 Step 1: Service Identity (May 20-21)

**Tasks:**
- [ ] Create `ServiceIdentityStepComponent`
- [ ] Dynamic service list: add/remove rows, minimum 1
- [ ] Per service row: `name` input + type toggle (BFF / MFE)
- [ ] Vertical slug shown as read-only prefix chip: `clarivolt / [name]`
- [ ] Name validation: kebab-case, BFF must end `-service`, MFE must not
- [ ] Real-time naming preview panel (debounced `POST /api/v1/validate`, 300ms):
  - Namespace, Docker Image, API Route, GitHub Repo, DB Schema
  - Updates per service independently
- [ ] "Add Another Service" button, remove icon per row

**Owner:** Frontend Engineer A  
**Effort:** 12 hours

---

### 4.3 Step 2: Module Selection (May 21-22)

**Tasks:**
- [ ] Create `ModuleSelectionStepComponent`
- [ ] Tab per service (or accordion if > 3 services)
- [ ] BFF tabs: Genesis module cards (from `GET /api/v1/modules`)
- [ ] MFE tabs: Canvas module cards (from `GET /api/v1/canvas-modules`)
- [ ] Each Genesis module card: display name, description, and a NuGet badge showing the **cloud-resolved** package name (e.g. `Pervaxis.Genesis.Caching.AWS` for an AWS vertical) — the API response includes this resolved name based on the vertical's enrolled cloud provider so the UI shows exactly what will end up in the generated `.csproj`
- [ ] Module names submitted in the generation request use the **cloud-agnostic name** only (e.g. `"Caching"`, `"Messaging"`) — the BFF resolves the cloud suffix from the vertical at generation time
- [ ] "Select All" / "Clear" shortcuts per tab
- [ ] Mixed BFF+MFE generation: show both Genesis + Canvas in separate sections

**Owner:** Frontend Engineer B  
**Effort:** 10 hours

---

### 4.4 Step 3: Database & Queues (May 22-23)

**Tasks:**
- [ ] Create `DatabaseQueuesStepComponent`
- [ ] Per BFF service: database engine select (None / PostgreSQL), schema name (auto-derived, editable)
- [ ] Per BFF service: dynamic queue list — name input, role select (Publish / Subscribe), add/remove
- [ ] MFE services: step shows "No database or queue config for Angular services" notice
- [ ] Queue name hint: shows SQS prefix from naming convention

**Owner:** Frontend Engineer A  
**Effort:** 8 hours

---

### 4.5 Step 4: Infrastructure Deployment (May 26-27)

**Tasks:**
- [ ] Create `InfrastructureStepComponent`
- [ ] "Deploy infrastructure now" slide toggle
- [ ] When enabled: AWS account shown read-only (from vertical), environment select (from vertical's enrolled environment list)
- [ ] IaC checkboxes pre-selected from vertical's tech defaults — overridable per generation
- [ ] Estimated resources summary (count of RDS, S3, SQS, etc. to be created)

**Owner:** Frontend Engineer B  
**Effort:** 6 hours

---

### 4.6 Step 5: GitHub Configuration (May 27-28)

**Tasks:**
- [ ] Create `GitHubConfigStepComponent`
- [ ] "Create GitHub repositories" toggle (default on)
- [ ] GitHub org shown read-only (from vertical)
- [ ] Visibility radio pre-selected from vertical default — overridable
- [ ] Branch protection toggle pre-selected from vertical default — overridable
- [ ] GitHub Secrets toggle (default on — seeds AWS credentials)

**Owner:** Frontend Engineer A  
**Effort:** 4 hours

---

### 4.7 Step 6: Preview & Generate (May 28-31)

**Tasks:**
- [ ] Create `PreviewGenerateStepComponent`
- [ ] Service summary table: name, type, modules, DB, queue count
- [ ] Infrastructure plan section: resources to create per service
- [ ] GitHub plan section: repos to create
- [ ] "Generate All Services" primary button → `POST /api/v1/generate/batch`
- [ ] Progress overlay: per-service status messages ("Generating intake-service...", "Creating GitHub repo...")
- [ ] Success state:
  - Per-service result rows with GitHub repo links
  - "Download All ZIPs" option
  - "Generate More Services" → back to Step 1
- [ ] Partial failure state: per-service success/failure chips, "Retry Failed" button
- [ ] Full failure: error message, "Try Again" → back to wizard

**Owner:** Frontend Engineer B  
**Effort:** 12 hours

---

### 4.8 Phase 2 Deliverables

- Full 6-step generation wizard functional
- Real-time naming preview working
- Batch generation (5+ services) tested
- Partial failure handling

---

## 5. Phase 3: Angular Shell & MFE Templates

**Duration:** 1 week (June 3 - June 7)  
**Goal:** Scriban templates for Angular Shell and Microfrontend scaffold generation  
**Team:** 2 engineers  
**Dependency:** Phase 1 Engine (Scriban infrastructure) complete in BFF repo

> These are `.sbn` template files that live in the **Engine project** (BFF repo), not in the Launchpad. The frontend team writes and tests the template content; templates are committed to the Engine project via PR to the BFF repo.

### 5.1 Angular Shell Template (9 files)

**Days 1-2:**

- [ ] `Templates/angular-shell/manifest.json.sbn`
- [ ] `Templates/angular-shell/SPEC.md.sbn`
- [ ] `Templates/angular-shell/README.md.sbn`
- [ ] `Templates/angular-shell/package.json.sbn` — Angular 18 + Canvas module deps (loop)
- [ ] `Templates/angular-shell/angular.json.sbn` — workspace config
- [ ] `Templates/angular-shell/tsconfig.json.sbn` — strict mode
- [ ] `Templates/angular-shell/app.component.ts.sbn` — root component stub
- [ ] `Templates/angular-shell/app.routes.ts.sbn` — lazy-loaded MFE routes
- [ ] `Templates/angular-shell/app.config.ts.sbn` — Angular providers, Canvas modules wired
- [ ] `Templates/angular-shell/.claude/CLAUDE.md.sbn` — Shell-specific Claude Code context
- [ ] Test: `ng build` succeeds on generated shell

**Owner:** Engineer A  
**Effort:** 12 hours

---

### 5.2 Angular MFE Template (9 files)

**Days 2-3:**

- [ ] `Templates/angular-microfrontend/manifest.json.sbn`
- [ ] `Templates/angular-microfrontend/SPEC.md.sbn`
- [ ] `Templates/angular-microfrontend/README.md.sbn`
- [ ] `Templates/angular-microfrontend/module.ts.sbn` — Angular feature module
- [ ] `Templates/angular-microfrontend/routing.module.ts.sbn` — placeholder route
- [ ] `Templates/angular-microfrontend/component.ts.sbn` — root feature component
- [ ] `Templates/angular-microfrontend/component.html.sbn` — placeholder template
- [ ] `Templates/angular-microfrontend/api.service.ts.sbn` — API service, base route from naming convention
- [ ] `Templates/angular-microfrontend/index.ts.sbn` — barrel export
- [ ] `Templates/angular-microfrontend/.claude/CLAUDE.md.sbn` — MFE-specific Claude Code context
- [ ] Test: MFE builds and lazy-loads in shell

**Owner:** Engineer B  
**Effort:** 12 hours

---

### 5.3 Canvas Module Wiring in Templates

**Days 3-4:**

- [ ] `package.json.sbn` — loop `selected_canvas_modules` for `@pervaxis/canvas-*` deps
- [ ] `app.config.ts.sbn` — conditionally provide each Canvas module (auth, HTTP, state, UI, etc.)
- [ ] Test matrix: generate shell with each combination of Canvas modules
- [ ] Verify no import errors when Canvas packages absent from local test

**Owner:** Engineer A  
**Effort:** 8 hours

---

### 5.4 Integration Testing (Days 4-5)

**Tasks:**
- [ ] Generate Clarivolt Shell + 3 MFEs (intake, validation, filing) via Launchpad in dev environment
- [ ] Verify all 4 projects build (`ng build`)
- [ ] Verify MFEs lazy-load in shell
- [ ] Verify Canvas modules functional
- [ ] Test navigation between MFEs
- [ ] Review both CLAUDE.md outputs with Claude Code CLI

**Owner:** Both  
**Effort:** 8 hours

---

### 5.5 Phase 3 Deliverables

- Angular Shell template (9 `.sbn` files) in Engine project
- Angular Microfrontend template (9 `.sbn` files) in Engine project
- Integration test: Clarivolt Shell + 3 MFEs builds and runs

---

## 6. UI/UX Specifications

### 6.1 Route Map

```
/                           Vertical Dashboard
/verticals/enroll           Enrollment Wizard (5 steps)
/verticals/:slug            Vertical Workspace
/verticals/:slug/generate   Service Generation Wizard (6 steps)
/unauthorized               Access denied
```

### 6.2 Vertical Dashboard

```
┌──────────────────────────────────────────────────────────────┐
│  Pervaxis Forge                       [+ Enroll New Vertical] │
├──────────────────────────────────────────────────────────────┤
│  Your Verticals                                              │
│                                                              │
│  ┌──────────────────┐  ┌──────────────────┐  ┌──────────┐   │
│  │  clarivolt       │  │  clarifrost      │  │    +     │   │
│  │  AWS · GitHub    │  │  AWS · GitHub    │  │  Enroll  │   │
│  │  12 services     │  │  0 services      │  │          │   │
│  │  [Open]          │  │  [Open]          │  │          │   │
│  └──────────────────┘  └──────────────────┘  └──────────┘   │
└──────────────────────────────────────────────────────────────┘
```

### 6.3 Enrollment Wizard

```
┌──────────────────────────────────────────────────────────────┐
│  Enroll New Vertical                             [✕ Cancel]  │
├──────────────────────────────────────────────────────────────┤
│  ① Identity  ② Cloud  ③ Source Control  ④ Defaults  ⑤ Review │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Step content area                                           │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│                                [Back]  [Next / Enroll]       │
└──────────────────────────────────────────────────────────────┘
```

### 6.4 Service Generation Wizard

```
┌──────────────────────────────────────────────────────────────┐
│  Clarivolt  ›  Generate Services                             │
├──────────────────────────────────────────────────────────────┤
│  ① Services  ② Modules  ③ DB/Queues  ④ Infra  ⑤ GitHub  ⑥ Go │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  Step content area                                           │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│                          [Back]  [Next / Generate All]       │
└──────────────────────────────────────────────────────────────┘
```

### 6.5 Real-Time Naming Preview (Step 1)

```
┌──────────────────────────────────────┐
│  Derived Names Preview               │
│  ─────────────────────────────────   │
│  Namespace:    Clarivolt.Intake      │
│               Service                │
│  Docker:       clarivolt/            │
│               intake-service         │
│  API Route:    /api/v1/intake        │
│  GitHub Repo:  clarivex-tech/        │
│               intake-service         │
│  DB Schema:    intake                │
└──────────────────────────────────────┘
```

Debounced 300ms, powered by `POST /api/v1/validate`.

---

## 7. Mock API Strategy

The UI team builds all HTTP calls against a swappable service. Controlled by a single environment flag.

```typescript
// environment.ts (week 1)
export const environment = {
  apiBaseUrl: 'http://localhost:5000',
  useMockApi: true   // ← flip to false on May 10
};

// app.config.ts
providers: [
  {
    provide: VerticalApiService,
    useClass: environment.useMockApi
      ? MockVerticalApiService
      : VerticalApiService
  }
]
```

### Mock contract (must match BFF Swagger by May 8)

| Endpoint | Mock response |
|---|---|
| `POST /api/v1/verticals` | `{ id, slug, displayName, enrolledAt }` |
| `GET /api/v1/verticals` | Array of 2 verticals |
| `GET /api/v1/verticals/:slug` | Single vertical (credentials masked) |
| `POST /api/v1/verticals/:slug/validate` | `{ awsConnectivity: { success: true }, githubConnectivity: { success: true } }` |
| `POST /api/v1/validate` | `{ valid: true, derivedNames: {...} }` |
| `POST /api/v1/generate/batch` | Per-service success results |
| `GET /api/v1/modules` | 8 Genesis module objects with cloud-resolved `packageName` (pass `?verticalSlug=clarivolt` or default to AWS in mock) |
| `GET /api/v1/canvas-modules` | 14 Canvas module objects |

---

## 8. Dependencies & Blockers

| Dependency | Owner | Status | Impact if Delayed |
|---|---|---|---|
| BFF API contract (Swagger JSON) | BFF team | 🔴 Must deliver May 8 | Cannot validate mock shapes |
| BFF API deployed to dev | BFF team | 🔴 Must deliver May 10 | Cannot integrate in Week 1 |
| Canvas packages available | Canvas Team | 🟡 In Progress | Angular templates cannot wire Canvas modules |
| Angular Material design approval | UX | 🟡 To Confirm | Use Material defaults until confirmed |

---

## 9. Quality Gates

### Phase 0 Gate — May 10, 2026

- [ ] Enrollment wizard navigates all 5 steps with validation
- [ ] All step validations enforced (kebab-case, ARN format, email)
- [ ] "Verify Access" calls connectivity endpoint and shows result
- [ ] Wizard state survives page refresh (sessionStorage)
- [ ] Integrated against real BFF API — full enrollment completes

### Phase 1 Gate — May 17, 2026

- [ ] Dashboard loads and displays enrolled verticals
- [ ] Empty state shown when no verticals enrolled
- [ ] Workspace loads vertical details and audit log
- [ ] Settings slide-out opens and saves changes

### Phase 2 Gate — May 31, 2026

- [ ] Wizard navigates all 6 steps
- [ ] Real-time naming preview updates correctly
- [ ] Batch generation (5 services) succeeds
- [ ] GitHub repo links shown in success state
- [ ] Partial failure handled — per-service result shown

### Phase 3 Gate — June 7, 2026

**Reviewers:** Tech Lead, Angular Team Lead

- [ ] Shell builds with `ng build`
- [ ] MFE lazy-loads in shell
- [ ] Canvas modules wired correctly
- [ ] Integration: Shell + 3 MFEs runs end-to-end

### Final Production Readiness Gate — June 14, 2026

**Reviewers:** VP Engineering, CTO

- [ ] All phase quality gates passed
- [ ] End-to-end: enroll vertical → generate 5 BFFs + 5 MFEs → deploy infra → create repos
- [ ] Launchpad page load < 1 second
- [ ] Security review complete
- [ ] Team trained, documentation complete
- [ ] Go-live approved

---

## 10. Testing Strategy

### Component Tests
- Angular TestBed for all wizard step components
- Signals: test all derived computed values
- HTTP: `HttpClientTestingModule` for mock integration

### E2E Tests (Cypress)
1. Enroll a vertical (full 5-step wizard)
2. Generate a single BFF service
3. Generate a batch of 3 services (2 BFF + 1 MFE)
4. Partial failure: handle 1 of 3 services failing

### Manual Testing
- Full enrollment with real AWS + GitHub credentials (dev environment)
- Generate Shell + 3 MFEs, verify build + lazy-load
- Claude Code CLI: open generated CLAUDE.md, verify it provides useful context

---

## 11. Risk Mitigation

| Risk | Mitigation |
|---|---|
| BFF API contract delayed | Start with spec-derived mocks; align on any diffs by May 8 |
| API shape changes after mock built | Freeze contract by May 8; UI team notified immediately of any changes |
| Canvas packages not ready for Week 5 | Generate Angular templates without Canvas wiring first; add in a follow-up PR once packages land |
| Wizard state complexity | All state in typed Signals with unit tests on every transition |
| Launchpad UX too complex for enrollment | Run 2-person walkthrough with a platform engineer before full build |

---

## 12. Definition of Done

### Feature-Level
- [ ] Component renders correctly (happy path + error states)
- [ ] Form validation covers all invalid inputs
- [ ] API integration handles success + failure responses
- [ ] Code reviewed and approved
- [ ] No accessibility regressions

### Phase-Level
- [ ] All features complete per feature DoD
- [ ] Quality gate passed
- [ ] Demo to stakeholders
- [ ] Sign-off from Tech Lead

---

**End of Frontend Blueprint**

*Pervaxis Forge v1.1 — Clarivex Technologies © 2026*
