# Pervaxis Forge BFF — UI Handoff

**For:** Pervaxis Forge Launchpad (Angular) team
**As of:** 2026-05-07
**Branch:** `feature/api-vertical-enrollment`
**Status:** Phase 0 vertical enrollment endpoints are wired against real RDS. Generation and module endpoints remain documented stubs (Phase 1).

---

## 1. Where the contract lives

The single source of truth is the committed OpenAPI snapshot:

```
pervaxis-forge-api/contract/openapi.json
```

Regenerate it whenever the BFF is changed (the file is checked in so the UI team can codegen clients without booting the BFF). To regenerate manually:

```powershell
ASPNETCORE_ENVIRONMENT=Development dotnet run --project pervaxis-forge-api/src/Pervaxis.Forge.Api --no-build --urls http://localhost:5500
# in another shell:
Invoke-WebRequest http://localhost:5500/swagger/v1/swagger.json -OutFile pervaxis-forge-api/contract/openapi.json
```

For interactive exploration: run the BFF (see §3) and open `http://localhost:<port>/swagger`.

---

## 2. What's real vs stubbed

| Method | Path | Status | Backed by |
|---|---|---|---|
| POST | `/api/v1/verticals` | **Real** | `VerticalService.EnrollAsync` |
| GET | `/api/v1/verticals` | **Real** | `VerticalService.ListAsync` |
| GET | `/api/v1/verticals/{slug}` | **Real** | `VerticalService.GetAsync` |
| PUT | `/api/v1/verticals/{slug}` | **Real** | `VerticalService.UpdateAsync` |
| DELETE | `/api/v1/verticals/{slug}` | **Real** | `VerticalService.UnenrollAsync` (soft-delete) |
| POST | `/api/v1/verticals/{slug}/validate` | **Stub (501)** | Connectivity validator lands next session |
| POST | `/api/v1/generate` | **Stub (501)** | Phase 1 |
| POST | `/api/v1/generate/batch` | **Stub (501)** | Phase 1 |
| GET | `/api/v1/modules` | **Stub (501)** | Phase 1 |
| GET | `/api/v1/canvas-modules` | **Stub (501)** | Phase 1 |

Stubs return `application/problem+json` with `status: 501` and `title: "Not implemented"` — matches the real-endpoint error shape so the UI can use a single error path.

---

## 3. Running the BFF locally

Prerequisites: .NET SDK 10.0.203 (pinned via `global.json`), network access to `forge-dev` RDS.

```powershell
cd pervaxis-forge-api
dotnet build
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Pervaxis.Forge.Api
# Default URL printed in the console (typically https://localhost:7xxx and http://localhost:5xxx)
```

Swagger UI: `https://<host>/swagger`. The OpenAPI doc itself: `https://<host>/swagger/v1/swagger.json`.

> **Note for office laptop / corporate proxy:** ZScaler ZPA mangles the Postgres wire protocol on port 5432, so the BFF won't connect to `forge-dev` from the corporate network. Run from a non-corporate network, or work against mock data while on-prem.

---

## 4. CORS

Configured in `Program.cs`. Allowed origins come from `Forge:AllowedOrigins` in `appsettings.json` (defaults to `["http://localhost:4200"]`).

If the UI dev server runs on a different port, override via:

```jsonc
// appsettings.Development.json (UI-side, illustrative — actual config is on the BFF)
"Forge": {
  "AllowedOrigins": ["http://localhost:4200", "http://localhost:4201"]
}
```

Both `Authorization` and custom headers are allowed; all standard verbs (GET/POST/PUT/DELETE) are allowed. No credentials/cookies (auth is deferred — see §7).

---

## 5. Sample payloads

### 5.1 POST /api/v1/verticals — request

```json
{
  "slug": "clarivolt",
  "displayName": "Clarivolt",
  "description": "Sales upload and validation platform",
  "ownerTeam": "Clarivolt Platform Team",
  "ownerEmail": "team@clarivex.tech",
  "cloudProvider": {
    "provider": "AWS",
    "awsAccountId": "123456789012",
    "iamRoleArn": "arn:aws:iam::123456789012:role/ForgeDeploymentRole",
    "defaultRegion": "us-east-1"
  },
  "sourceControl": {
    "platform": "GitHub",
    "gitHubOrg": "clarivex-tech",
    "accessToken": "ghp_...",
    "defaultVisibility": "Private",
    "defaultBranchProtection": true
  },
  "techDefaults": {
    "environments": ["test", "accp", "prod"],
    "defaultEnvironment": "test",
    "generateTerraform": true,
    "generateCdk": true,
    "defaultDbEngine": "postgresql"
  }
}
```

**201 Created** response (`Location: /api/v1/verticals/clarivolt`):

```json
{
  "id": "8a4c2e10-1b3f-42d8-9b21-f8e7f2c3d4a5",
  "slug": "clarivolt",
  "displayName": "Clarivolt",
  "cloudProvider": "AWS",
  "sourceControl": "GitHub",
  "gitHubOrg": "clarivex-tech",
  "environments": ["test", "accp", "prod"],
  "enrolledAt": "2026-05-07T14:30:00+00:00"
}
```

