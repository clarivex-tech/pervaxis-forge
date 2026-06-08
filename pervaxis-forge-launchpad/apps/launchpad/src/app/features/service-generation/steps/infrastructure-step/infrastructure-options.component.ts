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

import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatRadioModule } from '@angular/material/radio';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';

import { InfraOptionsFormControls } from '../../utils/infra-options-defaults';
import { countNonDefaultToggles } from '../../utils/infra-options-mapping';

@Component({
	selector: 'forge-infrastructure-options',
	standalone: true,
	imports: [
		CommonModule,
		ReactiveFormsModule,
		MatExpansionModule,
		MatCheckboxModule,
		MatRadioModule,
		MatIconModule,
		MatTooltipModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-expansion-panel [expanded]="false" class="infra-options-panel">
			<mat-expansion-panel-header>
				<mat-panel-title>
					<mat-icon>settings</mat-icon>
					Infrastructure Options
				</mat-panel-title>
				<mat-panel-description>
					@if (nonDefaultCount() > 0) {
						<span class="non-default-badge">{{ nonDefaultCount() }} change{{ nonDefaultCount() > 1 ? 's' : '' }} from defaults</span>
					} @else {
						<span class="all-defaults-label">All defaults</span>
					}
				</mat-panel-description>
			</mat-expansion-panel-header>

			<div class="infra-options-content" [formGroup]="infraForm()">
				<!-- Authentication Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>vpn_key</mat-icon> Authentication
					</h4>
					<div class="toggle-items">
						<mat-checkbox formControlName="apiKeyEnabled">API Key</mat-checkbox>
						<mat-checkbox formControlName="jwtEnabled">JWT / OIDC Auth0</mat-checkbox>
						<div class="coming-soon-toggle">
							<mat-checkbox formControlName="mtlsEnabled" [disabled]="true">mTLS</mat-checkbox>
							<span class="coming-soon-label">Coming Soon</span>
						</div>
					</div>
				</div>

				<!-- Resilience Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>shield</mat-icon> Resilience
					</h4>
					<div class="toggle-items">
						<mat-checkbox formControlName="retryEnabled">Retry + Circuit Breaker + Timeout</mat-checkbox>
						<mat-checkbox formControlName="internalHttpClient">Typed HTTP Clients — Internal</mat-checkbox>
						<mat-checkbox formControlName="externalHttpClient">Typed HTTP Clients — External</mat-checkbox>
					</div>
					@if (infraForm().hasError('externalHttpRequiresResilience')) {
						<div class="validation-warning">
							<mat-icon>warning</mat-icon>
							{{ infraForm().getError('externalHttpRequiresResilience') }}
						</div>
					}
				</div>

				<!-- Observability Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>monitoring</mat-icon> Observability
					</h4>
					<div class="toggle-items">
						<mat-checkbox formControlName="serilogEnabled">Serilog + CloudWatch</mat-checkbox>
						<mat-checkbox formControlName="openTelemetryEnabled">OpenTelemetry + OTLP</mat-checkbox>
						<mat-checkbox formControlName="correlationIdEnabled">Correlation ID</mat-checkbox>
						<div class="coming-soon-toggle">
							<mat-checkbox formControlName="prometheusEnabled" [disabled]="true">Prometheus Metrics</mat-checkbox>
							<span class="coming-soon-label">Coming Soon</span>
						</div>
					</div>
					@if (infraForm().hasError('cloudWatchRequiresSerilog')) {
						<div class="validation-warning">
							<mat-icon>warning</mat-icon>
							{{ infraForm().getError('cloudWatchRequiresSerilog') }}
						</div>
					}
				</div>

				<!-- Multi-Tenancy Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>apartment</mat-icon> Multi-Tenancy
					</h4>
					<mat-radio-group formControlName="multiTenancy" class="toggle-radio-group">
						<mat-radio-button [value]="true">Enabled</mat-radio-button>
						<mat-radio-button [value]="false">Disabled</mat-radio-button>
					</mat-radio-group>
					@if (infraForm().hasError('multiTenancyRequiresDatabase')) {
						<div class="validation-warning">
							<mat-icon>warning</mat-icon>
							{{ infraForm().getError('multiTenancyRequiresDatabase') }}
						</div>
					}
				</div>

				<!-- Validation Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>check_circle</mat-icon> Validation
					</h4>
					<mat-radio-group formControlName="fluentValidationEnabled" class="toggle-radio-group">
						<mat-radio-button [value]="true">FluentValidation</mat-radio-button>
						<mat-radio-button [value]="false">None</mat-radio-button>
					</mat-radio-group>
				</div>

				<!-- Background Jobs Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>schedule</mat-icon> Background Jobs
					</h4>
					<mat-radio-group formControlName="backgroundJobProvider" class="toggle-radio-group">
						<mat-radio-button value="EventBridge">EventBridge Scheduler</mat-radio-button>
						<mat-radio-button value="SqsLambda">SQS + Lambda</mat-radio-button>
						<mat-radio-button [value]="null">None</mat-radio-button>
					</mat-radio-group>
				</div>

				<!-- Utilities Toggle Group -->
				<div class="toggle-group">
					<h4 class="toggle-group-title">
						<mat-icon>build</mat-icon> Utilities
					</h4>
					<div class="toggle-items">
						<div class="coming-soon-toggle">
							<mat-checkbox formControlName="pdfEnabled" [disabled]="true">PDF Generation</mat-checkbox>
							<span class="coming-soon-label">Coming Soon</span>
						</div>
						<div class="coming-soon-toggle">
							<mat-checkbox formControlName="templateEngineEnabled" [disabled]="true">Template Engine</mat-checkbox>
							<span class="coming-soon-label">Coming Soon</span>
						</div>
						<mat-checkbox formControlName="hashingEnabled">Encryption / Hashing</mat-checkbox>
					</div>
				</div>
			</div>
		</mat-expansion-panel>
	`,
	styles: [`
		.infra-options-panel {
			margin: 1rem 0;
		}

		mat-panel-title {
			display: flex;
			align-items: center;
			gap: 0.5rem;
		}

		.non-default-badge {
			background: #e3f2fd;
			color: #1565c0;
			padding: 2px 8px;
			border-radius: 12px;
			font-size: 0.75rem;
			font-weight: 500;
		}

		.all-defaults-label {
			color: #666;
			font-size: 0.8rem;
		}

		.infra-options-content {
			display: flex;
			flex-direction: column;
			gap: 1.5rem;
			padding: 1rem 0;
		}

		.toggle-group {
			border-bottom: 1px solid #e0e0e0;
			padding-bottom: 1rem;
		}

		.toggle-group:last-child {
			border-bottom: none;
			padding-bottom: 0;
		}

		.toggle-group-title {
			display: flex;
			align-items: center;
			gap: 0.5rem;
			margin: 0 0 0.75rem 0;
			font-size: 0.9rem;
			font-weight: 500;
			color: #333;
		}

		.toggle-group-title mat-icon {
			font-size: 18px;
			width: 18px;
			height: 18px;
			color: #666;
		}

		.toggle-items {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
			padding-left: 1.5rem;
		}

		.toggle-radio-group {
			display: flex;
			flex-direction: column;
			gap: 0.5rem;
			padding-left: 1.5rem;
		}

		.coming-soon-toggle {
			display: flex;
			align-items: center;
			gap: 0.5rem;
		}

		.coming-soon-label {
			font-size: 0.7rem;
			font-weight: 500;
			color: #9e9e9e;
			background: #f5f5f5;
			padding: 1px 6px;
			border-radius: 4px;
			text-transform: uppercase;
			letter-spacing: 0.5px;
		}

		.validation-warning {
			display: flex;
			align-items: center;
			gap: 0.5rem;
			margin-top: 0.5rem;
			padding: 0.5rem 0.75rem;
			background: #fff3e0;
			border-radius: 4px;
			font-size: 0.8rem;
			color: #e65100;
		}

		.validation-warning mat-icon {
			font-size: 16px;
			width: 16px;
			height: 16px;
			color: #ff9800;
		}
	`],
})
export class InfrastructureOptionsComponent {
	/** The nested FormGroup for infrastructure options, passed from parent */
	infraForm = input.required<FormGroup<InfraOptionsFormControls>>();

	/** Whether a database is configured (for multi-tenancy validation) */
	databaseConfigured = input<boolean>(false);

	/** Computed: number of toggles that differ from defaults */
	nonDefaultCount = computed(() => {
		const form = this.infraForm();
		// Use getRawValue to include disabled controls in the count
		return countNonDefaultToggles(form.getRawValue());
	});
}
