# Requirements Document

## Introduction

This document specifies the requirements for adding infrastructure toggle controls to the Forge Launchpad Angular UI's service generation wizard. The API already accepts `AuthConfig`, `ResilienceConfig`, `ObservabilityConfig`, `ValidationConfig`, `BackgroundJobConfig`, `UtilitiesConfig`, and `MultiTenancy` fields on `GenerationRequest` — this spec covers the UI-only work to present those toggles and include them in the request payload.

The key constraint is **backward-compatible UX**: when all toggles are at their default values, the submitted request is functionally identical to today's request (the API treats null/default as "generate everything that was generated before"). This ensures existing users see no behavioral change unless they explicitly modify toggles.

## Glossary

- **Generation_Wizard**: The Angular component (`GenerationWizardV2Component`) that implements the 7-step service generation form in the Forge Launchpad UI
- **Generation_Form**: The reactive form (`FormGroup`) within the Generation_Wizard that collects all user inputs for service generation
- **Infrastructure_Options_Section**: A new collapsible UI section within the Generation_Wizard that groups all infrastructure toggle controls
- **Toggle_Group**: A labeled subsection within the Infrastructure_Options_Section that groups related toggles (e.g., Authentication, Resilience, Observability)
- **Generation_Request**: The TypeScript interface (`GenerationRequest`) representing the JSON payload sent to the Forge API's generation endpoint
- **AuthConfig**: The API configuration record controlling authentication concern generation with properties `apiKeyEnabled`, `jwtEnabled`, `mtlsEnabled`
- **ResilienceConfig**: The API configuration record controlling resilience concern generation with properties `retryEnabled`, `circuitBreakerEnabled`, `timeoutEnabled`, `internalHttpClient`, `externalHttpClient`
- **ObservabilityConfig**: The API configuration record controlling observability concern generation with properties `serilogEnabled`, `cloudWatchEnabled`, `openTelemetryEnabled`, `correlationIdEnabled`, `prometheusEnabled`
- **ValidationConfig**: The API configuration record controlling validation concern generation with property `fluentValidationEnabled`
- **BackgroundJobConfig**: The API configuration record controlling background job concern generation with property `provider`
- **UtilitiesConfig**: The API configuration record controlling utility service generation with properties `pdfEnabled`, `templateEngineEnabled`, `hashingEnabled`
- **Coming_Soon_Toggle**: A toggle control that is visible but disabled, indicating a planned feature not yet available for generation
- **Generation_API_Service**: The Angular service (`IGenerationApiService`) responsible for sending the Generation_Request to the Forge API

## Requirements

---

### Requirement 1: Infrastructure Options Section Layout

**User Story:** As a platform engineer using the Forge Launchpad, I want to see an organized "Infrastructure Options" section on the generation form, so that I can quickly understand and configure which cross-cutting concerns will be included in my generated scaffold.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display an "Infrastructure Options" section as a collapsible panel within the generation form, positioned between the "Production Readiness" step and the "Deployment Settings" step
2. WHEN the Infrastructure_Options_Section is collapsed, THE Generation_Wizard SHALL display a summary indicator showing how many toggles differ from their defaults
3. WHEN the Infrastructure_Options_Section is expanded, THE Generation_Wizard SHALL display all Toggle_Groups in the following order: Authentication, Resilience, Observability, Multi-Tenancy, Validation, Background Jobs, Utilities
4. THE Infrastructure_Options_Section SHALL default to the collapsed state on initial form load

---

### Requirement 2: Authentication Toggle Group

**User Story:** As a platform engineer, I want to select which authentication mechanisms are included in my generated service, so that I can tailor the auth stack to my service's requirements.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display an "Authentication" Toggle_Group containing three checkbox controls: "API Key" (default checked), "JWT / OIDC Auth0" (default unchecked), and "mTLS" (default unchecked)
2. THE "mTLS" checkbox SHALL be rendered in a disabled state with a "Coming Soon" label visible to the user
3. WHEN the user modifies Authentication toggle values, THE Generation_Form SHALL update its internal state to reflect the selected combination
4. THE Authentication Toggle_Group SHALL allow any combination of enabled checkboxes (API Key and JWT can both be checked simultaneously)

