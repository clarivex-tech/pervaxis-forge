# 2026-11-20 — Workspace service unit of work
- Isolated branch `feature/api-workspace-service-uow` from fetched `origin/develop` at `c28f3a44f1159d63dd72b0fc09f3d75947b32b88`.
- Changed REST reference task repositories to stage/query only. Services commit once through existing Core `IUnitOfWork`, using the same scoped tenant-aware context; kept V1 session signatures and host registration unchanged.
- Moved concurrency handling to the service commit boundary, preserving owner/tenant predicates, 404/412 behavior and cleanup of failed tracked work, including cancellation.
- Added relational staging, multi-repository atomicity, injected database failure/rollback, cancellation and concurrent deletion tests. Updated portable test fixtures and reference compatibility/orchestration documentation.
- Validation: freshly rendered API Workspace tests 36/36; full `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --verbosity minimal` passed Engine 169/169 and API 95/95, including rendered builds, unchanged Workspace copies into default/minimal hosts, MVC security/concurrency, architecture and offline model/snapshot checks. Behavioral persistence tests use disposable SQLite, not a live PostgreSQL database.
- No schema, applied migration history, generated customer apps, UI, AWS or live database changes. Architect verified; user authorized commit and publication after validation. Fetched develop remained at the verified base; no integration changes required.

# 2026-05-13
- Implemented Phase 1 tasks 3.6 and 3.7 in `Pervaxis.Forge.Engine`.
- Added `PrintGenerator`, `FileGenerator`, `ZipPackager`, expanded `TemplateModelBuilder`, and wired Scriban compatibility for `model.names` plus placeholder secrets.
- Implemented `GenesisModules` and `CanvasModules` lookup/name helpers and added module metadata records.
- Replaced TODO tests with concrete coverage for print generation, ZIP packaging, and module inventories.
- Added a minimal embedded template for stable end-to-end rendering tests.
- Verified with `dotnet test tests\Pervaxis.Forge.Engine.Tests\Pervaxis.Forge.Engine.Tests.csproj -p:UseSharedCompilation=false` and reached 60/60 passing.
