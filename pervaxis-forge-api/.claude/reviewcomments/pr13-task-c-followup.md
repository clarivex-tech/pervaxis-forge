# PR #13 Follow-up Checklist — Task C Remaining Items

**Reviewer:** Claude Sonnet  
**PR reviewed:** [#13 — Add update request validation](https://github.com/clarivex-tech/pervaxis-forge/pull/13)  
**Branch for fixes:** `feature/api-task-c-followup` (cut from `feature/api-vertical-enrollment`)  
**Target PR:** `feature/api-vertical-enrollment`

---

## Outstanding items

- [ ] **Trailing hyphen slug test** — add `[InlineData("slug-", false)]` to `ValidateSlug_Rules_AreEnforced` in `tests/Pervaxis.Forge.Api.Tests/Services/VerticalRequestValidatorTests.cs`. The implementation already rejects trailing hyphens; the test case is just missing.

- [ ] **`.gitignore` hygiene** — add the following entries to `pervaxis-forge-api/.gitignore` to prevent stray files from polluting future commits:
  ```
  # IDE / launch profiles
  **/Properties/launchSettings.json

  # Local Claude settings artefacts
  docs/.claude/
  .claude/settings.local.json.bak
  ```

---

## Minor cosmetic note (no action required)

- `ValidateDisplayName` and `ValidateOwnerTeam` use `nameof(VerticalEnrollmentRequest.DisplayName)` / `nameof(VerticalEnrollmentRequest.OwnerTeam)` as field keys even when validating `UpdateVerticalRequest`. The field names are identical so the API response is correct — acceptable as-is.

---

## Acceptance criteria

- Build green: 4/4 projects, 0 warnings, 0 errors.
- All existing tests still pass.
- `dotnet test pervaxis-forge-api/Pervaxis.Forge.slnx --filter "Category!=Integration"` exits 0.
