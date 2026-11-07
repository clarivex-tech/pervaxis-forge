# Forge multitenancy specification

Version: 1.0-draft.1  
Date: 2026-11-06  
Status: Design consolidated; implementation and runtime verification outstanding.

This document supersedes the multitenancy design discussion for v1. Requirements below describe intended behavior, not completed functionality. Source observations are limited to the targeted audit in Appendix A.

## 1. Decisions

| Topic | V1 decision |
|---|---|
| Identity authority | Validated Cognito user access token; exactly one tenant per user |
| Claim | Configurable name, default `tenant_id`; exactly one scalar JSON string |
| Core identifier | Preserve existing string-backed `TenantId` record struct |
| New-flow identifier format | Non-empty GUID, canonical lowercase `D` string |
| Database | PostgreSQL through Npgsql; shared schema, tenant-owned rows |
| Tenant storage type | PostgreSQL `uuid`, converted from/to Core `TenantId` |
| Keys | Composite primary key `(TenantId, Id)` and tenant-inclusive foreign keys |
| Repository | Preserve Core interfaces; add tenant-aware EF implementation |
| Context | Singleton-safe facade over bounded ambient execution scopes |
| Lifecycle | Directory lookup opt-in; cache TTL configurable, default 60 seconds |
| Exempt endpoints | Explicit metadata; tenant context intentionally unresolved |
| M2M | Client-credentials authentication excluded from v1 |
| EF baseline | Generated services target .NET 10, EF Core 10; named filters |
| Pooling | No pooled DbContext or pooled factory in v1 |
| Genesis compatibility | Strict unresolved-context behavior requires major-version release |

## 2. Scope and ownership

Core owns contracts, tenant parsing, scope management, resolution, exceptions, and reusable EF enforcement. Keep EF dependencies in a dedicated integration package. Forge owns generation, prerequisite validation, configuration, and developer examples. Genesis owns tenant-safe provider behavior using the same Core context.

V1 excludes tenant onboarding UI, multiple tenants per user, interactive tenant switching, client-credentials tokens, cross-tenant administrative access, and PostgreSQL row-level security. RLS can be added as defense in depth later.

Roles and permissions authorize actions only within the resolved tenant. An administrator role does not permit crossing tenant boundaries.

## 3. Authentication and provisioning

Validate signature using trusted issuer JWKS, allowed algorithm, issuer, lifetime, `token_use=access`, and allowlisted `client_id`. When resource binding is configured, also require the configured `aud`. Do not require an app-client audience on ordinary Cognito access tokens or accept ID tokens as API access tokens.

Require Essentials or Plus for new deployments and a V2_0 or V3_0 pre-token-generation trigger. Launchpad must display these prerequisites and require acknowledgment; configuration metadata is not proof that the remote pool is correctly configured. Some grandfathered Lite pools support V2_0, but generated deployments must not depend on that exception.

Trusted provisioning assigns exactly one tenant per user. End users cannot write the authoritative tenant attribute. The trigger validates that assignment and emits `tenant_id`; it does not accept a tenant chosen in untrusted client metadata. The source user attribute may be `custom:tenant_id`; the access-token claim is `tenant_id`.

Reject duplicate properties, arrays (including one-element arrays), objects, numbers, booleans, empty strings, invalid GUIDs, and Guid.Empty. Validate the authenticated token's original claim representation as needed: ClaimsPrincipal transformations can flatten arrays and conceal ambiguity. Parse only after authentication has succeeded.

Missing/invalid authentication returns 401. Authenticated requests lacking a valid tenant return 403 on tenant-protected endpoints. Tenant authority does not replace resource permission checks.

Remove all tenant resolution and response echo behavior associated with `X-Tenant-ID`. Incoming values have no effect.

