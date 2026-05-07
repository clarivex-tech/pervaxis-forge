# Pervaxis Forge BFF — Session Log

> Engineer-facing log of work, decisions, blockers, and todos for the Forge BFF (`Pervaxis.Forge.Api` + `Pervaxis.Forge.Engine`). Committed to the repo so any machine pulling the branch has full context.
>
> **Convention:** Latest entry at the top. Each entry is self-contained — a fresh Claude Code session on a new machine should be able to read the topmost entry and continue without backfill.
>
> **Companion docs:** Project narrative lives in `docs/session_log.md`. Authoritative standards in `pervaxis-forge-api/CLAUDE.md` and `docs/FORGE_*` files.

---

## How to bootstrap a fresh session — READ THIS FIRST

> **For a new Claude CLI session on any machine:** read this section before doing anything else. The user's auto-memory under `C:\Users\anand\.claude\projects\…\memory\` is **machine-local** and may not exist on this machine — **this file is the cross-machine source of truth.** All paths below are relative to the repo root (`pervaxis-forge/`) unless stated otherwise.

### Step 1 — read the topmost dated entry of this file

`pervaxis-forge-api/.claude/memory/session_log.md` (you're in it). The most recent dated entry is self-contained: current branch, current phase, what was just done, what's next, open blockers. Start there.

### Step 2 — read the authoritative repo docs, in this order

| Order | Path | What it gives you |
|---|---|---|
| 1 | `CLAUDE.md` | Repo-level branching strategy and reading order |
| 2 | `pervaxis-forge-api/CLAUDE.md` | BFF-specific rules: file header, dependency rules, C# standards |
| 3 | `docs/FORGE_SOLUTION_STRUCTURE.md` | Folder layout, naming, project structure |
| 4 | `docs/FORGE_TECHNICAL_SPECIFICATION.md` | API contracts, data models, DB schema |
| 5 | `docs/FORGE_BLUEPRINT_BFF.md` | Phase-by-phase implementation plan and current-week deadlines |
| 6 | `docs/FORGE_OVERVIEW.md` | Business context (skim only if needed) |

### Step 3 — read `.claude/` guides — but with skepticism

Located under `pervaxis-forge-api/.claude/guides/`. **Several were sourced from Pervaxis Genesis and describe code that Forge *generates*, not Forge itself.** Apply only the ones marked authoritative.

| File | When to read | Authoritative for *Forge* code? |
|---|---|---|
| `PERVAXIS_STANDARDS.md` | Before any new C# class | ✅ Yes |
| `DEPENDENCY_MANAGEMENT.md` | Before adding/changing NuGet packages | ✅ Yes |
| `GIT_WORKFLOW.md` | Before branching, committing, or PRing | ✅ Yes |
| `ci-sonarcloud-setup.md` | Before touching CI/CD | ✅ Yes |
| `GENESIS_PROVIDERS.md` | Before changing template logic that wires Genesis modules | ⚠️ Describes generated code; useful for template authoring only |
| `CLOUD_PROVIDER_SEPARATION.md` | Before changing cloud-provider resolution in templates | ⚠️ Describes generated code; useful for template authoring only |
| `OBSERVABILITY_PATTERN.md` | Skip on bootstrap | ❌ Generated-code pattern, not Forge itself |
| `RESILIENCE_PATTERN.md` | Skip on bootstrap | ❌ Generated-code pattern, not Forge itself |
| `METRICS_PATTERN.md` | Skip on bootstrap | ❌ Generated-code pattern, not Forge itself |
| `CORE_ABSTRACTIONS_COMPLIANCE.md` | Skip on bootstrap | ❌ Genesis-only, no Forge equivalent |

### Step 4 — read `.claude/` skills relevant to your current task

Located under `pervaxis-forge-api/.claude/skills/`. Read the matching skill **before** touching the matching code:

| Skill folder | Read before… |
|---|---|
| `csharp-coding-standards/` | Writing any new C# class |
| `csharp-api-design/` | Changing Engine's public API or any cross-project interface |
| `microsoft-extensions-dependency-injection/` | Editing `Program.cs` DI registrations or adding `IServiceCollection` extensions |
| `project-structure/` | Editing `.slnx`, `.csproj`, `Directory.Build.props`, `global.json`, or central package management |

### Step 5 — when guides and CLAUDE.md disagree, CLAUDE.md wins

Known conflicts the guides will pull you toward — use the CLAUDE.md / Forge-spec answer instead:
- Use the **multi-line Clarivex license header** (not the one-liner from some Genesis-sourced guides).
- `LangVersion=latest` (not `preview`).
- Test stack is **xUnit + FluentAssertions + Moq** (not NSubstitute).

### Active conventions to carry into every session

- **No local emulation** — real AWS resources only. No Docker, LocalStack, or Testcontainers in dev or CI.
- **Forge ≠ Genesis** — Forge has zero references to `Pervaxis.Core` or `Pervaxis.Genesis`.
- **Discuss before coding** — for non-trivial work, propose a plan and get explicit go-ahead before changing code.
- **One question at a time** — surface clarifications individually, never as bundled lists.
- **CI/CD from Day 1** — GitHub Actions + SonarCloud (org `clarivex-tech`, project `clarivex-tech_pervaxis-forge`; `SONAR_TOKEN` lives in GitHub repo secrets, configured by Anand).
- **Append, don't rewrite** — add a new dated entry at the top of this file; do not edit prior entries except for typo fixes.

### Model tier strategy — conserve usage for the full 4-week build

Spawn a **Haiku** subagent for all implementation work. Keep planning, design, and review on **Sonnet**. Use **Opus** only for genuinely hard architectural decisions.

| Tier | Model ID | Use for |
|---|---|---|
| Haiku | `claude-haiku-4-5-20251001` | Writing C# files, tests, any file edits |
| Sonnet | `claude-sonnet-4-6` | Planning, design calls, session log, PR review |
| Opus | `claude-opus-4-7` | Complex architecture or hard bug diagnosis only |

**How:** `Agent(model="haiku", prompt="...self-contained brief...")`. Never write code directly in the Sonnet conversation unless it's a single-line fix.

### Terse mode

Start every session with **terse mode on** (`/clear` then type "terse mode" or just keep responses short). Anand reads diffs — don't narrate what you just did. One sentence per update maximum.

### What you do NOT need to read on bootstrap

- The user's auto-memory under `C:\Users\anand\.claude\…` — machine-local, may not exist here. Anything load-bearing has been promoted to this log.
- Anything under `pervaxis-forge-launchpad/` — that's the UI repo's concern, separate team.
- Build/IDE artifacts: `bin/`, `obj/`, `.vs/`, `.idea/`, `*.user`.

---

## 2026-05-07 — Phase 0 Day 2 Session 4: ComponentPrefix, NamingConvention redesign, Task A/B merge

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude Sonnet as lead, Haiku for implementation)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6–10)
**Machine:** Home laptop (no ZScaler).

### What was done this session

1. **Reviewed and merged Codex Task A** (`feature/codex-bff`) — server-side `VerticalEnrollmentRequest` validation. Approved with one noted gap (trailing hyphen slug test — assigned to Task C). Merged into `feature/api-vertical-enrollment`.

2. **Reviewed Codex Task B** (`feature/engine-naming-convention`) — `NamingConvention` helpers. Identified design flaw: `GetComponentPrefix("clarivolt") → "clv"` contradicts the "first 3 chars" rule, and any algorithm would collide for Clarivolt/Clarifrost (both start `"clar"`).

3. **Redesigned `GetComponentPrefix`** — changed from a derivation algorithm to a **registered-abbreviation normaliser**: caller supplies a pre-registered 2–5-letter abbreviation; function validates (letters only, 2–5 chars) and lowercases. Uniqueness is the caller's responsibility at enrolment time. Updated implementation + tests. Merged Task B.