---

### Requirement 3: Resilience Toggle Group

**User Story:** As a platform engineer, I want to configure which resilience patterns are included in my generated service, so that simple services without outbound HTTP calls do not carry unused retry and circuit-breaker infrastructure.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display a "Resilience" Toggle_Group containing three checkbox controls: "Retry + Circuit Breaker + Timeout" (default checked), "Typed HTTP Clients — Internal" (default checked), and "Typed HTTP Clients — External" (default unchecked)
2. WHEN the user modifies Resilience toggle values, THE Generation_Form SHALL update its internal state to reflect the selected combination
3. WHEN "Typed HTTP Clients — External" is checked AND no resilience toggle ("Retry + Circuit Breaker + Timeout") is checked, THE Generation_Wizard SHALL display a validation warning indicating that External HTTP Client requires at least one resilience toggle enabled

---

### Requirement 4: Observability Toggle Group

**User Story:** As a platform engineer, I want to configure which observability components are included in my generated service, so that lightweight internal tools do not carry full OpenTelemetry and CloudWatch infrastructure when console logging suffices.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display an "Observability" Toggle_Group containing four checkbox controls: "Serilog + CloudWatch" (default checked), "OpenTelemetry + OTLP" (default checked), "Correlation ID" (default checked), and "Prometheus Metrics" (default unchecked)
2. THE "Prometheus Metrics" checkbox SHALL be rendered in a disabled state with a "Coming Soon" label visible to the user
3. WHEN "Serilog + CloudWatch" is unchecked, THE Generation_Wizard SHALL visually indicate that CloudWatch depends on Serilog by disabling the CloudWatch portion or displaying an informational note
4. WHEN the user modifies Observability toggle values, THE Generation_Form SHALL update its internal state to reflect the selected combination

---

### Requirement 5: Multi-Tenancy Toggle Group

**User Story:** As a platform engineer, I want to enable or disable multi-tenancy support for my generated service, so that single-tenant services do not carry tenant middleware and tenant-aware DbContext infrastructure.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display a "Multi-Tenancy" Toggle_Group containing a radio control with two options: "Enabled" and "Disabled" (default selected: "Disabled")
2. WHEN "Enabled" is selected AND no database is configured in the Deployment Settings step, THE Generation_Wizard SHALL display a validation warning indicating that Multi-Tenancy requires a database configuration
3. WHEN the user selects a Multi-Tenancy option, THE Generation_Form SHALL update its internal state to reflect the selection

---

### Requirement 6: Validation Toggle Group

**User Story:** As a platform engineer, I want to optionally exclude FluentValidation from my generated service, so that services with trivial request shapes do not carry the FluentValidation dependency.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display a "Validation" Toggle_Group containing a radio control with two options: "FluentValidation" (default selected) and "None"
2. WHEN the user selects a Validation option, THE Generation_Form SHALL update its internal state to reflect the selection

---

### Requirement 7: Background Jobs Toggle Group

**User Story:** As a platform engineer, I want to select a background job provider for my generated service, so that services can opt into EventBridge-scheduled or SQS-Lambda-triggered background processing only when needed.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display a "Background Jobs" Toggle_Group containing a radio control with three options: "EventBridge Scheduler", "SQS + Lambda", and "None" (default selected: "None")
2. WHEN the user selects a Background Jobs option, THE Generation_Form SHALL update its internal state to reflect the selection

---

### Requirement 8: Utilities Toggle Group

**User Story:** As a platform engineer, I want to select which utility services are included in my generated service, so that services only carry the utility dependencies they actually use.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL display a "Utilities" Toggle_Group containing three checkbox controls: "PDF Generation" (default unchecked), "Template Engine" (default unchecked), and "Encryption / Hashing" (default unchecked)
2. THE "PDF Generation" checkbox SHALL be rendered in a disabled state with a "Coming Soon" label visible to the user
3. THE "Template Engine" checkbox SHALL be rendered in a disabled state with a "Coming Soon" label visible to the user
4. WHEN the user modifies Utilities toggle values, THE Generation_Form SHALL update its internal state to reflect the selected combination