References: [AWS JWT verification](https://docs.aws.amazon.com/cognito/latest/developerguide/amazon-cognito-user-pools-using-tokens-verifying-a-jwt.html), [pre-token generation](https://docs.aws.amazon.com/cognito/latest/developerguide/user-pool-lambda-pre-token-generation.html).

## 4. Identifier and context contracts

Preserve the existing public `TenantId(string Value)` representation and existing legacy consumers. Add one shared GUID-validation/canonicalization helper used by claims resolution, background scopes, and EF conversion. Do not tighten the existing constructor globally in a minor release.

For the new flow, all accepted GUIDs become lowercase `D` strings. EF converts validated values to Guid on write and converts Guid values back to canonical strings on read. Reject empty/default values on tenant-owned writes. Include converter round-trip tests and noncanonical input tests. Genesis keys use the canonical string.

Expose a singleton-safe `ITenantContext` facade whose properties read the current scope dynamically. Never inject a request snapshot into singleton providers. Capture one immutable scope snapshot at the beginning of each provider operation to avoid inconsistent multiple-property reads. Do not store request context or tenant state in singleton fields.

Provide `ITenantScopeFactory.BeginScope(TenantId tenantId): IDisposable`:

- Validate/canonicalize the identifier; trusted ingress must authorize it before entry.
- Log scope entry and exit without tokens or credentials.
- Permit same-tenant nesting; reject a different tenant while a resolved scope is active.
- Restore prior context on reverse-order disposal, including exception paths.
- Invalidate completed scope state so inherited child work cannot continue using a disposed tenant scope.
- Reject out-of-order disposal and prevent stale scopes from restoring disposed parent state.

Outside request processing, hosted services, message handlers, scheduled jobs, and detached workers must explicitly call BeginScope for each work unit. Do not queue a captured execution context as durable tenant authority. Suppressed execution-context flow intentionally starts unresolved; pass a trusted tenant identity and establish a new scope.

Task.Run and ConfigureAwait(false) normally preserve ExecutionContext; tests must prove expected behavior rather than assuming either loses AsyncLocal. Test parallel requests, Task.Run within a live scope, suppressed flow, disposal while a child is pending, hosted services, async iterators at enumeration time, same-tenant nesting, rejection of different-tenant nesting, exceptions, and sequential reuse of singleton providers.

## 5. Lifecycle and propagation

Tenant-directory checks are opt-in through an abstraction, not a concrete in-memory store. Default mode trusts validated claims and provides no immediate suspension guarantee. Access-token expiry limits an already-issued token; it does not prevent refresh issuing another token. Claim issuance must consult lifecycle state if suspension is expected to stop new tokens.

When directory checks are enabled, cache status by tenant with configurable TTL (default 60 seconds). Deny inactive tenants with 403; directory unavailability without usable cached status returns 503. Define the suspension delay as TTL plus in-flight processing, rather than claiming immediate enforcement.

Forward a user access token only to an explicitly trusted downstream service for which it is valid; that service validates it independently. No ambient token forwarding to arbitrary destinations. Client credentials require a future V3_0 design and trusted app-client-to-tenant mapping.

For messages, authenticate/authorize the producer and protect tenant metadata against unauthorized replacement. A message attribute alone is not authentication. Consumers validate trusted metadata, then establish a bounded tenant scope. Concrete transport trust configuration must be reviewed per provider.

## 6. Database and repository enforcement

Add reusable `TenantEntity<TId>` implementing existing `ITenantScopedEntity<TId>`. Tenant assignment is controlled internally and immutable after insertion. Configure `(TenantId, Id)` as the primary key. Tenant-owned relationships use composite foreign keys; tenant-specific unique constraints include TenantId. Shared/reference entities are explicitly classified and excluded from tenant ownership rules.

Keep Core IRepository/IReadRepository/IWriteRepository signatures unchanged. Add `TenantEfRepository<TEntity, TId>` in the EF package implementing those interfaces and constrained to tenant-owned entities. Its one-ID methods obtain the current tenant internally; this is an implementation, not a decorator over an unsafe repository. Register it for tenant-owned entities in generated projects. Shared entities use separate ordinary repositories.

Every lookup, existence check, update, delete, and relationship resolution uses both tenant and entity ID. Routes may carry Id only. Prefer explicit filtered queries; any Find/FindAsync call must supply both configured key components. Prevent a tracked entity from another scope being returned. DbContext lifetime must not span changes to resolved tenant identity.

Use named EF Core 10 filters for tenant isolation and soft deletion. An unresolved context yields no tenant-owned rows. A resolved context yields only matching rows. Tenant filters coexist with domain filters. Selective soft-delete bypass may be authorized; ordinary tenant code cannot disable the tenant filter.

Enforce all synchronous/asynchronous SaveChanges overloads: reject missing context, stamp inserts, reject conflicting assignments, reject tenant mutation, and reject mismatched updates/deletes. Composite database predicates protect against attached-entity attacks. All tenant-owned writes without context throw `TenantContextRequiredException` from Core's exceptions package, mapped to 403 at the HTTP boundary.

Use ordinary AddDbContext; prohibit AddDbContextPool and pooled factories. Design-time factories may establish unresolved context for model construction but must not enable runtime tenant bypass.

### Failure classification

Define `TenantResourceNotFoundException` in Core's exceptions package. Generated exception middleware maps it to generic 404 Problem Details without exposing another tenant's existence.

On a zero-row tenant update/delete or DbUpdateConcurrencyException, inspect affected entries using a fresh database query under the same tenant predicate (not Find/tracked state). If the tenant-owned row is absent, translate to TenantResourceNotFoundException. A soft-deleted row counts as absent for ordinary CRUD. If it exists and the concurrency token differs, preserve a concurrency-conflict exception mapped to 409. Handle mixed-entry failures without partially committing; transactions roll back. Test both paths. Classification is based on visibility at the check and is not a guarantee against subsequent races.

## 7. Genesis strictness and compatibility

Use the existing per-provider `EnableTenantIsolation` option as the policy switch. When true, missing/unresolved tenant context rejects tenant-owned operations before external calls. When false, use documented non-tenant behavior. Do not add another overlapping strictness flag.

Caching and storage already default this option to true. Turning silent fallback into rejection is a behavior break: release affected Genesis packages as a major version, supply release notes, and require existing non-tenant applications to explicitly set false. Forge generates true for tenant services and false for non-tenant services. No silent compatibility fallback is permitted in strict mode.

Inventory every tenant-aware provider, including caching, storage, messaging, search, feature flags, AI, workflows, reporting, idempotency, and transaction logging. Verify lifetime, partitioning, unresolved behavior, propagation, and telemetry separately. The appendix does not claim a complete audit of these modules.

Resolved tenant supplies logging scopes, tracing tags, audit identity, cache namespaces, and rate-limit partitions. Avoid unbounded per-tenant metric labels.

## 8. Endpoint exemptions

Use endpoint metadata via an attribute or WithTenantExempt extension, not path lists. Authentication precedes tenant resolution, which precedes authorization and tenant-aware operations.

Tenant-exempt intentionally means tenant-neutral: ITenantContext.IsResolved is false even if a valid token contains tenant_id. Tenant-owned queries return empty; writes throw TenantContextRequiredException. This is deliberate for health endpoints. Diagnostic endpoints that require tenant identity must remain tenant-protected; they must not use this exemption.

AllowAnonymous does not imply exemption. Exemption does not imply AllowAnonymous. Test all combinations, including exempt endpoints supplied with valid tenant claims.

## 9. Forge validation, analyzers, and generation

Launchpad, API validation, and Engine validation reject tenancy without PostgreSQL or compatible Cognito JWT authentication. API-key-only tenancy and v1 client-credentials tenancy are invalid. Preserve non-tenant generation.

Generate Core package references, registrations, configuration, middleware ordering, tenant entity example, composite mappings, repository usage, design-time setup, tests, prerequisite checklist, and operational documentation. Replace duplicate local tenant contracts with Core contracts.

Ship Roslyn diagnostics with the EF slice for tenant application code:

- Direct ExecuteUpdate/ExecuteDelete, raw SQL/command execution, and tenant-filter bypass.
- Pooled context registration and single-key Find on tenant-owned entities.
- Tenant identifiers on generated request/input DTOs, including binding aliases; response DTOs may expose authorized tenant information.

Analyze symbol identities, overloads, and extension methods, not text matching. Generated projects enable diagnostics as errors in CI. Include positive and negative compilation fixtures. These diagnostics prevent supported source-level misuse; they do not sandbox arbitrary code or external libraries.

Escape hatch: narrowly scoped suppression with mandatory nonempty justification and review. CI rejects global diagnostic disabling and unjustified suppressions. Approved infrastructure wrappers must enforce tenant predicates and cannot modify tenant identity. A suppression does not create a runtime bypass or authorize cross-tenant work.

## 10. Existing-data migration deliverable

Ship an operator-run PostgreSQL migration tool and runbook, not only guidance. It must accept an explicit legacy-ID-to-GUID mapping, inspect declared tenant tables/relationships, and provide a dry-run report. Never invent mappings from string hashes or silently allocate tenants.

Validate completeness, duplicate targets/merges, orphan rows, FK consistency, and uniqueness conflicts before writes. Reject incomplete mappings and unintended many-to-one mappings. Require backup/restore and maintenance-window guidance. Backfill UUID shadow columns, validate counts and relationships, then replace keys/FKs and enforce non-null constraints within a documented transaction/locking strategy. Include application cutover and external partition-key migration steps for caches, object storage, search, and queued messages; database conversion alone is insufficient.

Deliver fixtures proving legacy mappings, existing GUID normalization, rollback on failure, repeat-run safety, orphan detection, and preservation of relationships. Do not run migration automatically at service startup. The actual table inventory and external-data cutover plan are deployment-specific prerequisites, not inferred from templates.

## 11. Delivery gates

| Slice | Gate |
|---|---|
| Core | Compiling sample; two-tenant resolution, singleton facade, background/async flow and exemption tests |
| EF + analyzers + migration tooling | PostgreSQL two-tenant reads/writes, all save overloads, attached entities, composite FKs, filters, concurrency classification, converter tests, diagnostic fixtures, migration fixtures |
| Forge | Enabled/disabled generated projects compile; incompatible combinations rejected at all entry points; enabled sample passes two-tenant tests |
| Genesis | Provider inventory completed; singleton regression, strict/non-tenant behavior and tenant partitioning tested per affected provider |
| Integration | Local signed-JWT issuer with discovery/JWKS in CI; separate scheduled real-Cognito verification |

CI includes expired/wrong-signature/wrong-issuer/wrong-client/wrong-token_use tokens, missing/wrong resource audience, malformed claim shapes, cancellation, exception cleanup, inherited background tasks, async enumeration, missing context, forged tenant writes, unrelated concurrency conflicts, migration backfill, and non-tenant compatibility.

Real Cognito tests verify trigger output and provisioning permissions. Local issuer tests cannot prove the remote user pool configuration is correct. Runtime isolation is not considered verified until these gates pass.

## 12. Outstanding implementation and deployment evidence

The six design questions are resolved above; the following evidence remains open:

1. Implement facade/scope API and prove the full async-flow matrix; AsyncLocal alone is not sufficient evidence.
2. Complete the Genesis provider inventory and major-version migration notes.
3. Implement tenant EF repository and update all generated lookup examples/tests.
4. Prove 404 versus 409 classification against PostgreSQL, including transactional failures.
5. Deliver migration tooling/tests and obtain each deployment's tenant/table/external-data inventory.
6. Verify exemption behavior, including diagnostics requirements, in generated integration tests.
7. **Mandatory Forge gate: raw-payload validation.** Core's middleware only sees the `ClaimsPrincipal`, which flattens a one-element JSON array into a plain string claim and hides duplicate JSON properties. Generated claims-based tenancy must therefore call `TenantClaimPayloadValidator.Validate` from the JwtBearer `TokenValidated` handler (using the token's raw payload and the configured claim name) and fail the request with 403 when the result is not `Resolved`. Generation of claims-based tenancy must not be enabled until the template includes this hook and generated tests prove that array-valued, one-element-array and duplicate-property tenant claims are rejected.
8. Scope handles (`ITenantScopeFactory.BeginScope`) must be disposed by the code that created them. Disposal from an inherited flow is not detectable; it only fails closed (the scope becomes unresolved) and never exposes another tenant. Accepted limitation for v1.