4. **Added `ComponentPrefix` to the full enrollment stack** (Haiku):
   - `VerticalEnrollmentRequest` — new `required string ComponentPrefix`
   - `Vertical` entity — new `required string ComponentPrefix`
   - `ForgeDbContext` — `HasMaxLength(5).IsRequired()`
   - `VerticalResponse` + `VerticalSummaryResponse` — new `required string ComponentPrefix`
   - `VerticalService.EnrollAsync` — calls `NamingConvention.GetComponentPrefix(request.ComponentPrefix)` to normalise before storing
   - `VerticalService.MapToResponse` — projects `ComponentPrefix`
   - `VerticalRequestValidator` — `ValidateComponentPrefix` via `NamingConvention.GetComponentPrefix` (catches `ArgumentException` → `ValidationFailure`)
   - EF migration `20260507173119_AddComponentPrefix` generated (NOT applied — apply manually against forge-dev)
   - Tests updated; 6 new `ComponentPrefix` validator test cases added

5. **Assigned Codex Task C** — brief is at the top of this log. Branch: `feature/api-task-c`.

6. **Session conventions** added to this log and `docs/FORGE_BLUEPRINT_BFF.md`: model-tier strategy (Haiku/Sonnet/Opus) and terse mode.

### End-of-session state

- **Build:** 4/4 projects, 0 warnings, 0 errors.
- **Unit tests (CI gate):** 95 passing — 31 Engine, 64 API.
- **Migration:** `20260507173119_AddComponentPrefix` generated, **not applied** — run `dotnet ef database update` from home laptop with RDS connection string set.

### Deferred to next session

- [ ] **Apply `AddComponentPrefix` migration** to `forge-dev` RDS (`dotnet ef database update --project src/Pervaxis.Forge.Api`). Requires home laptop + `appsettings.Development.json` with RDS connection string.
- [ ] **Re-export `openapi.json`** — DTOs changed (ComponentPrefix added). Boot API on `localhost:5500`, `Invoke-WebRequest /swagger/v1/swagger.json`, save to `pervaxis-forge-api/contract/openapi.json`.
- [ ] **Codex Task C** — `feature/api-task-c`: trailing hyphen test, `.gitignore` hygiene, `UpdateVerticalRequest` validation.
- [ ] **`VerticalConnectivityValidator`** — STS AssumeRole + Octokit org check. Wire `POST /api/v1/verticals/{slug}/validate` (slug-only, validates stored creds). `AWSSDK.SecurityToken` and `Octokit` already in csproj.
- [ ] **SonarCloud bootstrap** — waiting on `SONAR_TOKEN` from Anand.

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom and the bootstrap section at the top of this file.
3. `dotnet build && dotnet test --filter "Category!=Integration"` — should be 95 green.
4. Apply the migration if on home laptop with RDS access.
5. Pick up from Deferred list above.

---

## 2026-05-07 — Codex: Task C — validation fixes + gitignore hygiene + UpdateVerticalRequest validation

> **For the Codex CLI agent picking this up:** read the bootstrap section at the top of this file, then do exactly the three items below on branch `feature/api-task-c` cut from `feature/api-vertical-enrollment`. Open one PR targeting `feature/api-vertical-enrollment`. Do not merge.

**Branch:** `feature/api-task-c` (cut from `feature/api-vertical-enrollment`)

### Item 1 — Trailing hyphen slug test (1 line)

In `tests/Pervaxis.Forge.Api.Tests/Services/VerticalRequestValidatorTests.cs`, add one `[InlineData]` to `ValidateSlug_Rules_AreEnforced`:

```csharp
[InlineData("slug-", false)]   // trailing hyphen
```

### Item 2 — `.gitignore` hygiene

In `pervaxis-forge-api/.gitignore`, add entries to prevent stray files from polluting future commits:

```
# IDE / launch profiles
**/Properties/launchSettings.json

# Local Claude settings artefacts
docs/.claude/
.claude/settings.local.json.bak
```

### Item 3 — `UpdateVerticalRequest` server-side validation

Same pattern as `VerticalRequestValidator.Validate(VerticalEnrollmentRequest)` — add a second overload:

```csharp
public static IReadOnlyList<ValidationFailure> Validate(UpdateVerticalRequest request);
```

Rules (from `docs/FORGE_TECHNICAL_SPECIFICATION.md` §3.2 — apply only the fields that exist on `UpdateVerticalRequest`):

| Field | Rule |
|---|---|
| `DisplayName` | Trimmed length 1–255 (if provided / non-null) |
| `OwnerTeam` | Trimmed length 1–255 (if provided / non-null) |
| `OwnerEmail` | Regex `^\S+@\S+\.\S+$`, length 1–255 (if provided / non-null) |
| `SourceControl.AccessToken` | Trimmed length 1–512 (if provided / non-null) |
| `TechDefaults.Environments` | Non-empty, each kebab-case, each unique (if provided / non-null) |
| `TechDefaults.DefaultEnvironment` | Must be one of `Environments` (if `Environments` provided) |
| `TechDefaults.DefaultDbEngine` | Null OR one of `"postgresql"`, `"mysql"`, `"none"` (if provided) |

Wire it in `VerticalService.UpdateAsync` before any DB call (same pattern as `EnrollAsync`). Endpoint already catches `ValidationException` — no endpoint change needed.

Add `~10` tests in a new `[Theory]` block in `VerticalRequestValidatorTests.cs`.

### Acceptance criteria

- Build green: 4/4, 0 warnings, 0 errors.
- All existing tests still pass + new validator tests pass.
- `dotnet build pervaxis-forge-api/Pervaxis.Forge.slnx` and `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category!=Integration"` both exit 0.
- Append a session log entry (branch, what changed, test counts).

All hard guardrails apply: Clarivex license header on any new `.cs` file, Forge ≠ Genesis, xUnit + FluentAssertions + Moq, no `.Result`/`.Wait()`.

---

## 2026-05-07 — Codex: Task B + Task A review fixes

> **For the Codex CLI agent picking this up:** read the bootstrap section at the top of this file, then complete the two items below. Push to the branch listed and open a PR targeting `feature/api-vertical-enrollment`. Do not merge.

### Item 1 — Fix Task A review gaps (branch: `feature/codex-bff`)

In `tests/Pervaxis.Forge.Api.Tests/Services/VerticalRequestValidatorTests.cs`, add the missing test case to `ValidateSlug_Rules_AreEnforced`:

```csharp
[InlineData("slug-", false)]   // trailing hyphen
```

Then run `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category!=Integration"` — must stay green.

Amend or add a commit on `feature/codex-bff` and push. Do **not** open a new PR — the existing one covers this fix.

### Item 2 — Task B: NamingConvention helpers (branch: `feature/engine-naming-convention`, cut from `feature/api-vertical-enrollment`)

Full spec is in the entry **"2026-05-07 — Codex CLI (gpt-mini) handoff: 2 narrow tasks delegated"** → **Task B** section below. Summary:

- Edit `src/Pervaxis.Forge.Engine/Naming/NamingConvention.cs` — implement the four functions verbatim.
- Add `tests/Pervaxis.Forge.Engine.Tests/Naming/NamingConventionTests.cs` — one `[Theory]` per function, ≥5 cases each.
- No new NuGet packages in Engine.
- Build green, Engine test count goes from 1 to ~20+.
- Append a session log entry with: branch, what you did, build/test counts, open questions.

All hard guardrails from the handoff entry apply (license header, Forge ≠ Genesis, xUnit + FluentAssertions + Moq, no `.Result`/`.Wait()`).

---

## 2026-05-07 — Codex implemented

> **Branch:** `feature/codex-bff`
> **Scope:** Task A from the Codex handoff only — server-side `VerticalEnrollmentRequest` validation, service enforcement, endpoint mapping, and unit tests.

### What I changed

1. Added `ValidationFailure` and `ValidationException` under `src/Pervaxis.Forge.Api/Services/`.
2. Added `VerticalRequestValidator.Validate(VerticalEnrollmentRequest)` with the requested field rules for slug, owner metadata, cloud provider, source control, and tech defaults.
3. Updated `VerticalService.EnrollAsync` to validate before the slug existence check and throw `ValidationException` when the payload is malformed.
4. Updated `VerticalEndpoints.EnrollVertical` to translate `ValidationException` into `400 ValidationProblemDetails`.
5. Added `VerticalRequestValidatorTests` covering the validation rules with FluentAssertions.

