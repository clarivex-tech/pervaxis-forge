# Pervaxis Forge Production Handoff

**Scope:** Horizontal Forge work only. No vertical/domain service code belongs in this project.
**Reference:** Latest scaffold review and build feedback for `Generated-Services\RestApi\generate-service-scaffold`.
**Branch:** `feature/production-handoff`

---

## Working Rules

- Keep changes limited to Forge/template generation, shared defaults, and scaffold output.
- Do not add or modify vertical IntakeService/domain implementation code here.
- Treat this file as the session checkpoint for production readiness work.

---

## Open Forge Items

### P1 - Build Breaker

1. ~~Fix placeholder DI registration in `rest-api/src/**PROJECT**/Extensions/ServiceCollectionExtensions.cstemplate.sbn`.~~
2. ~~Replace `AddScoped<IService, ConcreteClass>(factory)` with the correct overload when the concrete type is not intended to exist.~~

### P2 - Missing Scaffold Outputs

3. ~~Add `.dockerignore` to generated REST API scaffolds.~~
4. ~~Add `appsettings.Production.json` to generated REST API scaffolds.~~
5. Add generation for the `.slnx` solution file if CI/CD expects it.

### P3 - Template Hardening

6. ~~Update `Dockerfile.sbn` to run as a non-root user.~~
7. ~~Add `HEALTHCHECK` to `Dockerfile.sbn`.~~
8. ~~Wire health checks in `Program.cstemplate.sbn` with `AddHealthChecks()` and `MapHealthChecks("/health")`.~~
9. ~~Pin Genesis package versions in `csproj.sbn` instead of generating floating versions.~~
10. ~~Review the production Swagger guard so it cannot be enabled accidentally in production.~~
11. ~~Add the missing guides library for GraphQL and gRPC if `FORGE_DEVELOPER_GUIDE.md.sbn` references guide files that are not shipped there.~~

---

## Already Fixed

12. `FORGE_DEVELOPER_GUIDE.md.sbn` was added to REST API, GraphQL, and gRPC templates.
13. Error handling for `ListGeneratedServices` and `RegenerateService` endpoints was already committed.

---

## Notes To Carry Forward

- REST API templates include the full `.claude/guides/` directory.
- GraphQL and gRPC templates currently ship only `.claude/CLAUDE.md`, so any generated developer guide references must be backed by actual shipped guides.
- Genesis package version resolution should happen at Forge generation time, not by leaving wildcard versions in generated projects.
- The current work is scaffold/horizontal only; keep all future edits aligned to that boundary.
