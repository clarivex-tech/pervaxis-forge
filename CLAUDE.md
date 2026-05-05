# Pervaxis Forge — Root Repo

## Git Branching Strategy

All work follows this flow — no exceptions:

```
feature/<name>  →  develop  →  main
```

- **`feature/<name>`** — all new work, cut from `develop`
- **`develop`** — integration branch; merge via PR from feature branches
- **`main`** — production-ready only; merge via PR from `develop`

Never merge a feature branch directly to `main`.  
Never commit directly to `develop` or `main`.

## Repository Layout

This repo holds documentation and skeleton scaffolds for Pervaxis Forge.

```
docs/                          Project documentation
  FORGE_OVERVIEW.md            Non-technical overview and business case
  FORGE_TECHNICAL_SPECIFICATION.md  API contracts, data models, DB schema
  FORGE_BLUEPRINT_BFF.md       BFF (backend) phase-by-phase implementation plan
  FORGE_BLUEPRINT_UI.md        UI (frontend) phase-by-phase implementation plan
  FORGE_SOLUTION_STRUCTURE.md  Folder layout and naming conventions
pervaxis-forge-api/            BFF repo skeleton (.NET 10, Clean Architecture)
pervaxis-forge-launchpad/      UI repo skeleton (Angular 18, Nx, Angular Material)
README.md                      Stakeholder-facing project summary
```

Read the docs in this order before making changes:
1. `docs/FORGE_SOLUTION_STRUCTURE.md` — folder layout and naming conventions
2. `docs/FORGE_TECHNICAL_SPECIFICATION.md` — API contracts and data models
3. `docs/FORGE_BLUEPRINT_BFF.md` or `docs/FORGE_BLUEPRINT_UI.md` — depending on which team
