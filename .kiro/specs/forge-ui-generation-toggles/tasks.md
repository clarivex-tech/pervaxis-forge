# Implementation Plan: Forge UI Generation Toggles

## Overview

This plan implements the Infrastructure Options UI for the Forge Launchpad generation wizard. Work is organized into: (1) core types and defaults, (2) pure mapping/utility functions, (3) cross-toggle validator, (4) the standalone Angular component, (5) wizard integration with FormGroup wiring, and (6) review step summary. Each task builds incrementally — types first, then logic, then UI, then integration.

## Tasks

- [x] 1. Create core types and defaults
  - [x] 1.1 Create `infra-options-defaults.ts` in `apps/launchpad/src/app/features/service-generation/utils/`
    - Define `InfraOptionsFormValue` interface with all 19 toggle fields (booleans + `backgroundJobProvider: string | null`)
    - Define `InfraOptionsFormControls` interface with typed `FormControl` declarations for each field
    - Define `InfraConfigPayload` interface with optional config sub-objects (`auth?`, `resilience?`, `observability?`, `validation?`, `backgroundJobs?`, `utilities?`, `multiTenancy?`)
    - Define `InfraReviewGroup` interface with `label`, `items` array
    - Export `INFRA_OPTIONS_DEFAULTS` constant matching the documented default values
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7, 11.2_

  - [x] 1.2 Extend `GenerationRequest` interface with optional infrastructure fields
    - Add `auth?: AuthConfig | null`, `resilience?: ResilienceConfig | null`, `observability?: ObservabilityConfig | null`, `validation?: ValidationConfig | null`, `backgroundJobs?: BackgroundJobConfig | null`, `utilities?: UtilitiesConfig | null`, `multiTenancy?: boolean` to the existing `GenerationRequest` interface
    - Define `AuthConfig`, `ResilienceConfig`, `ObservabilityConfig`, `ValidationConfig`, `BackgroundJobConfig`, `UtilitiesConfig` interfaces alongside or re-export from the defaults file
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_

- [x] 2. Implement pure mapping and utility functions
  - [x] 2.1 Create `infra-options-mapping.ts` in `apps/launchpad/src/app/features/service-generation/utils/`
    - Implement `mapInfraOptionsToRequest(formValue: InfraOptionsFormValue): InfraConfigPayload` — returns all-undefined when defaults, otherwise maps each field with Coming Soon values hardcoded to `false`
    - Implement `countNonDefaultToggles(formValue: InfraOptionsFormValue): number` — iterates keys comparing to `INFRA_OPTIONS_DEFAULTS`
    - Implement `isInfraOptionsAtDefaults(formValue: InfraOptionsFormValue): boolean` — returns `countNonDefaultToggles(formValue) === 0`
    - Implement `getInfraReviewSummary(formValue: InfraOptionsFormValue): InfraReviewGroup[]` — returns empty array when all defaults, otherwise returns grouped non-default entries
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 11.1, 13.4, 14.1, 14.2, 14.3_

  - [ ]* 2.2 Write property test: Form-to-request mapping faithfulness
    - Create `infra-options-mapping.property.spec.ts` in `steps/infrastructure-step/`
    - **Property 1: Mapping faithfulness** — For any random `InfraOptionsFormValue` with at least one non-default value, verify each non-Coming-Soon output field matches the corresponding input
    - Use fast-check arbitrary to generate random `InfraOptionsFormValue` objects
    - **Validates: Requirements 10.1, 10.2, 10.3, 10.7, 2.3, 3.2, 4.4**

  - [ ]* 2.3 Write property test: Default form values produce empty payload
    - **Property 2: Default payload** — Verify `mapInfraOptionsToRequest(INFRA_OPTIONS_DEFAULTS)` returns all fields as `undefined`
    - **Validates: Requirements 11.1, 11.2**

  - [ ]* 2.4 Write property test: Coming Soon toggles are always false in payload
    - **Property 3: Coming Soon always false** — For any random `InfraOptionsFormValue` (including random Coming Soon values), verify `auth.mtlsEnabled`, `observability.prometheusEnabled`, `utilities.pdfEnabled`, and `utilities.templateEngineEnabled` are always `false` in the output
    - **Validates: Requirements 13.4, 2.2, 4.2, 8.2, 8.3**

  - [ ]* 2.5 Write property test: Non-default toggle count accuracy
    - **Property 4: Non-default count** — For any random `InfraOptionsFormValue`, independently count differing keys and verify `countNonDefaultToggles` matches
    - **Validates: Requirements 1.2, 14.2, 14.3**

- [x] 3. Implement cross-toggle validator
  - [x] 3.1 Create `infra-options-validator.ts` in `apps/launchpad/src/app/features/service-generation/utils/`
    - Implement `infraOptionsValidator(databaseConfiguredFn: () => boolean): ValidatorFn`
    - Rule 1: CloudWatch requires Serilog — error key `cloudWatchRequiresSerilog`
    - Rule 2: Multi-Tenancy requires Database — error key `multiTenancyRequiresDatabase`
    - Rule 3: External HTTP Client requires Resilience — error key `externalHttpRequiresResilience`
    - Validator returns `null` when no errors, otherwise returns `Record<string, string>` with error messages
    - _Requirements: 12.1, 12.2, 12.3, 12.4_

  - [ ]* 3.2 Write unit tests for cross-toggle validator
    - Test each validation rule with specific invalid combinations
    - Test that valid combinations return `null`
    - Test that multiple simultaneous violations return all error keys
    - _Requirements: 12.1, 12.2, 12.3_

