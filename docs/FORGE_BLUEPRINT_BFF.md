# Pervaxis Forge — Backend Blueprint
**Engine, API & Infrastructure Implementation Plan**

**Version:** 1.1  
**Date:** May 5, 2026  
**Project Start:** May 6, 2026  
**Projected Completion:** May 31, 2026 (4 weeks)  
**Team:** Pervaxis Platform Team — Backend  
**Status:** Phase 0 Complete — Phase 1 Complete — Phase 2 Complete

> **Parallel Execution:** BFF and UI teams run independently from the start.  
> Week 1: BFF builds Vertical Enrollment API while UI builds the Enrollment Wizard against a mock.  
> End of Week 1: teams integrate — UI swaps mock for real API.  
> From Week 2 onwards: fully independent until final integration test.
>
> **Solution Structure:** See [FORGE_SOLUTION_STRUCTURE.md](FORGE_SOLUTION_STRUCTURE.md) for the full repo layout, project structure, folder conventions, and `.csproj` contents. Read this before writing a single file.

---

## Must Read — AI Session Conventions

> These rules apply to every Claude Code session working on the BFF. They are load-bearing — ignoring them wastes tokens or produces the wrong output.

### Model tier strategy (conserve usage for the full 4-week build)

| Tier | Model | Use for |
|---|---|---|
| **Haiku** | `claude-haiku-4-5-20251001` | All implementation: writing C# files, tests, edits |
| **Sonnet** | `claude-sonnet-4-6` | Planning, design decisions, session log updates, PR review |
| **Opus** | `claude-opus-4-7` | Hard architectural calls or complex bug diagnosis only |

**Rule:** Before writing any code, spawn `Agent(model="haiku")` for the implementation. Do not write code directly in the Sonnet conversation unless the change is a single-line fix.

### Codex delegation rule

Leverage maximum coding, analysis, and bug-fixing work to **Codex**. Claude CLI takes over only when the task is judged too complex or context-heavy for Codex (e.g. multi-file architectural changes, hard bug diagnosis). When Claude CLI does implement, the model tier rules above apply.

### Session log

> **File:** `pervaxis-forge-api/.claude/memory/session_log.md`
>
> Engineer-facing log of work, decisions, blockers, and todos for the Forge BFF. Committed to the repo so any machine pulling the branch has full context.
>
> **Convention:** Latest entry at the top. Each entry is self-contained — a fresh Claude Code session on a new machine should be able to read the topmost entry and continue without backfill.
>
> **Companion docs:** Project narrative lives in `docs/session_log.md`. Authoritative standards in `pervaxis-forge-api/CLAUDE.md` and `docs/FORGE_*` files.

### Other session conventions

- **Discuss before coding** — propose a plan and get explicit go-ahead before changing code.
- **One question at a time** — surface clarifications individually, never as a bundled list.
- **Forge ≠ Genesis** — zero references to `Pervaxis.Core` or `Pervaxis.Genesis` in Forge code.
- **No local emulation** — real AWS resources only; no Docker, LocalStack, or Testcontainers.
- **Append session log** — every session ends with a new dated entry at the top of `pervaxis-forge-api/.claude/memory/session_log.md`.

---