### Verification

- `dotnet build pervaxis-forge-api/Pervaxis.Forge.slnx -p:UseSharedCompilation=false`
- `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category!=Integration" -p:UseSharedCompilation=false`
- Result: build green, tests green, `0` warnings, `0` errors, `58` passing tests in the API test project plus `1` passing Engine test.

### Notes

- I did not add validation to `UpdateVerticalRequest`; that remains a follow-up, per the handoff.
- Missing `required` fields still surface as framework `JsonException` handling rather than a custom validation response. That was left untouched on purpose.

## 2026-05-07 — Codex CLI (gpt-mini) handoff: 2 narrow tasks delegated

> **For the Codex CLI agent picking this up:** read this entire entry top-to-bottom, then read the bootstrap section at the very top of this file (`How to bootstrap a fresh session`), then do exactly the tasks below — nothing more. Push to the branch listed for each task and stop. Anand or Claude (Opus) will review and merge.

**Why split work this way:** Claude (Opus) is implementing the medium- and high-risk Phase 0 / Phase 1 items (connectivity validator, auth, deploy pipeline, Engine generation core). Codex CLI gets two small, fully spec'd tasks where the design is unambiguous and easy to review. **Do not extend scope.** If you discover ambiguity, stop and write a question into the session log under your task; do not invent an answer.

### Hard guardrails — apply to every change you make

1. **Read first:** `pervaxis-forge-api/CLAUDE.md` (file header, dependency rules, C# standards) and the spec sections referenced per task.
2. **Multi-line Clarivex license header** on every new `.cs` file. Copy the exact block from any existing file in `src/Pervaxis.Forge.Api/Services/`. Not the one-liner from Genesis-sourced guides.
3. **Forge ≠ Genesis** — no references to `Pervaxis.Core` or `Pervaxis.Genesis`. Don't add NuGet packages with those prefixes. If you think you need one, you're solving the wrong problem.
4. **Don't refactor or "clean up" surrounding code** — touch only what the task requires.
5. **C# standards (already enforced via `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`):** `record` for DTOs/value objects, `class` for EF entities, `required` modifier on mandatory props, `async`/`await` all the way down (no `.Result` / `.Wait()`), one type per file, file name matches type.
6. **Test stack is xUnit + FluentAssertions + Moq** (not NSubstitute, not the in-memory Pervaxis test helpers).
7. **No local emulation** — no Docker, no Testcontainers, no LocalStack. EF in-memory provider is also out for this work; if a unit test needs a DB, mock at the service boundary instead.
8. **Run before reporting done:** `dotnet build pervaxis-forge-api/Pervaxis.Forge.slnx` and `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category!=Integration"` — both must be 0 errors / 0 failures.
9. **Branch + PR:** push to the task's named branch off `feature/api-vertical-enrollment` (so it stacks on the latest BFF work). Open a PR targeting `feature/api-vertical-enrollment`. Do not merge.
10. **Append a session log entry** under your branch with: what you did, what tests you added, build/test counts, any open questions. Convention: new dated entry at the top of `pervaxis-forge-api/.claude/memory/session_log.md`. Don't edit prior entries.

---

### Task A — Server-side enrollment input validation

**Branch:** `feature/api-input-validation` (cut from `feature/api-vertical-enrollment`)

**Goal:** Reject malformed `VerticalEnrollmentRequest` payloads with `400 ValidationProblemDetails` before they hit the DB. Today the BFF trusts UI-side validation; mismatches surface as opaque 500s. This is defense-in-depth.

**Spec references:** `docs/FORGE_TECHNICAL_SPECIFICATION.md` §3.2 (field rules) and §3.5 (request DTOs). Don't invent new rules — use exactly the rules below.

**Validation rules to implement:**
| Field | Rule |
|---|---|
| `Slug` | Regex `^[a-z][a-z0-9-]*$`, length 1–100, no consecutive hyphens (`--`), no trailing hyphen |
| `DisplayName` | Trimmed length 1–255 |
| `OwnerTeam` | Trimmed length 1–255 |
| `OwnerEmail` | Regex `^\S+@\S+\.\S+$`, length 1–255 |
| `CloudProvider.Provider` | Must equal `"AWS"` (Phase 0 only supports AWS) |
| `CloudProvider.AwsAccountId` | Regex `^\d{12}$` |
| `CloudProvider.IamRoleArn` | Starts with `arn:aws:iam::`, contains `:role/`, length ≤ 2048 |
| `CloudProvider.DefaultRegion` | Regex `^[a-z]{2}-[a-z]+-\d$` (e.g., `us-east-1`) |
| `SourceControl.Platform` | Must equal `"GitHub"` |
| `SourceControl.GitHubOrg` | Trimmed length 1–255, no whitespace |
| `SourceControl.AccessToken` | Trimmed length 1–512 |
| `SourceControl.DefaultVisibility` | One of `"Private"`, `"Public"` |
| `TechDefaults.Environments` | Non-empty, each entry kebab-case, each unique |
| `TechDefaults.DefaultEnvironment` | Must be one of the entries in `Environments` |
| `TechDefaults.DefaultDbEngine` | Null OR one of `"postgresql"`, `"mysql"`, `"none"` |

**Implementation outline:**

1. New file `src/Pervaxis.Forge.Api/Services/VerticalRequestValidator.cs` — `public static class VerticalRequestValidator` with one method:
   ```csharp
   public static IReadOnlyList<ValidationFailure> Validate(VerticalEnrollmentRequest request);
   ```
   `ValidationFailure` is a new `public sealed record ValidationFailure(string Field, string Message)` — one per file, in the same `Services/` folder.

2. New file `src/Pervaxis.Forge.Api/Services/ValidationException.cs` — mirrors `SlugConflictException`:
   ```csharp
   public sealed class ValidationException(IReadOnlyList<ValidationFailure> failures)
       : Exception("Request validation failed.")
   {
       public IReadOnlyList<ValidationFailure> Failures { get; } = failures;
   }
   ```

3. In `VerticalService.EnrollAsync`, **before** the slug-existence pre-check, call:
   ```csharp
   var failures = VerticalRequestValidator.Validate(request);
   if (failures.Count > 0) throw new ValidationException(failures);
   ```

4. In `VerticalEndpoints.EnrollVertical`, add a `catch (ValidationException ex)` arm that returns `Results.ValidationProblem(...)`:
   ```csharp
   var errors = ex.Failures.GroupBy(f => f.Field)
       .ToDictionary(g => g.Key, g => g.Select(f => f.Message).ToArray());
   return Results.ValidationProblem(errors);
   ```

5. **Do not** add validation to `UpdateVerticalRequest` in this task — keep scope tight. (If you have time, note in the session log that update-validation is a follow-up.)

**Tests to add** (under `tests/Pervaxis.Forge.Api.Tests/Services/`):

- New file `VerticalRequestValidatorTests.cs` — one `[Theory]` per rule with valid + invalid examples. Use `FluentAssertions`. Aim for ~15 tests total. Pure function — no DB, no mocks needed.
- Optionally extend `VerticalServiceTests.cs` with one test confirming `EnrollAsync` throws `ValidationException` when given a bad slug. **Don't** add a real-DB integration test for validation — pure unit tests are enough.

**Acceptance criteria:**
- Build green: 4/4 projects, 0 warnings, 0 errors.
- Unit tests green (filter `Category!=Integration`): all existing tests still pass + your new validator tests pass.
- A POST to `/api/v1/verticals` with `{"slug": "Bad_Slug", ...}` returns `400` with a JSON body containing `{ "errors": { "Slug": ["..."] } }`.

**Open question for you to surface in the log if it bites:** the request DTOs use `required` modifiers, so missing fields produce a `JsonException` from the framework, not a friendly 400. Don't try to fix that in this task — just note it in the session log.

---

### Task B — `NamingConvention` pure helpers in `Pervaxis.Forge.Engine`

**Branch:** `feature/engine-naming-convention` (cut from `feature/api-vertical-enrollment`)

**Goal:** Implement the four pure transformation functions per `docs/FORGE_TECHNICAL_SPECIFICATION.md` §10.1. These are foundational for Phase 1 generation. Pure functions, no I/O, trivially testable.

**File to edit:** `src/Pervaxis.Forge.Engine/Naming/NamingConvention.cs` (currently a minimal stub created on Day 1 — replace its body, keep the multi-line license header and namespace).

**Functions to implement (signatures verbatim from §10.1):**

```csharp
public static string ToPascalCase(string kebab);        // "intake-service" → "IntakeService"
public static string StripServiceSuffix(string name);   // "intake-service" → "intake"
public static string GetFirstSegment(string name);      // "intake-service" → "intake"
public static string GetComponentPrefix(string product);// "clarivolt"      → "clv"
```

**Behavior contracts beyond the spec one-liners:**
- `ToPascalCase`: throws `ArgumentException` on null/empty/whitespace. Handles single-segment input (`"intake"` → `"Intake"`). Preserves digits.
- `StripServiceSuffix`: case-insensitive match on `-service`, but only when it's a true suffix (don't strip `"my-service-account"`). Returns the input unchanged if no suffix.
- `GetFirstSegment`: returns the input unchanged if no hyphen present.
- `GetComponentPrefix`: lowercases first; returns the full string when length < 3; otherwise first 3 chars.

