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
		auth: {
			apiKeyEnabled: formValue.apiKeyEnabled,
			jwtEnabled: formValue.jwtEnabled,
			mtlsEnabled: false, // Coming Soon — always false
		},
		resilience: {
			retryEnabled: formValue.retryEnabled,
			circuitBreakerEnabled: formValue.circuitBreakerEnabled,
			timeoutEnabled: formValue.timeoutEnabled,
			internalHttpClient: formValue.internalHttpClient,
			externalHttpClient: formValue.externalHttpClient,
		},
		observability: {
			serilogEnabled: formValue.serilogEnabled,
			cloudWatchEnabled: formValue.cloudWatchEnabled,
			openTelemetryEnabled: formValue.openTelemetryEnabled,
			correlationIdEnabled: formValue.correlationIdEnabled,
			prometheusEnabled: false, // Coming Soon — always false
		},
		validation: {
			fluentValidationEnabled: formValue.fluentValidationEnabled,
		},
		backgroundJobs: {
			provider: formValue.backgroundJobProvider,
		},
		utilities: {
			pdfEnabled: false, // Coming Soon — always false
			templateEngineEnabled: false, // Coming Soon — always false
			hashingEnabled: formValue.hashingEnabled,
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

	// Authentication
	const authItems: InfraReviewGroup['items'] = [];
	if (formValue.apiKeyEnabled !== defaults.apiKeyEnabled) {
		authItems.push({ name: 'API Key', value: formValue.apiKeyEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.jwtEnabled !== defaults.jwtEnabled) {
		authItems.push({ name: 'JWT / OIDC Auth0', value: formValue.jwtEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (authItems.length > 0) {
		groups.push({ label: 'Authentication', items: authItems });
	}

	// Resilience
	const resilienceItems: InfraReviewGroup['items'] = [];
	if (formValue.retryEnabled !== defaults.retryEnabled) {
		resilienceItems.push({ name: 'Retry + Circuit Breaker + Timeout', value: formValue.retryEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.circuitBreakerEnabled !== defaults.circuitBreakerEnabled) {
		resilienceItems.push({ name: 'Circuit Breaker', value: formValue.circuitBreakerEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.timeoutEnabled !== defaults.timeoutEnabled) {
		resilienceItems.push({ name: 'Timeout', value: formValue.timeoutEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.internalHttpClient !== defaults.internalHttpClient) {
		resilienceItems.push({ name: 'Typed HTTP Clients — Internal', value: formValue.internalHttpClient ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.externalHttpClient !== defaults.externalHttpClient) {
		resilienceItems.push({ name: 'Typed HTTP Clients — External', value: formValue.externalHttpClient ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (resilienceItems.length > 0) {
		groups.push({ label: 'Resilience', items: resilienceItems });
	}

	// Observability
	const observabilityItems: InfraReviewGroup['items'] = [];
	if (formValue.serilogEnabled !== defaults.serilogEnabled) {
		observabilityItems.push({ name: 'Serilog + CloudWatch', value: formValue.serilogEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (formValue.cloudWatchEnabled !== defaults.cloudWatchEnabled) {
		observabilityItems.push({ name: 'CloudWatch', value: formValue.cloudWatchEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
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
			label: 'Multi-Tenancy',
			items: [{ name: 'Multi-Tenancy', value: formValue.multiTenancy ? 'Enabled' : 'Disabled', isNonDefault: true }],
		});
	}

	// Validation
	if (formValue.fluentValidationEnabled !== defaults.fluentValidationEnabled) {
		groups.push({
			label: 'Validation',
			items: [{ name: 'FluentValidation', value: formValue.fluentValidationEnabled ? 'Enabled' : 'None', isNonDefault: true }],
		});
	}

	// Background Jobs
	if (formValue.backgroundJobProvider !== defaults.backgroundJobProvider) {
		const providerLabel = formValue.backgroundJobProvider === 'EventBridge'
			? 'EventBridge Scheduler'
			: formValue.backgroundJobProvider === 'SqsLambda'
				? 'SQS + Lambda'
				: 'None';
		groups.push({
			label: 'Background Jobs',
			items: [{ name: 'Provider', value: providerLabel, isNonDefault: true }],
		});
	}

	// Utilities
	const utilitiesItems: InfraReviewGroup['items'] = [];
	if (formValue.hashingEnabled !== defaults.hashingEnabled) {
		utilitiesItems.push({ name: 'Encryption / Hashing', value: formValue.hashingEnabled ? 'Enabled' : 'Disabled', isNonDefault: true });
	}
	if (utilitiesItems.length > 0) {
		groups.push({ label: 'Utilities', items: utilitiesItems });
	}

	return groups;
}