- [x] 4. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Build InfrastructureOptionsComponent
  - [x] 5.1 Create `infrastructure-options.component.ts` in `steps/infrastructure-step/`
    - Standalone component with `ChangeDetectionStrategy.OnPush`
    - Input: `infraForm` (required signal input of type `FormGroup<InfraOptionsFormControls>`)
    - Input: `databaseConfigured` (boolean signal input, default `false`)
    - Computed: `nonDefaultCount` using `countNonDefaultToggles`
    - Import Angular Material modules: `MatExpansionModule`, `MatCheckboxModule`, `MatRadioModule`, `MatIconModule`, `MatTooltipModule`
    - _Requirements: 1.1, 1.2, 1.3, 1.4_

  - [x] 5.2 Implement toggle group templates in the component
    - Authentication group: API Key (checked), JWT (unchecked), mTLS (disabled + "Coming Soon")
    - Resilience group: Retry+CB+Timeout (checked), Internal HTTP (checked), External HTTP (unchecked)
    - Observability group: Serilog+CloudWatch (checked), OpenTelemetry (checked), Correlation ID (checked), Prometheus (disabled + "Coming Soon")
    - Multi-Tenancy group: Radio with Enabled/Disabled (default Disabled)
    - Validation group: Radio with FluentValidation/None (default FluentValidation)
    - Background Jobs group: Radio with EventBridge/SQS+Lambda/None (default None)
    - Utilities group: PDF (disabled + "Coming Soon"), Template Engine (disabled + "Coming Soon"), Hashing (unchecked)
    - Display validation warnings inline when cross-toggle errors are present
    - _Requirements: 2.1, 2.2, 3.1, 4.1, 4.2, 5.1, 6.1, 7.1, 8.1, 8.2, 8.3, 13.1, 13.2, 13.3_

  - [x] 5.3 Implement collapsible panel with summary badge
    - Use `MatExpansionPanel` for collapse/expand behavior
    - Default to collapsed state on initial load
    - Display summary badge showing `nonDefaultCount` when collapsed (e.g., "3 changes from defaults")
    - When count is 0, show no badge or "All defaults"
    - _Requirements: 1.1, 1.2, 1.4_

  - [ ]* 5.4 Write unit tests for InfrastructureOptionsComponent
    - Test toggle groups render in correct order
    - Test Coming Soon toggles are disabled and show label
    - Test default states match documented defaults
    - Test summary badge updates when toggles change
    - Test validation warnings display for invalid combinations
    - _Requirements: 1.1, 1.2, 1.3, 2.1, 2.2, 13.1, 13.2, 13.3_

- [x] 6. Integrate into GenerationWizardV2Component
  - [x] 6.1 Add `infraOptions` nested FormGroup to the wizard's reactive form
    - Create the FormGroup with all 19 controls at their default values
    - Disable Coming Soon controls (`mtlsEnabled`, `prometheusEnabled`, `pdfEnabled`, `templateEngineEnabled`)
    - Attach `infraOptionsValidator` as a cross-field validator on the group
    - Pass `databaseConfigured` signal derived from the Deployment Settings step
    - _Requirements: 2.3, 3.2, 4.4, 5.3, 6.2, 7.2, 8.4, 12.4_

  - [x] 6.2 Add `<forge-infrastructure-options>` to the wizard template
    - Position between Production Readiness step and Deployment Settings step
    - Pass `infraForm` and `databaseConfigured` inputs
    - _Requirements: 1.1_

  - [x] 6.3 Update `buildRequest()` to include infrastructure toggle mapping
    - Call `mapInfraOptionsToRequest()` with the `infraOptions` FormGroup value
    - Spread the result into the `GenerationRequest` payload
    - When all defaults, fields are `undefined` and omitted from JSON serialization
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6, 10.7, 11.1_

  - [ ]* 6.4 Write unit tests for wizard form integration
    - Test `infraOptions` FormGroup initializes with correct defaults
    - Test `buildRequest()` omits infrastructure fields when all defaults
    - Test `buildRequest()` includes infrastructure fields when toggles are modified
    - _Requirements: 11.1, 11.2, 10.1_

- [x] 7. Checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Implement review step integration
  - [x] 8.1 Add infrastructure summary to the review/preview step
    - Call `getInfraReviewSummary()` with current `infraOptions` form value
    - Display "Infrastructure Options" section in the review step (Step 7 / preview-generate-step)
    - When all defaults: show "All defaults" indicator
    - When non-default values exist: list each toggle group with changed values, visually highlight non-default items
    - _Requirements: 14.1, 14.2, 14.3_

  - [ ]* 8.2 Write unit tests for review step infrastructure summary
    - Test "All defaults" indicator when no changes
    - Test non-default highlighting for modified toggles
    - Test all toggle groups appear in summary when modified
    - _Requirements: 14.1, 14.2, 14.3_

- [x] 9. Final checkpoint — Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document
- The design uses TypeScript/Angular — all implementation uses that stack
- The existing `infrastructure-step/` directory already has a component; the new `InfrastructureOptionsComponent` is a child component within that step or replaces it depending on current usage
- Pure mapping functions in `utils/` are tested independently of Angular — fast to iterate on
