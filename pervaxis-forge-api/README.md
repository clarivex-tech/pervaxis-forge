# pervaxis-forge-api

Backend for Pervaxis Forge — Engine + API.

> **Branch:** `feature/api-vertical-enrollment` | **Phase:** Phase 0 — Vertical Enrollment | **Week:** 1 of 1 (May 6–10, 2026)

---

## Projects

| Project | Description |
|---|---|
| `src/Pervaxis.Forge.Engine` | Pure .NET 10 generation library — Scriban templates, naming conventions, ZIP packaging. Zero external dependencies beyond Scriban. |
| `src/Pervaxis.Forge.Api` | ASP.NET Core Minimal API — vertical enrollment, AWS provisioning, GitHub integration, PostgreSQL persistence. |
| `tests/Pervaxis.Forge.Engine.Tests` | xUnit unit tests for Engine (naming, validation, generation, templating, modules) |
| `tests/Pervaxis.Forge.Api.Tests` | xUnit tests for API (endpoint integration + service unit tests) |

---

## Prerequisites

| Tool | Version |
|---|---|
| .NET SDK | 10.0.203 (pinned in `global.json`) |
| PostgreSQL | 16+ (AWS RDS in all environments — no local Docker) |
| dotnet-ef | `dotnet tool install --global dotnet-ef --prerelease` |

---

## Build & Test

```bash
# Restore + build all 4 projects
dotnet restore
dotnet build

# Run all non-integration tests (no DB required)
dotnet test --filter "Category!=Integration"

# Run everything (requires RDS connection in ConnectionStrings:ForgeDb)
dotnet test
```

---

## Run Locally

1. Set your PostgreSQL connection string:

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "ForgeDb": "Host=<your-rds-host>;Database=forge_dev;Username=postgres;Password=<password>"
  }
}
```

2. Apply the EF migration (once RDS is provisioned):

```bash
dotnet ef database update --project src/Pervaxis.Forge.Api
```

3. Run the API:

```bash
dotnet run --project src/Pervaxis.Forge.Api
```

4. Open Swagger UI at `https://localhost:5001/swagger` (dev only).

---

## Swagger in Non-Dev Environments

Swagger UI is off by default in all non-dev environments. To enable it without a code deploy:

```bash
# Set environment variable before running
Forge__EnableSwagger=true dotnet run --project src/Pervaxis.Forge.Api
```

Or in `appsettings.json`:
```json
{ "Forge": { "EnableSwagger": true } }
```

---

## EF Migrations

```bash
# Generate a new migration (do not apply until RDS is ready)
dotnet ef migrations add <MigrationName> \
  --project src/Pervaxis.Forge.Api \
  --output-dir Data/Migrations

# Apply to database (requires live RDS connection)
dotnet ef database update --project src/Pervaxis.Forge.Api
```

Current migration: `20260506140655_InitialSchema` — **not applied** (RDS provisioning is Day 2).

---

## Key Decisions

| Topic | Decision |
|---|---|
| Local emulation | None — real AWS only. No Docker, LocalStack, or Testcontainers. |
| Swagger | Swashbuckle 7.x — dev-only by default, opt-in via `Forge:EnableSwagger` |
| Auth | Deferred — Phase 0 endpoints unauthenticated. `forge-admin` role enforcement is post-Phase-0. |
| Data Protection keys | `%LOCALAPPDATA%\Pervaxis.Forge\keys` in dev. S3/Secrets Manager in prod (Phase 3). |
| Test runner | xUnit 2.9.3 + xunit.runner.visualstudio 3.1.5 + Microsoft.NET.Test.Sdk 17.14.1 (pinned — nightly versions break .NET 10 testhost) |

---

## Before Writing Code

Read in this order:
1. `../CLAUDE.md` — branching strategy
2. `CLAUDE.md` (this folder) — BFF-specific rules, file header, dependency rules
3. `../docs/FORGE_SOLUTION_STRUCTURE.md` — folder layout
4. `../docs/FORGE_TECHNICAL_SPECIFICATION.md` — API contracts, DB schema
5. `../docs/FORGE_BLUEPRINT_BFF.md` — phase plan

---

*Pervaxis Forge v1.0 — Clarivex Technologies © 2026*