**409 Conflict** if the slug is already taken — body is RFC 7807 ProblemDetails with `title: "Slug already exists"`.

### 5.2 GET /api/v1/verticals — response

```json
[
  {
    "id": "8a4c2e10-...",
    "slug": "clarivolt",
    "displayName": "Clarivolt",
    "description": "Sales upload and validation platform",
    "cloudProvider": "AWS",
    "sourceControl": "GitHub",
    "serviceCount": 0,
    "enrolledAt": "2026-05-07T14:30:00+00:00"
  }
]
```

`serviceCount` is the sum of `ServiceCount` across the vertical's `GenerationLog` rows. It will be `0` until Phase 1 ships generation.

### 5.3 PUT /api/v1/verticals/{slug} — request (partial — slug, cloud, source control are immutable)

```json
{
  "displayName": "Clarivolt v2",
  "description": "Updated description",
  "ownerTeam": "Clarivolt Platform Team",
  "ownerEmail": "team@clarivex.tech",
  "techDefaults": {
    "environments": ["test", "accp", "prod"],
    "defaultEnvironment": "test",
    "generateTerraform": true,
    "generateCdk": true,
    "defaultDbEngine": "postgresql"
  }
}
```

Returns the updated `VerticalResponse`.

### 5.4 DELETE /api/v1/verticals/{slug} — soft delete

Returns `204 No Content` on success, `404 Not Found` if the slug doesn't exist (or has already been unenrolled — see §6).

---

## 6. Behavior the OpenAPI doc can't tell you

- **`slug` is immutable.** Set at enrollment; can never change. There is no rename endpoint by design — slugs are baked into resource names downstream.
- **`accessToken` and `iamRoleArn` are write-only.** They go through `EncryptedStringConverter` on the way into Postgres and are never returned in responses. Don't display them as "saved" values in the UI; only show on initial enrollment.
- **Soft-delete is invisible after the fact.** `DELETE /api/v1/verticals/{slug}` flips `is_active=false` in the DB; subsequent `GET`, `PUT`, and `DELETE` on the same slug return `404`. A re-enroll with the same slug currently fails with `409` (the row still exists, just inactive). If re-enroll matters, raise it — it's not in Phase 0 scope.
- **`enrolledAt` is the original creation timestamp** (`Vertical.CreatedAt`), not the last update. UTC; serialized as ISO-8601 with `+00:00` offset.
- **`serviceCount` is 0 in Phase 0.** Generation logs don't exist yet. Don't gate UI features on it being non-zero unless you handle the empty case gracefully.
- **The `/validate` endpoint is currently 501.** Until the connectivity validator ships, the wizard's "Validate" step should either skip the call or display a "validation unavailable" notice. The endpoint's URL signature (`{slug}/validate`) is awkward for a pre-enrollment call — when it lands, the URL slug will be informational only and the body's credentials are what's actually checked. Don't rely on the URL slug existing in the DB.

---

## 7. What's NOT in scope yet

- **Authentication** is deferred. Phase 0 endpoints run unauthenticated — no JWT, no session cookies, no `Authorization` header expected. The `forge-admin` role check happens in the UI's `forgeAuthGuard`; the BFF doesn't enforce it yet. Once auth lands, expect `Authorization: Bearer <jwt>` and `403` for missing roles.
- **`/api/v1/verticals/{slug}/validate`** — see above.
- **Generation endpoints** (`/api/v1/generate*`, `/api/v1/modules`, `/api/v1/canvas-modules`) — Phase 1 work, not before mid-May.
- **Server-side input validation** is minimal in Phase 0 — the BFF trusts the UI to enforce slug kebab-case, AWS account ID format (12 digits), ARN shape, and email shape. The DB will enforce length limits and uniqueness, but mismatches will surface as generic 500s, not `400 ValidationProblem`. Defense-in-depth validation on the BFF side is on the backlog.

---

## 8. May 10 swap checklist

For the UI's mock → real swap:

1. Pull latest `feature/api-vertical-enrollment` (or the merge target on `develop`).
2. Confirm `pervaxis-forge-api/contract/openapi.json` matches your generated client's expectations.
3. Boot the BFF locally (§3).
4. In the Launchpad app, flip `environment.useMockApi` to `false`. The DI token `VERTICAL_API_SERVICE` should now resolve to the real `VerticalApiService`.
5. Verify enrollment end-to-end against `forge-dev`. Watch for:
   - CORS errors → confirm the UI dev server origin is in the BFF's `Forge:AllowedOrigins`.
   - Connection refused → confirm your machine's public IP is in the `forge-dev` RDS security group, and you're not on a network that proxies port 5432 (ZScaler).
   - 501 from `/validate` → expected until the validator lands. Skip or stub on the UI side.
6. Surface `409` distinctly from `400` in the wizard — the slug conflict UX should suggest picking a different slug, not "fix your input".

---

## 9. Who to ping

- BFF questions: Anand Jayaseelan (architect / lead).
- Contract changes: BFF will bump `openapi.json` in the same PR as the code change. If the contract file diff and code diff disagree, the code is right and the JSON regeneration was missed — flag it on the PR.

---

*Generated as part of the Phase 0 handoff. Update this file when contract or behavior changes.*