### Implementation status (2026-11-07)

| Area | State | Evidence |
|---|---|---|
| Core claims resolution, scopes, exemption | Done, committed (`865db46`) | Core test suite |
| Core EF package, tenant exceptions | Done, committed (`c06b7fa`) | 86 EF tests pass, including the 41 PostgreSQL 18 tests (SQLite and PostgreSQL) |
| Forge validation (Engine and API) | Done: PostgreSQL and Cognito required, API keys rejected | `MultiTenancyGenerationTests` |
| Forge validation (Launchpad) | Done: inline rules plus blocking prerequisite acknowledgment | `ngc` type check; no UI test written |
| Generated service | Done: Core packages, claims middleware, `TenantDbContext`, raw-payload guard (item 7), exception mapping, tenant tests, developer reference | Generated tenant service builds with 0 warnings and passes 104 tests; non-tenant service still passes 70 |
| Roslyn analyzers (section 9) | Not started | |
| Migration tool and runbook (section 10) | Not started | |
| Genesis inventory and major release (section 7) | Not started | |
| Local-issuer CI and real-Cognito checks (section 11) | Local issuer covered in generated tests; scheduled real-Cognito check not started | |
| Release | Generated services pin `Pervaxis.Core.MultiTenancy` and `Pervaxis.Core.Data.EntityFrameworkCore` 1.7.0, which are unpublished (nuget.org is at 1.6.0). Tag `v1.7.0` before generating tenant services. | |

