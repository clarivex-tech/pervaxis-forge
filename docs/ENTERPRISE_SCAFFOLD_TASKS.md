# Enterprise Scaffold Tasks

Use this file to track the production-readiness requirements for generated services.

## Status Legend

- `[ ]` Pending
- `[x]` Completed

## Taxonomy

### 1. Default Scaffold Baseline

These should be generated for every service by default.

- [ ] Security headers
  - Add baseline headers in the scaffold.
  - Include HSTS for non-development environments.
  - Include `X-Content-Type-Options`, `X-Frame-Options`, and `Referrer-Policy`.
  - Add CSP when the service serves browser traffic.

- [ ] CORS policy
  - Generate a real CORS policy in the scaffold.
  - Prefer explicit allowed origins.
  - Keep it configurable for browser-facing services.
  - Disable or narrow it by default for backend-only services.

- [ ] Performance baseline
  - Enable response compression where appropriate.
  - Configure EF Core connection pooling explicitly.
  - Include retry settings on DB connections where applicable.

### 2. Selectable Features

These are chosen during generation, similar to Genesis provider selection.

- [x] Authentication / authorization scaffold
  - Generate a concrete auth setup, not just `UseAuthorization()`.
  - Support a default auth path such as JWT, Cognito, or API key validation.
  - Fail closed by default for protected endpoints.

- [x] Secrets management integration
  - Avoid plain-text production secrets in appsettings.
  - Support AWS Secrets Manager and/or SSM Parameter Store.
  - Keep local development fallback support.
  - Bind secret-backed config through normal configuration patterns.

- [ ] Resilience / Polly wiring
  - Wire retries, timeouts, and circuit breakers into generated external calls.
  - Apply resilience to Genesis provider calls and other dependencies.
  - Make policy tuning configurable per service.
  - Do not leave resilience as documentation-only.

### 3. Conditional Features

These are enabled only when the service’s use case calls for them.

- [ ] Audit logging
  - Capture who did what and when.
  - Include actor, action, target, timestamp, and request context.
  - Use structured logs or an audit sink.
  - Treat this as required for admin, provisioning, or data-access services.

- [ ] PII and data classification guidance
  - Define handling rules beyond "do not log PII".
  - Add data classification levels such as public, internal, confidential, and restricted.
  - Provide redaction patterns for logs and events.
  - Document how classification affects storage and transport.

- [ ] Output caching
  - Support output caching as an opt-in capability.
  - Prefer read-heavy, stable-response services.
  - Avoid for write-heavy or highly dynamic endpoints.

- [ ] Rate limiting
  - Add request rate limiting middleware when a service is directly exposed.
  - Prefer gateway enforcement first when a gateway is present.
  - Provide sane defaults and per-service override.
  - Keep it lower priority than core security and resilience concerns.

## Generation Model

- [ ] The UI should expose baseline items as always-on defaults.
- [ ] The UI should expose selectable features as explicit generation choices.
- [ ] The UI should expose conditional features with help text and recommendation guidance.
- [x] The generator should emit the correct wiring, config, and defaults when a feature is selected.
- [ ] Manual integration should remain possible, but not required for production-sensitive concerns.

## Priority Order

1. Authentication / authorization
2. Secrets management
3. Resilience / Polly wiring
4. Security headers
5. Audit logging
6. CORS
7. PII / data classification
8. Output caching
9. Rate limiting
10. Performance defaults