**Tests to add** (under `tests/Pervaxis.Forge.Engine.Tests/Naming/`):

- New file `NamingConventionTests.cs`. One `[Theory]` per function with at least 5 cases each (happy path + edges: empty, single segment, mixed case, digits, very short input).
- The existing smoke test `NamingConvention_ClassExists_InExpectedNamespace` can stay or be deleted — your call.

**Acceptance criteria:**
- Build green.
- Engine test count goes from 1 to ~20+. All pass.
- No new dependencies in `Pervaxis.Forge.Engine.csproj` — Engine stays Scriban-only.

---

### Review process

1. Push your branch.
2. Open a PR targeting `feature/api-vertical-enrollment` (not `develop`, not `main`).
3. Mention the branch + PR in your session log entry.
4. Anand pings Claude (Opus) for review. Claude reads the diff, runs tests locally, and either approves + merges or requests changes.
5. **Don't** open PRs for both tasks in one branch — one branch = one PR = one task.

---

## 2026-05-07 — Phase 0 Day 2 Session 3: UI handoff package (CORS + Swagger snapshot + HANDOFF.md)

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude as implementing engineer)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6–10)
**Machine:** Home laptop (no ZScaler).

### What was done this session

1. **CORS** wired in `Program.cs`. Named policy `ForgeUi`. Allowed origins read from `Forge:AllowedOrigins` in `appsettings.json` (default `["http://localhost:4200"]`). `UseCors` is in the pipeline between `UseHttpsRedirection` and the endpoint maps. `AllowAnyHeader` + `AllowAnyMethod`; **no** `AllowCredentials` because Phase 0 has no auth and we don't want to commit to credentialed CORS prematurely.

2. **OpenAPI snapshot committed** at `pervaxis-forge-api/contract/openapi.json` (~26 KB, OpenAPI 3.0.4). Captured by booting the API on HTTP `localhost:5500` (HTTPS redirect skipped silently when no HTTPS port is configured) and `Invoke-WebRequest`-ing `/swagger/v1/swagger.json`. PowerShell 5.1's `Out-File -Encoding utf8` writes BOM, so the file was re-saved via `[System.IO.File]::WriteAllText` with a no-BOM `UTF8Encoding(false)` to keep strict JSON parsers happy. All 7 paths captured: `/api/v1/verticals` (POST, GET), `/api/v1/verticals/{slug}` (GET, PUT, DELETE), `/api/v1/verticals/{slug}/validate` (POST), `/api/v1/generate` (POST), `/api/v1/generate/batch` (POST), `/api/v1/modules` (GET), `/api/v1/canvas-modules` (GET).

3. **`pervaxis-forge-api/HANDOFF.md`** — UI-facing handover doc. Sections: where the contract lives, real-vs-stubbed table, how to run locally (with the ZScaler caveat), CORS, sample request/response payloads for the 5 wired endpoints, behavior gotchas (slug immutable, encrypted fields write-only, soft-delete is invisible to subsequent reads, `enrolledAt` is creation not update, `/validate` URL slug oddity), what's out of scope (auth, validator, generation endpoints, full server-side validation), and a May 10 mock-to-real swap checklist.

### Deferred from this session (handed back to backlog)

- **VerticalConnectivityValidator** (STS AssumeRole + Octokit org check). The packages are already in `Pervaxis.Forge.Api.csproj` (`AWSSDK.SecurityToken`, `Octokit`); just needs `AWSSDK.Extensions.NETCore.Setup` plus the implementation. UI was told to skip / stub the wizard's Validate step until this lands.
- **Server-side input validation** for `VerticalEnrollmentRequest` (slug regex, AWS account 12-digit, ARN shape, email shape). BFF currently trusts UI-side validation; mismatches surface as 500s. On the backlog as defense-in-depth.

### End-of-session state

- **Build:** 4/4 projects, 0 warnings, 0 errors.
- **Unit tests (CI gate):** 4/4 still green (no test changes this session).
- **Files added:**
  - `pervaxis-forge-api/contract/openapi.json` — committed Swagger snapshot.
  - `pervaxis-forge-api/HANDOFF.md` — UI-facing handoff doc.
- **Files changed:**
  - `pervaxis-forge-api/src/Pervaxis.Forge.Api/Program.cs` — CORS registration + middleware.
  - `pervaxis-forge-api/src/Pervaxis.Forge.Api/appsettings.json` — `Forge:AllowedOrigins` default.

### Things to remember for the office laptop

- Nothing on the office laptop changed. CORS works regardless of network. Swagger snapshot is in the repo, so the UI team can read the contract without booting the BFF — useful when corporate-network laptops can't reach `forge-dev`.

### Next up

- [ ] **Implement `VerticalConnectivityValidator`** + wire `/api/v1/verticals/{slug}/validate` (next session). Decide URL/body relationship — recommendation: ignore the URL slug, validate the body's credentials. Re-export `openapi.json` after.
- [ ] **Server-side enrollment input validation** — slug `^[a-z][a-z0-9-]*$`, AWS account 12-digit, ARN format, email shape. Throw a typed `ValidationException`; endpoint maps to `400 ValidationProblemDetails`.
- [ ] **May 10 (Sunday)** — UI swaps mock for real API in dev. Handoff doc has the checklist.
- [ ] SonarCloud bootstrap when `SONAR_TOKEN` lands.
- [ ] **Re-export `openapi.json`** every time an endpoint or DTO changes (this should become a pre-commit hook or a CI step at some point).

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom and the bootstrap section at the top of this file.
3. `dotnet build && dotnet test --filter "Category!=Integration"` — should be 4/4 green.
4. Pick up from "Next up" above. The validator is the obvious next chunk.

---

## 2026-05-07 — Phase 0 Day 2 Session 2: VerticalService implemented + tested against real RDS

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude as implementing engineer)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6–10)
**Machine:** Home laptop (no ZScaler).

### What was done this session

1. **Implemented `IVerticalService` + `VerticalService`** (`src/Pervaxis.Forge.Api/Services/`):
   - Methods: `EnrollAsync`, `ListAsync`, `GetAsync`, `UpdateAsync`, `UnenrollAsync` (soft-delete via `IsActive=false`).
   - Encryption is transparent — the service deals in plaintext; `EncryptedStringConverter` in `ForgeDbContext` handles `IamRoleArn` and `AccessToken` at the EF write/read boundary. The service does **not** call `IDataProtector` itself.
   - `EnrollAsync` does a pre-check (`AnyAsync` on slug) for a clean 409 path, with a fallback `PostgresException(23505)` → `SlugConflictException` translation in case of races.
   - `ListAsync` uses `AsNoTracking()` projections — `ServiceCount` materializes via correlated subquery on `GenerationLogs.Sum(g => g.ServiceCount)`, no `Include` needed.
   - `MapToResponse` is `public static` so it's testable as a pure function.

