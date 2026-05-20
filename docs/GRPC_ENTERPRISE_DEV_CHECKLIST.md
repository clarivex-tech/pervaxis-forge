# gRPC Enterprise Dev Checklist

Use this checklist to harden the gRPC scaffold for an internal east-west service profile.

Scope:
- High-throughput, low-latency internal service.
- gRPC first, not browser-facing REST.
- Shared persistence and service-to-service dependencies.

Status legend:
- `[ ]` Pending
- `[x]` Done

## 1. Fix First

These items are either confirmed defects or missing baseline files that block a clean gRPC scaffold.

- [x] Remove duplicate `using` directives in `Program.cs`.
  - Confirm the generated file only imports each namespace once.

- [x] Add `appsettings.json`.
  - Include the base configuration shape required by the gRPC template.
  - Keep production secrets out of source-controlled plaintext values.

- [x] Add `appsettings.Production.json`.
  - Define production-only defaults for deployment-time configuration.

- [x] Verify the generated `Models/Configuration` files are embedded and emitted correctly.
  - Confirm the template produces the expected configuration types.
  - Check that namespace and file naming match the generator conventions.

- [x] Verify the generated `Services` files are embedded and emitted correctly.
  - Confirm service helpers and any cross-cutting service classes are generated.

- [x] Add a tests project for the gRPC scaffold.
  - Include unit or integration coverage for generated service behavior.

- [x] Add a Dockerfile.
  - Support containerized execution of the generated gRPC service.

- [ ] Add a CI workflow.
  - Ensure the scaffold has a repeatable validation path.

## 2. gRPC Runtime Baseline

These are the service-level primitives the template should include for production readiness.

- [x] Add gRPC health checks.
  - Use the gRPC health protocol for platform probing and service readiness.

- [x] Add graceful shutdown handling.
  - Drain in-flight requests before process exit.
  - Avoid hard-killing active calls on deploy.

- [x] Add outbound resilience for internal service calls.
  - Use a standard resilience handler or equivalent policy for HTTP-based dependencies.

- [ ] Configure gRPC performance settings explicitly.
  - Set keepalive timing and timeout behavior.
  - Configure message size and stream limits as needed.
  - Document compression decisions instead of leaving them implicit.

- [ ] Enable database connection pooling where persistence is present.
  - Use pooled DbContext registration if the service owns a DbContext.
  - Keep retry behavior explicit.

## 3. Security Model

These items should reflect the internal service trust model rather than the REST/browser baseline.

- [ ] Document the auth model for east-west traffic.
  - Prefer mesh identity or mTLS at the transport layer.
  - Avoid API key auth as the primary long-lived service identity mechanism.

- [ ] Gate HSTS by deployment topology.
  - Keep it if TLS terminates at the service.
  - Treat it as redundant if the mesh or edge terminates TLS.

- [ ] Remove browser-only security assumptions where they do not apply.
  - Do not treat CORS as a runtime requirement for pure gRPC east-west traffic.
  - Do not treat HTML security headers as a core gRPC primitive.

## 4. Observability

These controls are important for service-to-service debugging and production support.

- [x] Add distributed tracing with OpenTelemetry.
  - Propagate trace context across every hop.
  - Use gRPC-aware instrumentation.

- [x] Add gRPC-specific metrics.
  - Track request volume, failures, latency, and gRPC status codes.
  - Do not rely only on HTTP status code reporting.

- [x] Keep audit logging as a first-class concern where appropriate.
  - Preserve structured request context.
  - Capture actor, action, target, and trace correlation data.

## 5. Developer Experience

These items make the service easier to inspect, debug, and extend.

- [x] Add gRPC reflection for development only.
  - Make grpcurl and Postman inspection possible in non-production environments.

- [x] Version the proto package.
  - Use a versioned namespace boundary such as `.v1` for breaking changes.

- [x] Use gRPC interceptors for cross-cutting concerns.
  - Prefer interceptors for logging, auth, and tracing.
  - Keep middleware only for concerns that truly belong at the ASP.NET pipeline level.

- [x] Verify generated file paths and namespaces.
  - Check for namespace suffix mismatches such as missing `.Extensions` imports.
  - Keep generator output consistent with the source template layout.

## 6. Execution Order

Suggested fix order:

1. Fix template compile and generation defects.
2. Add missing config and packaging files.
3. Add service readiness and shutdown primitives.
4. Add observability primitives.
5. Add developer experience improvements.
6. Revisit any profile-specific security decisions after the scaffold is runnable.
