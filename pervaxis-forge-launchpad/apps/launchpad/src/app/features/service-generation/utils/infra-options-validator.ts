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

import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { InfraOptionsFormValue } from './infra-options-defaults';

/**
 * Cross-toggle validator for the infrastructure options FormGroup.
 * Evaluates dependency rules between toggles and returns validation errors
 * as warnings (the form remains submittable — the API performs its own validation).
 *
 * Rules:
 * 1. CloudWatch requires Serilog (CloudWatch is a Serilog sink)
 * 2. Multi-Tenancy requires a database configuration
 * 3. External HTTP Client requires at least one resilience toggle (Retry+CB+Timeout)
 */
export function infraOptionsValidator(databaseConfiguredFn: () => boolean): ValidatorFn {
	return (group: AbstractControl): ValidationErrors | null => {
		const value = group.value as InfraOptionsFormValue;
		const errors: Record<string, string> = {};

		// Rule 1: CloudWatch requires Serilog
		if (!value.serilogEnabled && value.cloudWatchEnabled) {
			errors['cloudWatchRequiresSerilog'] =
				'CloudWatch is a Serilog sink — enable Serilog + CloudWatch together or disable both.';
		}

		// Rule 2: Multi-Tenancy requires Database
		if (value.multiTenancy && !databaseConfiguredFn()) {
			errors['multiTenancyRequiresDatabase'] =
				'Multi-Tenancy requires a database. Configure one in Deployment Settings.';
		}

		// Rule 3: External HTTP Client requires Resilience
		if (value.externalHttpClient && !value.retryEnabled) {
			errors['externalHttpRequiresResilience'] =
				'External HTTP Client requires Retry + Circuit Breaker + Timeout enabled.';
		}

		return Object.keys(errors).length > 0 ? errors : null;
	};
}