2. **`SlugConflictException`** — a sealed `Exception` subclass with the offending slug as a property. Endpoint catches it and returns 409 ProblemDetails.

3. **Wired 5 of 6 vertical endpoints** to the service (`Endpoints/VerticalEndpoints.cs`):
   - `POST /api/v1/verticals` → `EnrollAsync` → 201 Created (or 409 on slug conflict)
   - `GET /api/v1/verticals` → `ListAsync` → 200
   - `GET /api/v1/verticals/{slug}` → `GetAsync` → 200 or 404
   - `PUT /api/v1/verticals/{slug}` → `UpdateAsync` → 200 or 404
   - `DELETE /api/v1/verticals/{slug}` → `UnenrollAsync` → 204 or 404
   - `POST /api/v1/verticals/{slug}/validate` — **left as 501**. That's the `IVerticalConnectivityValidator` task (STS AssumeRole + GitHub org check), separate.

4. **Registered service in DI** — `builder.Services.AddScoped<IVerticalService, VerticalService>();` in `Program.cs`.

5. **Tests:**
   - **Unit tests** (`tests/.../Services/VerticalServiceTests.cs`):
     - Pre-existing smoke test (request constructable) preserved.
     - Added `MapToResponse_ProjectsAllFields_FromVerticalAggregate` — full happy-path mapping.
     - Added `MapToResponse_FallsBackToDefaults_WhenChildConfigsAreMissing` — defensive null-handling.
     - Used type aliases (`EntityTechDefaults`, `RequestTechDefaults`) to disambiguate the two `VerticalTechDefaults` types in different namespaces.
   - **Integration tests** (`tests/.../Services/VerticalServiceIntegrationTests.cs`, new file):
     - 3 tests, all `[Trait("Category", "Integration")]` so excluded by CI's `--filter "Category!=Integration"`.
     - Each test early-returns if `RDS_TEST_CONNECTION` env var is unset (so they no-op locally without a real DB).
     - Covers: encryption round-trip (write encrypted, read back plaintext), slug conflict via PG unique-violation, full Get/Update/Unenroll cycle.
     - Each test uses a unique slug (`itest-{guid}`) and `IAsyncLifetime.DisposeAsync` deletes the row after.
     - Constructed `ForgeDbContext` directly with `EphemeralDataProtectionProvider` (in-memory throwaway keys) — no `WebApplicationFactory` plumbing needed.

### End-of-session state

- **Build:** 4/4 projects, 0 warnings, 0 errors.
- **Unit tests (CI gate, `Category!=Integration`):** 4/4 passing — 1 engine smoke, 1 api smoke, 2 mapping.
- **Integration tests (`Category=Integration`, `RDS_TEST_CONNECTION` set):** 3/3 passing in ~3s against `forge-dev`.

### Proof points from the integration pass

These are the things that don't show up in unit-test land and that the integration run actually verified against real RDS:

- `EncryptedStringConverter` write/read round-trip works — wrote `ghp_secret_token`, read back `ghp_secret_token`.
- Postgres unique-violation on `slug` is correctly classified by `PostgresErrorCodes.UniqueViolation` (SQLSTATE `23505`).
- `gen_random_uuid()` and `NOW()` defaults populate via RETURNING — `vertical.Id` and `vertical.CreatedAt` are non-default after `SaveChangesAsync`, no manual reload needed.
- `text[]` for `Environments` round-trips as `string[]`.
- FK cascades work (deleting a Vertical removes its child configs; verified implicitly by cleanup).
- Soft-delete + `IsActive` filter correctly hides unenrolled rows from `GetAsync`.

### How to run integration tests on another machine

```powershell
$env:RDS_TEST_CONNECTION='Host=...;Database=forge_dev;Username=postgres;Password=...;Port=5432;SSL Mode=Require;Trust Server Certificate=true'
dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category=Integration"
```

Connection string is the same one in `appsettings.Development.json` on this machine. Office laptop will need `SSL Mode=Disable` instead of `SSL Mode=Require` — and integration tests probably won't work there at all because of ZScaler ZPA mangling the wire protocol (see Session 1 entry below).

### Cross-machine notes

- `appsettings.Development.json` is gitignored — unchanged this session, still has `SSL Mode=Require;Trust Server Certificate=true` on home laptop.
- Home IP `73.197.181.23/32` still in the RDS SG. Revoke when done.

### Next up

- [ ] Implement `IVerticalConnectivityValidator` + `VerticalConnectivityValidator` — STS `AssumeRole` dry-run for AWS, Octokit org membership check for GitHub. Wire into `POST /api/v1/verticals/{slug}/validate`. Likely needs `AWSSDK.SecurityToken` and `Octokit` NuGet packages; check `Pervaxis.Forge.Api.csproj`.
- [ ] Fix the validate endpoint's signature — currently takes both `string slug` AND `VerticalEnrollmentRequest`, which is awkward. Probably should be slug-only (validate stored creds) OR request-only (validate before enroll). Decide with Anand.
- [ ] Wire the remaining stub endpoints in `GenerationEndpoints.cs` and `ModuleEndpoints.cs` once the Engine has real generation logic — not Phase 0.
- [ ] **May 8 (tomorrow)** — Swagger contract handoff to UI team. The 5 wired vertical endpoints are now real; the rest are documented stubs. Confirm UI team is happy with this surface for their mock-to-real swap on May 10.
- [ ] SonarCloud bootstrap when `SONAR_TOKEN` lands.

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom and the bootstrap section at the top of this file.
3. `dotnet build && dotnet test --filter "Category!=Integration"` — should be 4/4 green.
4. If you want to run integration tests, set `RDS_TEST_CONNECTION` and your machine's IP must be in the RDS SG.

---

## 2026-05-07 — Phase 0 Day 2 (Home Laptop): RDS migration applied

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude as implementing engineer)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6–10)
**Machine:** Home laptop (no ZScaler) — moved off office laptop because ZPA was mangling the Postgres wire protocol.

### What was done this session

1. **Confirmed ZScaler block is gone on home network.** DNS now resolves `forge-dev.cafy4a22q90j.us-east-1.rds.amazonaws.com` to a real AWS public IP (`18.211.4.220`), not the ZPA `100.64.x.x` range that the office laptop was getting.

2. **Added home laptop's public IP to the RDS security group.** Inbound rule: PostgreSQL/TCP/5432 from `73.197.181.23/32`. TCP test against the RDS endpoint then succeeded.

3. **Reverted the office-only ZScaler workaround in `appsettings.Development.json`:** flipped `SSL Mode=Disable` → `SSL Mode=Require;Trust Server Certificate=true`. RDS has `rds.force_ssl=1` and only `hostssl` lines in `pg_hba.conf`, so unencrypted connections are rejected (`28000: no pg_hba.conf entry for host "...", no encryption`). The file is gitignored (per commit `a32b33e`), so this change is local-only and does not propagate to the office laptop.

4. **Applied `20260506140655_InitialSchema` to `forge-dev`** via `dotnet ef database update --project src/Pervaxis.Forge.Api`. Clean run:
   - 6 tables created: `verticals`, `vertical_cloud_configs`, `vertical_source_control_configs`, `vertical_tech_defaults`, `generation_logs`, `deployment_outputs`.
   - All indexes from the spec (`idx_verticals_slug` UNIQUE, `idx_generation_logs_vertical`, `idx_generation_logs_created_at DESC`, `idx_deployment_outputs_generation`) and all uniqueness constraints (`IX_vertical_cloud_configs_VerticalId`, etc.).
   - All FK constraints with the spec's cascade rules (`ON DELETE CASCADE` for the per-vertical config tables; `ON DELETE RESTRICT` for `generation_logs.VerticalId`).
   - `__EFMigrationsHistory` row inserted: `20260506140655_InitialSchema` / `10.0.7`.