Item 4: the 404 versus concurrency classification is tested on PostgreSQL in the Core EF package, and the generated middleware maps the results to 404 and 409. A test for mixed-entry failures rolling back has not been confirmed. Items 6 and 7 are verified in generated tests.

No production isolation guarantee is asserted by this document or the targeted source audit.

## Appendix A. Targeted source audit

Paths are relative to this document. Findings describe the local working tree, including pre-existing uncommitted work, on 2026-11-06. They are not tied to an immutable commit. References were spot-checked during consolidation. This is not a full security audit.

| Observation | Source |
|---|---|
| String-backed TenantId with non-whitespace constructor validation | [TenantId](../../Core/src/Pervaxis.Core.Abstractions/MultiTenancy/TenantId.cs) |
| Tenant entity contract already exposes typed TenantId | [ITenantScopedEntity](../../Core/src/Pervaxis.Core.Abstractions/Data/ITenantScopedEntity.cs) |
| Repository interfaces use one entity ID | [IReadRepository](../../Core/src/Pervaxis.Core.Abstractions/Data/IReadRepository.cs), [IWriteRepository](../../Core/src/Pervaxis.Core.Abstractions/Data/IWriteRepository.cs) |
| Header resolver/accessor/store registered; no ITenantContext registration in this method | [Core registrations](../../Core/src/Pervaxis.Core.MultiTenancy/ServiceCollectionExtensions.cs) |
| Concrete in-memory store dependency; unresolved requests continue | [Core middleware](../../Core/src/Pervaxis.Core.MultiTenancy/TenantMiddleware.cs) |
| Current accessor uses a static AsyncLocal containing an ITenantContext | [Accessor](../../Core/src/Pervaxis.Core.MultiTenancy/TenantContextAccessor.cs) |
| Generated context is separate; resolution precedes authentication | [Program template](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/src/__PROJECT__/Program.cstemplate.sbn) |
| Missing tenant permits all rows; only insert handling in one async save override | DbContext template under `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/src/__PROJECT__/Data/` (filename contains the Scriban class-name expression) |
| Existing registration uses AddDbContext, not pooling | [Database registrations](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/src/__PROJECT__/Infrastructure/Database/DatabaseServiceCollectionExtensions.cstemplate.sbn) |
| Generated net10.0; EF 10.0.8 and Npgsql 10.0.0 package pins | [Project](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/src/__PROJECT__/csproj.sbn), [Packages](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/Directory.Packages.props.sbn) |
| Engine requires database for tenancy, without PostgreSQL-specific check | [Validator](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Validation/ManifestValidator.cs) |
| Generic exception mapping exists and needs shared tenant exceptions | [Exception middleware](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/src/__PROJECT__/Infrastructure/Middleware/ExceptionHandlerMiddleware.cstemplate.sbn) |
| Cache singleton registration and captured optional context | [Registration](../../Genesis/src/Pervaxis.Genesis.Caching.AWS/Extensions/CachingServiceCollectionExtensions.cs), [Provider](../../Genesis/src/Pervaxis.Genesis.Caching.AWS/Providers/ElastiCache/ElastiCacheProvider.cs) |
| Cache/storage options already default EnableTenantIsolation to true | [Cache options](../../Genesis/src/Pervaxis.Genesis.Caching.AWS/Options/CachingOptions.cs), [Storage options](../../Genesis/src/Pervaxis.Genesis.FileStorage.AWS/Options/FileStorageOptions.cs) |
| Storage partitioning conditional on resolved context | [S3 provider](../../Genesis/src/Pervaxis.Genesis.FileStorage.AWS/Providers/S3/S3FileStorageProvider.cs) |
| Messaging/search/workflow also register singleton providers; behavior audit incomplete | [Messaging](../../Genesis/src/Pervaxis.Genesis.Messaging.AWS/Extensions/MessagingServiceCollectionExtensions.cs), [Search](../../Genesis/src/Pervaxis.Genesis.Search.AWS/Extensions/SearchServiceCollectionExtensions.cs), [Workflow](../../Genesis/src/Pervaxis.Genesis.Workflow.AWS/Extensions/WorkflowServiceCollectionExtensions.cs) |

Singleton registration plus a captured optional context establishes a lifetime hazard if supplied a scoped snapshot; it does not, by itself, prove a running deployment leaks tenants. DI scope-validation tests and runtime isolation tests must establish actual behavior.
