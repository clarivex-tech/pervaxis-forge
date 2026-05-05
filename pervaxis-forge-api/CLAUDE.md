# Pervaxis Forge — API & Engine

## Project Context

Forge API is the backend for the Pervaxis Forge platform — an internal admin tool at Clarivex Technologies for enrolling business verticals and generating production-ready service scaffolds. It consists of two projects:

- `Pervaxis.Forge.Engine` — pure .NET 10 generation library (Scriban templates, naming conventions, ZIP packaging). No external dependencies beyond Scriban.
- `Pervaxis.Forge.Api` — ASP.NET Core Minimal API. Orchestrates vertical enrollment, AWS resource provisioning, GitHub integration, and delegates generation to the Engine.

Before writing any code, read:
- `docs/FORGE_SOLUTION_STRUCTURE.md` — folder layout, naming conventions, project structure
- `docs/FORGE_BLUEPRINT_BFF.md` — phase-by-phase implementation plan
- `docs/FORGE_TECHNICAL_SPECIFICATION.md` — API contracts, data models, DB schema

---

## File Header

Every `.cs` file must start with this exact license header — no exceptions:

```csharp
/*
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */
```

---

## Architecture

### Dependency Rules

```
Engine   → no external dependencies (Scriban only, zero Genesis/Core references)
Api      → references Engine only; no Pervaxis.Core or Pervaxis.Genesis references
Generated code (prints) → can reference Genesis, Canvas, etc.
```

### Key Design Principles

- **Vertical-First** — all generation happens within an enrolled vertical; vertical context (cloud provider, GitHub org, environments) is resolved once at enrollment, never per-generation
- **Cloud-Agnostic Prints** — Genesis module names in manifests are cloud-agnostic (`Caching`, `Messaging`); the vertical's enrolled `CloudProvider` resolves the package suffix (`Pervaxis.Genesis.Caching.AWS`) at generation time via `GenesisModules.GetPackageName(module, cloudProvider)`
- **Deterministic Generation** — same `manifest.json` + same vertical = identical output every time; no AI, no randomness
- **Engine Independence** — `Pervaxis.Forge.Engine` has zero knowledge of HTTP, databases, or cloud SDKs

---

## C# Standards

- `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` are on for all projects
- Use `record` for all DTOs, requests, responses, and value objects
- Use `required` modifier on all mandatory record properties — no nullable workarounds
- `async`/`await` all the way down — no `.Result` or `.Wait()`
- One class per file — filename matches type name exactly
- No `static` classes except for pure transformation utilities (`NamingConvention`, `GenesisModules`)
- XML documentation on all `public` APIs in Engine

---

## guides/ and skills/

The `.claude/guides/` and `.claude/skills/` folders contain development standards and patterns sourced from Genesis. Read them before implementing:

| Guide | When to read |
|---|---|
| `OBSERVABILITY_PATTERN.md` | Before adding any logging, tracing, or metrics |
| `RESILIENCE_PATTERN.md` | Before adding any AWS SDK calls |
| `METRICS_PATTERN.md` | Before adding OpenTelemetry metrics |
| `GENESIS_PROVIDERS.md` | Before wiring any Genesis module into generated templates |
| `CLOUD_PROVIDER_SEPARATION.md` | Before changing how cloud provider is resolved |
| `PERVAXIS_STANDARDS.md` | General platform-wide coding standards |
| `skills/csharp-coding-standards/` | C# coding patterns — read before writing any new class |
| `skills/csharp-api-design/` | API design for NuGet-published or shared interfaces |
| `skills/microsoft-extensions-dependency-injection/` | DI registration patterns |
| `skills/project-structure/` | .NET project structure, Directory.Build.props, .slnx |
