# Pervaxis Forge — Development Blueprint
**Phase-by-Phase Implementation Plan**

**Version:** 1.0  
**Project Start:** May 6, 2026  
**Projected Completion:** June 14, 2026 (6 weeks)  
**Team:** Pervaxis Platform Team  
**Status:** Pre-Implementation  

---

## Table of Contents
1. [Project Timeline & Milestones](#1-project-timeline--milestones)
2. [Phase 1: Core Engine](#2-phase-1-core-engine)
3. [Phase 2: REST API Templates](#3-phase-2-rest-api-templates)
4. [Phase 3: Infrastructure Provisioning](#4-phase-3-infrastructure-provisioning)
5. [Phase 4: Launchpad UI](#5-phase-4-launchpad-ui)
6. [Phase 5: Angular Templates](#6-phase-5-angular-templates)
7. [Task Index](#7-task-index)
8. [Dependencies & Blockers](#8-dependencies--blockers)
9. [Quality Gates](#9-quality-gates)
10. [Testing Strategy](#10-testing-strategy)
11. [Risk Mitigation](#11-risk-mitigation)
12. [Definition of Done](#12-definition-of-done)
13. [Glossary](#13-glossary)
14. [Change Log](#14-change-log)

---

## 1. Project Timeline & Milestones

### 1.1 High-Level Schedule

```
Week 1 (May 6-10):   Phase 1 — Core Engine Foundation
Week 2 (May 13-17):  Phase 1 (cont.) + Phase 2 Start
Week 3 (May 20-24):  Phase 2 — REST API Templates Complete
Week 4 (May 27-31):  Phase 3 — Infrastructure Provisioning
Week 5 (Jun 3-7):    Phase 4 — Launchpad UI
Week 6 (Jun 10-14):  Phase 5 — Angular Templates + Final Polish
```

### 1.2 Milestone Tracker

| Milestone | Target Date | Status | Deliverable |
|---|---|---|---|
| **M1: Engine Core Complete** | May 10, 2026 | 🔴 Not Started | Manifest parsing, naming derivation, template engine |
| **M2: REST API Generation Works** | May 24, 2026 | 🔴 Not Started | Generate compilable .NET service from manifest |
| **M3: Infrastructure Deploy Works** | May 31, 2026 | 🔴 Not Started | AWS resources created, Terraform/CDK generated |
| **M4: Launchpad UI Functional** | June 7, 2026 | 🔴 Not Started | Multi-service wizard, GitHub integration |
| **M5: Angular Templates Complete** | June 14, 2026 | 🔴 Not Started | Shell + MFE generation working |
| **M6: Production Ready** | June 14, 2026 | 🔴 Not Started | All quality gates passed, documentation complete |

### 1.3 Critical Path

```
Engine (Week 1-2)
    ↓
REST Templates (Week 2-3) ← CRITICAL PATH
    ↓
Infrastructure (Week 4) ← CRITICAL PATH
    ↓
Launchpad UI (Week 5)
    ↓
Angular Templates (Week 6)
```

**Critical Path Items:**
1. Engine naming convention logic (Week 1)
2. Scriban template engine integration (Week 1)
3. REST API template set (Week 2-3)
4. AWS SDK integration (Week 4)
5. GitHub repository creation (Week 5)

Any delay in critical path items pushes entire timeline.

---

## 2. Phase 1: Core Engine

**Duration:** 2 weeks (May 6 - May 17)  
**Goal:** Bulletproof deterministic generation engine  
**Team Allocation:** 2 engineers full-time  

### 2.1 Week 1 Tasks (May 6-10)

#### Day 1-2: Project Setup & Manifest Model

**Tasks:**
- [ ] Create `Pervaxis.Forge.slnx` solution
- [ ] Create `Pervaxis.Forge.Engine` class library project (.NET 10)
- [ ] Configure `Directory.Build.props` (nullable, warnings as errors)
- [ ] Create `nuget.config` (no Pervaxis.Core dependency)
- [ ] Define `ForgeManifest` record with all properties
- [ ] Define `ServiceType`, `DatabaseConfig`, `QueueConfig`, `ApiConfig`, `AngularConfig` models
- [ ] Write JSON serialization tests (System.Text.Json)

**Acceptance Criteria:**
- Solution compiles
- Manifest deserializes from JSON with no errors
- All property types correctly mapped

**Owner:** Engineer A  
**Estimated Effort:** 12 hours

---

#### Day 2-3: Naming Convention Engine

**Tasks:**
- [ ] Create `NamingConvention` static class
- [ ] Implement `ToPascalCase(string kebab)`
- [ ] Implement `StripServiceSuffix(string name)`
- [ ] Implement `GetFirstSegment(string name)`
- [ ] Implement `GetComponentPrefix(string product)`
- [ ] Implement `DeriveDotNetNames(product, name)` → returns all 10+ derived names
- [ ] Implement `DeriveAngularShellNames(product, name)`
- [ ] Implement `DeriveAngularMfeNames(product, name)`
- [ ] Write 50+ unit tests covering all edge cases
  - Empty strings
  - Single-word names
  - Multi-hyphen names (e.g., "user-profile-service")
  - Unicode handling
  - Case sensitivity

**Acceptance Criteria:**
- All naming rules from requirements doc implemented
- 100% test coverage on naming logic
- Edge cases handled gracefully

**Owner:** Engineer B  
**Estimated Effort:** 16 hours

---

#### Day 3-4: Manifest Validation

**Tasks:**
- [ ] Create `ManifestValidator` class
- [ ] Implement kebab-case regex validation
- [ ] Implement service-type-specific name rules (.NET must end with `-service`, Angular must not)
- [ ] Implement Genesis module validation (check against known list)
- [ ] Implement Canvas module validation
- [ ] Implement required field validation
- [ ] Return `ValidationResult` with specific error messages
- [ ] Write 30+ validation tests (valid + invalid manifests)

**Acceptance Criteria:**
- Invalid manifests rejected with clear error messages
- Valid manifests pass all checks
- All edge cases covered

**Owner:** Engineer A  
**Estimated Effort:** 12 hours

---

#### Day 4-5: Scriban Template Engine Integration

**Tasks:**
- [ ] Add Scriban NuGet package
- [ ] Create `ITemplateEngine` interface
- [ ] Implement `ScribanTemplateEngine` with strict mode
- [ ] Create `TemplateModel` class (normalized manifest + derived names)
- [ ] Implement `TemplateModelBuilder.Build(manifest)` — sets all defaults
- [ ] Write sample template: `test-template.sbn`
- [ ] Test rendering with mock data
- [ ] Handle template errors gracefully
- [ ] Write 20+ template engine tests

**Acceptance Criteria:**
- Templates render with correct variable substitution
- Undefined variables cause errors (strict mode)
- Template errors surface with helpful messages

**Owner:** Engineer B  
**Estimated Effort:** 12 hours

---

### 2.2 Week 2 Tasks (May 13-17)

#### Day 1-2: Embedded Template Loading

**Tasks:**
- [ ] Create folder structure: `Templates/rest-api/`, `Templates/angular-shell/`, etc.
- [ ] Add placeholder `.sbn` files
- [ ] Configure `.csproj` to embed templates as resources
- [ ] Implement `TemplateLoader.LoadAsync(path)` using `GetManifestResourceStream`
- [ ] Write tests for template loading
- [ ] Handle missing template errors

**Acceptance Criteria:**
- Templates load from embedded resources
- Missing templates throw helpful exceptions

**Owner:** Engineer A  
**Estimated Effort:** 8 hours

---

#### Day 2-4: Print Generator Core

**Tasks:**
- [ ] Create `PrintGenerator` class
- [ ] Implement `GenerateAsync(manifest)` orchestration:
  1. Validate manifest
  2. Normalize manifest (TemplateModelBuilder)
  3. Derive names
  4. Load templates for service type
  5. Render each template
  6. Resolve output paths
  7. Package as ZIP
- [ ] Create `FileGenerator` for individual file rendering
- [ ] Create `ZipPackager` using `System.IO.Compression`
- [ ] Write integration test: manifest → ZIP with expected files
- [ ] Verify ZIP can be extracted

**Acceptance Criteria:**
- `GenerateAsync` produces valid ZIP
- ZIP contains expected folder structure
- File contents match template output

**Owner:** Engineer B  
**Estimated Effort:** 16 hours

---

#### Day 4-5: Genesis & Canvas Module Metadata

**Tasks:**
- [ ] Create `GenesisModules` static class
- [ ] Define all 8 module constants with metadata:
  - Id, DisplayName, Description, NugetPackage, Version, IAM Permissions
- [ ] Create `CanvasModules` static class
- [ ] Define all 14 Canvas module metadata
- [ ] Write tests to verify module lists
- [ ] Implement `GetAll()`, `GetById(id)` methods

**Acceptance Criteria:**
- All modules documented with correct metadata
- Metadata accessible from Engine

**Owner:** Engineer A  
**Estimated Effort:** 6 hours

---

### 2.3 Phase 1 Deliverables

**Code:**
- `Pervaxis.Forge.Engine` — fully functional, 90%+ test coverage

**Tests:**
- 150+ unit tests passing
- Integration test: Generate simple REST API print end-to-end

**Documentation:**
- XML documentation on all public APIs
- README with usage examples

---

## 3. Phase 2: REST API Templates

**Duration:** 1.5 weeks (May 20 - May 28)  
**Goal:** Complete, production-ready REST API template set  
**Team Allocation:** 2 engineers full-time  

### 3.1 Template Inventory (18 files)

| File | Purpose | Complexity |
|---|---|---|
| `manifest.json.sbn` | Copy of input manifest | Low |
| `SPEC.md.sbn` | Empty spec template | Low |
| `README.md.sbn` | Service overview | Low |
| `Dockerfile.sbn` | Multi-stage .NET 10 build | Medium |
| `docker-compose.localstack.yml.sbn` | LocalStack services | Medium |
| `.claude/CLAUDE.md.sbn` | Dynamic context for Claude Code CLI | **High** |
| `.github/workflows/build-test.yml.sbn` | CI pipeline | Medium |
| `csproj.sbn` | Project file with NuGet refs | Medium |
| `Program.cs.sbn` | DI wiring + Genesis modules | **High** |
| `appsettings.json.sbn` | Config sections for modules | Medium |
| `appsettings.Development.json.sbn` | LocalStack overrides | Low |
| `Controller.cs.sbn` | Stub controller | Medium |
| `IService.cs.sbn` | Service interface | Low |
| `Request.cs.sbn` | Request DTO | Low |
| `Response.cs.sbn` | Response DTO | Low |
| `ServiceCollectionExtensions.cs.sbn` | DI wiring | **High** |
| `tests.csproj.sbn` | Test project | Low |
| `TestBase.cs.sbn` | Test infrastructure | Medium |

### 3.2 Week 1 Tasks (May 20-24)

#### Day 1: Simple Templates (manifest, README, SPEC, DTOs)

**Tasks:**
- [ ] Write `manifest.json.sbn` — passthrough
- [ ] Write `SPEC.md.sbn` with placeholder structure
- [ ] Write `README.md.sbn` with service overview + naming summary
- [ ] Write `Request.cs.sbn` stub
- [ ] Write `Response.cs.sbn` stub
- [ ] Write `IService.cs.sbn` stub
- [ ] Test rendering with sample manifest

**Owner:** Engineer A  
**Effort:** 6 hours

---

#### Day 1-2: Docker Templates

**Tasks:**
- [ ] Write `Dockerfile.sbn` with .NET 10 multi-stage build
- [ ] Write `docker-compose.localstack.yml.sbn` with conditional service wiring:
  - If `Messaging.AWS` → include SQS/SNS
  - If `Caching.AWS` → include Redis
  - If `FileStorage.AWS` → include S3
  - If `database.engine` → include PostgreSQL
- [ ] Test Docker build locally
- [ ] Verify LocalStack containers start

**Owner:** Engineer B  
**Effort:** 8 hours

---

#### Day 2-3: .csproj Template

**Tasks:**
- [ ] Write `csproj.sbn` with:
  - .NET 10 target framework
  - Nullable enabled
  - Warnings as errors
  - Dynamic `<PackageReference>` for selected Genesis modules
- [ ] Write `tests.csproj.sbn` with xUnit + FluentAssertions
- [ ] Test: generated .csproj restores packages successfully

**Owner:** Engineer A  
**Effort:** 6 hours

---

#### Day 3-4: Program.cs + DI Wiring (CRITICAL)

**Tasks:**
- [ ] Write `Program.cs.sbn` with:
  - Minimal API host setup
  - Dynamic Genesis module wiring (loop over `selected_modules`)
  - Controller registration
  - Swagger setup
- [ ] Write `ServiceCollectionExtensions.cs.sbn` with:
  - Genesis module DI calls (conditional on manifest)
  - Domain service registration stub
- [ ] Test: generated service compiles
- [ ] Test: service starts without errors

**Owner:** Engineer B  
**Effort:** 12 hours

---

#### Day 4-5: Configuration Templates

**Tasks:**
- [ ] Write `appsettings.json.sbn` with:
  - Dynamic sections for each selected Genesis module
  - Database connection string placeholder
  - Logging configuration
- [ ] Write `appsettings.Development.json.sbn` with:
  - LocalStack endpoint overrides
- [ ] Test: configuration loads correctly

**Owner:** Engineer A  
**Effort:** 6 hours

---

#### Day 5: Controller Template

**Tasks:**
- [ ] Write `Controller.cs.sbn` with:
  - Route attribute derived from naming convention
  - Stub GET/POST endpoints
  - XML documentation
- [ ] Test: controller endpoints respond

**Owner:** Engineer B  
**Effort:** 4 hours

---

### 3.3 Week 2 Tasks (May 27-28)

#### Day 1: CLAUDE.md Template (CRITICAL)

**Tasks:**
- [ ] Write `.claude/CLAUDE.md.sbn` with dynamic content:
  - Service identity section
  - Genesis modules wired (list + usage)
  - Queue contracts
  - Database info
  - Coding standards
  - What Claude Code should build
- [ ] Verify Claude Code CLI can parse generated CLAUDE.md
- [ ] Test with sample service

**Owner:** Engineer A  
**Effort:** 8 hours

---

#### Day 1-2: CI/CD Workflow Template

**Tasks:**
- [ ] Write `.github/workflows/build-test.yml.sbn`
  - Triggers: PR to main/develop, push to main/develop
  - Steps: checkout, setup .NET, restore, build, test, coverage
- [ ] Test: workflow runs on generated repo

**Owner:** Engineer B  
**Effort:** 6 hours

---

#### Day 2: Test Infrastructure Template

**Tasks:**
- [ ] Write `TestBase.cs.sbn` with:
  - Mock factory helpers
  - DI test host setup
  - Fixture base class
- [ ] Test: test project compiles and runs

**Owner:** Engineer A  
**Effort:** 4 hours

---

### 3.4 Phase 2 Deliverables

**Templates:**
- 18 `.sbn` files embedded in Engine

**Integration Test:**
- Generate `clarivolt/intake-service` with:
  - Genesis modules: FileStorage, Messaging
  - Database: PostgreSQL
  - Queue: clarivolt.intake.submitted
- Verify:
  - Compiles with `dotnet build`
  - Tests pass with `dotnet test`
  - Docker builds successfully
  - LocalStack starts all services

**Documentation:**
- Template development guide

---

## 4. Phase 3: Infrastructure Provisioning

**Duration:** 1 week (May 29 - June 4)  
**Goal:** AWS resource creation + IaC generation  
**Team Allocation:** 2 engineers full-time  

### 4.1 Week Tasks

#### Day 1-2: Terraform Template Generation

**Tasks:**
- [ ] Create `Templates/terraform/` folder
- [ ] Write `main.tf.sbn` with conditional resource blocks:
  - ElastiCache (if Caching.AWS)
  - RDS PostgreSQL (if database.engine)
  - S3 bucket (if FileStorage.AWS)
  - SQS queues (loop over manifest.queues)
  - SNS topics (if Messaging.AWS)
  - OpenSearch (if Search.AWS)
- [ ] Write `variables.tf.sbn` with environment variable
- [ ] Write `outputs.tf.sbn` with connection strings
- [ ] Write `README.md.sbn` for Terraform usage
- [ ] Test: `terraform plan` succeeds on generated files

**Owner:** Engineer A  
**Effort:** 12 hours

---

#### Day 2-3: AWS CDK Template Generation

**Tasks:**
- [ ] Create `Templates/cdk/` folder
- [ ] Write `Program.cs.sbn` CDK app
- [ ] Write `InfrastructureStack.cs.sbn` with same conditional logic as Terraform
- [ ] Test: `cdk synth` produces CloudFormation template

**Owner:** Engineer B  
**Effort:** 12 hours

---

#### Day 3-5: Direct AWS Deployment (Deploy Now)

**Tasks:**
- [ ] Create `Pervaxis.Forge.Api` project
- [ ] Add AWS SDK NuGet packages:
  - AWSSDK.ElastiCache
  - AWSSDK.RDS
  - AWSSDK.S3
  - AWSSDK.SQS
  - AWSSDK.SNS
  - AWSSDK.SecretsManager
- [ ] Implement `AwsDeploymentService`:
  - `DeployElastiCacheAsync()`
  - `DeployRdsAsync()`
  - `DeployS3BucketAsync()`
  - `DeploySqsQueueAsync()`
  - `StoreSecretsAsync()` → AWS Secrets Manager
- [ ] Implement environment-prefixed resource naming
- [ ] Write integration tests with LocalStack
- [ ] Test end-to-end: Deploy → verify resources exist

**Owner:** Both Engineers  
**Effort:** 20 hours

---

#### Day 5: PostgreSQL Credential Store

**Tasks:**
- [ ] Design database schema (organizations, aws_accounts, generation_logs, deployment_outputs)
- [ ] Create EF Core models
- [ ] Create `ForgeDbContext`
- [ ] Write migrations
- [ ] Implement `AwsAccountService` with encryption
- [ ] Implement `GenerationLogService`
- [ ] Write CRUD endpoints for AWS account management
- [ ] Test: Store and retrieve AWS credentials

**Owner:** Engineer A  
**Effort:** 8 hours

---

### 4.2 Phase 3 Deliverables

**Code:**
- `Pervaxis.Forge.Api` with AWS deployment endpoints
- Terraform and CDK template generation
- PostgreSQL credential store

**Tests:**
- Integration tests with LocalStack
- Database CRUD tests

**Infrastructure:**
- Forge API deployed to dev environment
- PostgreSQL database provisioned

---

## 5. Phase 4: Launchpad UI

**Duration:** 1 week (June 5 - June 11)  
**Goal:** Multi-service wizard with GitHub integration  
**Team Allocation:** 2 frontend engineers  

### 5.1 Week Tasks

#### Day 1: Angular Project Setup

**Tasks:**
- [ ] Create `Pervaxis.Forge.Launchpad` Angular 18 project
- [ ] Configure Nx workspace
- [ ] Add Angular Material
- [ ] Set up routing
- [ ] Configure HttpClient with API base URL
- [ ] Create authentication service (admin-only)
- [ ] Create auth guard

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

#### Day 1-2: Step 1 - Service Identity

**Tasks:**
- [ ] Create `ServiceIdentityComponent`
- [ ] Implement dynamic service list (add/remove)
- [ ] Product name input with kebab-case validation
- [ ] Service name input with type-specific validation
- [ ] Type selector (BFF / MFE / Both)
- [ ] Real-time naming preview (uses `NamingService` calling API)
- [ ] Form validation with error messages

**Owner:** Frontend Engineer A  
**Effort:** 10 hours

---

#### Day 2-3: Step 2 - Module Selection

**Tasks:**
- [ ] Create `ModuleSelectionComponent`
- [ ] Fetch Genesis modules from API
- [ ] Fetch Canvas modules from API
- [ ] Display checkboxes with module descriptions
- [ ] Conditional display (Genesis for BFF, Canvas for MFE)
- [ ] Per-service configuration (loop over services from Step 1)

**Owner:** Frontend Engineer B  
**Effort:** 10 hours

---

#### Day 3-4: Step 3 - Database & Queues

**Tasks:**
- [ ] Create `DatabaseQueueComponent`
- [ ] Database selection (PostgreSQL / None)
- [ ] Schema name input (auto-derived, editable)
- [ ] Dynamic queue list (add/remove)
- [ ] Queue name input with SQS prefix hint
- [ ] Role selector (Publish / Subscribe)

**Owner:** Frontend Engineer A  
**Effort:** 8 hours

---

#### Day 4: Step 4 - Infrastructure Deployment

**Tasks:**
- [ ] Create `InfrastructureComponent`
- [ ] "Deploy Now" checkbox
- [ ] AWS account dropdown (fetch from API)
- [ ] Environment input (test / accp / prod)
- [ ] "Generate IaC" checkboxes (Terraform / CDK)

**Owner:** Frontend Engineer B  
**Effort:** 6 hours

---

#### Day 5: Step 5 - GitHub Configuration

**Tasks:**
- [ ] Create `GitHubConfigComponent`
- [ ] "Create repos" checkbox
- [ ] Organization dropdown (hardcoded: clarivex-tech)
- [ ] Visibility radio (Private / Public)
- [ ] Branch protection checkbox
- [ ] GitHub Secrets checkbox

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

#### Day 5: Step 6 - Preview & Generate

**Tasks:**
- [ ] Create `PreviewGenerateComponent`
- [ ] Display service summary list
- [ ] Show infrastructure deployment plan
- [ ] Show GitHub repo plan
- [ ] "Generate All Services" button
- [ ] Loading indicator during generation
- [ ] Success/error result display
- [ ] Links to created GitHub repos

**Owner:** Frontend Engineer B  
**Effort:** 8 hours

---

#### Day 6: Wizard Navigation & State

**Tasks:**
- [ ] Create wizard stepper component
- [ ] Implement state management (Angular Signals)
- [ ] Back/Next navigation
- [ ] Validation before progressing
- [ ] Unsaved changes warning
- [ ] Persist state to localStorage (optional)

**Owner:** Frontend Engineer A  
**Effort:** 6 hours

---

#### Day 7: GitHub Integration Backend

**Tasks:**
- [ ] Add Octokit NuGet package to API
- [ ] Implement `GitHubService`:
  - `CreateRepositoryAsync()`
  - `ConfigureBranchProtectionAsync()`
  - `ConfigureSecretsAsync()`
  - `PushInitialCommitAsync()` using LibGit2Sharp
- [ ] Test: Create repo, push generated scaffold

**Owner:** Backend Engineer (from Phase 3 team)  
**Effort:** 10 hours

---

### 5.2 Phase 4 Deliverables

**Code:**
- `Pervaxis.Forge.Launchpad` Angular app
- GitHub integration in `Pervaxis.Forge.Api`

**Features:**
- 6-step wizard functional
- Real-time naming preview
- Batch service generation (5+ services)
- GitHub repo creation

**Deployment:**
- Launchpad deployed to dev environment

---

## 6. Phase 5: Angular Templates

**Duration:** 1 week (June 12 - June 18)  
**Goal:** Angular Shell + Microfrontend stub templates  
**Team Allocation:** 2 engineers  

### 6.1 Angular Shell Template (9 files)

#### Day 1-2: Shell Templates

**Tasks:**
- [ ] Create `Templates/angular-shell/` folder
- [ ] Write `manifest.json.sbn`
- [ ] Write `SPEC.md.sbn`
- [ ] Write `README.md.sbn`
- [ ] Write `package.json.sbn` with Angular 18 + Ionic dependencies
- [ ] Write `angular.json.sbn` workspace config
- [ ] Write `tsconfig.json.sbn` strict mode
- [ ] Write `app.component.ts.sbn` root component stub
- [ ] Write `app.routes.ts.sbn` lazy-loaded MFE routes
- [ ] Write `app.config.ts.sbn` Angular providers
- [ ] Write `.claude/CLAUDE.md.sbn` for Shell context
- [ ] Test: `ng build` succeeds on generated shell

**Owner:** Engineer A  
**Effort:** 12 hours

---

### 6.2 Angular Microfrontend Template (9 files)

#### Day 2-3: MFE Templates

**Tasks:**
- [ ] Create `Templates/angular-microfrontend/` folder
- [ ] Write `manifest.json.sbn`
- [ ] Write `SPEC.md.sbn`
- [ ] Write `README.md.sbn`
- [ ] Write `module.ts.sbn` Angular feature module
- [ ] Write `routing.module.ts.sbn` with placeholder route
- [ ] Write `component.ts.sbn` root feature component
- [ ] Write `component.html.sbn` placeholder template
- [ ] Write `api.service.ts.sbn` API service with base route
- [ ] Write `index.ts.sbn` barrel export
- [ ] Write `.claude/CLAUDE.md.sbn` for MFE context
- [ ] Test: MFE builds and lazy-loads in shell

**Owner:** Engineer B  
**Effort:** 12 hours

---

#### Day 4-5: Integration Testing

**Tasks:**
- [ ] Generate Shell + 3 MFEs using Launchpad
- [ ] Verify all projects build
- [ ] Verify MFEs lazy-load in shell
- [ ] Verify Canvas modules imported correctly
- [ ] Test navigation between MFEs
- [ ] Document any issues

**Owner:** Both Engineers  
**Effort:** 8 hours

---

### 6.3 Phase 5 Deliverables

**Templates:**
- Angular Shell (9 files)
- Angular Microfrontend (9 files)

**Integration Test:**
- Generate Clarivolt Shell + Intake MFE
- Verify builds and runs

---

## 7. Task Index

### 7.1 By Category

#### Infrastructure Setup
- [x] Create solution and projects
- [ ] Configure build properties
- [ ] Set up NuGet feeds
- [ ] Configure CI/CD for Forge itself

#### Core Engine
- [ ] Manifest model and parsing
- [ ] Naming convention logic
- [ ] Manifest validation
- [ ] Template engine integration
- [ ] Embedded template loading
- [ ] Print generator core
- [ ] ZIP packaging
- [ ] Module metadata

#### Templates - REST API
- [ ] Simple templates (manifest, README, DTOs)
- [ ] Docker templates
- [ ] .csproj templates
- [ ] Program.cs + DI wiring
- [ ] Configuration templates
- [ ] Controller template
- [ ] CLAUDE.md template
- [ ] CI/CD workflow template
- [ ] Test infrastructure template

#### Templates - Angular
- [ ] Shell templates (9 files)
- [ ] Microfrontend templates (9 files)

#### Infrastructure Provisioning
- [ ] Terraform generation
- [ ] AWS CDK generation
- [ ] AWS SDK integration
- [ ] Direct resource deployment
- [ ] Secrets Manager integration
- [ ] PostgreSQL credential store

#### API Layer
- [ ] Generate endpoint
- [ ] Validate endpoint
- [ ] Batch generate endpoint
- [ ] Module metadata endpoints
- [ ] AWS account management endpoints
- [ ] GitHub integration

#### Launchpad UI
- [ ] Project setup
- [ ] Step 1: Service Identity
- [ ] Step 2: Module Selection
- [ ] Step 3: Database & Queues
- [ ] Step 4: Infrastructure
- [ ] Step 5: GitHub Config
- [ ] Step 6: Preview & Generate
- [ ] Wizard navigation
- [ ] State management

### 7.2 By Priority

**P0 - Critical Path (Must Complete on Time):**
1. Engine naming convention
2. Scriban integration
3. REST API templates (all 18)
4. AWS SDK integration
5. GitHub repo creation

**P1 - High Priority:**
1. Manifest validation
2. Infrastructure deployment (Deploy Now)
3. Launchpad wizard
4. Angular templates

**P2 - Nice to Have (v1):**
1. Multi-environment configuration
2. Deployment workflows
3. Advanced error handling

**P3 - Deferred to v2:**
1. CLI tool
2. gRPC templates
3. Worker templates
4. Security compliance automation

---

## 8. Dependencies & Blockers

### 8.1 External Dependencies

| Dependency | Owner | Status | Impact if Delayed |
|---|---|---|---|
| GitHub Personal Access Token | DevOps | 🟢 Ready | Cannot create repos |
| AWS Account for Forge | Cloud Ops | 🟢 Ready | Cannot deploy Forge API |
| PostgreSQL Instance for Forge | Cloud Ops | 🟡 In Progress | Credential store unavailable |
| Claude Code CLI (for testing) | Anthropic | 🟢 Ready | Cannot test CLAUDE.md |
| Pervaxis.Genesis packages | Genesis Team | 🟢 Ready | Cannot wire Genesis modules |
| Pervaxis.Canvas packages | Canvas Team | 🟡 In Progress | Cannot wire Canvas modules |

### 8.2 Internal Dependencies

```
Phase 1 (Engine) ─────┬────> Phase 2 (REST Templates)
                      │
                      └────> Phase 3 (Infrastructure)
                                │
Phase 3 ──────────────────────┼────> Phase 4 (Launchpad)
                                │
Phase 1 ────────────────────────────> Phase 5 (Angular Templates)
```

**Blocking Relationships:**
- Phase 2 cannot start until Phase 1 Week 1 complete
- Phase 3 can start in parallel with Phase 2 Week 2
- Phase 4 requires Phase 3 API endpoints
- Phase 5 requires Phase 1 Engine, independent of Phases 2-4

### 8.3 Risk Register

| Risk | Probability | Impact | Mitigation |
|---|---|---|---|
| Scriban learning curve steeper than expected | Medium | High | Start with simple templates, iterate |
| AWS resource creation hits quota limits | Low | High | Request quota increases proactively |
| GitHub API rate limiting | Medium | Medium | Implement retry logic, use OAuth app token |
| Template complexity explodes (too many conditionals) | High | Medium | Keep templates simple, extract helper functions |
| Team members unavailable (sick, vacation) | Medium | High | Cross-train, document everything |
| Requirements change mid-development | Low | High | Lock requirements after Phase 1, defer changes to v2 |

---

## 9. Quality Gates

### 9.1 Phase 1 Quality Gate

**Exit Criteria:**
- [ ] 90%+ test coverage on Engine
- [ ] All naming convention tests pass
- [ ] Manifest validation catches all invalid cases
- [ ] Template engine renders sample template correctly
- [ ] Integration test: Generate dummy print end-to-end
- [ ] Code review complete
- [ ] No critical bugs

**Review Date:** May 17, 2026  
**Reviewers:** Tech Lead, Architect

---

### 9.2 Phase 2 Quality Gate

**Exit Criteria:**
- [ ] All 18 REST API templates render correctly
- [ ] Generated service compiles with `dotnet build`
- [ ] Generated tests pass with `dotnet test`
- [ ] Docker image builds successfully
- [ ] LocalStack services start
- [ ] CLAUDE.md passes manual review (readable, accurate)
- [ ] CI/CD workflow runs without errors
- [ ] Code review complete

**Review Date:** May 28, 2026  
**Reviewers:** Tech Lead, .NET Team Lead

---

### 9.3 Phase 3 Quality Gate

**Exit Criteria:**
- [ ] Terraform templates pass `terraform plan`
- [ ] CDK templates synthesize CloudFormation
- [ ] AWS SDK creates resources in LocalStack
- [ ] Secrets stored in AWS Secrets Manager
- [ ] Credential store encrypts IAM role ARNs
- [ ] Integration tests pass
- [ ] API endpoints functional
- [ ] Code review complete

**Review Date:** June 4, 2026  
**Reviewers:** Tech Lead, Cloud Ops

---

### 9.4 Phase 4 Quality Gate

**Exit Criteria:**
- [ ] Wizard navigates through all 6 steps
- [ ] Real-time naming preview works
- [ ] Batch generation (5 services) succeeds
- [ ] GitHub repos created with correct settings
- [ ] Branch protection configured
- [ ] GitHub Secrets populated
- [ ] Generated code pushed to repos
- [ ] UI/UX review complete
- [ ] Code review complete

**Review Date:** June 11, 2026  
**Reviewers:** Tech Lead, Product Manager, UX Designer

---

### 9.5 Phase 5 Quality Gate

**Exit Criteria:**
- [ ] Angular Shell builds with `ng build`
- [ ] Angular MFE builds and lazy-loads
- [ ] Canvas modules import correctly
- [ ] Generated code follows Angular best practices
- [ ] Integration test: Shell + 3 MFEs works end-to-end
- [ ] Code review complete

**Review Date:** June 18, 2026  
**Reviewers:** Tech Lead, Angular Team Lead

---

### 9.6 Final Production Readiness Gate

**Exit Criteria:**
- [ ] All phase quality gates passed
- [ ] End-to-end test: Generate 5 BFFs + 5 MFEs, deploy infrastructure, create repos
- [ ] All critical bugs fixed
- [ ] Performance test: Generate 10 services in < 30 seconds
- [ ] Security review complete
- [ ] Documentation complete (technical spec, user guide, API docs)
- [ ] Deployment runbook written
- [ ] Rollback plan documented
- [ ] Team trained on Forge usage

**Review Date:** June 18, 2026  
**Reviewers:** VP Engineering, CTO

---

## 10. Testing Strategy

### 10.1 Unit Tests

**Coverage Target:** 90%+

**Focus Areas:**
- Naming convention logic (100% coverage)
- Manifest validation (all edge cases)
- Template rendering (error handling)
- AWS SDK calls (mocked)
- GitHub API calls (mocked)

**Tools:**
- xUnit
- FluentAssertions
- Moq (for mocking)

**CI Integration:**
- Run on every PR
- Block merge if coverage drops below 90%

---

### 10.2 Integration Tests

**Scenarios:**
1. **Engine Integration:**
   - manifest.json → ZIP → extract → verify files
   - Invalid manifest → validation errors
   
2. **Infrastructure Integration (LocalStack):**
   - Deploy ElastiCache → verify cluster exists
   - Deploy RDS → verify database accessible
   - Deploy S3 → verify bucket created
   - Deploy SQS → send/receive message
   
3. **GitHub Integration (Test Org):**
   - Create repo → verify settings
   - Configure branch protection → verify rules
   - Push code → verify commit history
   
4. **End-to-End:**
   - Launchpad: Create 5 services → GitHub repos created → infrastructure deployed → verify all resources

**Environment:**
- LocalStack for AWS services
- Test GitHub organization
- Disposable PostgreSQL database

**Frequency:**
- Run nightly
- Run before each release

---

### 10.3 Performance Tests

**Metrics:**
- Generation time (target: < 2 seconds per service)
- Batch generation time (10 services target: < 20 seconds)
- API response time (target: < 3 seconds)
- Launchpad page load time (target: < 1 second)

**Tools:**
- BenchmarkDotNet (.NET performance)
- Lighthouse (UI performance)

---

### 10.4 Manual Testing

**Test Cases:**
1. Generate service with each Genesis module combination
2. Generate service with database and queues
3. Generate Angular Shell + 3 MFEs
4. Deploy infrastructure to real AWS (dev account)
5. Claude Code CLI: Verify CLAUDE.md provides enough context
6. Developer workflow: Clone generated repo → build → run tests → start service

**Testers:**
- Platform engineers
- Product team

**Timeline:**
- Week 6 (June 12-18)

---

## 11. Risk Mitigation

### 11.1 Technical Risks

**Risk: Template complexity becomes unmaintainable**

**Mitigation:**
- Keep templates under 200 lines
- Extract repeated logic into Scriban functions
- Document template variables clearly
- Regular template reviews

---

**Risk: AWS resource creation fails in production**

**Mitigation:**
- Extensive LocalStack testing
- Dry-run mode before actual deployment
- Retry logic with exponential backoff
- Detailed error messages and logging
- Manual rollback procedures documented

---

**Risk: Generated code doesn't compile**

**Mitigation:**
- Automated compilation test in CI
- Test matrix: all Genesis module combinations
- Sample projects that exercise all templates
- Peer review of all template changes

---

### 11.2 Schedule Risks

**Risk: Phase slips push final deadline**

**Mitigation:**
- 20% time buffer built into each phase
- Critical path identified and monitored weekly
- Daily standups to catch blockers early
- Ready to descope nice-to-haves if needed

---

**Risk: Key team member unavailable**

**Mitigation:**
- Pair programming on critical components
- Knowledge sharing sessions weekly
- Comprehensive documentation
- Cross-training on each phase

---

### 11.3 Product Risks

**Risk: Generated scaffolds don't meet team needs**

**Mitigation:**
- Early prototype shared with Genesis/Canvas teams
- Feedback loop with first users (Clarivolt team)
- Iterative refinement based on usage
- Easy template updates post-launch

---

## 12. Definition of Done

### 12.1 Feature-Level DoD

A feature is done when:
- [ ] Code complete and committed
- [ ] Unit tests written and passing (90%+ coverage)
- [ ] Integration tests passing (if applicable)
- [ ] Code reviewed and approved
- [ ] Documentation updated
- [ ] No critical or high-severity bugs
- [ ] Demonstrated to team in standup

---

### 12.2 Phase-Level DoD

A phase is done when:
- [ ] All features complete per Definition of Done
- [ ] Quality gate passed
- [ ] Phase retrospective completed
- [ ] Lessons learned documented
- [ ] Demo to stakeholders completed
- [ ] Sign-off from Tech Lead

---

### 12.3 Project-Level DoD

Forge v1 is done when:
- [ ] All 5 phases complete
- [ ] All quality gates passed
- [ ] Production readiness review passed
- [ ] End-to-end test successful (10 services generated, deployed, verified)
- [ ] Documentation complete:
  - [ ] FORGE_OVERVIEW.md
  - [ ] FORGE_TECHNICAL_SPECIFICATION.md
  - [ ] User guide
  - [ ] API documentation (Swagger)
  - [ ] Deployment runbook
- [ ] Team training complete
- [ ] Deployed to production environment
- [ ] Monitoring and alerts configured
- [ ] Go-live approved by CTO

---

## 13. Glossary

**BFF (Backend for Frontend)**  
A microservice that provides data and business logic to frontend applications. Typically RESTful or gRPC.

**Canvas**  
Pervaxis's Angular platform library suite. Provides authentication, state management, HTTP, UI components for all web applications.

**Deploy Now**  
Feature in Forge Launchpad that directly creates AWS resources using SDK instead of requiring manual Terraform/CDK application.

**Forge**  
The complete product: Engine + API + Launchpad.

**Forge.Engine**  
Core .NET library that performs deterministic scaffold generation. No dependencies on Pervaxis.Core or Genesis.

**Forge.Launchpad**  
Angular web UI for configuring and generating prints. Admin-only tool.

**Genesis**  
Pervaxis's AWS provider library suite. 8 modules covering caching, messaging, file storage, search, notifications, workflows, AI, and reporting.

**IaC (Infrastructure-as-Code)**  
Configuration files (Terraform or CDK) that define cloud resources in a declarative, version-controlled way.

**LocalStack**  
Tool that emulates AWS services locally for development and testing. No cloud costs.

**Manifest**  
The `manifest.json` file that describes a service's identity, type, modules, and configuration. Single source of truth for generation.

**MFE (Microfrontend)**  
An Angular feature module that can be developed, deployed, and lazy-loaded independently. Part of a larger Shell application.

**Polyrepo**  
Architecture where each service lives in its own GitHub repository. Forge generates one repo per service.

**Print**  
The complete project scaffold generated by Forge. Includes code, config, docs, CI/CD, and infrastructure templates.

**Scriban**  
Text templating engine used by Forge. Simple syntax, strict mode, fast rendering.

**Shell**  
Angular host application that provides routing, authentication, layout, and loads Microfrontends.

---

## 14. Change Log

| Date | Version | Change | Author |
|---|---|---|---|
| 2026-05-04 | 1.0 | Initial blueprint created | Anand Jayaseelan |
| - | - | - | - |

---

**End of Development Blueprint**

*Pervaxis Forge v1.0 — Clarivex Technologies © 2026*
