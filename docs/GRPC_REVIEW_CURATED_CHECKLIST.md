# gRPC Service Scaffold Curated Checklist

Scope:
- .NET 10 / ASP.NET Core gRPC / AWS / OpenTelemetry
- Service scaffold review for `Pervaxis.Mat.Searchg`
- Goal: fix production blockers first, then performance, then standards and CI rigor

Status legend:
- `[ ]` Pending
- `[x]` Done

## 1. Blockers

- [x] Fix CI so it runs the test project, not the application project.
  - Target `tests/Pervaxis.Mat.Searchg.Tests/Pervaxis.Mat.Searchg.Tests.csproj`.
  - Ensure coverage is actually produced and gated.

- [x] Fix observability timing so success is not recorded before real work completes.
  - Record RPC result and duration after the operation finishes.
  - Capture success or failure based on the actual outcome.

- [x] Fix encoding corruption in `Program.cs`.
  - Replace mojibake in the root endpoint message.
  - Prefer plain ASCII or clean UTF-8 text.

## 2. Security

- [x] Remove `dev-api-key` from committed development config.
  - Use `dotnet user-secrets` for local developer auth values.
  - Keep source-controlled config free of secret-like placeholders.

- [x] Remove or redact user-supplied PII from trace tags.
  - Avoid writing `request.Name` directly into telemetry.
  - Use hashing, truncation, or redaction helpers instead.

- [x] Make the gRPC health endpoint anonymous.
  - Apply `.AllowAnonymous()` to the health check mapping.
  - Keep kube/ALB probes from needing API-key auth.

- [x] Add rate limiting for authenticated callers.
  - Use a per-key policy instead of relying on auth alone.
  - Prefer a sliding-window limiter with sane defaults.

- [x] Remove browser-only HTTP headers from the gRPC surface.
  - Drop `X-Frame-Options` and `Referrer-Policy` from the gRPC path.
  - Keep browser headers only where an HTTP/browser edge exists.

- [x] Document and scaffold mTLS or a mesh pattern for east-west calls.
  - Prefer service mesh identity or mTLS over API keys for service-to-service traffic.
  - Add a concrete pattern in the service guidance.

## 3. Performance and Observability

- [x] Export metrics via OTLP.
  - Add `.AddOtlpExporter()` to the metrics pipeline.
  - Verify dashboards receive metrics from the service.

- [x] Use explicit histogram boundaries for millisecond duration metrics.
  - Define sensible millisecond buckets.
  - Keep percentiles meaningful.

- [x] Add jitter to retry backoff.
  - Avoid synchronized retry spikes.
  - Use randomized delay instead of pure exponential growth.

- [x] Resolve the dead `AddForgeResilience()` path.
  - Either wire the HTTP client to use it or delete it.
  - Do not ship a config class that is never exercised.

- [x] Demonstrate `CancellationToken` propagation in the scaffold.
  - Pass `context.CancellationToken` to downstream async work.
  - Show the pattern in service implementation guidance.

## 4. Code Quality

- [x] Add `Moq` to the test project.
  - Match the developer guide’s test guidance.
  - Prevent compile failures for future unit tests.

- [x] Rename private static fields to follow C# conventions.
  - Prefer `_activitySource`, `_meter`, `_rpcCounter`, `_rpcDuration`.
  - Avoid names that shadow framework types.

- [x] Add `global.json` at the repo root.
  - Pin the SDK version for local and CI consistency.
  - Avoid surprise behavior from SDK drift.

- [ ] Pin core package versions more tightly.
  - Avoid wildcard major version ranges.
  - Prefer minor/patch pinning or central package management.

- [x] Extract shared build properties into `Directory.Build.props`.
  - Remove duplicated `Nullable`, `ImplicitUsings`, `LangVersion`, and warning settings.
  - Keep project files aligned.

- [x] Add copyright headers to files that are missing them.
  - Apply the standard Clarivex header to the remaining scaffold files.

- [x] Add a solution file.
  - Let CI and IDEs target both app and test projects together.
  - Simplify `dotnet test` and package management.

## 5. CI Rigour

- [x] Enforce the coverage threshold in CI.
  - Make the 90% gate real, not just documented.
  - Fail builds when coverage drops below the threshold.

- [x] Remove unnecessary `fetch-depth: 0` from build-only jobs.
  - Keep the workflow lean.
  - Only fetch full history when a job actually needs it.

- [x] Add liveness/readiness checks for critical dependencies.
  - Register DB and other critical dependency checks.
  - Avoid a health endpoint that always reports healthy.

## 6. Suggested Order

1. Fix CI test targeting.
2. Fix observability timing and encoding corruption.
3. Remove security footguns.
4. Add missing performance and retry improvements.
5. Tighten code quality and project structure.
6. Enforce CI thresholds and dependency health checks.
