# Pervaxis Forge

**Accelerating Innovation Through Automation**  
*Clarivex Technologies — Internal Platform Tool*

---

## What is Forge?

Pervaxis Forge is a project factory. Platform administrators describe what kind of service they need, and Forge generates a complete, production-ready codebase — with all infrastructure, pipelines, documentation, and configuration — in under 2 seconds.

Instead of spending 3–5 days manually setting up a new microservice or web application, teams start building business features on day one.

---

## How It Works

Forge operates in two phases:

### 1. Vertical Enrollment *(one-time per business domain)*
Register a business vertical — such as Clarivolt or ClariFrost — with its cloud account, GitHub organisation, and environment defaults. Done once. Reused forever.

### 2. Service Generation *(everyday use)*
Select an enrolled vertical, describe your services (type, capabilities, database, queues), and Forge generates everything — source code, CI/CD pipelines, infrastructure templates, Docker config, and developer documentation — as a ready-to-run project.

---

## What Gets Generated

For every service, Forge produces:

| Artifact | Description |
|---|---|
| Source code | .NET 10 REST API or Angular 18 Microfrontend, fully wired |
| Tests | xUnit or Karma test projects with 90%+ coverage baseline |
| Docker | Multi-stage `Dockerfile` + `docker-compose` with LocalStack |
| CI/CD | GitHub Actions workflow for build, test, and coverage |
| Infrastructure | Terraform + AWS CDK templates for all selected AWS resources |
| Configuration | `appsettings.json` with all module sections pre-configured |
| Documentation | `README.md`, `SPEC.md`, and a `CLAUDE.md` for AI-assisted development |
| GitHub repo | Created automatically with branch protection and secrets configured |

---

## Key Capabilities

- **Genesis Modules** — 8 pre-integrated AWS capabilities: File Storage, Messaging, Caching, Search, Notifications, Workflows, AI (Bedrock), Reporting
- **Canvas Libraries** — 14 pre-integrated Angular platform libraries: authentication, HTTP, state management, UI components, internationalisation, and more
- **Deploy Now** — optionally creates all AWS resources (RDS, S3, SQS, ElastiCache) immediately using the vertical's enrolled IAM role
- **Batch Generation** — generate 5, 10, or 20 services at once, all correctly configured
- **Multi-Vertical** — manage multiple business domains, each with isolated cloud accounts and GitHub organisations

---

## Technology

| Layer | Technology |
|---|---|
| Generation Engine | .NET 10 + Scriban templates |
| API | ASP.NET Core Minimal API |
| Database | PostgreSQL + Entity Framework Core |
| Cloud | AWS SDK (S3, RDS, SQS, ElastiCache, Secrets Manager) |
| Source Control | GitHub API (Octokit + LibGit2Sharp) |
| Admin UI | Angular 18 + Angular Material + Angular Signals |
| Infrastructure-as-Code | Terraform (HCL) + AWS CDK (C#) |

---

## Current Status

| Team | Phase | Branch | Status |
|---|---|---|---|
| BFF (API + Engine) | Phase 0 — Vertical Enrollment | `feature/api-vertical-enrollment` | 🟢 In progress — models, Swagger, entities, migration done |
| UI (Launchpad) | Phase 0 — Vertical Enrollment | `feature/ui-vertical-enrollment` | 🟢 In progress — enrollment wizard done, serve + tests pending |

**Hard deadline: May 8, 2026** — Swagger JSON contract delivered to UI team.

---

## Repository Structure

This repository contains the planning and documentation for Forge. The two implementation repos are:

| Repo | Description |
|---|---|
| `pervaxis-forge-api` | Engine + API — contained here under `pervaxis-forge-api/` |
| `pervaxis-forge-launchpad` | Angular admin UI — contained here under `pervaxis-forge-launchpad/` |

---

## Documentation

| Document | Audience | Description |
|---|---|---|
| [FORGE_OVERVIEW.md](docs/FORGE_OVERVIEW.md) | All stakeholders | Business value, how it works, FAQ |
| [FORGE_TECHNICAL_SPECIFICATION.md](docs/FORGE_TECHNICAL_SPECIFICATION.md) | Engineers, Architects | System architecture, API reference, component design |
| [FORGE_SOLUTION_STRUCTURE.md](docs/FORGE_SOLUTION_STRUCTURE.md) | All engineers | Repo layout, folder conventions — read on Day 1 |
| [FORGE_BLUEPRINT_BFF.md](docs/FORGE_BLUEPRINT_BFF.md) | Backend team | Engine, API, Vertical Enrollment — 4-week implementation plan |
| [FORGE_BLUEPRINT_UI.md](docs/FORGE_BLUEPRINT_UI.md) | Frontend team | Launchpad UI, Angular templates — 6-week implementation plan |

---

## Project Timeline

| Phase | Dates | Deliverable |
|---|---|---|
| Phase 0 — Vertical Enrollment | May 6–10, 2026 | Enrollment API + wizard (both teams, parallel) |
| Phase 1 — Core Engine | May 6–17, 2026 | Manifest parsing, naming, template engine, ZIP |
| Phase 2 — REST API Templates | May 20–24, 2026 | 18 Scriban templates, generated service compiles |
| Phase 3 — Infrastructure + GitHub | May 27–31, 2026 | AWS deploy, Terraform/CDK, GitHub repo creation |
| Phase 4 — Generation Wizard | May 20–31, 2026 | Full 6-step UI wizard |
| Phase 5 — Angular Templates | June 3–7, 2026 | Shell + MFE scaffold templates |
| Production Ready | June 14, 2026 | All quality gates passed, end-to-end test passing |

---

## Business Impact

- **99.9% reduction** in project setup time — days to seconds
- **100% consistency** — every project follows the same platform standards
- **Zero configuration errors** — automated generation eliminates copy-paste mistakes
- **Immediate productivity** — development teams start writing business logic on day one

---

*Pervaxis Forge v1.0 — Clarivex Technologies © 2026*
