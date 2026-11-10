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

import {
	InfraOptionsFormValue,
	InfraConfigPayload,
	InfraReviewGroup,
	INFRA_OPTIONS_DEFAULTS,
} from './infra-options-defaults';

/**
 * Maps infrastructure form values to the GenerationRequest config fields.
 * Returns all-undefined when all values match defaults (null-emission strategy).
 * Coming Soon toggle values are always hardcoded to `false` regardless of input.
 */
export function mapInfraOptionsToRequest(formValue: InfraOptionsFormValue): InfraConfigPayload {
	if (isInfraOptionsAtDefaults(formValue)) {
		return {
			auth: undefined,
			resilience: undefined,
			observability: undefined,
			validation: undefined,
			backgroundJobs: undefined,
			utilities: undefined,
			multiTenancy: undefined,
		};
	}

	return {
		// Sign-in comes from the Cognito step; these flags are not read by the print.
		auth: {
			apiKeyEnabled: true,
			jwtEnabled: false,
			mtlsEnabled: false, // Coming Soon — always false
		},
		// One switch: retry, circuit breaker and timeout are generated together.
		// Both typed HTTP clients are always included.
		resilience: {
			retryEnabled: formValue.retryEnabled,
			circuitBreakerEnabled: formValue.retryEnabled,
			timeoutEnabled: formValue.retryEnabled,
			internalHttpClient: true,
			externalHttpClient: true,
		},
		observability: {
			serilogEnabled: formValue.serilogEnabled,
			cloudWatchEnabled: formValue.serilogEnabled, // CloudWatch is the Serilog sink
			openTelemetryEnabled: formValue.openTelemetryEnabled,
			correlationIdEnabled: formValue.correlationIdEnabled,
			prometheusEnabled: false, // Coming Soon — always false
		},
		validation: {
			fluentValidationEnabled: formValue.fluentValidationEnabled,
		},
		backgroundJobs: {
			provider: null, // Coming Soon — no template generates background jobs yet
		},
		utilities: {
			pdfEnabled: false, // Coming Soon — always false
			templateEngineEnabled: false, // Coming Soon — always false
			hashingEnabled: false, // Not an option: every print includes hashing; the API ignores this flag.
		},
		multiTenancy: formValue.multiTenancy,
	};
}

/**
 * Counts how many toggle values differ from their defaults.
 */
export function countNonDefaultToggles(formValue: InfraOptionsFormValue): number {
	const defaults = INFRA_OPTIONS_DEFAULTS;
	let count = 0;
	for (const key of Object.keys(defaults) as (keyof InfraOptionsFormValue)[]) {
		if (formValue[key] !== defaults[key]) {
			count++;
		}
	}
	return count;
}

/**
 * Returns true if all infrastructure options match their defaults.
 */
export function isInfraOptionsAtDefaults(formValue: InfraOptionsFormValue): boolean {
	return countNonDefaultToggles(formValue) === 0;
}

/**
 * Returns a list of toggle groups with their non-default values for review display.
 * Returns empty array if all defaults (triggers "All defaults" indicator).
 */
export function getInfraReviewSummary(formValue: InfraOptionsFormValue): InfraReviewGroup[] {
	if (isInfraOptionsAtDefaults(formValue)) {
		return [];
	}

	const defaults = INFRA_OPTIONS_DEFAULTS;
	const groups: InfraReviewGroup[] = [];

	// Resilience
	if (formValue.retryEnabled !== defaults.retryEnabled) {
		groups.push({
			label: 'Resilience',
			items: [{ name: 'Retry + Circuit Breaker + Timeout', value: formValue.retryEnabled ? 'Enabled' : 'Disabled', isNonDefault: true }],
		});
	}

	// Observability
	const observabilityItems: InfraReviewGroup['items'] = [];
	if (formValue.serilogEnabled !== defaults.serilogEnabled) {
		observabilityItems.push({ name: 'Serilog + CloudWatch', value: formValue.serilogEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.openTelemetryEnabled !== defaults.openTelemetryEnabled) {
		observabilityItems.push({ name: 'OpenTelemetry + OTLP', value: formValue.openTelemetryEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.correlationIdEnabled !== defaults.correlationIdEnabled) {
		observabilityItems.push({ name: 'Correlation ID', value: formValue.correlationIdEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (observabilityItems.length > 0) {
		groups.push({ label: 'Observability', items: observabilityItems });
	}

	// Multi-Tenancy
	if (formValue.multiTenancy !== defaults.multiTenancy) {
		groups.push({
			label: 'Tenancy',
			items: [{ name: 'Tenancy', value: formValue.multiTenancy ? 'Multi-tenant' : 'Single tenant', isNonDefault: true }],
		});
	}

	// Validation
	if (formValue.fluentValidationEnabled !== defaults.fluentValidationEnabled) {
		groups.push({
			label: 'Validation',
			items: [{ name: 'FluentValidation', value: formValue.fluentValidationEnabled ? 'Enabled' : 'None', isNonDefault: true }],
		});
	}

	// Background jobs, PDF and template engine are "Coming Soon" (always off), so there is nothing to review.

	return groups;
}