## Table of Contents
1. [Timeline & Milestones](#1-timeline--milestones)
2. [Phase 0: Vertical Enrollment Backend](#2-phase-0-vertical-enrollment-backend)
3. [Phase 1: Core Engine](#3-phase-1-core-engine)
4. [Phase 2: REST API Templates](#4-phase-2-rest-api-templates)
5. [Phase 3: Infrastructure Provisioning & GitHub Integration](#5-phase-3-infrastructure-provisioning--github-integration)
6. [Phase 4 (V2 Roadmap): Forge-Managed Infrastructure](#6-phase-4-v2-roadmap-forge-managed-infrastructure)
7. [Dependencies & Blockers](#7-dependencies--blockers)
8. [Quality Gates](#8-quality-gates)
9. [Testing Strategy](#9-testing-strategy)
10. [Risk Mitigation](#10-risk-mitigation)
11. [Definition of Done](#11-definition-of-done)

---

## 1. Timeline & Milestones

### 1.1 Schedule

```
Week 1 (May 6-10):   Phase 0 — Vertical Enrollment API  [parallel with UI team]
                     Phase 1 start — Engine Foundation
Week 2 (May 13-17):  Phase 1 — Engine Complete
Week 3 (May 20-24):  Phase 2 — REST API Templates
Week 4 (May 27-31):  Phase 2 (wrap) + Phase 3 — Infrastructure + GitHub
```

### 1.2 Milestones

| Milestone | Target Date | Status | Deliverable |
|---|---|---|---|
| **M0: Vertical Enrollment API Live** | May 10, 2026 | ✅ Complete — Deployed to Lambda (ACCP) May 8 | Enroll endpoint, DB schema, AWS + GitHub validation, Lambda deploy pipeline |
| **M1: Engine Core Complete** | May 17, 2026 | ✅ Complete — Engine core implemented and verified May 13 | Manifest parsing, naming derivation, template engine, ZIP |
| **M2: REST API Templates Complete** | May 24, 2026 | ✅ Complete — Templates implemented and verified May 13 | 18 templates, generated service compiles + tests pass |
| **M3: Infrastructure + GitHub Complete** | May 31, 2026 | 🟡 In Progress — GitHub + generation endpoints complete May 14; §5.1/§5.2 moved to Phase 4 | GitHub repos created, generation endpoints live |

### 1.3 Critical Path

```
Phase 0: Vertical Enrollment API (Week 1)  ← UI team integrates end of Week 1
    ↓ (parallel start)
Phase 1: Engine Core (Week 1-2)
    ↓
Phase 2: REST Templates (Week 3)           ← CRITICAL PATH
    ↓
Phase 3: Infrastructure + GitHub (Week 4)  ← CRITICAL PATH
```

---

## 2. Phase 0: Vertical Enrollment Backend

**Duration:** 1 week (May 6 - May 10)  
**Goal:** Fully functional Vertical Enrollment API that the UI team can integrate against by May 10  
**Team:** 1-2 backend engineers  
**Runs in parallel with:** Phase 1 Engine start (second engineer) and UI Phase 0

> This is the team's first shared deliverable. The UI team will be building the enrollment wizard against a mock API this week. By end of week 1, they swap the mock for these real endpoints.

### 2.1 Project Setup & Database

**Tasks:**
- [x] `2.1.1` Create `Pervaxis.Forge.Api` ASP.NET Core Minimal API project (.NET 10)
- [x] `2.1.2` Create `Pervaxis.Forge.Api.Tests` xUnit project
- [x] `2.1.3` Add EF Core + Npgsql, ASP.NET Core Data Protection
- [x] `2.1.4` Define all entity models: `Vertical`, `VerticalCloudConfig`, `VerticalSourceControlConfig`, `VerticalTechDefaults`, `GenerationLog`, `DeploymentOutput`
- [x] `2.1.5` Create `ForgeDbContext` with all DbSets and relationships
- [x] `2.1.6` Write and apply initial migration
- [x] `2.1.7` Seed one test vertical: `clarivolt`
- [x] `2.1.8` Configure `Directory.Build.props` and `nuget.config`

**Acceptance Criteria:**
- Migration applies cleanly to a local PostgreSQL instance
- Seeded vertical readable via EF Core

**Owner:** Engineer A  
**Effort:** 8 hours

---

### 2.2 Vertical Enrollment Service

**Tasks:**
- [x] `2.2.1` Implement `VerticalService` with Data Protection encryption:
  - `EnrollAsync(VerticalEnrollmentRequest)` — validate, encrypt credentials, persist all sub-configs
  - `GetAsync(slug)` — return vertical with masked credentials
  - `ListAsync()` — return all active verticals (no credentials)
  - `UpdateAsync(slug, request)` — update mutable fields
  - `DeactivateAsync(slug)` — soft delete
- [x] `2.2.2` Implement `VerticalConnectivityValidator`:
  - `ValidateAwsAsync(iamRoleArn, accountId, region)` — STS AssumeRole dry-run
  - `ValidateGitHubAsync(githubOrg, accessToken)` — Octokit org membership check
- [x] `2.2.3` Write unit tests for `VerticalService` (mock DB, verify encryption called)
- [x] `2.2.4` Write unit tests for connectivity validation (mock STS + Octokit)

**Owner:** Engineer A  
**Effort:** 12 hours

---

### 2.3 Enrollment API Endpoints

**Tasks:**
- [x] `2.3.1` `POST /api/v1/verticals` - validate request, validate connectivity, enroll
- [x] `2.3.2` `GET /api/v1/verticals` - list all active verticals
- [x] `2.3.3` `GET /api/v1/verticals/{slug}` - get single vertical (credentials masked)
- [x] `2.3.4` `PUT /api/v1/verticals/{slug}` - update mutable fields
- [x] `2.3.5` `DELETE /api/v1/verticals/{slug}` - soft delete
- [x] `2.3.6` `POST /api/v1/verticals/{slug}/validate` - connectivity check only, no persist
- [x] `2.3.7` Add request validation (FluentValidation or DataAnnotations)
- [x] `2.3.8` Add consistent error response shape `{ errors: string[] }`
- [x] `2.3.9` Write integration tests for all endpoints against real PostgreSQL (test DB)
- [x] `2.3.10` Write OpenAPI/Swagger docs for all endpoints

**Owner:** Engineer A  
**Effort:** 14 hours

---

### 2.4 API Contract Document for UI Team

> The UI team needs this to build accurate mocks. Produce it by the end of the first week at latest.

**Tasks:**
- [x] `2.4.1` Confirm all request/response shapes are final
- [x] `2.4.2` Export Swagger JSON from running API
- [x] `2.4.3` Share with UI team lead (used to build `HttpClientTestingModule` mocks)
- [x] `2.4.4` Smoke test all 6 endpoints manually via Swagger UI or Postman
- [x] `2.4.5` Deploy API to shared dev environment so UI team can integrate immediately after contract freeze

**Owner:** Engineer A  
**Effort:** 4 hours

---

### 2.5 Phase 0 Deliverables

- `2.5.1` 6 enrollment endpoints functional and documented
- `2.5.2` PostgreSQL migration applied to dev environment
- `2.5.3` Credentials encrypted at rest (Data Protection)
- `2.5.4` AWS + GitHub connectivity validation working
- `2.5.5` API deployed to dev environment by May 10
- `2.5.6` Swagger JSON shared with UI team by May 8

---

## 3. Phase 1: Core Engine

**Duration:** 1.5 weeks (May 6 start, complete May 17)  
**Goal:** Bulletproof deterministic generation engine  
**Team:** 1-2 engineers (can start in parallel with Phase 0)

> One engineer starts on Engine from the start of the first week (pure .NET, no API/DB dependencies). The other focuses on Phase 0 API. They converge when Engine integrates with the API in Phase 3.

### 3.1 Solution + Manifest Model

> Target window: First week

**Tasks:**
- [x] `3.1.1` Create `Pervaxis.Forge.slnx` solution
- [x] `3.1.2` Create `Pervaxis.Forge.Engine` class library (.NET 10)
- [x] `3.1.3` Create `Pervaxis.Forge.Engine.Tests` xUnit project
- [x] `3.1.4` Configure `Directory.Build.props` (nullable, warnings as errors, no Pervaxis.Core dependency)
- [x] `3.1.5` Define `ForgeManifest` record with all properties
- [x] `3.1.6` Define `ServiceType`, `DatabaseConfig`, `QueueConfig`, `ApiConfig`, `AngularConfig` models
- [x] `3.1.7` Write JSON serialization tests

**Owner:** Engineer B  
**Effort:** 10 hours

---

### 3.2 Naming Convention Engine

> Target window: First week

**Tasks:**
- [x] `3.2.1` Create `NamingConvention` static class
- [x] `3.2.2` Implement `ToPascalCase(string kebab)`
- [x] `3.2.3` Implement `StripServiceSuffix(string name)`
- [x] `3.2.4` Implement `GetFirstSegment(string name)`
- [x] `3.2.5` Implement `GetComponentPrefix(string product)`
- [x] `3.2.6` Implement `DeriveDotNetNames(product, name)`
- [x] `3.2.7` Implement `DeriveAngularShellNames(product, name)`
- [x] `3.2.8` Implement `DeriveAngularMfeNames(product, name)`
- [x] Write 50+ unit tests — edge cases: single-word, multi-hyphen, names with numbers (e.g. `v2-service`), Unicode

**Owner:** Engineer B  
**Effort:** 16 hours

---

### 3.3 Manifest Validation

> Target window: Week 1 (May 6-10)

**Tasks:**
- [x] `3.3.1` Create `ManifestValidator` class
- [x] `3.3.2` Kebab-case regex validation
- [x] `3.3.3` Type-specific name rules (`.NET` must end `-service`, MFE must not)
- [x] `3.3.4` Genesis + Canvas module validation against known lists
- [x] `3.3.5` Required field checks
- [x] `3.3.6` `ValidationResult` with specific error messages
- [x] `3.3.7` Write 30+ tests

**Owner:** Engineer B  
**Effort:** 10 hours

---

### 3.4 Scriban Template Engine

> Target window: First week

**Tasks:**
- [x] `3.4.1` Add Scriban NuGet package
- [x] `3.4.2` Create `ITemplateEngine` interface
- [x] `3.4.3` Implement `ScribanTemplateEngine` with strict mode
- [x] `3.4.4` Create `TemplateModel` class (manifest + derived names)
- [x] `3.4.5` Implement `TemplateModelBuilder.Build(manifest)`
- [x] `3.4.6` Write sample `test-template.sbn` and render test
- [x] `3.4.7` Write 20+ template engine tests

**Owner:** Engineer B  
**Effort:** 12 hours

---

### 3.5 Embedded Template Loading

> Target window: Week 2 (May 13-17)

**Tasks:**
- [x] Create folder structure: `Templates/rest-api/`, `Templates/angular-shell/`, etc.
- [x] Add placeholder `.sbn` files
- [x] Configure `.csproj` to embed all templates as resources
- [x] Implement `TemplateLoader.LoadAsync(path)` via `GetManifestResourceStream`
- [x] Write tests, handle missing template errors gracefully

**Note:** ZIP entry paths are good enough for the current repo-push flow, but if we later extract directly into a repo root without staging, we may need to refine path mapping.

**Owner:** Engineer B  
**Effort:** 8 hours

---

### 3.6 Print Generator Core

> Target window: Week 2 (May 13-17)

**Tasks:**
- [x] Create `PrintGenerator` class
- [x] Implement `GenerateAsync(manifest, cloudProvider)` — validate → normalize → derive names → load templates → render files → package ZIP
- [x] `cloudProvider` flows from the vertical's enrolled cloud into `TemplateModelBuilder.Build(manifest, cloudProvider)` — Engine stays cloud-agnostic by treating it as an opaque string (`"AWS"`, `"Azure"`, `"GCP"`)
- [x] Create `FileGenerator` for individual file rendering
- [x] Create `ZipPackager` using `System.IO.Compression`
- [x] Integration test: manifest → ZIP → extract → verify full file tree

**Owner:** Engineer B  
**Effort:** 16 hours

---

### 3.7 Module Metadata

> Target window: Week 2 (May 13-17)

**Tasks:**
- [x] `GenesisModules` static class — 8 modules with Id, DisplayName, IAM Permissions
- [x] Module names are **cloud-agnostic** (`Caching`, `Messaging`, `FileStorage`, `Search`, `Notifications`, `Workflow`, `AIAssistance`, `Reporting`) — no cloud suffix on the module name itself
- [x] `GetPackageName(moduleName, cloudProvider)` → `Pervaxis.Genesis.{Module}.{CloudProvider}` (e.g. `"Caching" + "AWS"` → `Pervaxis.Genesis.Caching.AWS`)
- [x] `GetDiExtensionName(moduleName, cloudProvider)` → `AddGenesis{Module}{CloudProvider}` (e.g. `AddGenesisCachingAWS`) — matches the extension method naming in each Genesis provider package
- [x] `GetAll()`, `GetById(id)`, `GetAllNames()` methods
- [x] `CanvasModules` static class — 14 modules
- [x] Tests to verify module lists are complete, `GetPackageName` resolves correctly for `"AWS"`, and `GetDiExtensionName` produces correct method names

**Owner:** Engineer B  
**Effort:** 6 hours

---

### 3.8 Phase 1 Deliverables

- [x] `3.8.1` `Pervaxis.Forge.Engine` — 90%+ test coverage
- [x] `3.8.2` 150+ unit tests passing
- [x] `3.8.3` Integration test: manifest → ZIP end-to-end
- [x] `3.8.4` XML documentation on all public APIs

---

## 4. Phase 2: REST API Templates

**Duration:** 1 week (May 20 - May 24)  
**Goal:** 18 production-ready REST API Scriban templates  
**Team:** 2 engineers

### 4.1 Template Inventory (18 files)

| File | Complexity |
|---|---|
| `manifest.json.sbn` | Low |
| `SPEC.md.sbn` | Low |
| `README.md.sbn` | Low |
| `Request.cs.sbn` | Low |
| `Response.cs.sbn` | Low |
| `IService.cs.sbn` | Low |
| `tests.csproj.sbn` | Low |
| `appsettings.Development.json.sbn` | Low |
| `Dockerfile.sbn` | Medium |
| `docker-compose.localstack.yml.sbn` | Medium |
| `.github/workflows/build-test.yml.sbn` | Medium |
| `csproj.sbn` | Medium |
| `appsettings.json.sbn` | Medium |
| `Controller.cs.sbn` | Medium |
| `TestBase.cs.sbn` | Medium |
| `Program.cs.sbn` | **High** |
| `ServiceCollectionExtensions.cs.sbn` | **High** |
| `.claude/CLAUDE.md.sbn` | **High** |

### 4.2 Simple Templates + DTOs

- [x] `manifest.json.sbn`, `SPEC.md.sbn`, `README.md.sbn`
- [x] `Request.cs.sbn`, `Response.cs.sbn`, `IService.cs.sbn`

**Owner:** Engineer A | **Effort:** 6 hours

---

### 4.3 Docker Templates

- [x] `Dockerfile.sbn` — .NET 10 multi-stage
- [x] `docker-compose.localstack.yml.sbn` — conditional services per Genesis modules (SQS/SNS, Redis, S3, PostgreSQL)
- [ ] Test: Docker build runs, LocalStack starts

**Owner:** Engineer B | **Effort:** 8 hours

---

### 4.4 .csproj + Configuration

- [x] `csproj.sbn` — .NET 10, nullable, dynamic `<PackageReference>` loop — package name resolved as `Pervaxis.Genesis.{{ module.name }}.{{ model.cloud_provider }}` so an AWS vertical generates `Pervaxis.Genesis.Caching.AWS` and a future Azure vertical generates `Pervaxis.Genesis.Caching.Azure` with zero template change
- [x] `tests.csproj.sbn` — xUnit + FluentAssertions
- [x] `appsettings.json.sbn` — dynamic sections per Genesis module + DB connection + logging
- [x] `appsettings.Development.json.sbn` — LocalStack overrides

**Owner:** Engineer A | **Effort:** 10 hours

---

### 4.5 Program.cs + DI Wiring (CRITICAL)

- [x] `Program.cs.sbn` — Minimal API host, Genesis module loop, Swagger — `using` directives rendered as `Pervaxis.Genesis.{{ module.name }}.{{ model.cloud_provider }}`
- [x] `ServiceCollectionExtensions.cs.sbn` — Genesis DI calls rendered as `services.AddGenesis{{ module.name }}{{ model.cloud_provider }}(...)` — cloud provider from vertical, module name from manifest
- [ ] Test: generated service for an AWS vertical compiles with `dotnet build` and starts; verify `csproj` references `*.AWS` packages only (blocked pending Genesis NuGet publish)

**Owner:** Engineer B | **Effort:** 12 hours

---

### 4.6 Controller + Test Infrastructure

- [x] `Controller.cs.sbn` — route from naming convention, stub GET/POST
- [x] `TestBase.cs.sbn` — mock factory helpers, DI test host, fixture base

**Owner:** Engineer A | **Effort:** 6 hours

---

### 4.7 CLAUDE.md + CI/CD Templates (CRITICAL)

- [x] `.claude/CLAUDE.md.sbn` — service identity, Genesis modules wired, queue contracts, DB info, coding standards, what Claude Code should build
- [x] `.github/workflows/build-test.yml.sbn` — triggers, checkout, .NET setup, restore, build, test, coverage
- [ ] Verify Claude Code CLI parses generated CLAUDE.md

**Owner:** Engineer A | **Effort:** 10 hours

---

### 4.8 Phase 2 Deliverables

- 18 `.sbn` files embedded in Engine
- Integration test: generate `clarivolt/intake-service` (FileStorage + Messaging + PostgreSQL + queue)
  - `dotnet build` ✓
  - `dotnet test` ✓
  - Docker builds ✓
  - LocalStack starts ✓

---

## 5. Phase 3: Infrastructure Provisioning & GitHub Integration

**Duration:** 1 week (May 27 - May 31)  
**Goal:** AWS resource creation, IaC generation, GitHub repo creation  
**Team:** 2 engineers

### 5.1 Terraform + CDK Template Generation

- [ ] Create `Templates/terraform/` — `main.tf.sbn` (conditional resources: ElastiCache, RDS, S3, SQS, SNS), `variables.tf.sbn`, `outputs.tf.sbn`
- [ ] Create `Templates/cdk/` — `Program.cs.sbn`, `InfrastructureStack.cs.sbn` (same conditionals)
- [ ] Test: `terraform plan` succeeds, `cdk synth` produces CloudFormation

**Owner:** Engineer A | **Effort:** 12 hours

---

### 5.2 Direct AWS Deployment (Deploy Now)

- [ ] Add AWS SDK packages: ElastiCache, RDS, S3, SQS, SNS, SecretsManager
- [ ] Implement `AwsDeploymentService`:
  - Assume vertical's IAM role via STS
  - `DeployElastiCacheAsync()`, `DeployRdsAsync()`, `DeployS3BucketAsync()`, `DeploySqsQueueAsync()`
  - `StoreSecretsAsync()` — write connection strings to Secrets Manager at `{env}/{vertical}/{service}/config`
- [ ] Environment-prefixed resource naming (`test-clarivolt-intake-cache`)
- [ ] Write integration tests with LocalStack

**Owner:** Both | **Effort:** 20 hours

---

### 5.3 GitHub Integration

- [x] Add Octokit + LibGit2Sharp packages
- [x] Implement `GitHubService`:
  - `CreateRepositoryAsync()` — uses vertical's enrolled GitHub token + org
  - `ConfigureBranchProtectionAsync()`
  - `PushInitialCommitAsync()` — extract ZIP, init repo, commit, push
- [x] Wire GitHub service into `POST /api/v1/generate` (opt-in via `CreateGitHubRepo` flag)
- [ ] Test: create repo → configure protection → push scaffold (smoke test against real org)

**Owner:** Engineer B | **Effort:** 10 hours

---

### 5.4 Generation Endpoints Polish

- [x] `POST /api/v1/generate` — resolve vertical, call PrintGenerator, optionally create GitHub repo
- [x] `POST /api/v1/generate/batch` — loop services, collect per-service results, combined ZIP
- [x] `POST /api/v1/generate/validate` — manifest validation + derived names preview
- [x] Audit log every generation to `generation_logs`

**Owner:** Engineer A | **Effort:** 6 hours

---

### 5.5 Phase 3 Deliverables

- Terraform + CDK templates generated for every print
- AWS resources created via SDK in LocalStack integration tests
- GitHub repos created with branch protection + initial commit
- Generation endpoints fully wired with vertical context
- Full API deployed to dev environment

---

## 6. Phase 4 (V2 Roadmap): Forge-Managed Infrastructure

**Status:** Planned — not in V1 scope  
**Goal:** All cloud resources for every vertical and service are created, updated, and destroyed exclusively through Forge. No developer ever touches the AWS console or runs `terraform apply` manually. Forge becomes the single policy-enforcement point for infrastructure across all verticals.

> **Why this matters:** Today, V1 generates IaC templates that engineers apply themselves. That still allows console drift, manual overrides, and inconsistent naming. V2 closes that gap — Forge owns the infrastructure lifecycle end-to-end.

---

### 6.1 Core Principle: Forge as the Infrastructure Control Plane

```
Developer  →  Forge Launchpad  →  Forge API  →  Cloud (AWS / Azure / GCP)
                                      ↑
                              No other path to provision
                              resources is permitted
```

- **No console access** for resource creation — IAM policies on the vertical's role deny `Create*` / `Delete*` actions except when called from Forge's assumed role session
- **No manual `terraform apply`** — Forge executes the plan internally; engineers receive outputs (ARNs, endpoints), not the ability to run Terraform themselves
- **Full audit trail** — every resource create/update/destroy is logged to `generation_logs` with operator identity, manifest snapshot, and outcome

---

### 6.2 Terraform Execution Engine

V1 generates Terraform HCL files that teams apply themselves. V2 internalises the execution:

- [ ] Embed Terraform binary or invoke via child process (`terraform init`, `terraform plan`, `terraform apply -auto-approve`)
- [ ] Store Terraform state in a Forge-managed S3 bucket (one state file per `{vertical}/{service}/{environment}`)
- [ ] State locking via DynamoDB to prevent concurrent applies
- [ ] `POST /api/v1/infrastructure/apply` — trigger apply for a specific service + environment
- [ ] `POST /api/v1/infrastructure/destroy` — tear down resources for a service + environment
- [ ] `GET /api/v1/infrastructure/{vertical}/{service}/{environment}` — current state summary (resources, endpoints, last applied)

**Terraform module source — decision pending:**  
The Terraform templates Forge generates will use community modules rather than raw resources where appropriate. The `terraform-aws-modules` community registry (`registry.terraform.io/modules/terraform-aws-modules`) is the leading candidate — it covers ElastiCache, RDS, S3, SQS, VPC, and more with production-hardened defaults. Final decision on which modules (community vs. custom internal) to be made before Phase 4 kicks off.

```hcl
# Example — using terraform-aws-modules/elasticache (decision pending)
module "cache" {
  source  = "terraform-aws-modules/elasticache/aws"
  version = "~> 1.0"

  cluster_id = "${var.environment}-${var.vertical}-${var.service}-cache"
  # ... module inputs
}
```

---

### 6.3 Policy Enforcement (IAM Guardrails)

For Forge to be the sole provisioning path, the vertical's `ForgeDeploymentRole` must be scoped so that no other session can create or delete infrastructure resources:

- [ ] Define a `ForgeResourcePolicy` IAM policy template (generated per vertical at enrollment time)
- [ ] Policy denies `ec2:*`, `rds:*`, `elasticache:*`, `s3:CreateBucket`, `sqs:CreateQueue`, etc. unless the session principal is Forge's own assumed-role session tag (`forge:managed=true`)
- [ ] Forge sets `aws:RequestTag/forge:managed=true` on every STS AssumeRole call
- [ ] Policy template emitted as an additional IaC file (`iam-policy.tf.sbn`) during vertical enrollment
- [ ] Cloud Ops applies this policy to the vertical's AWS account — once applied, console creation is blocked

---

### 6.4 Infrastructure Drift Detection

- [ ] `GET /api/v1/infrastructure/{vertical}/{service}/{environment}/drift` — run `terraform plan` without apply; return diff of actual vs. declared state
- [ ] Scheduled drift check (configurable per vertical — default daily)
- [ ] Drift alert written to `generation_logs` with `drift_detected: true`
- [ ] Launchpad surfaces drift warnings on the Vertical Workspace (V2 UI scope)

---

### 6.5 Multi-Cloud Readiness

Phase 4 targets AWS first. The same execution engine will support Azure and GCP by swapping the Terraform provider — no structural changes to Forge required:

| Cloud | Terraform Provider | Module Candidate |
|---|---|---|
| AWS | `hashicorp/aws` | `terraform-aws-modules/*` (TBD) |
| Azure | `hashicorp/azurerm` | `Azure/` modules on registry (TBD) |
| GCP | `hashicorp/google` | `terraform-google-modules/*` (TBD) |

The `cloudProvider` already flows through `TemplateModel` from V1 — Forge templates conditionally emit the correct provider block at generation time.

---

### 6.6 Phase 4 Prerequisites (before scheduling)

- [ ] Decision: `terraform-aws-modules` community modules vs. custom internal modules — evaluate security posture, versioning, and coverage
- [ ] Terraform binary bundling strategy (sidecar container vs. embedded CLI vs. Terraform Cloud API)
- [ ] S3 state bucket and DynamoDB lock table provisioned per Forge environment
- [ ] IAM policy template reviewed by Cloud Ops and Security
- [ ] UI scope defined for infrastructure management screens (Launchpad V2)

---

## 7. Dependencies & Blockers

| Dependency | Owner | Status | Impact if Delayed |
|---|---|---|---|
| GitHub PAT with `repo` + `admin:org` scopes | DevOps | 🟢 Ready | Cannot create repos |
| AWS Account + `ForgeDeploymentRole` IAM role | Cloud Ops | 🟢 Ready | Cannot deploy resources |
| PostgreSQL instance for Forge | Cloud Ops | 🟡 In Progress | Use local Docker in Week 1 |
| STS AssumeRole permissions configured | Cloud Ops | 🟡 In Progress | Mock in unit tests; real in integration |
| Pervaxis.Genesis packages | Genesis Team | 🟢 Ready | Cannot wire Genesis modules |
| API contract shared with UI team | BFF team | 🔴 Must deliver by May 8 | UI team blocked on real integration |

---

## 8. Quality Gates

### Phase 0 Gate — May 10, 2026

- [ ] All 6 enrollment endpoints functional
- [ ] AWS connectivity validation works (STS dry-run)
- [ ] GitHub connectivity validation works (Octokit)
- [ ] Credentials encrypted at rest
- [ ] API deployed to dev environment
- [ ] Swagger JSON shared with UI team
- [ ] Integration tests passing against real PostgreSQL

### Phase 1 Gate — May 17, 2026

- [ ] 90%+ test coverage on Engine
- [ ] 50+ naming convention tests pass
- [ ] Manifest validation catches all invalid cases
- [ ] Template engine renders in strict mode
- [ ] Integration test: manifest → ZIP end-to-end

### Phase 2 Gate — May 24, 2026

- [ ] All 18 REST API templates render correctly
- [ ] Generated service: `dotnet build` ✓
- [ ] Generated tests: `dotnet test` ✓
- [ ] Docker image builds
- [ ] LocalStack services start
- [ ] CLAUDE.md passes manual review (readable, accurate context)

### Phase 3 Gate — May 31, 2026

- [ ] Terraform templates pass `terraform plan`
- [ ] CDK templates synthesize CloudFormation
- [ ] AWS SDK creates all resource types in LocalStack
- [ ] GitHub repo created, branch protection set, initial commit pushed
- [ ] Audit log written on every generation
- [ ] Full end-to-end: enroll vertical → generate service → deploy AWS → create repo

---

## 9. Testing Strategy

### Unit Tests (90%+ coverage)
- xUnit + FluentAssertions + Moq
- Focus areas: naming logic (100%), manifest validation, template rendering, VerticalService encryption

### Integration Tests
- LocalStack for all AWS operations
- Disposable PostgreSQL (Docker) for EF Core
- Test GitHub org for repo creation

### Key Scenarios
1. Enroll vertical → validate AWS → validate GitHub → persist → retrieve
2. `manifest.json` → ZIP → extract → verify file tree
3. Deploy ElastiCache / RDS / S3 / SQS to LocalStack
4. Create GitHub repo → configure branch protection → push commit
5. Full flow: vertical enrolled → generate `clarivolt/intake-service` → repo exists

---

## 10. Risk Mitigation

| Risk | Mitigation |
|---|---|
| PostgreSQL not ready week 1 | Run local Docker PostgreSQL for development |
| STS AssumeRole permissions not ready | Mock STS in unit tests; unblock with real creds in week 2 |
| API contract changes break UI mocks | Freeze API contract by May 8 (end of Week 1), communicate any changes immediately |
| Scriban template complexity | Keep templates under 200 lines, extract helper functions |
| GitHub API rate limiting | Retry with exponential backoff, use OAuth app token over PAT |

---

## 11. Definition of Done

### Feature-Level
- [ ] Code complete and committed
- [ ] Unit tests passing (90%+ coverage)
- [ ] Integration tests passing
- [ ] Code reviewed and approved
- [ ] No critical bugs

### Phase-Level
- [ ] All features complete per feature DoD
- [ ] Quality gate passed
- [ ] Demo to stakeholders
- [ ] Sign-off from Tech Lead

---

**End of Backend Blueprint**

*Pervaxis Forge v1.1 — Clarivex Technologies © 2026*
