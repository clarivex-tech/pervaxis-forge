# Pervaxis Forge

Internal platform tool for generating production-ready software project scaffolds in seconds.

## Documentation

| Document | Audience | Description |
|---|---|---|
| [FORGE_OVERVIEW.md](docs/FORGE_OVERVIEW.md) | All stakeholders | What Forge is, business value, how it works |
| [FORGE_TECHNICAL_SPECIFICATION.md](docs/FORGE_TECHNICAL_SPECIFICATION.md) | Engineers, Architects | System architecture, component design, API reference |
| [FORGE_SOLUTION_STRUCTURE.md](docs/FORGE_SOLUTION_STRUCTURE.md) | All engineers | Repo layout, folder structure, naming conventions — read on Day 1 |
| [FORGE_BLUEPRINT_BFF.md](docs/FORGE_BLUEPRINT_BFF.md) | Backend team | Engine, API, Vertical Enrollment backend, Infrastructure — implementation plan |
| [FORGE_BLUEPRINT_UI.md](docs/FORGE_BLUEPRINT_UI.md) | Frontend team | Launchpad UI, Angular templates — implementation plan |

## Quick Summary

Forge works in two phases:

1. **Vertical Enrollment** — one-time registration of a business domain (Clarivolt, ClariFrost, etc.) with its cloud account, GitHub org, and environment defaults
2. **Service Generation** — select an enrolled vertical, describe your services, generate complete scaffolds in under 2 seconds

**Tech stack:** .NET 10 · Scriban · ASP.NET Core · Angular 18 · PostgreSQL · AWS SDK · Octokit

**Project start:** May 6, 2026 · **Target completion:** June 18, 2026

---

*Pervaxis Forge — Clarivex Technologies © 2026*
