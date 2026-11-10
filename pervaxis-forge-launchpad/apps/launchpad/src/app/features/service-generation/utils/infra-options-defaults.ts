/**
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */

import { FormControl } from '@angular/forms';
import {
	GenerationAuthConfig,
	GenerationResilienceConfig,
	GenerationObservabilityConfig,
	GenerationValidationConfig,
	GenerationBackgroundJobConfig,
	GenerationUtilitiesConfig,
} from '../../../core/models/generation.model';

// Re-export config types for convenience
export type AuthConfig = GenerationAuthConfig;
export type ResilienceConfig = GenerationResilienceConfig;
export type ObservabilityConfig = GenerationObservabilityConfig;
export type ValidationConfig = GenerationValidationConfig;
export type BackgroundJobConfig = GenerationBackgroundJobConfig;
export type UtilitiesConfig = GenerationUtilitiesConfig;

// ─── Form Value Type (flat representation of the toggle fields) ──────────────

// Not toggles (every print includes them, or another step decides them), so they are not in the form:
// API key / JWT (decided by the Cognito sign-in step), internal and external typed HTTP clients (always
// included), circuit breaker and timeout (follow retryEnabled), CloudWatch (follows serilogEnabled).
export interface InfraOptionsFormValue {
	mtlsEnabled: boolean;
	retryEnabled: boolean;
	serilogEnabled: boolean;
	openTelemetryEnabled: boolean;
	correlationIdEnabled: boolean;
	prometheusEnabled: boolean;
	multiTenancy: boolean;
	fluentValidationEnabled: boolean;
	backgroundJobProvider: string | null;
	pdfEnabled: boolean;
	templateEngineEnabled: boolean;
}

// ─── Typed FormControls Interface ────────────────────────────────────────────

export interface InfraOptionsFormControls {
	// Authentication
	mtlsEnabled: FormControl<boolean>; // disabled — Coming Soon

	// Resilience: one switch for retry, circuit breaker and timeout
	retryEnabled: FormControl<boolean>;

	// Observability
	serilogEnabled: FormControl<boolean>; // Serilog with the CloudWatch sink
	openTelemetryEnabled: FormControl<boolean>;
	correlationIdEnabled: FormControl<boolean>;
	prometheusEnabled: FormControl<boolean>; // disabled — Coming Soon

	// Multi-Tenancy
	multiTenancy: FormControl<boolean>;

	// Validation
	fluentValidationEnabled: FormControl<boolean>;

	// Background Jobs
	backgroundJobProvider: FormControl<string | null>; // disabled — Coming Soon

	// Utilities
	pdfEnabled: FormControl<boolean>; // disabled — Coming Soon
	templateEngineEnabled: FormControl<boolean>; // disabled — Coming Soon
	// Hashing is not an option: every print includes the BCrypt hashing service.
}

// ─── Payload Interface (output of mapping function) ──────────────────────────

export interface InfraConfigPayload {
	auth?: AuthConfig;
	resilience?: ResilienceConfig;
	observability?: ObservabilityConfig;
	validation?: ValidationConfig;
	backgroundJobs?: BackgroundJobConfig;
	utilities?: UtilitiesConfig;
	multiTenancy?: boolean;
}

// ─── Review Step Display Model ───────────────────────────────────────────────

export interface InfraReviewGroup {
	label: string;
	items: { name: string; value: string; isNonDefault: boolean }[];
}

// ─── Default Values (match API null-semantics) ───────────────────────────────

export const INFRA_OPTIONS_DEFAULTS: InfraOptionsFormValue = {
	// Authentication
	mtlsEnabled: false,

	// Resilience
	retryEnabled: true,

	// Observability
	serilogEnabled: true,
	openTelemetryEnabled: true,
	correlationIdEnabled: true,
	prometheusEnabled: false,

	// Multi-Tenancy
	multiTenancy: false,

	// Validation
	fluentValidationEnabled: true,

	// Background Jobs
	backgroundJobProvider: null,

	// Utilities
	pdfEnabled: false,
	templateEngineEnabled: false,
};