### End-of-session state

- **DB:** `forge-dev` schema is live. Migration recorded.
- **Build/tests:** unchanged from Day 1 Session 2 — 4/4 projects, 0 warnings, 0 errors, 2/2 tests passing.

### Gotchas resolved this session

| Issue | Root cause | Fix |
|---|---|---|
| TCP 5432 timeout from home laptop | RDS SG had no inbound rule for the home public IP | Added `73.197.181.23/32` to the SG |
| `28000: no pg_hba.conf entry for host "73.197.181.23", user "postgres", ..., no encryption` | `SSL Mode=Disable` was an office-only workaround for ZScaler; RDS forces SSL | `SSL Mode=Require;Trust Server Certificate=true` in `appsettings.Development.json` |

### Cross-machine notes (read this on the office laptop)

- `appsettings.Development.json` is per-machine and gitignored. The office laptop still has `SSL Mode=Disable` because ZScaler ZPA strips SSL mid-handshake. If/when ZPA stops intercepting Postgres traffic, flip the office laptop to `SSL Mode=Require;Trust Server Certificate=true` to match.
- The home laptop's IP (`73.197.181.23/32`) is now in the `forge-dev` SG. If the home IP rotates or you're done with home-laptop work, revoke the rule.
- Office laptop's public IP is presumably already in the SG (TCP succeeded yesterday before SSL failed) — leave it.

### Next up — Phase 0 Day 2/3

- [ ] Implement `IVerticalService` + `VerticalService` against the real DB — CRUD with credential encryption via Data Protection (`EncryptedStringConverter` is already wired in `ForgeDbContext`).
- [ ] Implement `VerticalConnectivityValidator` (STS `AssumeRole` dry-run + GitHub org check).
- [ ] Replace the eleven `501 Not Implemented` endpoint stubs with real handlers — **May 10 deadline** (UI team swaps mock for real API in dev).
- [ ] SonarCloud bootstrap when `SONAR_TOKEN` lands (org `clarivex-tech`, project `clarivex-tech_pervaxis-forge`).

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom, then `appsettings.Development.json` notes above.
3. If you're on a network with no ZScaler/proxy: copy this laptop's connection string (`SSL Mode=Require;Trust Server Certificate=true`). If you're on the office laptop: keep `SSL Mode=Disable` until ZPA is bypassed.
4. Make sure your machine's public IP is in the `forge-dev` RDS security group.
5. `dotnet build && dotnet test --filter "Category!=Integration"` — should be green. The migration is already applied; `dotnet ef database update` will be a no-op.

---

## 2026-05-06 — Phase 0 Day 1 (Session 2): Contract + entities + CI + tests

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude as implementing engineer)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6–10)

### What was done this session

1. **TODO 4 — Request/response records** (18 files, all green):
   - New supporting types: `CloudProviderConfig`, `SourceControlConfig`, `VerticalTechDefaults`, `GenerationDatabaseConfig`, `GenerationQueueConfig`, `GenerationMetadata`, `ServiceGenerationSpec`, `AwsConnectivityResult`, `GitHubConnectivityResult`
   - Filled stubs: `VerticalEnrollmentRequest`, `UpdateVerticalRequest`, `GenerationRequest`, `BatchGenerationRequest`, `VerticalResponse`, `VerticalSummaryResponse`, `ConnectivityValidationResponse`, `GenerationResult`, `BatchGenerationResult`
   - All records, all `required` modifiers, all Clarivex license headers. Build: 0 warnings, 0 errors.

2. **TODO 5 — Program.cs + Swagger wiring**:
   - Added `Swashbuckle.AspNetCore 7.*` + `Microsoft.OpenApi 1.*` + `Microsoft.EntityFrameworkCore.Design 10.*` to csproj.
   - `Program.cs`: EF Core + Npgsql, Data Protection (keys → `%LOCALAPPDATA%\Pervaxis.Forge\keys`), Swashbuckle with full OpenAPI info block.
   - Swagger UI gated: dev-only **OR** `Forge:EnableSwagger=true` config flag (prod opt-in without code deploy).
   - All 3 endpoint groups wired via `MapVerticalEndpoints()`, `MapGenerationEndpoints()`, `MapModuleEndpoints()` extension methods.
   - Endpoint stubs return `501 Not Implemented` with full Swagger metadata (`WithSummary`, `Produces<T>`, `ProducesProblem`).
   - `appsettings.json`: `Forge:EnableSwagger` defaulted to `false`.
   - `ForgeDbContext`: promoted from namespace-only stub to compilable skeleton (full implementation is TODO 3, which followed immediately).

3. **TODO 2 + 3 — EF entities + ForgeDbContext**:
   - All 6 entities implemented: `Vertical`, `VerticalCloudConfig`, `VerticalSourceControlConfig`, `VerticalTechDefaults`, `GenerationLog`, `DeploymentOutput` — classes (not records) per EF change-tracking requirement, `required` on mandatory props.
   - `ForgeDbContext`: full Fluent API — table names, column constraints, relationships, cascade rules, all indexes from spec (`idx_verticals_slug`, `idx_generation_logs_vertical`, `idx_generation_logs_created_at DESC`, `idx_deployment_outputs_generation`).
   - `EncryptedStringConverter` (`file sealed`) — transparent Data Protection encrypt/decrypt for `IamRoleArn` and `AccessToken`. Null-safe.
   - `GenerationLog.Manifest` stored as `jsonb`. `VerticalTechDefaults.Environments` stored as `text[]`.

4. **TODO 6 — EF migration**:
   - Installed `dotnet-ef` global tool (`--prerelease` for .NET 10).
   - Added `Microsoft.EntityFrameworkCore.Design 10.*` to csproj (required for migrations, `PrivateAssets=all`).
   - Generated `20260506140655_InitialSchema`. **Not applied** — no RDS yet (Day 2).

5. **TODO 7 — CI rework**:
   - Deleted `build-test.yml` (had postgres:16 service container — violates no-local-emulation rule).
   - Created `pr-check.yml`: triggers on PR to `main`/`develop`. Runs build + non-integration tests (`--filter "Category!=Integration"`). Codecov upload. `TODO(sonar)` marker for future SonarCloud step.
   - Created `deploy.yml`: triggers on push to `main`. Same build/test pattern. `TODO` for deploy job once ECS infra is provisioned.
   - DB-dependent tests gated behind `RUN_INTEGRATION_TESTS` env var (off by default until RDS exists).

6. **TODO 8 — Stub tests (one per project, CI green)**:
   - `Pervaxis.Forge.Engine.Tests` — `NamingConventionTests.NamingConvention_ClassExists_InExpectedNamespace` (smoke test; namespace/class existence assertion).
   - `Pervaxis.Forge.Api.Tests` — `VerticalServiceTests.VerticalEnrollmentRequest_CanBeConstructed_WithRequiredProperties` (full request object construction + FluentAssertions).
   - Added minimal `NamingConvention` static class stub to Engine (was namespace-only, blocked compilation).
   - Result: **2/2 passed, 0 failures**.

7. **TODO 9 — README updates**:
   - `pervaxis-forge-api/README.md`: full rewrite with prerequisites, build/test/run instructions, migration commands, Swagger opt-in instructions, key decisions table, pinned package rationale.
   - Root `README.md`: added Current Status table (BFF + UI phase, branch, status).

### Gotchas resolved this session

