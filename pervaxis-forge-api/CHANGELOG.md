# Changelog

All notable changes to Pervaxis Forge API are documented here.

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).
Versioning follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Added

#### Cross-Cutting Infrastructure Layer (`feat(api): 11 production-readiness concerns`)

**RFC 7807 Exception Handling Middleware**
- Standardised `application/problem+json` responses with `type`, `title`, `status`, `detail`, `errorId`, `correlationId`
- Domain exception mapping: `ValidationException` → 400, `SlugConflictException` → 409, `OperationCanceledException` → 499, `BrokenCircuitException` → 503
- `Retry-After` header on 503 responses sourced from `ForgeResilienceOptions`
- Stack trace included in `detail` only in Development environment
- Unique `errorId` (UUID) per error for support correlation

**Correlation ID Propagation**
- `CorrelationIdMiddleware` extracts or generates `X-Correlation-ID` on every inbound request
- Validates incoming correlation IDs (must be valid GUID, ≤ 128 chars); generates new UUID if invalid
- Pushes correlation ID into `ILogger` scope — every log line carries `CorrelationId`
- `CorrelationIdHandler` (DelegatingHandler) injects `X-Correlation-ID` on all outbound HTTP calls
- Response always includes `X-Correlation-ID` header

**Structured Logging (Serilog)**
- Serilog replaces default `AddConsole()` as sole logging provider
- JSON-formatted structured log entries with enrichers: `ServiceName`, `Environment`, `MachineName`, `TraceId` (from OpenTelemetry span)
- Development: console sink with compact JSON formatting
- Production: AWS CloudWatch Logs sink via `AWS.Logger.SeriLog` (AWS Labs official package)
- Minimum levels and CloudWatch log group configurable via `appsettings.json`

**Resilience Pipeline (Microsoft.Extensions.Http.Resilience)**
- Per-client configurable retry (exponential backoff + jitter), circuit breaker, and timeout policies
- Circuit breaker logs state transitions (opened/closed/half-opened) at Warning level
- `ForgeResilienceOptions` (default thresholds) and `ForgeResilienceClientOptions` (per-client nullable overrides)
- GitHub client: 3 retries, CB opens after 3 failures in 60s, 120s break, 15s timeout
- Internal client: 2 retries, CB opens after 5 failures in 30s, 60s break, 10s timeout
- No Polly v7 dependency — uses modern `Microsoft.Extensions.Http.Resilience` APIs

**Outbound HTTP Logging**
- `HttpLoggingHandler` (DelegatingHandler) logs method, URI, status code, elapsed time, correlation ID
- Sensitive header redaction: `Authorization`, `X-Api-Key`, and any `*-Token` header replaced with `[REDACTED]`
- Registered on all typed HTTP clients

**Security Headers Enhancement**
- `SecurityHeadersMiddleware` adds via `OnStarting` callback (applies on error responses too):
  - `Content-Security-Policy: default-src 'self'` (preserved if already set by endpoint)
  - `Permissions-Policy: geolocation=(), microphone=()`
  - `X-Permitted-Cross-Domain-Policies: none`
  - Retains existing: `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, `HSTS` (non-dev)
- No NWebsec dependency — raw header manipulation

**FluentValidation Integration**
- `ValidationEndpointFilter` automatically resolves and runs `IValidator<T>` for all endpoint request DTOs
- Aggregates ALL validation failures before throwing — no "fix one error at a time" UX
- Validators discovered via assembly scanning (`AddValidatorsFromAssemblyContaining<Program>()`)
- `WithValidation()` extension applies filter to any `RouteGroupBuilder`
- Forwards `CancellationToken` to validators

**OpenTelemetry Distributed Tracing**
- Automatic span creation for inbound HTTP requests, outbound HTTP calls, and EF Core operations
- W3C TraceContext propagation on all outbound requests
- OTLP exporter to configurable endpoint (default `http://localhost:4317`, AWS X-Ray compatible)
- Console exporter in Development for local debugging
- `TraceId` from `Activity.Current` enriches every Serilog log entry via `Serilog.Enrichers.Span`

**Typed HTTP Client Separation**
- `IGitHubHttpClient` / `GitHubHttpClient` — GitHub API calls with dedicated resilience config
- `IForgeInternalHttpClient` / `ForgeInternalHttpClient` — internal platform calls with independent config
- Both clients share `CorrelationIdHandler` → `HttpLoggingHandler` → resilience pipeline chain

**CancellationToken Propagation**
- Verified throughout all async paths: endpoint handlers, service methods, EF Core queries, HTTP calls
- `HttpContext.RequestAborted` automatically bound to endpoint handler `CancellationToken` parameters

**API Versioning (Asp.Versioning.Http)**
- URI segment versioning: `/api/v{version}/resource`
- Default version: 1.0 (assumed when unspecified)
- `api-supported-versions` and `api-deprecated-versions` response headers
- Swagger/OpenAPI integration via `Asp.Versioning.Mvc.ApiExplorer`
- All existing endpoints grouped under v1 with no behaviour changes

#### Configuration

New `appsettings.json` sections:
- `Serilog` — minimum levels, CloudWatch log group and stream prefix
- `Resilience.Default` / `Resilience.GitHub` / `Resilience.Internal` — per-client resilience thresholds
- `OpenTelemetry.OtlpEndpoint` — OTLP collector endpoint
- `GitHub.BaseUrl` — GitHub API base URL

#### New NuGet Packages
- `Serilog.AspNetCore`, `Serilog.Formatting.Compact`, `Serilog.Enrichers.Span`, `Serilog.Enrichers.Environment`
- `AWS.Logger.SeriLog` (AWS Labs official)
- `Microsoft.Extensions.Http.Resilience`
- `FluentValidation.DependencyInjectionExtensions`
- `OpenTelemetry.Extensions.Hosting`, `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Instrumentation.EntityFrameworkCore`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`, `OpenTelemetry.Exporter.Console`
- `Asp.Versioning.Http`, `Asp.Versioning.Mvc.ApiExplorer`

### Changed

- `Program.cs` — replaced inline exception handler and security headers with dedicated middleware; added all new service registrations; middleware pipeline order enforced
- `Pervaxis.Forge.Api.csproj` — added 14 new package references
- `appsettings.json` — added `Serilog`, `Resilience`, `OpenTelemetry`, `GitHub` configuration sections

### Infrastructure

- Spec: `.kiro/specs/forge-cross-cutting-concerns/` (requirements, design, tasks)
- Tests: 88/89 passing (1 pre-existing Scriban template failure unrelated to this change)
- Build: 0 warnings, 0 errors

---

## Previous Releases

See git history for changes prior to this changelog.
