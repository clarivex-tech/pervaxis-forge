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

// ─── Form Value Type (flat representation of all 19 toggle fields) ───────────

export interface InfraOptionsFormValue {
	apiKeyEnabled: boolean;
	jwtEnabled: boolean;
	mtlsEnabled: boolean;
	retryEnabled: boolean;
	circuitBreakerEnabled: boolean;
	timeoutEnabled: boolean;
	internalHttpClient: boolean;
	externalHttpClient: boolean;
	serilogEnabled: boolean;
	cloudWatchEnabled: boolean;
	openTelemetryEnabled: boolean;
	correlationIdEnabled: boolean;
	prometheusEnabled: boolean;
	multiTenancy: boolean;
	fluentValidationEnabled: boolean;
	backgroundJobProvider: string | null;
	pdfEnabled: boolean;
	templateEngineEnabled: boolean;
	hashingEnabled: boolean;
}

// ─── Typed FormControls Interface ────────────────────────────────────────────

export interface InfraOptionsFormControls {
	// Authentication
	apiKeyEnabled: FormControl<boolean>;
	jwtEnabled: FormControl<boolean>;
	mtlsEnabled: FormControl<boolean>; // disabled — Coming Soon

	// Resilience
	retryEnabled: FormControl<boolean>;
	circuitBreakerEnabled: FormControl<boolean>;
	timeoutEnabled: FormControl<boolean>;
	internalHttpClient: FormControl<boolean>;
	externalHttpClient: FormControl<boolean>;

	// Observability
	serilogEnabled: FormControl<boolean>;
	cloudWatchEnabled: FormControl<boolean>;
	openTelemetryEnabled: FormControl<boolean>;
	correlationIdEnabled: FormControl<boolean>;
	prometheusEnabled: FormControl<boolean>; // disabled — Coming Soon

	// Multi-Tenancy
	multiTenancy: FormControl<boolean>;

	// Validation
	fluentValidationEnabled: FormControl<boolean>;

	// Background Jobs
	backgroundJobProvider: FormControl<string | null>;

	// Utilities
	pdfEnabled: FormControl<boolean>; // disabled — Coming Soon
	templateEngineEnabled: FormControl<boolean>; // disabled — Coming Soon
	hashingEnabled: FormControl<boolean>;
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
	apiKeyEnabled: true,
	jwtEnabled: false,
	mtlsEnabled: false,

	// Resilience
	retryEnabled: true,
	circuitBreakerEnabled: true,
	timeoutEnabled: true,
	internalHttpClient: true,
	externalHttpClient: false,

	// Observability
	serilogEnabled: true,
	cloudWatchEnabled: true,
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
	hashingEnabled: false,
};
