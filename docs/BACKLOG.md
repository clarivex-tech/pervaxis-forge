# Forge Platform Backlog

Items tracked for future implementation. Prioritized by impact.

---

## Backend Template Reference Modules

### REST API Template (`Templates/rest-api/`)
- [ ] Reference controller (endpoint patterns, DTOs, validation)
- [ ] Reference service layer (business logic separation)
- [ ] Reference repository (EF Core / DynamoDB data access)
- [ ] Kiro steering file (thin controllers, service layer, result pattern)
- [ ] VERTICAL_HANDBOOK.md (endpoint naming, error format, pagination)
- [ ] Production readiness audit + fixes (same process as Angular templates)

### GraphQL Template (`Templates/graphql/`)
- [ ] Reference query/mutation (resolver patterns, input/return types)
- [ ] Reference data loader (N+1 prevention)
- [ ] Kiro steering file (nullability, pagination via connections, field auth)
- [ ] VERTICAL_HANDBOOK.md (schema conventions, error handling)
- [ ] Production readiness audit + fixes

### gRPC Template (`Templates/grpc/`)
- [ ] Reference .proto file (message/service definition conventions)
- [ ] Reference service implementation (generated interface pattern)
- [ ] Kiro steering file (proto naming, error codes, streaming)
- [ ] VERTICAL_HANDBOOK.md (proto versioning, backward compatibility)
- [ ] Production readiness audit + fixes

### REST Print Hardening — Deferred Review Findings
- [ ] Rate limiting: enabled by default with sensible limits; distributed limits across instances (Redis or API gateway). Until then, document that limits are per instance and off by default.
- [ ] Business audit trail: business action names, target record IDs and before/after values (who changed which record), as a Core `IAuditLogger` package. Prints already log one request-level audit entry per request with actor ID and outcome.
- [ ] Application data encryption: field-level encryption with KMS-managed keys; decide searchability and key rotation. Database encryption at rest covers the baseline meanwhile.
- [ ] Data classification enforcement: field-level PII/sensitivity tags that drive redaction, exports, AI prompts and encryption (shared Core design with encryption). Until then, document that classification is a label only.

- [ ] gRPC template: apply the same retry safety (safe methods or `Idempotency-Key` only) to its `ForgeRetryDelegatingHandler`.
- [ ] Redaction when Serilog is disabled: the redaction enricher only runs on the Serilog pipeline.

### Genesis Follow-ups (from OData/Sanitization fixes, PR #212)
- [ ] Rebuild and republish Genesis against Core 1.8.0, then remove the Core pins from `Directory.Packages.props.sbn` (NU1603 workaround).
- [ ] `QueryValidator` rejects negative `$top`/`$skip`, then remove `ODataPagingGuardFilter` from prints.
- [ ] Sanitization middleware fails closed on over-long input, then reconsider `JsonBodyLimitMiddleware`.
- [ ] Sanitization `PlainText` mode must not alter passwords or free text, so middleware can be enabled by default.
- [ ] Update fallback Genesis versions (`PrintGenerator` 3.2.0, `NuGetVersionResolver` 3.4.1) to a version that includes Sanitization (3.4.4+).
- [ ] Multi-tenancy audit: per-module tenant source, provider lifetimes, partitioning (Search holds `ITenantContext?` in a singleton); OData `EnableTenantIsolation` does not filter rows.

### Search
- [ ] OpenSearch ingestion pattern: outbox-driven indexer, full reindex job, alias swap for zero-downtime rebuilds.

---

## Angular Template Deferred Items

### Angular Monolith (`Templates/angular-monolith/`)
- [ ] Real IdP integration example (Cognito/Auth0/Azure AD)
- [ ] Wire ErrorReporter to a real service (Sentry/Datadog)
- [ ] Remote log shipping in LoggerService
- [ ] CD pipeline (Docker push, env promotion)
- [ ] Core service unit tests (LoadingService, LoggerService, AuthService)
- [ ] E2E test setup (Playwright or Cypress)
- [ ] Route preloading strategy
- [ ] CSP header in nginx (supplement meta tag)
- [ ] Health check endpoint stub
- [ ] CI coverage threshold gate

### MFE Shell (`Templates/angular-shell/`)
- [ ] Add `ChangeDetectionStrategy.OnPush` to app.component and home.component
- [ ] Improve app.config.spec.ts with real provider assertions
- [ ] Clean up auth.interceptor.ts — wire to CanvasAuthModule
- [ ] Add Canvas auth redirect on 401 in error.interceptor.ts
- [ ] Document deploy.yml placeholder in README.md

### MFE App (`Templates/angular-microfrontend/`)
- [ ] CommonModule → individual directives in component
- [ ] DomainItem index signature → proper typed placeholder
- [ ] Add `start` script to package.json
- [ ] Document .npmrc token setup for local dev in README
- [ ] Expand SPEC.md with API routes, permissions, i18n namespaces
- [ ] Configure @nx/enforce-module-boundaries
- [ ] Prettier: add `"endOfLine": "lf"`
- [ ] ESLint: add cyclomatic complexity rules
- [ ] a11y ESLint plugin

---

## Platform Improvements
- [ ] Canvas module version matrix documentation
- [ ] Forge Engine: support binary asset embedding (favicon.ico without .sbn)
- [ ] Property-based tests (FsCheck) for all template correctness properties
- [ ] Integration tests for full print generation pipeline
