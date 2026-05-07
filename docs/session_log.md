## Session Log

### 2026-05-05

**What we did**
- Reviewed UI blueprint and workspace scaffolds for Launchpad.
- Confirmed contract-first sequencing before component decomposition.
- Captured and refined execution plan in /memories/session/plan.md.
- Aligned framework/tooling baseline to exact versions:
  - Angular: 21.2.9
  - Angular Material: 21.2.9
  - Nx: 22.7.0
  - TypeScript: ~5.9.2
- Validated prior local noise was mostly IDE .vs index artifacts before proceeding.
- Implemented contract-first TypeScript freeze for enrollment:
  - Added shared models in core for cloud provider, enrollment requests, and vertical responses.
  - Added typed `EnrollmentState` with `initialEnrollmentState` and `toEnrollmentRequest` mapper.
  - Implemented `IVerticalApiService` with 5 methods (enroll, list, get, update, validate).
  - Implemented both real `VerticalApiService` and `MockVerticalApiService` with matching contract.
- Ran file-level diagnostics on all changed files with zero errors.
- Implemented Phase 0 foundation wiring:
  - Added app routes for dashboard, enrollment, workspace, generation, and unauthorized pages.
  - Added `AuthService` stub + `forgeAuthGuard` role check and redirect behavior.
  - Added app provider wiring with `VERTICAL_API_SERVICE` token switched by `environment.useMockApi`.
  - Implemented root app shell and route placeholder components for dashboard/workspace/generation/unauthorized.
- Implemented vertical enrollment wizard shell with 5-step flow, reactive forms, state sync, connectivity validation, and enroll submit call.
- Performed guide-compliance refactor on touched Launchpad files:
  - Routes now lazy-load all feature pages via `loadComponent()`.
  - Added `ChangeDetectionStrategy.OnPush` on root and route components.
  - Updated API service to use `inject()` pattern.
  - Upgraded enrollment wizard UI to Angular Material Stepper + Material form controls.
  - Added `sessionStorage` persistence/restore for enrollment state.
- Completed Launchpad workspace scaffolding and dependency setup:
  - Added `package.json`, `nx.json`, `tsconfig.base.json`, and `apps/launchpad/project.json`.
  - Added bootstrap files: `apps/launchpad/src/main.ts`, `apps/launchpad/src/index.html`, `apps/launchpad/src/styles.scss`, and `apps/launchpad/tsconfig.app.json`.
  - Installed Angular/Nx dependencies and added missing `@angular/build` package.
- Resolved strict union typing error in enrollment state sync (`CloudProvider`, `SourceControlPlatform`, `RepoVisibility`).
- Verified build success with `npx nx build launchpad` (development build completed; output in `dist/apps/launchpad`).

**Challenges**
- Nx interactive prompts (analytics/extension recommendations) can interrupt non-interactive runs in this environment; we now use `NX_INTERACTIVE=false` for reliable automation.
- Build currently succeeds but with large bundle sizes for the enrollment lazy chunk, indicating follow-up optimization work is needed.

**Next**
- Run `npx nx serve launchpad` and manually verify the full enrollment wizard path with mock API enabled.
- Add unit tests for enrollment state mapper, masking helpers, and mock API service contract behavior.
- Implement step-level validation UX (touched/submitted error surfacing) and accessibility checks per guide.
- Start Phase 0 quality gate checks: lint, unit tests, and initial Cypress enrollment flow.

---

### 2026-05-07 — BFF: RDS Migration (Partial — ZScaler blocked)

**What we did**
- Confirmed EF Core migration `20260506140655_InitialSchema` exists and is ready to apply.
- Added `SSL Mode=Disable` to `appsettings.Development.json` connection string (ZScaler strips SSL mid-handshake on port 5432).
- TCP connectivity to `forge-dev.cafy4a22q90j.us-east-1.rds.amazonaws.com:5432` succeeded (`TcpTestSucceeded: True`).
- Migration still failed at authentication phase — ZScaler ZPA is intercepting and mangling the Postgres wire protocol even without SSL.

**Challenges**
- ZScaler ZPA blocks Postgres binary protocol on port 5432 from the office network even when SSL is disabled.
- The RDS endpoint resolves to `100.64.1.172` (CGNAT/ZPA range) on the office machine, confirming traffic routes through ZPA.

**What needs to be done on home laptop (no ZScaler)**
1. Pull `feature/api-vertical-enrollment` branch.
2. Ensure `ASPNETCORE_ENVIRONMENT=Development` is set (picks up `appsettings.Development.json`).
3. Run: `dotnet ef database update --project src/Pervaxis.Forge.Api`
4. Verify migration applies cleanly — expect all 6 tables: `verticals`, `vertical_cloud_configs`, `vertical_source_control_configs`, `vertical_tech_defaults`, `generation_logs`, `deployment_outputs`.
5. Run the API: `dotnet run --project src/Pervaxis.Forge.Api` and hit Swagger at `https://localhost:<port>/swagger` to confirm DB connectivity.

**Next (after migration succeeds)**
- Implement `VerticalService` CRUD against real DB (currently stubs).
- Wire `VerticalConnectivityValidator` to call real AWS/GitHub checks.
- Start Phase 1: generation endpoint + Engine integration.

---

### 2026-05-07 — BFF: RDS Migration (Resolved on Home Laptop)

**What we did**
- Switched to home laptop (no ZScaler). DNS now resolves the `forge-dev` RDS endpoint to a real public AWS IP (`18.211.4.220`) instead of the ZPA `100.64.x.x` range.
- Added home laptop public IP `73.197.181.23/32` to the RDS `forge-dev` security group inbound rule (PostgreSQL/TCP/5432).
- Reverted the office-only ZScaler workaround: changed `SSL Mode=Disable` → `SSL Mode=Require;Trust Server Certificate=true` in `appsettings.Development.json` (gitignored, local-only). RDS has `rds.force_ssl=1` and `pg_hba.conf` only allows `hostssl`.
- Applied `20260506140655_InitialSchema` migration cleanly. All 6 tables (`verticals`, `vertical_cloud_configs`, `vertical_source_control_configs`, `vertical_tech_defaults`, `generation_logs`, `deployment_outputs`) and all spec indexes/FKs created. EF migrations history row recorded.

**Next**
- Implement `VerticalService` CRUD against the real DB.
- Wire `VerticalConnectivityValidator` to real AWS/GitHub.
- Replace 11 endpoint `501` stubs with real handlers — May 10 deadline (UI swaps mock for real API in dev).

---

## Template For Next Sessions

### YYYY-MM-DD

**What we did**
- 

**Next**
- 