| Issue | Root cause | Fix |
|---|---|---|
| `Microsoft.OpenApi.Models` not found | `Swashbuckle Version="*"` resolved to v8.x which dropped the namespace | Pinned `Swashbuckle.AspNetCore 7.*` + explicit `Microsoft.OpenApi 1.*` |
| `ForgeDbContext` not found in Program.cs | Stub was namespace-only, no class | Promoted to compilable skeleton before full implementation |
| Test run aborted — `Castle.Core lib/net6.0` not found | .NET 10 testhost assembly resolution | Added `CopyLocalLockFileAssemblies=true` to both test csproj files |
| Test run aborted — `testhost 18.3.0-release-26219-105` not found | `Version="*"` pulled nightly xUnit runner which dragged in a missing nightly testhost | Pinned `xunit 2.9.3`, `xunit.runner.visualstudio 3.1.5`, `Microsoft.NET.Test.Sdk 17.14.1` |
| MSB3277 EF Relational version conflict | `Microsoft.AspNetCore.Mvc.Testing 10.*` and Api project pulling different EF Relational versions | Explicit `Microsoft.EntityFrameworkCore.Relational 10.*` pin in Api.Tests csproj |

### End-of-day state

- **Build:** 4/4 projects, 0 warnings, 0 errors.
- **Tests:** 2/2 passed (Engine + Api), 0 failures.
- **Migration:** `InitialSchema` generated, not applied.
- **CI:** `pr-check.yml` + `deploy.yml` in place, old postgres workflow deleted.
- **Swagger contract:** All 11 endpoints wired with full metadata — ready for UI team to consume on May 8.

### Open items for Day 2

- [ ] Align with Anand on AWS Organizations / RDS provisioning approach.
- [ ] Apply `InitialSchema` migration once RDS endpoint is available.
- [ ] Implement `VerticalService` + `IVerticalService` (enrollment, CRUD, credential encryption).
- [ ] Implement `VerticalConnectivityValidator` (STS AssumeRole dry-run + GitHub org check).
- [ ] SonarCloud bootstrap (org `clarivex-tech`, project `clarivex-tech_pervaxis-forge`, `SONAR_TOKEN` from Anand).
- [ ] Update `docs/FORGE_SOLUTION_STRUCTURE.md` — Scriban `5.*` → `7.*`, drop DataProtection PackageReference, drop Testcontainers.

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom.
3. `dotnet restore && dotnet build && dotnet test --filter "Category!=Integration"` — should all be green.
4. Continue with Day 2 open items above.

---

## 2026-05-06 — Phase 0 Day 1: Alignment + skeleton fill-in

**Branch:** `feature/api-vertical-enrollment`
**Engineer:** Anand Jayaseelan (with Claude as implementing engineer)
**Phase:** Phase 0 — Vertical Enrollment Backend (Week 1, May 6 – May 10)

### What was done this session

1. Pulled latest `develop`, cut `feature/api-vertical-enrollment` from it.
   - Stashed local `.claude/settings.local.json` to `.bak` to resolve a fast-forward conflict (incoming version had a different permission set). Local backup file is gitignored / untracked; merge or delete at will.
2. Read all authoritative docs end-to-end:
   - `docs/FORGE_BLUEPRINT_BFF.md` — phase plan, Day 1 task list
   - `docs/FORGE_SOLUTION_STRUCTURE.md` — folder/project layout
   - `docs/FORGE_TECHNICAL_SPECIFICATION.md` — API contracts, DB schema, entities
   - `pervaxis-forge-api/CLAUDE.md` — Forge-specific rules (file header, dependency rules)
   - All 7 guides + 4 skills under `pervaxis-forge-api/.claude/`
3. Identified that several guides are sourced from Genesis and describe *generated* code or Genesis providers, **not Forge itself** (`GENESIS_PROVIDERS.md`, `CLOUD_PROVIDER_SEPARATION.md`, `CORE_ABSTRACTIONS_COMPLIANCE.md`, `METRICS_PATTERN.md`, `OBSERVABILITY_PATTERN.md`, `RESILIENCE_PATTERN.md`). Forge has zero `Pervaxis.Core`/`Pervaxis.Genesis` references.
4. Aligned on direction with Anand. Decisions captured below.
5. **Discovered existing scaffold:** the BFF is not greenfield — commit `356b537` ("Add Vertical Enrollment module, split blueprints, and create solution skeletons") created the full file/folder skeleton, all with TODO stubs. e.g. `Vertical.cs` is `// TODO: implement Vertical entity`, `Program.cs` is a 12-line stub. Solution + projects + entity/service/endpoint files all exist; implementations do not. This shifts today from "scaffold from scratch" to "fill in the skeletons".
6. **Discovered existing CI conflicts with no-local-emulation rule:** `pervaxis-forge-api/.github/workflows/build-test.yml` runs a `postgres:16` Docker service container in GitHub Actions. Per Anand's call this morning, we use real AWS only. CI needs to be reworked — split into `pr-check.yml` + `deploy.yml`, drop the postgres service, gate DB-dependent tests behind a real-RDS env var. Codecov action present today; SonarCloud not set up.

### Decisions locked in

| Topic | Decision |
|---|---|
| Repo layout | **Mono repo.** Treat `pervaxis-forge-api/` as the BFF root inside `pervaxis-forge`. All `.slnx`, `src/`, `tests/`, `Directory.Build.props`, `nuget.config`, `global.json` live inside `pervaxis-forge-api/`. UI repo split deferred. |
| Local emulation | **None.** No Docker, LocalStack, or Testcontainers. Use real AWS resources for PostgreSQL, S3, Secrets Manager, etc., even in dev. Ignore LocalStack references in the blueprint. |
| CI/CD | **Set up Day 1.** GitHub Actions + SonarCloud per `pervaxis-forge-api/.claude/guides/ci-sonarcloud-setup.md`. Two workflow files (`pr-check.yml`, `deploy.yml`). Sonar gates only PRs targeting `main` (free-tier constraint). |
| Doc conflicts | CLAUDE.md + `FORGE_SOLUTION_STRUCTURE.md` win over Genesis-sourced guides. Specifically: multi-line copyright header (not the one-liner), `LangVersion=latest` (not `preview`), xUnit + FluentAssertions + Moq (not NSubstitute). |
| EF entities vs DTOs | **Classes for EF entities** (need settable properties for change tracking); **records with `required` for DTOs/Requests/Responses**. |
| Auth | **Deferred** — Phase 0 endpoints run unauthenticated in dev. `forge-admin` role enforcement is a post-Phase-0 concern. |
| Data Protection keys | Persist to `%LOCALAPPDATA%\Pervaxis.Forge\keys` in dev. Prod key store (S3 / Secrets Manager) is a Phase 3 task. |

### Open blockers / questions — resolved

| # | Question | Answer |
|---|---|---|
| 1 | AWS PostgreSQL endpoint? | **Deferred to Day 2.** No RDS today; we'll plan AWS Organizations + RDS together tomorrow morning. Today: write entities + migration without applying. |
| 2 | AWS creds for Forge account? | `.aws/credentials` exists locally. Not used today — code work doesn't need AWS. |
| 3 | GitHub remote? | Confirmed `clarivex-tech/pervaxis-forge`. Branch protection state TBD; will respect as configured. |
| 4 | SonarCloud? | **Skip for today.** Not yet set up. Ship CI with build+test only; leave TODO for Sonar bootstrap on a follow-up PR. Keys provided by Anand for the follow-up: org `clarivex-tech`, project `clarivex-tech_pervaxis-forge`. `SONAR_TOKEN` secret still TBD. |
| 5 | AWS Organizations? | **Deferred to Day 2.** Anand to research independently today; we align tomorrow. Lean: single account for now, Organizations as a separate dedicated effort. |

### Today's TODO (Phase 0 Day 1 — revised after skeleton discovery)

Order optimized for the **May 8 contract deadline** — request/response records before DB internals. Local toolchain confirmed: .NET SDK 10.0.203 installed.

- [x] 1. Verify existing scaffold matches `FORGE_SOLUTION_STRUCTURE.md`: `Directory.Build.props`, `nuget.config`, `Pervaxis.Forge.slnx`, `.gitignore`, csproj contents. Add `global.json` pinning SDK 10.0.203 if missing. **Done — see "TODO 1 results" below.**
- [ ] 2. Implement EF entities (fill in stubs) under `src/Pervaxis.Forge.Api/Data/Entities/`:
  - `Vertical`, `VerticalCloudConfig`, `VerticalSourceControlConfig`, `VerticalTechDefaults`, `GenerationLog`, `DeploymentOutput`
  - `required` modifier on mandatory props per CLAUDE.md
  - Multi-line Clarivex license header on every `.cs` file
