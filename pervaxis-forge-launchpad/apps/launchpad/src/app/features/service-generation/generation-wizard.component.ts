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
import { CanvasModule, GenerationRequest, GenesisModule, ValidationPreviewResult } from '@core/models/generation.model';
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
	selector: 'forge-generation-wizard',
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
		<mat-card class="wizard-card">
			<h2>Generate Service</h2>
			<p class="subtitle">Step 1: Select Vertical Context</p>

			<div class="wizard-progress">
				@for (step of wizardSteps; track step; let index = $index) {
					<div
						class="progress-pill"
						[class.complete]="index < completedStepIndex()"
						[class.current]="index === completedStepIndex()"
					>
						{{ index + 1 }}. {{ step }}
					</div>
				}
			</div>

			<form [formGroup]="form" class="form-grid">
				<mat-form-field class="full-width">
					<mat-label>Vertical</mat-label>
					<mat-select formControlName="verticalSlug" [disabled]="isLoadingVerticals()">
						@for (vertical of verticals(); track vertical.slug) {
							<mat-option [value]="vertical.slug">
								{{ vertical.displayName }} ({{ vertical.slug }})
							</mat-option>
						}
					</mat-select>
				</mat-form-field>

				<mat-form-field floatLabel="always">
					<mat-label>Cloud Provider</mat-label>
					<input matInput [value]="selectedVertical()?.cloudProvider ?? 'N/A'" readonly />
				</mat-form-field>

				<mat-form-field floatLabel="always">
					<mat-label>GitHub Org</mat-label>
					<input matInput [value]="selectedVertical()?.githubOrg ?? 'N/A'" readonly />
				</mat-form-field>

				<section class="full-width type-section">
					<h3>Step 2: What are you building?</h3>
					<div class="family-switch" role="tablist" aria-label="Service family">
						@for (category of serviceCategories; track category) {
							<button
								type="button"
								class="family-switch-button"
								[class.selected]="isTypeCategoryActive(category)"
								(click)="selectServiceCategory(category)"
							>
								{{ category }}
							</button>
						}
					</div>
					<div class="type-grid">
						<div class="type-group" [class.inactive]="!isTypeCategoryActive('Backend Service')">
							<h4>Backend Service</h4>
							<div class="type-options">
								@for (option of backendTypeOptions; track option.value) {
									<button
										type="button"
										class="type-option"
										[class.selected]="form.controls.type.value === option.value"
										[class.coming-soon]="!option.live"
										[class.inactive]="isTypeOptionDisabled(option) && option.live"
										[disabled]="isTypeOptionDisabled(option)"
										(click)="selectBuildType(option.value)"
									>
										<span>{{ option.label }}</span>
										<small>{{ option.note }}</small>
									</button>
								}
							</div>
						</div>

						<div class="type-group" [class.inactive]="!isTypeCategoryActive('Frontend App')">
							<h4>Frontend App</h4>
							<div class="type-options">
								@for (option of frontendTypeOptions; track option.value) {
									<button
										type="button"
										class="type-option"
										[class.selected]="form.controls.type.value === option.value"
										[class.coming-soon]="!option.live"
										[class.inactive]="isTypeOptionDisabled(option) && option.live"
										[disabled]="isTypeOptionDisabled(option)"
										(click)="selectBuildType(option.value)"
									>
										<span>{{ option.label }}</span>
										<small>{{ option.note }}</small>
									</button>
								}
							</div>
						</div>
					</div>
				</section>

				<section class="full-width step-section">
					<h3>Step 3: Service Details</h3>
				</section>

				<mat-form-field>
					<mat-label>Service Name *</mat-label>
					<input matInput formControlName="name" placeholder="e.g. intake-service" />
					<mat-hint>{{ serviceNameHint() }}</mat-hint>
				</mat-form-field>

				<mat-form-field>
					<mat-label>Display Name *</mat-label>
					<input matInput formControlName="displayName" placeholder="e.g. Intake Service" />
				</mat-form-field>

				<mat-form-field class="full-width">
					<mat-label>Description *</mat-label>
					<textarea matInput rows="3" formControlName="description" placeholder="Short description of the service"></textarea>
				</mat-form-field>

				<mat-form-field>
					<mat-label>Version</mat-label>
					<input matInput formControlName="version" placeholder="1.0.0" />
				</mat-form-field>

				<section class="full-width step-section">
					<h3>{{ modulesStepTitle() }}</h3>
					@if (isBackendTypeSelected() && isLoadingModules()) {
						<p class="loading-hint">Loading genesis modules...</p>
					} @else if (isCanvasTypeSelected() && isLoadingCanvasModules()) {
						<p class="loading-hint">Loading Canvas modules...</p>
					} @else if (isBackendTypeSelected()) {
						<div class="module-grid">
							@for (module of availableModules(); track module.name) {
								<button
									type="button"
									class="module-option"
									[class.selected]="isModuleSelected(module.name)"
									(click)="toggleModule(module.name)"
								>
									<strong>{{ module.label }}</strong>
									<small>{{ module.description }}</small>
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
									class="module-option"
									[class.selected]="isCanvasModuleSelected(module.name)"
									(click)="toggleCanvasModule(module.name)"
								>
									<strong>{{ module.label }}</strong>
									@if (isShellDefaultCanvasModule(module.name)) {
										<small>Default for shell apps</small>
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
				</section>

				<section class="full-width step-section">
					<h3>Step 5: Database Configuration</h3>
					<mat-slide-toggle formControlName="useDatabase" [disabled]="!isBackendTypeSelected()">
						Include database
					</mat-slide-toggle>
				</section>

				<section class="full-width step-section">
					<h3>Messaging Queues (Backend optional)</h3>
					@if (!isBackendTypeSelected()) {
						<p class="loading-hint">Queues are available only for backend services.</p>
					} @else {
						<div class="queue-builder-grid">
							<div class="queue-builder-col">
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
											{{ queue }} <span aria-hidden="true">x</span>
										</button>
									}
								</div>
							</div>

							<div class="queue-builder-col">
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
											{{ queue }} <span aria-hidden="true">x</span>
										</button>
									}
								</div>
							</div>
						</div>
					}
				</section>

				@if (form.controls.useDatabase.value && isBackendTypeSelected()) {
					<mat-form-field>
						<mat-label>Database Engine *</mat-label>
						<mat-select formControlName="databaseEngine">
							<mat-option value="">Select engine</mat-option>
							<mat-option value="postgresql">postgresql</mat-option>
							<mat-option value="sqlserver">sqlserver</mat-option>
							<mat-option value="mysql">mysql</mat-option>
						</mat-select>
					</mat-form-field>

					<mat-form-field>
						<mat-label>DB Schema *</mat-label>
						<input matInput formControlName="databaseSchema" placeholder="e.g. intake" />
					</mat-form-field>
				}

				<section class="full-width step-section">
					<h3>Step 6: GitHub Repository</h3>
					<div class="checkbox-row">
						<mat-checkbox formControlName="pushToGitHub">Push scaffold to GitHub</mat-checkbox>
						<mat-checkbox formControlName="createGitHubRepo" [disabled]="!form.controls.pushToGitHub.value">
							Create GitHub repository
						</mat-checkbox>
					</div>
				</section>

				<section class="full-width step-section">
					<h3>Infrastructure</h3>
					<div class="checkbox-row">
						<mat-checkbox formControlName="deployInfrastructure">
							Deploy infrastructure
						</mat-checkbox>
					</div>
				</section>

				<section class="full-width step-section">
					<h3>Step 7: Review & Generate</h3>
					<div class="review-grid">
						<div class="review-item">
							<span class="review-label">Vertical</span>
							<span class="review-value">{{ form.controls.verticalSlug.value || 'N/A' }}</span>
						</div>
						<div class="review-item">
							<span class="review-label">Build Type</span>
							<span class="review-value">{{ selectedBuildTypeLabel() }}</span>
						</div>
						<div class="review-item">
							<span class="review-label">Service Name</span>
							<span class="review-value">{{ form.controls.name.value || 'N/A' }}</span>
						</div>
						<div class="review-item full-width">
							<span class="review-label">{{ reviewModulesLabel() }}</span>
							<span class="review-value">{{ reviewModulesSummary() }}</span>
						</div>
						<div class="review-item full-width">
							<span class="review-label">Database</span>
							<span class="review-value">{{ reviewDatabaseSummary() }}</span>
						</div>
						<div class="review-item full-width">
							<span class="review-label">Queues</span>
							<span class="review-value">{{ reviewQueuesSummary() }}</span>
						</div>
						<div class="review-item full-width">
							<span class="review-label">GitHub</span>
							<span class="review-value">{{ reviewGitHubSummary() }}</span>
						</div>
						<div class="review-item full-width">
							<span class="review-label">Infrastructure</span>
							<span class="review-value">
								{{ form.controls.deployInfrastructure.value ? 'Deploy enabled' : 'Deploy disabled' }}
							</span>
						</div>
					</div>
				</section>
			</form>

			<div class="actions">
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

			@if (validationPreview()) {
				<section class="preview-card" [class.preview-invalid]="!validationPreview()!.isValid">
					<h3>Manifest Preview</h3>
					<p><strong>Valid:</strong> {{ validationPreview()!.isValid ? 'Yes' : 'No' }}</p>
					<p><strong>Service:</strong> {{ validationPreview()!.serviceName ?? 'N/A' }}</p>
					<p><strong>Namespace:</strong> {{ validationPreview()!.namespace ?? 'N/A' }}</p>
					<p><strong>Project:</strong> {{ validationPreview()!.projectName ?? 'N/A' }}</p>
					@if (validationPreview()!.errors.length > 0) {
						<ul>
							@for (error of validationPreview()!.errors; track error) {
								<li>{{ error }}</li>
							}
						</ul>
					}
				</section>
			}

			@if (generationGitHubUrl()) {
				<p class="success">
					Repository created:
					<a [href]="generationGitHubUrl()" target="_blank" rel="noopener noreferrer">
						{{ generationGitHubUrl() }}
					</a>
				</p>
			}

			@if (generationSummaryVisible()) {
				<section class="result-card">
					<h3>Generation Complete</h3>
					<p><strong>ZIP:</strong> {{ generatedZipFileName() }}</p>
					<p><strong>Generated At:</strong> {{ generatedAt() | date: 'short' }}</p>
					@if (generationGitHubUrl()) {
						<p>
							<strong>Repository:</strong>
							<a [href]="generationGitHubUrl()" target="_blank" rel="noopener noreferrer">
								{{ generationGitHubUrl() }}
							</a>
						</p>
					}
				</section>
			}

			@if (validationError()) {
				<p class="error">{{ validationError() }}</p>
			}

			@if (generationError()) {
				<p class="error">{{ generationError() }}</p>
			}
		</mat-card>
	`,
	styles: [
		`
			.wizard-card {
				max-width: 64rem;
				margin: 0 auto;
			}

			.subtitle {
				margin-bottom: 1rem;
			}

			.wizard-progress {
				display: flex;
				gap: 0.5rem;
				flex-wrap: wrap;
				margin-bottom: 1rem;
			}

			.progress-pill {
				padding: 0.25rem 0.6rem;
				border-radius: 999px;
				border: 1px solid #d1d5db;
				background: #f9fafb;
				font-size: 0.75rem;
				color: #4b5563;
			}

			.progress-pill.complete {
				border-color: #065f46;
				background: #d1fae5;
				color: #065f46;
			}

			.progress-pill.current {
				border-color: #1d4ed8;
				background: #dbeafe;
				color: #1e40af;
				font-weight: 600;
			}

			.form-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1rem;
			}

			.wizard-card ::ng-deep .mat-mdc-text-field-wrapper {
				min-height: 3.75rem;
			}

			.wizard-card ::ng-deep .mat-mdc-form-field-infix {
				min-height: 3.75rem;
				padding-top: 1rem;
				padding-bottom: 1rem;
			}

			.wizard-card ::ng-deep textarea.mat-mdc-input-element {
				min-height: 4.75rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.checkbox-row {
				display: flex;
				gap: 1rem;
				flex-wrap: wrap;
			}

			.type-section {
				border: 1px solid #d0d7de;
				border-radius: 0.5rem;
				padding: 1rem;
			}

			.type-section h3 {
				margin: 0 0 0.75rem;
			}

			.family-switch {
				display: flex;
				gap: 0.75rem;
				margin-bottom: 1rem;
				flex-wrap: wrap;
			}

			.family-switch-button {
				padding: 0.7rem 1rem;
				border-radius: 0.65rem;
				border: 1px solid #d1d5db;
				background: #f9fafb;
				color: #374151;
				font-weight: 600;
				cursor: pointer;
			}

			.family-switch-button.selected {
				border-color: #2563eb;
				background: #dbeafe;
				color: #1d4ed8;
			}

			.type-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1rem;
			}

			.type-group h4 {
				margin: 0 0 0.5rem;
				font-size: 0.95rem;
			}

			.type-group.inactive h4 {
				color: #9ca3af;
			}

			.type-options {
				display: flex;
				flex-direction: column;
				gap: 0.5rem;
			}

			.type-option {
				display: flex;
				flex-direction: column;
				align-items: flex-start;
				gap: 0.2rem;
				padding: 0.75rem;
				border: 1px solid #c7d2fe;
				border-radius: 0.5rem;
				background: #eef2ff;
				color: #1f2937;
				cursor: pointer;
			}

			.type-option small {
				color: #4b5563;
			}

			.type-option.selected {
				border-color: #2563eb;
				background: #dbeafe;
			}

			.type-option.coming-soon {
				border-color: #d1d5db;
				background: #f3f4f6;
				color: #6b7280;
				cursor: not-allowed;
			}

			.type-option.inactive {
				border-color: #d1d5db;
				background: #f8fafc;
				color: #9ca3af;
				cursor: not-allowed;
			}

			.step-section {
				border-top: 1px solid #e5e7eb;
				padding-top: 0.75rem;
				margin-top: 0.25rem;
			}

			.step-section h3 {
				margin: 0;
				font-size: 0.9rem;
				font-weight: 600;
				color: #374151;
				text-transform: uppercase;
				letter-spacing: 0.05em;
			}

			.module-grid {
				display: grid;
				grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
				gap: 0.75rem;
				margin-top: 0.5rem;
			}

			.module-option {
				display: flex;
				flex-direction: column;
				gap: 0.25rem;
				padding: 0.75rem 1rem;
				border: 1px solid #d1d5db;
				border-radius: 0.5rem;
				background: #fff;
				cursor: pointer;
				text-align: left;
				transition: all 0.15s ease;
			}

			.module-option:hover {
				border-color: #6b7280;
				background: #f9fafb;
			}

			.module-option.selected {
				border-color: #2563eb;
				background: #dbeafe;
			}

			.module-option strong {
				font-size: 0.875rem;
			}

			.module-option small {
				font-size: 0.75rem;
				color: #6b7280;
			}

			.module-actions {
				display: flex;
				gap: 0.5rem;
				margin-top: 0.5rem;
			}

			.queue-builder-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1rem;
				margin-top: 0.5rem;
			}

			.queue-builder-col {
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
				border: 1px solid #c7d2fe;
				background: #eef2ff;
				color: #1f2937;
				border-radius: 999px;
				padding: 0.25rem 0.6rem;
				font-size: 0.75rem;
				cursor: pointer;
			}

			.loading-hint {
				color: #6b7280;
				font-size: 0.875rem;
				margin: 0.5rem 0;
			}

			.review-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 0.75rem;
				margin-top: 0.5rem;
			}

			.review-item {
				display: flex;
				flex-direction: column;
				gap: 0.15rem;
				padding: 0.6rem 0.75rem;
				border: 1px solid #e5e7eb;
				border-radius: 0.5rem;
				background: #f9fafb;
			}

			.review-label {
				font-size: 0.75rem;
				font-weight: 600;
				color: #6b7280;
				text-transform: uppercase;
				letter-spacing: 0.03em;
			}

			.review-value {
				font-size: 0.9rem;
				color: #111827;
			}

			.actions {
				display: flex;
				gap: 0.75rem;
				margin: 1rem 0;
			}

			.preview-card {
				border: 1px solid #d0d7de;
				border-radius: 0.5rem;
				padding: 1rem;
				margin-top: 1rem;
			}

			.preview-invalid {
				border-color: #b3261e;
			}

			.result-card {
				border: 1px solid #0f766e;
				border-radius: 0.5rem;
				padding: 1rem;
				margin-top: 1rem;
				background: #f0fdfa;
			}

			.error {
				color: #b3261e;
				margin-top: 0.75rem;
			}

			.success {
				color: #0f766e;
				margin-top: 0.75rem;
			}

			@media (max-width: 768px) {
				.form-grid {
					grid-template-columns: 1fr;
				}

				.type-grid {
					grid-template-columns: 1fr;
				}

				.review-grid {
					grid-template-columns: 1fr;
				}

				.queue-builder-grid {
					grid-template-columns: 1fr;
				}
			}
		`,
	],
})
export class GenerationWizardComponent {
	private readonly fb = inject(FormBuilder);
	private readonly route = inject(ActivatedRoute);
	private readonly generationApiService = inject(GENERATION_API_SERVICE) as IGenerationApiService;
	private readonly verticalApiService = inject(VERTICAL_API_SERVICE) as IVerticalApiService;
	private readonly modulesApiService = inject(MODULES_API_SERVICE) as IModulesApiService;
	readonly serviceCategories: ServiceCategory[] = ['Backend Service', 'Frontend App'];

	readonly buildTypeOptions: BuildTypeOption[] = [
		{
			label: 'REST API',
			value: 'RestApi',
			category: 'Backend Service',
			live: true,
			note: 'Live now',
		},
		{
			label: 'GraphQL',
			value: 'GraphQL',
			category: 'Backend Service',
			live: true,
			note: 'Live now',
		},
		{
			label: 'gRPC',
			value: 'Grpc',
			category: 'Backend Service',
			live: true,
			note: 'Live now',
		},
		{
			label: 'Shell App',
			value: 'AngularShell',
			category: 'Frontend App',
			live: true,
			note: 'Live now',
		},
		{
			label: 'Micro Frontend (MFE)',
			value: 'AngularMfe',
			category: 'Frontend App',
			live: true,
			note: 'Live now',
		},
	];

	readonly backendTypeOptions = this.buildTypeOptions.filter(
		(option) => option.category === 'Backend Service'
	);
	readonly frontendTypeOptions = this.buildTypeOptions.filter(
		(option) => option.category === 'Frontend App'
	);

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

	readonly wizardSteps = [
		'Select Vertical',
		'Choose Build Type',
		'Service Details',
		'Genesis Modules',
		'Database',
		'GitHub',
		'Review & Generate',
	] as const;

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
			this.generationGitHubUrl.set(null);
			this.generatedZipFileName.set(null);
			this.generatedAt.set(null);
		});
	}

	selectBuildType(type: BuildTypeOption['value']): void {
		const option = this.buildTypeOptions.find((item) => item.value === type);
		if (!option || !option.live) {
			return;
		}

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
		if (this.form.controls.serviceCategory.value === category) {
			return;
		}

		this.form.controls.serviceCategory.setValue(category);

		const nextOption = this.buildTypeOptions.find(
			(option) => option.category === category && option.live
		);

		if (nextOption) {
			this.selectBuildType(nextOption.value);
		}
	}

	isTypeCategoryActive(category: ServiceCategory): boolean {
		return this.form.controls.serviceCategory.value === category;
	}

	isTypeOptionDisabled(option: BuildTypeOption): boolean {
		return !option.live || !this.isTypeCategoryActive(option.category);
	}

	completedStepIndex(): number {
		if (!this.form.controls.verticalSlug.value.trim()) {
			return 0;
		}

		if (!this.form.controls.name.valid || !this.form.controls.displayName.valid || !this.form.controls.description.valid) {
			return 2;
		}

		if (this.isBackendTypeSelected() && this.form.controls.useDatabase.value) {
			if (!this.form.controls.databaseEngine.value.trim() || !this.form.controls.databaseSchema.value.trim()) {
				return 4;
			}
		}

		if (!this.canGenerate()) {
			return 6;
		}

		return 7;
	}

	isBackendTypeSelected(): boolean {
		return this.form.controls.type.value === 'RestApi' ||
			this.form.controls.type.value === 'GraphQL' ||
			this.form.controls.type.value === 'Grpc';
	}

	isCanvasTypeSelected(): boolean {
		return this.form.controls.type.value === 'AngularShell' ||
			this.form.controls.type.value === 'AngularMfe';
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
		if (this.form.controls.type.value === 'AngularShell') {
			return 'Kebab-case only, and the name must end with -shell.';
		}

		if (this.form.controls.type.value === 'AngularMfe') {
			return 'Kebab-case only, and the name must not end with -shell or -service.';
		}

		return 'Kebab-case only, e.g. intake-service';
	}

	modulesStepTitle(): string {
		if (this.form.controls.type.value === 'AngularShell') {
			return 'Step 4: Canvas Modules (Shell)';
		}

		if (this.form.controls.type.value === 'AngularMfe') {
			return 'Step 4: Canvas Modules (Micro Frontend)';
		}

		return 'Step 4: Genesis Modules (Backend only)';
	}

	reviewModulesLabel(): string {
		return this.isCanvasTypeSelected() ? 'Canvas Modules' : 'Genesis Modules';
	}

	reviewModulesSummary(): string {
		if (this.isCanvasTypeSelected()) {
			const modules = this.selectedCanvasModules();
			return modules.length > 0 ? modules.join(', ') : 'No Canvas modules selected';
		}

		if (!this.isBackendTypeSelected()) {
			return 'Not applicable for this type';
		}

		const modules = this.selectedModules();
		return modules.length > 0 ? modules.join(', ') : 'No modules selected';
	}

	reviewDatabaseSummary(): string {
		if (!this.isBackendTypeSelected()) {
			return 'Not applicable for frontend apps';
		}

		if (!this.form.controls.useDatabase.value) {
			return 'Database not included';
		}

		const engine = this.form.controls.databaseEngine.value || 'engine pending';
		const schema = this.form.controls.databaseSchema.value || 'schema pending';
		return `${engine} / ${schema}`;
	}

	reviewGitHubSummary(): string {
		if (!this.form.controls.pushToGitHub.value) {
			return 'GitHub push disabled';
		}

		return this.form.controls.createGitHubRepo.value
			? 'Push enabled, create repository enabled'
			: 'Push enabled, create repository disabled';
	}

	reviewQueuesSummary(): string {
		if (!this.isBackendTypeSelected()) {
			return 'Not applicable for frontend apps';
		}

		const publish = this.publishQueues();
		const subscribe = this.subscribeQueues();
		if (publish.length === 0 && subscribe.length === 0) {
			return 'No queues configured';
		}

		const parts: string[] = [];
		if (publish.length > 0) {
			parts.push(`Publish: ${publish.join(', ')}`);
		}
		if (subscribe.length > 0) {
			parts.push(`Subscribe: ${subscribe.join(', ')}`);
		}

		return parts.join(' | ');
	}

	addQueue(role: 'publish' | 'subscribe'): void {
		const sourceControl = role === 'publish'
			? this.form.controls.publishQueueInput
			: this.form.controls.subscribeQueueInput;
		const value = sourceControl.value.trim();
		if (!value) {
			return;
		}

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
			this.publishQueues.set(this.publishQueues().filter((queue) => queue !== queueName));
			return;
		}

		this.subscribeQueues.set(this.subscribeQueues().filter((queue) => queue !== queueName));
	}

	generationSummaryVisible(): boolean {
		return !!this.generatedZipFileName() || !!this.generationGitHubUrl();
	}

	validateManifest(): void {
		const request = this.buildRequest();
		if (!request) {
			return;
		}

		this.isValidating.set(true);
		this.validationError.set(null);
		this.generationError.set(null);
		this.generationGitHubUrl.set(null);

		this.generationApiService.validateManifest(request).subscribe({
			next: (preview) => {
				this.validationPreview.set(preview);
				this.isValidating.set(false);
			},
			error: () => {
				this.validationPreview.set(null);
				this.validationError.set('Manifest validation failed. Please review your input.');
				this.isValidating.set(false);
			},
		});
	}

	generateService(): void {
		const request = this.buildRequest();
		if (!request) {
			return;
		}

		if (!this.canGenerate()) {
			this.generationError.set('Validate manifest successfully before generating.');
			return;
		}

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
			error: (error) => {
				this.handleGenerationError(error);
				this.isGenerating.set(false);
			},
		});
	}

	private handleGenerationError(error: unknown): void {
		if (!(error instanceof HttpErrorResponse)) {
			this.generationError.set('Generation failed. Please retry.');
			return;
		}

		if (Array.isArray(error.error?.errors) && error.error.errors.length > 0) {
			this.generationError.set(error.error.errors.join(' '));
			return;
		}

		if (error.error instanceof Blob) {
			void error.error.text().then((raw) => {
				try {
					const payload = JSON.parse(raw) as { errors?: string[]; message?: string };
					if (Array.isArray(payload.errors) && payload.errors.length > 0) {
						this.generationError.set(payload.errors.join(' '));
						return;
					}

					if (payload.message) {
						this.generationError.set(payload.message);
						return;
					}
				} catch {
					if (raw.trim()) {
						this.generationError.set(raw.trim());
						return;
					}
				}

				this.generationError.set('Generation failed. Please retry.');
			});
			return;
		}

		if (typeof error.error === 'string' && error.error.trim()) {
			this.generationError.set(error.error.trim());
			return;
		}

		if (error.message) {
			this.generationError.set(error.message);
			return;
		}

		this.generationError.set('Generation failed. Please retry.');
	}

	private buildRequest(): GenerationRequest | null {
		if (this.form.invalid) {
			this.validationError.set('Please complete required fields before continuing.');
			return null;
		}

		const value = this.form.getRawValue();
		const verticalSlug = value.verticalSlug.trim();
		const serviceName = value.name.trim();
		if (!verticalSlug) {
			this.validationError.set('Please select a vertical before continuing.');
			return null;
		}

		if (value.type === 'AngularShell' && !serviceName.endsWith('-shell')) {
			this.validationError.set('Shell App names must end with -shell.');
			return null;
		}

		if (value.type === 'AngularMfe' && (serviceName.endsWith('-shell') || serviceName.endsWith('-service'))) {
			this.validationError.set('Micro Frontend (MFE) names must not end with -shell or -service.');
			return null;
		}

		const genesisModules = this.isBackendTypeSelected() ? this.selectedModules() : [];
		const canvasModules = this.isCanvasTypeSelected() ? this.selectedCanvasModules() : undefined;
		if (value.useDatabase) {
			if (!value.databaseEngine.trim() || !value.databaseSchema.trim()) {
				this.validationError.set('Database engine and schema are required when database is enabled.');
				return null;
			}
		}

		const hasDatabase = value.useDatabase;

		const databaseEngineMap: Record<string, string> = {
			postgresql: 'PostgreSQL',
			sqlserver: 'SqlServer',
			mysql: 'MySQL',
		};

		return {
			verticalSlug,
			name: serviceName,
			displayName: value.displayName.trim(),
			description: value.description.trim(),
			version: value.version.trim(),
			type: value.type,
			genesisModules,
			canvasModules,
			database: hasDatabase
				? {
					engine: databaseEngineMap[value.databaseEngine.trim()] ?? value.databaseEngine.trim(),
					schema: value.databaseSchema.trim(),
				}
				: null,
			createGitHubRepo: value.createGitHubRepo,
		};
	}

	private loadVerticals(preferredSlug: string): void {
		this.isLoadingVerticals.set(true);

		this.verticalApiService.listVerticals().subscribe({
			next: (verticals) => {
				this.verticals.set(verticals);

				const selectedSlug = this.form.controls.verticalSlug.value.trim();
				const initialVertical =
					verticals.find((vertical) => vertical.slug === preferredSlug) ??
					verticals.find((vertical) => vertical.slug === selectedSlug) ??
					verticals[0] ??
					null;

				if (initialVertical) {
					this.form.controls.verticalSlug.setValue(initialVertical.slug, { emitEvent: false });
					this.selectedVertical.set(initialVertical);
				}

				this.isLoadingVerticals.set(false);
			},
			error: () => {
				this.validationError.set('Unable to load verticals. Please refresh and retry.');
				this.isLoadingVerticals.set(false);
			},
		});
	}

	private applyVerticalSelection(slug: string): void {
		const selected = this.verticals().find((vertical) => vertical.slug === slug) ?? null;
		this.selectedVertical.set(selected);

	}

	toggleModule(name: string): void {
		const current = this.selectedModules();
		if (current.includes(name)) {
			this.selectedModules.set(current.filter((m) => m !== name));
		} else {
			this.selectedModules.set([...current, name]);
		}
	}

	isModuleSelected(name: string): boolean {
		return this.selectedModules().includes(name);
	}

	toggleCanvasModule(name: string): void {
		const current = this.selectedCanvasModules();
		if (current.includes(name)) {
			this.selectedCanvasModules.set(current.filter((moduleName) => moduleName !== name));
			return;
		}

		this.selectedCanvasModules.set([...current, name]);
	}

	isCanvasModuleSelected(name: string): boolean {
		return this.selectedCanvasModules().includes(name);
	}

	selectAllModules(): void {
		this.selectedModules.set(this.availableModules().map((m) => m.name));
	}

	selectAllCanvasModules(): void {
		this.selectedCanvasModules.set(this.visibleCanvasModules().map((module) => module.name));
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
		return this.form.controls.type.value === 'AngularShell' && SHELL_PRESELECTED_CANVAS_MODULES.includes(name as typeof SHELL_PRESELECTED_CANVAS_MODULES[number]);
	}

	private loadModules(): void {
		this.isLoadingModules.set(true);

		this.modulesApiService.listModules().subscribe({
			next: (modules) => {
				this.availableModules.set(modules);
				this.isLoadingModules.set(false);
			},
			error: () => {
				this.isLoadingModules.set(false);
			},
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
			error: () => {
				this.isLoadingCanvasModules.set(false);
			},
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
		if (type !== 'AngularShell' && type !== 'AngularMfe') {
			this.clearCanvasModules();
			return;
		}

		const allowed = new Set(this.allowedCanvasModuleNamesForCurrentType());
		const next = this.selectedCanvasModules().filter((name) => allowed.has(name));

		if (type === 'AngularShell') {
			for (const moduleName of SHELL_PRESELECTED_CANVAS_MODULES) {
				next.push(moduleName);
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