---

### Requirement 9: TypeScript Model Extension

**User Story:** As a frontend developer, I want the `GenerationRequest` TypeScript interface to include optional infrastructure toggle fields, so that the UI can send toggle selections to the API without type errors.

#### Acceptance Criteria

1. THE Generation_Request interface SHALL include an optional `auth` property of type `{ apiKeyEnabled: boolean; jwtEnabled: boolean; mtlsEnabled: boolean } | null`
2. THE Generation_Request interface SHALL include an optional `resilience` property of type `{ retryEnabled: boolean; circuitBreakerEnabled: boolean; timeoutEnabled: boolean; internalHttpClient: boolean; externalHttpClient: boolean } | null`
3. THE Generation_Request interface SHALL include an optional `observability` property of type `{ serilogEnabled: boolean; cloudWatchEnabled: boolean; openTelemetryEnabled: boolean; correlationIdEnabled: boolean; prometheusEnabled: boolean } | null`
4. THE Generation_Request interface SHALL include an optional `validation` property of type `{ fluentValidationEnabled: boolean } | null`
5. THE Generation_Request interface SHALL include an optional `backgroundJobs` property of type `{ provider: string | null } | null`
6. THE Generation_Request interface SHALL include an optional `utilities` property of type `{ pdfEnabled: boolean; templateEngineEnabled: boolean; hashingEnabled: boolean } | null`
7. THE Generation_Request interface SHALL include an optional `multiTenancy` property of type `boolean`

---

### Requirement 10: Form-to-Request Mapping

**User Story:** As a platform engineer, I want my toggle selections to be correctly included in the generation request payload, so that the API receives the exact configuration I specified and generates the appropriate scaffold.

#### Acceptance Criteria

1. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Authentication toggle states to an `AuthConfig` object with `apiKeyEnabled`, `jwtEnabled`, and `mtlsEnabled` properties set to their respective checkbox values
2. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Resilience toggle states to a `ResilienceConfig` object with `retryEnabled`, `circuitBreakerEnabled`, and `timeoutEnabled` derived from the "Retry + Circuit Breaker + Timeout" checkbox, and `internalHttpClient` and `externalHttpClient` from their respective checkboxes
3. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Observability toggle states to an `ObservabilityConfig` object with `serilogEnabled` and `cloudWatchEnabled` both derived from the "Serilog + CloudWatch" checkbox, `openTelemetryEnabled` from the "OpenTelemetry + OTLP" checkbox, `correlationIdEnabled` from the "Correlation ID" checkbox, and `prometheusEnabled` set to `false`
4. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Multi-Tenancy radio selection to the `multiTenancy` boolean property on the Generation_Request
5. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Validation radio selection to a `ValidationConfig` object with `fluentValidationEnabled` set to `true` when "FluentValidation" is selected and `false` when "None" is selected
6. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Background Jobs radio selection to a `BackgroundJobConfig` object with `provider` set to `"EventBridge"`, `"SqsLambda"`, or `null` based on the selected option
7. WHEN the user submits the generation form, THE Generation_Wizard SHALL map the Utilities toggle states to a `UtilitiesConfig` object with `pdfEnabled`, `templateEngineEnabled`, and `hashingEnabled` set to their respective checkbox values

---

### Requirement 11: Backward-Compatible Default Behavior

**User Story:** As a platform engineer who does not interact with the infrastructure toggles, I want the generation request to produce the same output as before the toggles were added, so that existing workflows are not disrupted.

#### Acceptance Criteria

1. WHEN all infrastructure toggles are at their default values (Authentication: API Key checked, JWT unchecked, mTLS unchecked; Resilience: Retry+CB+Timeout checked, Internal checked, External unchecked; Observability: Serilog+CW checked, OTel checked, CorrelationID checked, Prometheus unchecked; Multi-Tenancy: Disabled; Validation: FluentValidation; Background Jobs: None; Utilities: all unchecked), THE Generation_Wizard SHALL either omit the toggle fields from the request payload entirely OR include them with values that the API treats as equivalent to null/default
2. THE Infrastructure_Options_Section default toggle values SHALL match the API's null-semantics defaults so that a user who never expands the section receives the same generated output as before this feature existed