- [ ] 3. Implement `ForgeDbContext` — DbSets, Fluent API relationships, indexes, encrypted-property value converters using Data Protection (`IDataProtector`)
- [ ] 4. Implement request/response records under `src/Pervaxis.Forge.Api/Models/` — **the contract to UI by May 8**:
  - Requests: `VerticalEnrollmentRequest`, `UpdateVerticalRequest`, `CloudProviderConfig`, `SourceControlConfig`, `VerticalTechDefaults` (DTO), `GenerationRequest`, `BatchGenerationRequest`
  - Responses: `VerticalResponse`, `VerticalSummaryResponse`, `ConnectivityValidationResponse`, `GenerationResult`, `BatchGenerationResult`
- [ ] 5. Wire DI in `Program.cs`: EF Core + Npgsql, Data Protection (keys → `%LOCALAPPDATA%\Pervaxis.Forge\keys` in dev), Swagger with full doc, endpoint mapping. Endpoints return `Results.Problem(statusCode: 501, title: "Not implemented")` for now — but signatures + Swagger metadata are final.
- [ ] 6. Generate initial EF Core migration: `dotnet ef migrations add InitialSchema`. **Do not** run `database update` (no RDS yet).
- [ ] 7. **Rework CI**: split `build-test.yml` → `pr-check.yml` + `deploy.yml`. Drop the `postgres:16` service container. DB-tagged tests gated behind `RUN_INTEGRATION_TESTS` env var (off by default until RDS exists). Add `# TODO(sonar)` markers where Sonar steps will slot in.
- [ ] 8. Implement one passing test in each of `Pervaxis.Forge.Api.Tests` and `Pervaxis.Forge.Engine.Tests` so CI has green tests to run. (Real coverage comes Day 2+.)
- [ ] 9. Update README.md (root + `pervaxis-forge-api/`) with run instructions.
- [ ] 10. Update this session log with end-of-day status. Commit on `feature/api-vertical-enrollment`.

**Out of scope for today (parked for Day 2):**
- AWS Organizations design + provisioning
- RDS provisioning + applying the migration
- Implementing `VerticalService`, `VerticalConnectivityValidator` (those need Data Protection + AWS SDK; Day 1-2 in blueprint)
- Implementing endpoints beyond stubs
- SonarCloud bootstrap

### Hard deadlines this week

- **May 8 (Fri)** — Swagger JSON contract delivered to UI team (they're mocking against it from Day 1)
- **May 10 (Sun)** — UI swaps mock for real API in dev environment

### Notes / challenges

- `feature/api-vertical-enrollment` branch name doesn't follow the `feature/<ticket-id>-...` example in `PERVAXIS_STANDARDS.md`, but matches the descriptive style allowed by `GIT_WORKFLOW.md`. Keeping as-is.
- Local backup file `.claude/settings.local.json.bak` is sitting in the working tree — Anand can decide whether to merge his local perms back in or just keep the version that came in via develop.
- Untracked stray folder: `docs/.claude/` (probably a misplaced settings file from earlier work). Worth cleaning up before any commits.
- Solution Structure shows entities in `Pervaxis.Forge.Api/Data/Entities/` (not in a separate domain library). Tech Spec §8.2 also shows them as plain classes. Sticking with both.
- Existing CI's postgres service container is the most visible legacy of the "Docker-friendly" assumption. Removing it without replacement means DB-dependent tests must be gated until real RDS exists. Trade-off: less test coverage in CI for Day 1, but consistent with the no-local-emulation policy.
- Codecov is wired in the existing CI. Keep it for now (free, zero-config) and revisit once SonarCloud is set up.

### TODO 1 results — scaffold audit + green build (afternoon, 2026-05-06)

**Audit results vs `FORGE_SOLUTION_STRUCTURE.md`:**

| File | Status | Notes |
|---|---|---|
| `Directory.Build.props` | ✅ Match | All 5 properties exact match to §1.2. |
| `nuget.config` | ✅ Match | Single `nuget.org` feed; Pervaxis.Core feed correctly excluded with explanatory comment. |
| `Pervaxis.Forge.slnx` | ✅ Match | All 4 projects referenced. |
| Root `.gitignore` | ✅ OK | Comprehensive .NET Dotnet.gitignore template. |
| `pervaxis-forge-api/.gitignore` | ⚠️ Minimal but harmless | 6-line redundant subset of root; left as-is. |
| `Pervaxis.Forge.Engine.csproj` | ✏️ Fixed | Bumped `Scriban` from `5.*` → `7.*` — see "Scriban CVE remediation" below. |
| `Pervaxis.Forge.Api.csproj` | ✏️ Fixed | Removed `Microsoft.AspNetCore.DataProtection` PackageReference — `NU1510` (it's in the ASP.NET Core shared framework on .NET 10). DataProtection APIs still available via the framework. |
| `Pervaxis.Forge.Engine.Tests.csproj` | ✅ Match | xunit + FluentAssertions + Moq, no Testcontainers. |
| `Pervaxis.Forge.Api.Tests.csproj` | ✏️ Fixed | Removed `Testcontainers.PostgreSql` — violates the no-local-emulation rule from this morning's call. |
| `global.json` | ➕ Added | New file pinning SDK to `10.0.203`, `rollForward: latestPatch`, `allowPrerelease: false`. SDK 10.0.203 confirmed installed locally. |

**Scriban CVE remediation (security-driven, not in original spec):**

`Scriban 5.*` floats to `5.12.1`, which has 12 active GHSA advisories: 1 critical (GHSA-5wr9-m6jw-xx44, member-filter bypass), 7 high, 4 moderate. With `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` plus .NET 10's default `NuGetAudit`, all 12 promoted to errors and blocked `dotnet restore`. All advisories fixed in Scriban `7.0.0+`. Latest stable is `7.1.0` (released 2026-04-08). Bumped to `7.*`. Scriban 7.x API is API-compatible with 5.x for our usage (load .sbn, render with model, ZIP output) — no code changes needed in Engine. Action: update `FORGE_SOLUTION_STRUCTURE.md` §1.3 example from `5.*` → `7.*` in a follow-up doc PR (not done today).

**Program.cs stub trimmed:**

The skeleton `Program.cs` referenced Swashbuckle methods (`AddSwaggerGen`/`UseSwagger`/`UseSwaggerUI`) without the package. Stripped to the minimal-compiling form `var builder = WebApplication.CreateBuilder(args); var app = builder.Build(); app.Run();` with a TODO pointing to TODO 5. Full OpenAPI/Swagger wiring (likely .NET 10 built-in `Microsoft.AspNetCore.OpenApi` or Swashbuckle — call to make in TODO 5) comes when we wire DI properly.

**Final state:** `dotnet restore` clean, `dotnet build` green — 4/4 projects, 0 warnings, 0 errors.

**Open follow-ups from TODO 1:**

- Update `docs/FORGE_SOLUTION_STRUCTURE.md` §1.3 — change Scriban example version `5.*` → `7.*`.
- Update §1.4 example — drop `Microsoft.AspNetCore.DataProtection` PackageReference (it's in the shared framework on .NET 10).
- Update §1.5 example — drop `Testcontainers.PostgreSql` per no-local-emulation rule.
- Decide OpenAPI approach in TODO 5: `Microsoft.AspNetCore.OpenApi` (built-in, spec-only) vs Swashbuckle (external, includes UI). May 8 deadline is "Swagger JSON contract delivered to UI team" — both produce JSON; Swashbuckle adds the UI page.

### How to resume on another machine

1. `git fetch && git checkout feature/api-vertical-enrollment && git pull`
2. Read this entry top-to-bottom.
3. Check off completed todos, append new findings, ask Anand for any unresolved blockers.
4. Continue with the next unchecked item.

---
