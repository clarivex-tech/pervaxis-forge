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

import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import {
	GENERATION_API_SERVICE,
	IGenerationApiService,
} from '@core/api/generation-api.service';
import { IVerticalApiService, VERTICAL_API_SERVICE } from '@core/api/vertical-api.service';
import { MODULES_API_SERVICE, IModulesApiService } from '@core/api/modules-api.service';
import { CanvasModule, EnterpriseScaffoldOptions, GenerationRequest, GenesisModule, ValidationPreviewResult } from '@core/models/generation.model';
import { VerticalSummaryResponse } from '@core/models/vertical.model';

type BuildTypeOption = {
	label: string;
	value: 'RestApi' | 'GraphQL' | 'Grpc' | 'AngularShell' | 'AngularMfe';
	category: 'Backend Service' | 'Frontend App';
	live: boolean;
	note: string;
};

type ServiceCategory = BuildTypeOption['category'];

const SHELL_PRESELECTED_CANVAS_MODULES = ['Shell', 'Layout', 'Navigation', 'Auth', 'Workspace'] as const;
const SHARED_CANVAS_MODULES = ['Settings', 'Profile', 'Notifications', 'Search'] as const;
const MFE_ONLY_CANVAS_MODULES = ['Dashboard', 'Reports', 'Analytics', 'Admin', 'Support'] as const;