---

### Requirement 12: Cross-Toggle Validation Rules

**User Story:** As a platform engineer, I want the UI to prevent me from selecting invalid toggle combinations before I submit, so that I receive immediate feedback rather than a server-side validation error.

#### Acceptance Criteria

1. WHEN "Serilog + CloudWatch" is unchecked in the Observability Toggle_Group, THE Generation_Wizard SHALL prevent the user from independently enabling CloudWatch (since CloudWatch is a Serilog sink and cannot function without Serilog)
2. WHEN "Multi-Tenancy" is set to "Enabled" AND no database is configured in the Deployment Settings step, THE Generation_Wizard SHALL display a validation message indicating that multi-tenancy requires a database configuration
3. WHEN "Typed HTTP Clients — External" is checked AND "Retry + Circuit Breaker + Timeout" is unchecked in the Resilience Toggle_Group, THE Generation_Wizard SHALL display a validation message indicating that External HTTP Client requires at least one resilience toggle enabled
4. THE Generation_Wizard SHALL evaluate cross-toggle validation rules in real-time as the user modifies toggle values, without requiring form submission

---

### Requirement 13: Coming Soon Toggle Presentation

**User Story:** As a platform engineer, I want to see planned infrastructure options that are not yet available, so that I understand the platform roadmap without being confused about what I can currently use.

#### Acceptance Criteria

1. THE Generation_Wizard SHALL render Coming_Soon_Toggles (mTLS, Prometheus Metrics, PDF Generation, Template Engine) as visually distinct from active toggles using a disabled/greyed-out appearance
2. THE Generation_Wizard SHALL display a "Coming Soon" text label adjacent to each Coming_Soon_Toggle
3. THE Coming_Soon_Toggles SHALL NOT be interactive — the user SHALL NOT be able to check, uncheck, or select them
4. WHEN the Generation_Request is built, THE Generation_Wizard SHALL NOT include Coming_Soon_Toggle values in the payload (their values remain at their hardcoded defaults: `false`)

---

### Requirement 14: Review Step Integration

**User Story:** As a platform engineer, I want to see a summary of my infrastructure toggle selections in the Review step before generating, so that I can verify my configuration is correct.

#### Acceptance Criteria

1. THE Generation_Wizard review step (Step 7) SHALL display an "Infrastructure Options" summary section showing all toggle groups and their current selections
2. WHEN any toggle differs from its default value, THE review step SHALL visually highlight that toggle to draw attention to non-default configurations
3. WHEN all toggles are at their default values, THE review step SHALL display a concise "All defaults" indicator rather than listing every individual toggle

## Out of Scope

- API changes (already implemented in `forge-conditional-generation` spec)
- Engine/template changes (already implemented in `forge-conditional-generation` spec)
- Vertical enrollment form changes (this spec targets the generation form only)
- New concerns not yet implemented in the engine (mTLS, Prometheus, PDF, Template Engine) — toggles are shown as "Coming Soon" but do not affect generation
- Data Access toggle group changes (EF Core + PostgreSQL already exists in the Deployment Settings step)
- Cloud Provider (Genesis) changes (already exists, no modification needed)

## Constraints

- All toggle default values MUST match the API's null-semantics defaults to ensure backward compatibility
- The Infrastructure_Options_Section MUST use Angular Material components consistent with the existing wizard design (MatCheckbox, MatRadioButton, MatExpansionPanel)
- Toggle field names in the TypeScript model MUST use camelCase and match the API's expected JSON property names (`auth`, `resilience`, `observability`, `validation`, `backgroundJobs`, `utilities`, `multiTenancy`)
- The UI MUST NOT send Coming_Soon_Toggle values that differ from their hardcoded defaults — disabled toggles are presentation-only
- Cross-toggle validation MUST mirror the API's validation rules (CloudWatch requires Serilog, Multi-Tenancy requires Database, External HTTP Client requires Resilience) to prevent unnecessary server round-trips
- The feature MUST work within the existing `GenerationWizardV2Component` single-file component pattern or be extracted into a child component following Angular standalone component conventions
