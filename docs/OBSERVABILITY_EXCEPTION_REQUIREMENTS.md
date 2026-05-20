# Observability & Exception Handling Requirements

## Status Legend

- `[ ]` Pending
- `[x]` Completed

---

## 1. Exception Model — Pervaxis.Core.Exceptions

- [ ] Create `ClarivexException` in a new `Pervaxis.Core.Exceptions` NuGet package
  - `ExceptionId` — generated in constructor as `ex_{Guid:N}[..16]`, `init`-only, never settable externally
  - `TenantId` — nullable, `init`-only
  - `CorrelationId` — nullable, `init`-only; populated from `Activity.Current?.TraceId` in middleware, not by callers
  - `ServiceName` — required, `init`-only
  - `OperationName` — required, `init`-only
  - `Severity` — `SeverityLevel` enum (`Info`, `Warning`, `Error`, `Critical`)
  - `Context` — `Dictionary<string, object>?` for arbitrary structured metadata
  - `Timestamp` — `DateTimeOffset`, set to `UtcNow` in constructor, `init`-only
  - Do NOT include `RetryCount` — execution context belongs in the log payload, not the exception model
- [ ] Define `SeverityLevel` enum in the same package
- [ ] Publish as `Pervaxis.Core.Exceptions` to the Pervaxis NuGet feed

---

## 2. Exceptions Table — Supabase

- [ ] Create `exceptions` table with the following columns:

  | Column | Type | Notes |
  |---|---|---|
  | `exception_id` | `text` | PK |
  | `tenant_id` | `text` | Indexed |
  | `correlation_id` | `text` | Indexed |
  | `service_name` | `text` | Indexed |
  | `operation_name` | `text` | Indexed |
  | `severity` | `text` | |
  | `message` | `text` | |
  | `stack_trace` | `text` | |
  | `context` | `jsonb` | Full `Context` dictionary |
  | `timestamp` | `timestamptz` | |

- [ ] Index on `(tenant_id, correlation_id)` for support lookups
- [ ] Index on `(service_name, operation_name)` for agentic pattern queries

---

## 3. Shared Exception Handler — Pervaxis.Core

- [ ] Create `ClarivexExceptionHandler` class in Core (shared across all service types)
  - Populates `CorrelationId` from `Activity.Current?.TraceId.ToString()`
  - Logs structured exception entry to CloudWatch with `ExceptionId`, `TenantId`, `CorrelationId`, `ServiceName`, `OperationName`, `Severity`
  - Inserts row into Supabase `exceptions` table
  - Returns a wire-format-neutral result for the service adapter to render

---

## 4. Service Adapters — One Per Print Type

Each Forge-generated service wires a thin adapter that calls `ClarivexExceptionHandler` and renders the result in the correct protocol format.

### REST API

- [ ] Wire `UseExceptionHandler` middleware in `Program.cs` template
- [ ] Return `ProblemDetails` JSON response:
  ```json
  {
    "error": {
      "exceptionId": "ex_abc123xyz",
      "message": "Intake validation failed",
      "timestamp": "2026-05-20T10:30:00Z"
    }
  }
  ```
- [ ] Set `X-Exception-Id` response header with the `exceptionId` value
- [ ] Do NOT include `severity` in the client response — CloudWatch only

### GraphQL

- [ ] Implement `IErrorFilter` adapter
- [ ] Return `exceptionId` in the GraphQL `errors[]` extensions field
- [ ] Set `X-Exception-Id` response header

### gRPC

- [ ] Implement `UnaryServerHandler` interceptor adapter
- [ ] Return `StatusCode.Internal` with `exceptionId` in gRPC trailing metadata
- [ ] Do NOT return HTTP headers for gRPC — metadata only

---

## 5. Forge Template Wiring

- [ ] REST API template auto-wires the exception middleware
- [ ] GraphQL template auto-wires the error filter
- [ ] gRPC template auto-wires the server interceptor
- [ ] All three templates reference `Pervaxis.Core.Exceptions` package
- [ ] All three templates reference `Pervaxis.Core` shared handler

---

## 6. Distributed Tracing — OpenTelemetry

- [ ] Add `AddOpenTelemetry()` to all three service templates (REST, GraphQL, gRPC)
- [ ] Instrument HTTP, gRPC, and EF Core automatically
- [ ] Export to CloudWatch (via OTLP or AWS X-Ray exporter)
- [ ] `CorrelationId` on `ClarivexException` is always populated from `Activity.Current?.TraceId` — no manual propagation required
- [ ] Single `TraceId` follows the full request chain: REST → gRPC → DB → CloudWatch

---

## 7. Support Workflow

- [ ] Support team queries `exceptionId` in CloudWatch → full request lifecycle (calls, DB ops, retries)
- [ ] Support team queries Supabase `exceptions` table by `tenant_id` or `correlation_id` for tenant-scoped history
- [ ] Future: Agentic support system accepts `exceptionId` → queries CloudWatch + Supabase → diagnoses root cause automatically

---

## 8. Forge API — Self-Implementation

The Forge API itself must implement the same exception handling and observability stack once the prerequisites are in place. This is blocked on sections 1–3 above.

**Prerequisites before starting:**
- `Pervaxis.Core.Exceptions` published to the Pervaxis NuGet feed
- `Pervaxis.Core` shared handler available
- Supabase `exceptions` table provisioned

**Work items:**
- [ ] Add `Pervaxis.Core.Exceptions` and `Pervaxis.Core` package references to `Pervaxis.Forge.Api.csproj`
- [ ] Wire `UseExceptionHandler` middleware in Forge API `Program.cs`
- [ ] Return `ProblemDetails` with `exceptionId` + `X-Exception-Id` header on all unhandled exceptions
- [ ] Add `AddOpenTelemetry()` with HTTP and EF Core instrumentation to Forge API
- [ ] Verify `exceptionId` appears in Forge API CloudWatch logs and Supabase `exceptions` table

> **Order:** Implement in generated prints first. Migrate Forge API after prints are validated in production.

---

## Scope

| Service type | Exception model | Adapter | OTel tracing |
|---|---|---|---|
| REST API (print) | `[ ]` | `[ ]` | `[ ]` |
| GraphQL (print) | `[ ]` | `[ ]` | `[ ]` |
| gRPC (print) | `[ ]` | `[ ]` | `[ ]` |
| Forge API | `[ ]` | `[ ]` | `[ ]` |