@Component({
	selector: 'forge-generation-wizard-v2',
	standalone: true,
	imports: [
		CommonModule,
		ReactiveFormsModule,
		MatCardModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
		MatCheckboxModule,
		MatButtonModule,
		MatIconModule,
		MatSlideToggleModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<div class="wizard-container">
			<div class="wizard-header">
				<h1>Generate Service</h1>
				<p>7-Step Configuration Wizard</p>
			</div>

			<div class="wizard-progress-bar">
				<div class="progress-track">
					@for (step of wizardSteps; track step; let index = $index) {
						<div
							class="progress-node"
							[class.complete]="index < completedStepIndex()"
							[class.current]="index === completedStepIndex()"
						>
							<div class="node-circle">{{ index + 1 }}</div>
							<div class="node-label">{{ step }}</div>
						</div>
					}
				</div>
			</div>

			<form [formGroup]="form" class="wizard-steps">
				<!-- STEP 1: Select Vertical -->
				<mat-card class="step-card" [class.active]="activeStep() === 0">
					<mat-card-header>
						<mat-card-title>Step 1: Select Vertical Context</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						<mat-form-field class="full-width">
							<mat-label>Vertical *</mat-label>
							<mat-select formControlName="verticalSlug" [disabled]="isLoadingVerticals()">
								@for (vertical of verticals(); track vertical.slug) {
									<mat-option [value]="vertical.slug">
										{{ vertical.displayName }} ({{ vertical.slug }})
									</mat-option>
								}
							</mat-select>
						</mat-form-field>

						<div class="info-grid">
							<div class="info-item">
								<span class="label">Cloud Provider</span>
								<span class="value">{{ selectedVertical()?.cloudProvider ?? 'N/A' }}</span>
							</div>
							<div class="info-item">
								<span class="label">GitHub Org</span>
								<span class="value">{{ selectedVertical()?.githubOrg ?? 'N/A' }}</span>
							</div>
						</div>
					</mat-card-content>
				</mat-card>

				<!-- STEP 2: Choose Build Type -->
				<mat-card class="step-card" [class.active]="activeStep() === 1">
					<mat-card-header>
						<mat-card-title>Step 2: What Are You Building?</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						<div class="family-switch">
							@for (category of serviceCategories; track category) {
								<button
									type="button"
									class="family-button"
									[class.selected]="isTypeCategoryActive(category)"
									(click)="selectServiceCategory(category)"
								>
									{{ category }}
								</button>
							}
						</div>

						<div class="build-types">
							<div class="build-group" [class.inactive]="!isTypeCategoryActive('Backend Service')">
								<h4>Backend Service</h4>
								<div class="type-options">
									@for (option of backendTypeOptions; track option.value) {
										<button
											type="button"
											class="type-button"
											[class.selected]="form.controls.type.value === option.value"
											[class.coming-soon]="!option.live"
											[disabled]="isTypeOptionDisabled(option)"
											(click)="selectBuildType(option.value)"
										>
											<span class="type-label">{{ option.label }}</span>
											<span class="type-note">{{ option.note }}</span>
										</button>
									}
								</div>
							</div>

							<div class="build-group" [class.inactive]="!isTypeCategoryActive('Frontend App')">
								<h4>Frontend App</h4>
								<div class="type-options">
									@for (option of frontendTypeOptions; track option.value) {
										<button
											type="button"
											class="type-button"
											[class.selected]="form.controls.type.value === option.value"
											[class.coming-soon]="!option.live"
											[disabled]="isTypeOptionDisabled(option)"
											(click)="selectBuildType(option.value)"
										>
											<span class="type-label">{{ option.label }}</span>
											<span class="type-note">{{ option.note }}</span>
										</button>
									}
								</div>
							</div>
						</div>
					</mat-card-content>
				</mat-card>

				<!-- STEP 3: Service Details -->
				<mat-card class="step-card" [class.active]="activeStep() === 2">
					<mat-card-header>
						<mat-card-title>Step 3: Service Details</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						<div class="form-grid-2col">
							<mat-form-field>
								<mat-label>Service Name *</mat-label>
								<input matInput formControlName="name" placeholder="e.g. intake-service" />
								<mat-hint>{{ serviceNameHint() }}</mat-hint>
							</mat-form-field>

							<mat-form-field>
								<mat-label>Display Name *</mat-label>
								<input matInput formControlName="displayName" placeholder="e.g. Intake Service" />
							</mat-form-field>
						</div>

						<mat-form-field class="full-width">
							<mat-label>Description *</mat-label>
							<textarea matInput rows="3" formControlName="description" placeholder="Short description of the service"></textarea>
						</mat-form-field>

						<mat-form-field>
							<mat-label>Version</mat-label>
							<input matInput formControlName="version" placeholder="1.0.0" />
						</mat-form-field>
					</mat-card-content>
				</mat-card>

				<!-- STEP 4: Modules -->
				<mat-card class="step-card" [class.active]="activeStep() === 3">
					<mat-card-header>
						<mat-card-title>Step 4: {{ modulesStepTitle() }}</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						@if (isBackendTypeSelected() && isLoadingModules()) {
							<p class="loading-hint">Loading Genesis modules...</p>
						} @else if (isCanvasTypeSelected() && isLoadingCanvasModules()) {
							<p class="loading-hint">Loading Canvas modules...</p>
						} @else if (isBackendTypeSelected()) {
							<div class="module-grid">
								@for (module of availableModules(); track module.name) {
									<button
										type="button"
										class="module-button"
										[class.selected]="isModuleSelected(module.name)"
										(click)="toggleModule(module.name)"
									>
										<span class="module-label">{{ module.label }}</span>
										<span class="module-desc">{{ module.description }}</span>
									</button>
								}
							</div>
							@if (availableModules().length > 0) {
								<div class="module-actions">
									<button type="button" mat-button (click)="selectAllModules()">Select All</button>
									<button type="button" mat-button (click)="clearModules()">Clear</button>
								</div>
							}
						} @else if (isCanvasTypeSelected()) {
							<div class="module-grid">
								@for (module of visibleCanvasModules(); track module.id) {
									<button
										type="button"
										class="module-button"
										[class.selected]="isCanvasModuleSelected(module.name)"
										(click)="toggleCanvasModule(module.name)"
									>
										<span class="module-label">{{ module.label }}</span>
										@if (isShellDefaultCanvasModule(module.name)) {
											<span class="module-badge">Default</span>
										}
									</button>
								}
							</div>
							@if (visibleCanvasModules().length > 0) {
								<div class="module-actions">
									<button type="button" mat-button (click)="selectAllCanvasModules()">Select All</button>
									<button type="button" mat-button (click)="clearCanvasModules()">Clear</button>
								</div>
							}
						} @else {
							<p class="loading-hint">Module selection is not available for this type.</p>
						}
					</mat-card-content>
				</mat-card>

				<!-- STEP 5: Production Readiness (Enterprise Scaffold) -->
				<mat-card class="step-card" [class.active]="activeStep() === 4">
					<mat-card-header>
						<mat-card-title>Step 5: Production Readiness</mat-card-title>
						<mat-card-subtitle>Enterprise Scaffold Configuration</mat-card-subtitle>
					</mat-card-header>
					<mat-card-content>
						<!-- Baseline Features (Read-only) -->
						<div class="feature-section">
							<h4 class="section-title">
								<mat-icon>lock</mat-icon> Always-On Baseline (Enabled by Default)
							</h4>
							<div class="baseline-features">
								<div class="feature-item">
									<div class="feature-badge success">✓</div>
									<div class="feature-info">
										<span class="feature-name">Security Headers</span>
										<span class="feature-desc">HSTS, X-Content-Type-Options, X-Frame-Options, CSP</span>
									</div>
								</div>
								<div class="feature-item">
									<div class="feature-badge success">✓</div>
									<div class="feature-info">
										<span class="feature-name">Authentication &amp; Authorization</span>
										<span class="feature-desc">Fail-closed by default for protected endpoints</span>
									</div>
								</div>
								<div class="feature-item">
									<div class="feature-badge success">✓</div>
									<div class="feature-info">
										<span class="feature-name">CORS Policy</span>
										<span class="feature-desc">Explicit allowed origins, configurable per service</span>
									</div>
								</div>
							</div>
						</div>

						<!-- Selectable Features -->
						<div class="feature-section">
							<h4 class="section-title">
								<mat-icon>tune</mat-icon> Selectable Features
							</h4>
							<div class="selectable-features">
								<div class="feature-checkbox">
									<mat-checkbox formControlName="secretsManagementEnabled">
										<strong>Secrets Management</strong>
										<p>AWS Secrets Manager &amp; SSM Parameter Store integration</p>
									</mat-checkbox>
								</div>
								<div class="feature-checkbox">
									<mat-checkbox formControlName="resilienceEnabled">
										<strong>Resilience &amp; Polly Wiring</strong>
										<p>Retries, timeouts, circuit breakers for external calls</p>
									</mat-checkbox>
								</div>
								<div class="feature-checkbox">
									<mat-checkbox formControlName="rateLimitingEnabled">
										<strong>Rate Limiting</strong>
										<p>Request throttling with configurable limits</p>
									</mat-checkbox>
								</div>
							</div>
						</div>

						<!-- Conditional Features -->
						<div class="feature-section">
							<h4 class="section-title">
								<mat-icon>info</mat-icon> Conditional Features (Use-Case Dependent)
							</h4>
							<div class="conditional-features">
								<div class="feature-checkbox">
									<mat-checkbox formControlName="auditLoggingEnabled">
										<strong>Audit Logging</strong>
										<p>Capture actor, action, target, timestamp (recommended for admin/provisioning services)</p>
									</mat-checkbox>
								</div>
								<div class="feature-checkbox">
									<mat-checkbox formControlName="piiClassificationEnabled">
										<strong>PII &amp; Data Classification</strong>
										<p>Data classification levels and redaction patterns for logs</p>
									</mat-checkbox>
								</div>
								<div class="feature-checkbox">
									<mat-checkbox formControlName="outputCachingEnabled">
										<strong>Output Caching</strong>
										<p>Cache stable responses (opt-in, not for write-heavy endpoints)</p>
									</mat-checkbox>
								</div>
							</div>
						</div>
					</mat-card-content>
				</mat-card>

				<!-- STEP 6: Deployment Settings -->
				<mat-card class="step-card" [class.active]="activeStep() === 5">
					<mat-card-header>
						<mat-card-title>Step 6: Deployment Settings</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						<!-- Database Section -->
						@if (isBackendTypeSelected()) {
							<div class="settings-section">
								<h4>Database Configuration</h4>
								<div class="checkbox-row">
									<mat-checkbox formControlName="useDatabase">
										Include database
									</mat-checkbox>
								</div>

								@if (form.controls.useDatabase.value) {
									<div class="form-grid-2col">
										<mat-form-field>
											<mat-label>Database Engine *</mat-label>
											<mat-select formControlName="databaseEngine">
												<mat-option value="">Select engine</mat-option>
												<mat-option value="postgresql">PostgreSQL</mat-option>
												<mat-option value="sqlserver">SQL Server</mat-option>
												<mat-option value="mysql">MySQL</mat-option>
											</mat-select>
										</mat-form-field>

										<mat-form-field>
											<mat-label>DB Schema *</mat-label>
											<input matInput formControlName="databaseSchema" placeholder="e.g. intake" />
										</mat-form-field>
									</div>
								}
							</div>
						}

						<!-- Messaging Queues -->
						@if (isBackendTypeSelected()) {
							<div class="settings-section">
								<h4>Messaging Queues (Optional)</h4>
								<div class="queue-builder">
									<div class="queue-col">
										<div class="queue-row">
											<mat-form-field class="queue-input">
												<mat-label>Publish Queue</mat-label>
												<input matInput formControlName="publishQueueInput" placeholder="e.g. billing-events" />
											</mat-form-field>
											<button mat-stroked-button type="button" (click)="addQueue('publish')">Add</button>
										</div>
										<div class="queue-chip-wrap">
											@for (queue of publishQueues(); track queue) {
												<button class="queue-chip" type="button" (click)="removeQueue('publish', queue)">
													{{ queue }} <span>×</span>
												</button>
											}
										</div>
									</div>

									<div class="queue-col">
										<div class="queue-row">
											<mat-form-field class="queue-input">
												<mat-label>Subscribe Queue</mat-label>
												<input matInput formControlName="subscribeQueueInput" placeholder="e.g. intake-events" />
											</mat-form-field>
											<button mat-stroked-button type="button" (click)="addQueue('subscribe')">Add</button>
										</div>
										<div class="queue-chip-wrap">
											@for (queue of subscribeQueues(); track queue) {
												<button class="queue-chip" type="button" (click)="removeQueue('subscribe', queue)">
													{{ queue }} <span>×</span>
												</button>
											}
										</div>
									</div>
								</div>
							</div>
						}

						<!-- GitHub & Infrastructure -->
						<div class="settings-section">
							<h4>GitHub &amp; Infrastructure</h4>
							<div class="checkbox-col">
								<mat-checkbox formControlName="pushToGitHub">
									Push scaffold to GitHub
								</mat-checkbox>
								<mat-checkbox formControlName="createGitHubRepo" [disabled]="!form.controls.pushToGitHub.value">
									Create GitHub repository
								</mat-checkbox>
								<mat-checkbox formControlName="deployInfrastructure">
									Deploy infrastructure
								</mat-checkbox>
							</div>
						</div>
					</mat-card-content>
				</mat-card>

				<!-- STEP 7: Review & Generate -->
				<mat-card class="step-card" [class.active]="activeStep() === 6">
					<mat-card-header>
						<mat-card-title>Step 7: Review &amp; Generate</mat-card-title>
					</mat-card-header>
					<mat-card-content>
						<div class="review-sections">
							<div class="review-section">
								<h4>Configuration Summary</h4>
								<div class="review-grid">
									<div class="review-item">
										<span class="label">Vertical</span>
										<span class="value">{{ form.controls.verticalSlug.value || 'N/A' }}</span>
									</div>
									<div class="review-item">
										<span class="label">Build Type</span>
										<span class="value">{{ selectedBuildTypeLabel() }}</span>
									</div>
									<div class="review-item">
										<span class="label">Service Name</span>
										<span class="value">{{ form.controls.name.value || 'N/A' }}</span>
									</div>
									<div class="review-item">
										<span class="label">{{ reviewModulesLabel() }}</span>
										<span class="value">{{ reviewModulesSummary() }}</span>
									</div>
								</div>
							</div>

							<div class="review-section">
								<h4>Deployment Configuration</h4>
								<div class="review-grid">
									<div class="review-item">
										<span class="label">Database</span>
										<span class="value">{{ reviewDatabaseSummary() }}</span>
									</div>
									<div class="review-item">
										<span class="label">Queues</span>
										<span class="value">{{ reviewQueuesSummary() }}</span>
									</div>
									<div class="review-item">
										<span class="label">GitHub</span>
										<span class="value">{{ reviewGitHubSummary() }}</span>
									</div>
									<div class="review-item">
										<span class="label">Infrastructure</span>
										<span class="value">{{ form.controls.deployInfrastructure.value ? 'Deploy enabled' : 'Deploy disabled' }}</span>
									</div>
								</div>
							</div>
						</div>

						@if (validationPreview()) {
							<div class="preview-card" [class.invalid]="!validationPreview()!.isValid">
								<h4>Manifest Validation</h4>
								<p><strong>Status:</strong> {{ validationPreview()!.isValid ? '✓ Valid' : '✗ Invalid' }}</p>
								@if (validationPreview()!.errors.length > 0) {
									<ul class="error-list">
										@for (error of validationPreview()!.errors; track error) {
											<li>{{ error }}</li>
										}
									</ul>
								}
							</div>
						}

						@if (generationGitHubUrl()) {
							<div class="success-card">
								<p><strong>Repository:</strong> <a [href]="generationGitHubUrl()" target="_blank">{{ generationGitHubUrl() }}</a></p>
							</div>
						}

						@if (generationError() || validationError()) {
							<div class="error-card">
								<p>{{ generationError() || validationError() }}</p>
							</div>
						}
					</mat-card-content>
				</mat-card>
			</form>

			<!-- Footer Actions -->
			<div class="wizard-footer">
				<div class="footer-actions">
					<button
						mat-stroked-button
						type="button"
						(click)="validateManifest()"
						[disabled]="form.invalid || isValidating() || isGenerating()"
					>
						{{ isValidating() ? 'Validating...' : 'Validate Manifest' }}
					</button>
					<button
						mat-raised-button
						color="primary"
						type="button"
						(click)="generateService()"
						[disabled]="form.invalid || !canGenerate() || isGenerating()"
					>
						{{ isGenerating() ? 'Generating...' : generateActionLabel() }}
					</button>
				</div>
			</div>
		</div>
	`,
	styles: [
		`
			.wizard-container {
				max-width: 1000px;
				margin: 0 auto;
				padding: 2rem 1rem;
				background: #fafbfc;
				min-height: 100vh;
			}

			.wizard-header {
				text-align: center;
				margin-bottom: 2rem;
			}

			.wizard-header h1 {
				font-size: 1.875rem;
				font-weight: 700;
				color: #0f172a;
				margin: 0;
			}

			.wizard-header p {
				font-size: 0.95rem;
				color: #64748b;
				margin: 0.5rem 0 0;
			}

			.wizard-progress-bar {
				margin-bottom: 2rem;
				padding: 1.5rem;
				background: #fff;
				border-radius: 0.75rem;
				box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
			}

			.progress-track {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
				gap: 1rem;
			}

			.progress-node {
				display: flex;
				flex-direction: column;
				align-items: center;
				gap: 0.5rem;
				position: relative;
			}

			.progress-node.complete .node-circle {
				background: #10b981;
				color: #fff;
				font-weight: 700;
			}

			.progress-node.current .node-circle {
				background: #3b82f6;
				color: #fff;
				box-shadow: 0 0 0 4px rgba(59, 130, 246, 0.1);
			}

			.progress-node.future .node-circle {
				background: #e2e8f0;
				color: #94a3b8;
			}

			.node-circle {
				width: 32px;
				height: 32px;
				border-radius: 50%;
				display: flex;
				align-items: center;
				justify-content: center;
				font-size: 0.875rem;
				font-weight: 600;
				transition: all 0.2s ease;
			}

			.node-label {
				font-size: 0.75rem;
				font-weight: 500;
				color: #475569;
				text-align: center;
				line-height: 1.2;
			}

			.wizard-steps {
				display: flex;
				flex-direction: column;
				gap: 1.5rem;
				margin-bottom: 2rem;
			}

			.step-card {
				opacity: 0.5;
				pointer-events: none;
				transition: opacity 0.3s ease;
			}

			.step-card.active {
				opacity: 1;
				pointer-events: auto;
			}

			mat-card-header {
				margin-bottom: 1rem;
			}

			mat-card-title {
				font-size: 1.25rem;
				font-weight: 600;
				color: #0f172a;
				margin: 0;
			}

			mat-card-subtitle {
				font-size: 0.875rem;
				color: #64748b;
				margin-top: 0.25rem;
			}

			.info-grid {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
				gap: 1rem;
				margin-top: 1rem;
				padding-top: 1rem;
				border-top: 1px solid #e2e8f0;
			}

			.info-item {
				display: flex;
				flex-direction: column;
				gap: 0.25rem;
			}

			.info-item .label {
				font-size: 0.75rem;
				font-weight: 600;
				color: #64748b;
				text-transform: uppercase;
				letter-spacing: 0.05em;
			}

			.info-item .value {
				font-size: 0.95rem;
				font-weight: 500;
				color: #0f172a;
			}

			.family-switch {
				display: flex;
				gap: 0.75rem;
				margin-bottom: 1.5rem;
				flex-wrap: wrap;
			}

			.family-button {
				padding: 0.6rem 1rem;
				border: 2px solid #cbd5e1;
				border-radius: 0.5rem;
				background: #f8fafc;
				color: #475569;
				font-weight: 600;
				cursor: pointer;
				transition: all 0.2s ease;
			}

			.family-button.selected {
				border-color: #3b82f6;
				background: #dbeafe;
				color: #1e40af;
			}

			.build-types {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
				gap: 1.5rem;
			}

			.build-group h4 {
				font-size: 0.95rem;
				font-weight: 600;
				color: #0f172a;
				margin: 0 0 0.75rem;
			}

			.build-group.inactive h4 {
				color: #cbd5e1;
			}

			.type-options {
				display: flex;
				flex-direction: column;
				gap: 0.5rem;
			}

			.type-button {
				display: flex;
				flex-direction: column;
				align-items: flex-start;
				gap: 0.25rem;
				padding: 0.75rem;
				border: 1px solid #cbd5e1;
				border-radius: 0.5rem;
				background: #fff;
				cursor: pointer;
				transition: all 0.2s ease;
				text-align: left;
			}

			.type-button:hover:not(:disabled) {
				border-color: #94a3b8;
				background: #f1f5f9;
			}

			.type-button.selected {
				border-color: #3b82f6;
				background: #dbeafe;
			}

			.type-button.coming-soon {
				border-color: #cbd5e1;
				background: #f1f5f9;
				color: #94a3b8;
				cursor: not-allowed;
			}

			.type-label {
				font-weight: 600;
				font-size: 0.9rem;
				color: inherit;
			}

			.type-note {
				font-size: 0.75rem;
				color: #64748b;
			}

			.form-grid-2col {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1rem;
				margin-bottom: 1rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.module-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
				gap: 0.75rem;
				margin: 1rem 0;
			}

			.module-button {
				display: flex;
				flex-direction: column;
				align-items: flex-start;
				gap: 0.25rem;
				padding: 0.75rem;
				border: 1px solid #cbd5e1;
				border-radius: 0.5rem;
				background: #fff;
				cursor: pointer;
				transition: all 0.2s ease;
				text-align: left;
				position: relative;
			}

			.module-button:hover {
				border-color: #94a3b8;
				background: #f8fafc;
			}

			.module-button.selected {
				border-color: #3b82f6;
				background: #dbeafe;
			}

			.module-label {
				font-weight: 600;
				font-size: 0.85rem;
				color: #0f172a;
			}

			.module-desc {
				font-size: 0.7rem;
				color: #64748b;
			}

			.module-badge {
				position: absolute;
				top: 0.25rem;
				right: 0.25rem;
				background: #059669;
				color: #fff;
				font-size: 0.6rem;
				padding: 0.15rem 0.35rem;
				border-radius: 999px;
				font-weight: 600;
			}

			.module-actions {
				display: flex;
				gap: 0.5rem;
				margin-top: 0.75rem;
			}

			.feature-section {
				margin-bottom: 2rem;
			}

			.section-title {
				display: flex;
				align-items: center;
				gap: 0.5rem;
				font-size: 0.95rem;
				font-weight: 600;
				color: #0f172a;
				margin: 0 0 1rem;
				text-transform: uppercase;
				letter-spacing: 0.05em;
			}

			.section-title mat-icon {
				font-size: 1.1rem;
				width: 1.1rem;
				height: 1.1rem;
			}

			.baseline-features {
				display: flex;
				flex-direction: column;
				gap: 0.75rem;
			}

			.feature-item {
				display: flex;
				align-items: flex-start;
				gap: 0.75rem;
				padding: 0.75rem;
				border: 1px solid #d1e7dd;
				border-radius: 0.5rem;
				background: #f0f9f6;
			}

			.feature-badge {
				flex-shrink: 0;
				width: 28px;
				height: 28px;
				display: flex;
				align-items: center;
				justify-content: center;
				border-radius: 50%;
				font-weight: 700;
				font-size: 0.9rem;
			}

			.feature-badge.success {
				background: #10b981;
				color: #fff;
			}

			.feature-info {
				display: flex;
				flex-direction: column;
				gap: 0.15rem;
			}

			.feature-name {
				font-weight: 600;
				font-size: 0.9rem;
				color: #0f172a;
			}

			.feature-desc {
				font-size: 0.8rem;
				color: #64748b;
			}

			.selectable-features,
			.conditional-features {
				display: flex;
				flex-direction: column;
				gap: 0.75rem;
			}

			.feature-checkbox {
				padding: 0.75rem;
				border: 1px solid #cbd5e1;
				border-radius: 0.5rem;
				background: #fff;
				transition: all 0.2s ease;
			}

			.feature-checkbox:has(input:checked) {
				border-color: #3b82f6;
				background: #dbeafe;
			}

			.feature-checkbox strong {
				display: block;
				font-size: 0.9rem;
				color: #0f172a;
				margin-bottom: 0.25rem;
			}

			.feature-checkbox p {
				font-size: 0.8rem;
				color: #64748b;
				margin: 0;
			}

			.settings-section {
				margin-bottom: 1.5rem;
				padding-bottom: 1.5rem;
				border-bottom: 1px solid #e2e8f0;
			}

			.settings-section:last-child {
				border-bottom: none;
			}

			.settings-section h4 {
				font-size: 0.95rem;
				font-weight: 600;
				color: #0f172a;
				margin: 0 0 0.75rem;
			}

			.checkbox-row,
			.checkbox-col {
				display: flex;
				flex-direction: column;
				gap: 0.5rem;
			}

			.queue-builder {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
				gap: 1rem;
				margin-top: 0.75rem;
			}

			.queue-col {
				display: flex;
				flex-direction: column;
				gap: 0.5rem;
			}

			.queue-row {
				display: flex;
				gap: 0.5rem;
				align-items: center;
			}

			.queue-input {
				flex: 1;
			}

			.queue-chip-wrap {
				display: flex;
				gap: 0.4rem;
				flex-wrap: wrap;
			}

			.queue-chip {
				border: 1px solid #cbd5e1;
				background: #f8fafc;
				color: #0f172a;
				border-radius: 999px;
				padding: 0.3rem 0.6rem;
				font-size: 0.75rem;
				cursor: pointer;
				transition: all 0.2s ease;
			}

			.queue-chip:hover {
				background: #fee2e2;
				border-color: #ef4444;
			}

			.review-sections {
				display: flex;
				flex-direction: column;
				gap: 1.5rem;
			}

			.review-section h4 {
				font-size: 0.95rem;
				font-weight: 600;
				color: #0f172a;
				margin: 0 0 0.75rem;
			}

			.review-grid {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
				gap: 0.75rem;
			}

			.review-item {
				display: flex;
				flex-direction: column;
				gap: 0.25rem;
				padding: 0.75rem;
				border: 1px solid #e2e8f0;
				border-radius: 0.5rem;
				background: #f8fafc;
			}

			.review-item .label {
				font-size: 0.7rem;
				font-weight: 600;
				color: #64748b;
				text-transform: uppercase;
				letter-spacing: 0.05em;
			}

			.review-item .value {
				font-size: 0.9rem;
				color: #0f172a;
				font-weight: 500;
				word-break: break-word;
			}

			.preview-card {
				border: 1px solid #d1e7dd;
				border-radius: 0.5rem;
				padding: 1rem;
				margin-top: 1rem;
				background: #f0f9f6;
			}

			.preview-card.invalid {
				border-color: #f87171;
				background: #fef2f2;
			}

			.preview-card h4 {
				margin: 0 0 0.5rem;
				font-size: 0.95rem;
				font-weight: 600;
				color: #0f172a;
			}

			.preview-card p {
				margin: 0.25rem 0;
				font-size: 0.9rem;
				color: #0f172a;
			}

			.error-list {
				list-style: none;
				margin: 0.5rem 0 0;
				padding: 0;
			}

			.error-list li {
				font-size: 0.85rem;
				color: #dc2626;
				margin: 0.25rem 0;
				padding-left: 1.25rem;
				position: relative;
			}

			.error-list li:before {
				content: '✗';
				position: absolute;
				left: 0;
				font-weight: 700;
			}

			.success-card {
				border: 1px solid #d1e7dd;
				border-radius: 0.5rem;
				padding: 1rem;
				margin-top: 1rem;
				background: #f0f9f6;
			}

			.success-card p {
				margin: 0;
				font-size: 0.9rem;
				color: #0f172a;
			}

			.success-card a {
				color: #0f766e;
				font-weight: 600;
				text-decoration: none;
			}

			.success-card a:hover {
				text-decoration: underline;
			}

			.error-card {
				border: 1px solid #f87171;
				border-radius: 0.5rem;
				padding: 1rem;
				margin-top: 1rem;
				background: #fef2f2;
			}

			.error-card p {
				margin: 0;
				font-size: 0.9rem;
				color: #dc2626;
				font-weight: 500;
			}

			.loading-hint {
				color: #64748b;
				font-size: 0.875rem;
				margin: 0.5rem 0;
			}

			.wizard-footer {
				position: sticky;
				bottom: 0;
				background: #fff;
				border-top: 1px solid #e2e8f0;
				padding: 1rem 2rem;
				display: flex;
				justify-content: center;
			}

			.footer-actions {
				display: flex;
				gap: 1rem;
				justify-content: center;
			}

			@media (max-width: 768px) {
				.wizard-container {
					padding: 1rem;
				}

				.progress-track {
					grid-template-columns: repeat(2, 1fr);
				}

				.node-label {
					font-size: 0.65rem;
				}

				.build-types {
					grid-template-columns: 1fr;
				}

				.form-grid-2col {
					grid-template-columns: 1fr;
				}

				.module-grid {
					grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
				}

				.review-grid {
					grid-template-columns: 1fr;
				}

				.queue-builder {
					grid-template-columns: 1fr;
				}

				.wizard-footer {
					padding: 1rem;
				}

				.footer-actions {
					flex-direction: column;
					width: 100%;
				}

				.footer-actions button {
					width: 100%;
				}
			}
		`,
	],
})
export class GenerationWizardV2Component {
	private readonly fb = inject(FormBuilder);
	private readonly route = inject(ActivatedRoute);
	private readonly generationApiService = inject(GENERATION_API_SERVICE) as IGenerationApiService;
	private readonly verticalApiService = inject(VERTICAL_API_SERVICE) as IVerticalApiService;
	private readonly modulesApiService = inject(MODULES_API_SERVICE) as IModulesApiService;
	
	readonly serviceCategories: ServiceCategory[] = ['Backend Service', 'Frontend App'];
	readonly buildTypeOptions: BuildTypeOption[] = [
		{ label: 'REST API', value: 'RestApi', category: 'Backend Service', live: true, note: 'Live now' },
		{ label: 'GraphQL', value: 'GraphQL', category: 'Backend Service', live: true, note: 'Live now' },
		{ label: 'gRPC', value: 'Grpc', category: 'Backend Service', live: true, note: 'Live now' },
		{ label: 'Shell App', value: 'AngularShell', category: 'Frontend App', live: true, note: 'Live now' },
		{ label: 'Micro Frontend (MFE)', value: 'AngularMfe', category: 'Frontend App', live: true, note: 'Live now' },
	];

	readonly backendTypeOptions = this.buildTypeOptions.filter((option) => option.category === 'Backend Service');
	readonly frontendTypeOptions = this.buildTypeOptions.filter((option) => option.category === 'Frontend App');

	readonly form = this.fb.nonNullable.group({
		verticalSlug: ['', Validators.required],
		serviceCategory: ['Backend Service' as ServiceCategory],
		name: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
		displayName: ['', Validators.required],
		description: ['', [Validators.required, Validators.minLength(10)]],
		version: ['1.0.0', Validators.required],
		type: ['RestApi' as BuildTypeOption['value']],
		publishQueueInput: [''],
		subscribeQueueInput: [''],
		useDatabase: [false],
		databaseEngine: [''],
		databaseSchema: [''],
		deployInfrastructure: [false],
		pushToGitHub: [true],
		createGitHubRepo: [true],
		authenticationEnabled: [true],
		secretsManagementEnabled: [false],
		resilienceEnabled: [false],
		rateLimitingEnabled: [false],
		auditLoggingEnabled: [false],
		piiClassificationEnabled: [false],
		outputCachingEnabled: [false],
	});

	readonly verticals = signal<VerticalSummaryResponse[]>([]);
	readonly selectedVertical = signal<VerticalSummaryResponse | null>(null);
	readonly isLoadingVerticals = signal(false);
	readonly availableModules = signal<GenesisModule[]>([]);
	readonly selectedModules = signal<string[]>([]);
	readonly isLoadingModules = signal(false);
	readonly availableCanvasModules = signal<CanvasModule[]>([]);
	readonly selectedCanvasModules = signal<string[]>([]);
	readonly isLoadingCanvasModules = signal(false);
	readonly publishQueues = signal<string[]>([]);
	readonly subscribeQueues = signal<string[]>([]);

	readonly wizardSteps = ['Select Vertical', 'Choose Build Type', 'Service Details', 'Modules', 'Production Readiness', 'Deployment Settings', 'Review & Generate'] as const;
	readonly activeStep = signal(0);

	readonly isValidating = signal(false);
	readonly isGenerating = signal(false);
	readonly validationPreview = signal<ValidationPreviewResult | null>(null);
	readonly validationError = signal<string | null>(null);
	readonly generationError = signal<string | null>(null);
	readonly generationGitHubUrl = signal<string | null>(null);
	readonly generatedZipFileName = signal<string | null>(null);
	readonly generatedAt = signal<string | null>(null);

	constructor() {
		const routeSlug = this.route.snapshot.paramMap.get('slug')?.trim() ?? '';
		if (routeSlug) {
			this.form.controls.verticalSlug.setValue(routeSlug, { emitEvent: false });
		}

		this.loadVerticals(routeSlug);
		this.loadModules();
		this.loadCanvasModules();

		this.form.controls.verticalSlug.valueChanges.subscribe((slug) => {
			this.applyVerticalSelection(slug);
		});

		this.form.controls.pushToGitHub.valueChanges.subscribe((pushEnabled) => {
			if (!pushEnabled) {
				this.form.controls.createGitHubRepo.setValue(false, { emitEvent: false });
			}
		});

		this.form.controls.useDatabase.valueChanges.subscribe((enabled) => {
			if (!enabled) {
				this.form.controls.databaseEngine.setValue('', { emitEvent: false });
				this.form.controls.databaseSchema.setValue('', { emitEvent: false });
			}
		});

		this.form.valueChanges.subscribe(() => {
			this.validationPreview.set(null);
			this.validationError.set(null);
			this.generationError.set(null);
		});
	}

	selectBuildType(type: BuildTypeOption['value']): void {
		const option = this.buildTypeOptions.find((item) => item.value === type);
		if (!option || !option.live) return;

		this.form.controls.serviceCategory.setValue(option.category, { emitEvent: false });
		this.form.controls.type.setValue(type);

		if (option.category !== 'Backend Service') {
			this.clearModules();
			this.form.controls.useDatabase.setValue(false);
			this.publishQueues.set([]);
			this.subscribeQueues.set([]);
			this.form.controls.publishQueueInput.setValue('');
			this.form.controls.subscribeQueueInput.setValue('');
			this.syncCanvasModulesToType(type);
		} else {
			this.clearCanvasModules();
		}
	}

	selectServiceCategory(category: ServiceCategory): void {
		if (this.form.controls.serviceCategory.value === category) return;

		this.form.controls.serviceCategory.setValue(category);
		const nextOption = this.buildTypeOptions.find((option) => option.category === category && option.live);
		if (nextOption) this.selectBuildType(nextOption.value);
	}

	isTypeCategoryActive(category: ServiceCategory): boolean {
		return this.form.controls.serviceCategory.value === category;
	}

	isTypeOptionDisabled(option: BuildTypeOption): boolean {
		return !option.live || !this.isTypeCategoryActive(option.category);
	}

	completedStepIndex(): number {
		if (!this.form.controls.verticalSlug.value.trim()) return 0;
		if (!this.form.controls.type.value) return 1;
		if (!this.form.controls.name.valid || !this.form.controls.displayName.valid || !this.form.controls.description.valid) return 2;
		if (this.isBackendTypeSelected() && this.form.controls.useDatabase.value && (!this.form.controls.databaseEngine.value.trim() || !this.form.controls.databaseSchema.value.trim())) return 5;
		if (!this.canGenerate()) return 6;
		return 7;
	}

	isBackendTypeSelected(): boolean {
		return ['RestApi', 'GraphQL', 'Grpc'].includes(this.form.controls.type.value);
	}

	isCanvasTypeSelected(): boolean {
		return ['AngularShell', 'AngularMfe'].includes(this.form.controls.type.value);
	}

	canGenerate(): boolean {
		return this.validationPreview()?.isValid === true;
	}

	generateActionLabel(): string {
		return this.form.controls.pushToGitHub.value ? 'Generate + Push' : 'Generate ZIP';
	}

	selectedBuildTypeLabel(): string {
		const selected = this.buildTypeOptions.find((option) => option.value === this.form.controls.type.value);
		return selected?.label ?? this.form.controls.type.value;
	}

	serviceNameHint(): string {
		if (this.form.controls.type.value === 'AngularShell') return 'Kebab-case, must end with -shell.';
		if (this.form.controls.type.value === 'AngularMfe') return 'Kebab-case, must not end with -shell or -service.';
		return 'Kebab-case only, e.g. intake-service';
	}

	modulesStepTitle(): string {
		if (this.form.controls.type.value === 'AngularShell') return 'Canvas Modules (Shell)';
		if (this.form.controls.type.value === 'AngularMfe') return 'Canvas Modules (Micro Frontend)';
		return 'Genesis Modules (Backend)';
	}

	reviewModulesLabel(): string {
		return this.isCanvasTypeSelected() ? 'Canvas Modules' : 'Genesis Modules';
	}

	reviewModulesSummary(): string {
		if (this.isCanvasTypeSelected()) {
			const modules = this.selectedCanvasModules();
			return modules.length > 0 ? modules.join(', ') : 'No modules';
		}
		if (!this.isBackendTypeSelected()) return 'N/A';
		const modules = this.selectedModules();
		return modules.length > 0 ? modules.join(', ') : 'No modules';
	}

	reviewDatabaseSummary(): string {
		if (!this.isBackendTypeSelected()) return 'N/A';
		if (!this.form.controls.useDatabase.value) return 'Not included';
		const engine = this.form.controls.databaseEngine.value || '?';
		const schema = this.form.controls.databaseSchema.value || '?';
		return `${engine} / ${schema}`;
	}

	reviewGitHubSummary(): string {
		if (!this.form.controls.pushToGitHub.value) return 'Disabled';
		return this.form.controls.createGitHubRepo.value ? 'Push + Create' : 'Push only';
	}

	reviewQueuesSummary(): string {
		if (!this.isBackendTypeSelected()) return 'N/A';
		const publish = this.publishQueues();
		const subscribe = this.subscribeQueues();
		if (publish.length === 0 && subscribe.length === 0) return 'None';
		const parts: string[] = [];
		if (publish.length > 0) parts.push(`Pub: ${publish.join(',')}`);
		if (subscribe.length > 0) parts.push(`Sub: ${subscribe.join(',')}`);
		return parts.join(' | ');
	}

	toggleModule(name: string): void {
		const current = this.selectedModules();
		this.selectedModules.set(current.includes(name) ? current.filter((m) => m !== name) : [...current, name]);
	}

	isModuleSelected(name: string): boolean {
		return this.selectedModules().includes(name);
	}

	toggleCanvasModule(name: string): void {
		const current = this.selectedCanvasModules();
		this.selectedCanvasModules.set(current.includes(name) ? current.filter((m) => m !== name) : [...current, name]);
	}

	isCanvasModuleSelected(name: string): boolean {
		return this.selectedCanvasModules().includes(name);
	}

	selectAllModules(): void {
		this.selectedModules.set(this.availableModules().map((m) => m.name));
	}

	selectAllCanvasModules(): void {
		this.selectedCanvasModules.set(this.visibleCanvasModules().map((m) => m.name));
	}

	clearModules(): void {
		this.selectedModules.set([]);
	}

	clearCanvasModules(): void {
		this.selectedCanvasModules.set([]);
	}

	visibleCanvasModules(): CanvasModule[] {
		const allowed = new Set(this.allowedCanvasModuleNamesForCurrentType());
		return this.availableCanvasModules().filter((module) => allowed.has(module.name));
	}

	isShellDefaultCanvasModule(name: string): boolean {
		return this.form.controls.type.value === 'AngularShell' && SHELL_PRESELECTED_CANVAS_MODULES.includes(name as any);
	}

	addQueue(role: 'publish' | 'subscribe'): void {
		const sourceControl = role === 'publish' ? this.form.controls.publishQueueInput : this.form.controls.subscribeQueueInput;
		const value = sourceControl.value.trim();
		if (!value) return;

		const existing = role === 'publish' ? this.publishQueues() : this.subscribeQueues();
		if (existing.includes(value)) {
			sourceControl.setValue('');
			return;
		}

		const next = [...existing, value];
		if (role === 'publish') {
			this.publishQueues.set(next);
		} else {
			this.subscribeQueues.set(next);
		}
		sourceControl.setValue('');
	}

	removeQueue(role: 'publish' | 'subscribe', queueName: string): void {
		if (role === 'publish') {
			this.publishQueues.set(this.publishQueues().filter((q) => q !== queueName));
		} else {
			this.subscribeQueues.set(this.subscribeQueues().filter((q) => q !== queueName));
		}
	}

	validateManifest(): void {
		const request = this.buildRequest();
		if (!request) return;

		this.isValidating.set(true);
		this.validationError.set(null);
		this.generationApiService.validateManifest(request).subscribe({
			next: (preview) => {
				this.validationPreview.set(preview);
				this.isValidating.set(false);
			},
			error: () => {
				this.validationPreview.set(null);
				this.validationError.set('Manifest validation failed.');
				this.isValidating.set(false);
			},
		});
	}

	generateService(): void {
		const request = this.buildRequest();
		if (!request || !this.canGenerate()) return;

		this.isGenerating.set(true);
		this.generationError.set(null);

		this.generationApiService.generateService(request).subscribe({
			next: (response) => {
				this.downloadZip(response.zipBlob, response.fileName);
				this.generatedZipFileName.set(response.fileName);
				this.generatedAt.set(response.generationTimestamp ?? new Date().toISOString());
				this.generationGitHubUrl.set(response.gitHubRepoUrl);
				this.isGenerating.set(false);
			},
			error: (error) => this.handleGenerationError(error),
		});
	}

	private handleGenerationError(error: unknown): void {
		if (!(error instanceof HttpErrorResponse)) {
			this.generationError.set('Generation failed.');
			this.isGenerating.set(false);
			return;
		}

		if (Array.isArray(error.error?.errors)) {
			this.generationError.set(error.error.errors.join(' '));
		} else if (error.error instanceof Blob) {
			error.error.text().then((text) => {
				try {
					const payload = JSON.parse(text);
					this.generationError.set(payload.errors?.[0] || payload.message || text);
				} catch {
					this.generationError.set(text.trim() || 'Generation failed.');
				}
				this.isGenerating.set(false);
			});
			return;
		} else {
			this.generationError.set(error.message || 'Generation failed.');
		}
		this.isGenerating.set(false);
	}

	private buildRequest(): GenerationRequest | null {
		if (this.form.invalid) {
			this.validationError.set('Please complete required fields.');
			return null;
		}

		const value = this.form.getRawValue();
		const verticalSlug = value.verticalSlug.trim();
		const serviceName = value.name.trim();

		if (!verticalSlug) {
			this.validationError.set('Please select a vertical.');
			return null;
		}

		if (value.type === 'AngularShell' && !serviceName.endsWith('-shell')) {
			this.validationError.set('Shell names must end with -shell.');
			return null;
		}

		if (value.type === 'AngularMfe' && (serviceName.endsWith('-shell') || serviceName.endsWith('-service'))) {
			this.validationError.set('MFE names must not end with -shell or -service.');
			return null;
		}

		const databaseEngineMap: Record<string, string> = {
			postgresql: 'PostgreSQL',
			sqlserver: 'SqlServer',
			mysql: 'MySQL',
		};

		const enterprise: EnterpriseScaffoldOptions = {
			authenticationEnabled: value.authenticationEnabled,
			secretsManagementEnabled: value.secretsManagementEnabled,
			resilienceEnabled: value.resilienceEnabled,
			rateLimitingEnabled: value.rateLimitingEnabled,
			auditLoggingEnabled: value.auditLoggingEnabled,
			piiClassificationEnabled: value.piiClassificationEnabled,
			outputCachingEnabled: value.outputCachingEnabled,
		};

		return {
			verticalSlug,
			name: serviceName,
			displayName: value.displayName.trim(),
			description: value.description.trim(),
			version: value.version.trim(),
			type: value.type,
			genesisModules: this.isBackendTypeSelected() ? this.selectedModules() : [],
			canvasModules: this.isCanvasTypeSelected() ? this.selectedCanvasModules() : undefined,
			database: value.useDatabase ? {
				engine: databaseEngineMap[value.databaseEngine.trim()] ?? value.databaseEngine.trim(),
				schema: value.databaseSchema.trim(),
			} : null,
			createGitHubRepo: value.createGitHubRepo,
			enterprise,
		};
	}

	private loadVerticals(preferredSlug: string): void {
		this.isLoadingVerticals.set(true);
		this.verticalApiService.listVerticals().subscribe({
			next: (verticals) => {
				this.verticals.set(verticals);
				const initialVertical = verticals.find((v) => v.slug === preferredSlug) ?? verticals[0] ?? null;
				if (initialVertical) {
					this.form.controls.verticalSlug.setValue(initialVertical.slug, { emitEvent: false });
					this.selectedVertical.set(initialVertical);
				}
				this.isLoadingVerticals.set(false);
			},
			error: () => {
				this.validationError.set('Unable to load verticals.');
				this.isLoadingVerticals.set(false);
			},
		});
	}

	private applyVerticalSelection(slug: string): void {
		this.selectedVertical.set(this.verticals().find((v) => v.slug === slug) ?? null);
	}

	private loadModules(): void {
		this.isLoadingModules.set(true);
		this.modulesApiService.listModules().subscribe({
			next: (modules) => {
				this.availableModules.set(modules);
				this.isLoadingModules.set(false);
			},
			error: () => this.isLoadingModules.set(false),
		});
	}

	private loadCanvasModules(): void {
		this.isLoadingCanvasModules.set(true);
		this.modulesApiService.listCanvasModules().subscribe({
			next: (modules) => {
				this.availableCanvasModules.set(modules);
				this.syncCanvasModulesToType(this.form.controls.type.value);
				this.isLoadingCanvasModules.set(false);
			},
			error: () => this.isLoadingCanvasModules.set(false),
		});
	}

	private allowedCanvasModuleNamesForCurrentType(): string[] {
		if (this.form.controls.type.value === 'AngularShell') {
			return [...SHELL_PRESELECTED_CANVAS_MODULES, ...SHARED_CANVAS_MODULES];
		}
		if (this.form.controls.type.value === 'AngularMfe') {
			return [...SHARED_CANVAS_MODULES, ...MFE_ONLY_CANVAS_MODULES];
		}
		return [];
	}

	private syncCanvasModulesToType(type: BuildTypeOption['value']): void {
		if (!['AngularShell', 'AngularMfe'].includes(type)) {
			this.clearCanvasModules();
			return;
		}

		const allowed = new Set(this.allowedCanvasModuleNamesForCurrentType());
		const next = this.selectedCanvasModules().filter((name) => allowed.has(name));

		if (type === 'AngularShell') {
			for (const moduleName of SHELL_PRESELECTED_CANVAS_MODULES) {
				if (!next.includes(moduleName)) next.push(moduleName);
			}
		}

		this.selectedCanvasModules.set([...new Set(next)]);
	}

	private downloadZip(blob: Blob, fileName: string): void {
		const objectUrl = URL.createObjectURL(blob);
		const anchor = document.createElement('a');
		anchor.href = objectUrl;
		anchor.download = fileName;
		anchor.click();
		URL.revokeObjectURL(objectUrl);
	}
}
