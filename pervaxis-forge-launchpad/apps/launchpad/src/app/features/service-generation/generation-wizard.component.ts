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
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import {
	GENERATION_API_SERVICE,
	IGenerationApiService,
} from '@core/api/generation-api.service';
import { GenerationRequest, ValidationPreviewResult } from '@core/models/generation.model';

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
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-card class="wizard-card">
			<h2>Generate Service</h2>
			<p class="subtitle">Vertical: <strong>{{ slug }}</strong></p>

			<form [formGroup]="form" class="form-grid">
				<mat-form-field>
					<mat-label>Service Name</mat-label>
					<input matInput formControlName="name" />
				</mat-form-field>

				<mat-form-field>
					<mat-label>Display Name</mat-label>
					<input matInput formControlName="displayName" />
				</mat-form-field>

				<mat-form-field class="full-width">
					<mat-label>Description</mat-label>
					<textarea matInput rows="3" formControlName="description"></textarea>
				</mat-form-field>

				<mat-form-field>
					<mat-label>Type</mat-label>
					<mat-select formControlName="type">
						<mat-option value="BFF">BFF</mat-option>
						<mat-option value="MFE">MFE</mat-option>
					</mat-select>
				</mat-form-field>

				<mat-form-field>
					<mat-label>Version</mat-label>
					<input matInput formControlName="version" />
				</mat-form-field>

				<mat-form-field>
					<mat-label>Environment</mat-label>
					<input matInput formControlName="environment" />
				</mat-form-field>

				<mat-form-field>
					<mat-label>Database Engine (optional)</mat-label>
					<mat-select formControlName="databaseEngine">
						<mat-option value="">None</mat-option>
						<mat-option value="postgresql">postgresql</mat-option>
						<mat-option value="mysql">mysql</mat-option>
					</mat-select>
				</mat-form-field>

				<mat-form-field>
					<mat-label>DB Schema (optional)</mat-label>
					<input matInput formControlName="databaseSchema" />
				</mat-form-field>

				<mat-form-field class="full-width">
					<mat-label>Genesis Modules (comma separated)</mat-label>
					<input matInput formControlName="genesisModulesCsv" />
				</mat-form-field>

				<mat-form-field class="full-width">
					<mat-label>Publish Queues (comma separated)</mat-label>
					<input matInput formControlName="publishQueuesCsv" />
				</mat-form-field>

				<mat-form-field class="full-width">
					<mat-label>Subscribe Queues (comma separated)</mat-label>
					<input matInput formControlName="subscribeQueuesCsv" />
				</mat-form-field>

				<div class="checkbox-row full-width">
					<mat-checkbox formControlName="deployInfrastructure">
						Deploy infrastructure
					</mat-checkbox>
					<mat-checkbox formControlName="pushToGitHub">Push to GitHub</mat-checkbox>
					<mat-checkbox formControlName="createGitHubRepo">
						Create GitHub repository
					</mat-checkbox>
				</div>
			</form>

			<div class="actions">
				<button
					mat-stroked-button
					type="button"
					(click)="validateManifest()"
					[disabled]="form.invalid || isValidating()"
				>
					Validate Manifest
				</button>
				<button
					mat-raised-button
					color="primary"
					type="button"
					(click)="generateService()"
					[disabled]="form.invalid || !canGenerate() || isGenerating()"
				>
					Generate + Push
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

			.form-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.checkbox-row {
				display: flex;
				gap: 1rem;
				flex-wrap: wrap;
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
			}
		`,
	],
})
export class GenerationWizardComponent {
	private readonly fb = inject(FormBuilder);
	private readonly route = inject(ActivatedRoute);
	private readonly generationApiService = inject(GENERATION_API_SERVICE) as IGenerationApiService;

	readonly form = this.fb.nonNullable.group({
		name: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
		displayName: ['', Validators.required],
		description: ['', [Validators.required, Validators.minLength(10)]],
		version: ['1.0.0', Validators.required],
		type: ['BFF'],
		genesisModulesCsv: [''],
		publishQueuesCsv: [''],
		subscribeQueuesCsv: [''],
		databaseEngine: [''],
		databaseSchema: [''],
		environment: ['test', Validators.required],
		deployInfrastructure: [false],
		pushToGitHub: [true],
		createGitHubRepo: [true],
	});

	readonly isValidating = signal(false);
	readonly isGenerating = signal(false);
	readonly validationPreview = signal<ValidationPreviewResult | null>(null);
	readonly validationError = signal<string | null>(null);
	readonly generationError = signal<string | null>(null);
	readonly generationGitHubUrl = signal<string | null>(null);
	readonly generatedZipFileName = signal<string | null>(null);
	readonly generatedAt = signal<string | null>(null);

	constructor() {
		this.form.valueChanges.subscribe(() => {
			this.validationPreview.set(null);
			this.validationError.set(null);
			this.generationError.set(null);
			this.generationGitHubUrl.set(null);
			this.generatedZipFileName.set(null);
			this.generatedAt.set(null);
		});
	}

	get slug(): string {
		return this.route.snapshot.paramMap.get('slug') ?? 'unknown';
	}

	canGenerate(): boolean {
		return this.validationPreview()?.isValid === true;
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
				this.generatedAt.set(new Date().toISOString());
				this.generationGitHubUrl.set(response.gitHubRepoUrl);
				this.isGenerating.set(false);
			},
			error: () => {
				this.generationError.set('Generation failed. Please retry.');
				this.isGenerating.set(false);
			},
		});
	}

	private buildRequest(): GenerationRequest | null {
		if (this.form.invalid || !this.slug || this.slug === 'unknown') {
			this.validationError.set('Please complete required fields before continuing.');
			return null;
		}

		const value = this.form.getRawValue();
		const genesisModules = value.genesisModulesCsv
			.split(',')
			.map((item: string) => item.trim())
			.filter((item: string) => item.length > 0);
		const publishQueues = value.publishQueuesCsv
			.split(',')
			.map((item: string) => item.trim())
			.filter((item: string) => item.length > 0)
			.map((name: string) => ({ name, role: 'publish' as const }));
		const subscribeQueues = value.subscribeQueuesCsv
			.split(',')
			.map((item: string) => item.trim())
			.filter((item: string) => item.length > 0)
			.map((name: string) => ({ name, role: 'subscribe' as const }));

		const hasDatabase = value.databaseEngine.trim().length > 0 && value.databaseSchema.trim().length > 0;

		return {
			verticalSlug: this.slug,
			name: value.name.trim(),
			displayName: value.displayName.trim(),
			description: value.description.trim(),
			version: value.version.trim(),
			type: value.type as 'BFF' | 'MFE',
			genesisModules,
			database: hasDatabase
				? {
					engine: value.databaseEngine.trim(),
					schema: value.databaseSchema.trim(),
				}
				: null,
			queues: [...publishQueues, ...subscribeQueues],
			metadata: {
				deployInfrastructure: value.deployInfrastructure,
				pushToGitHub: value.pushToGitHub,
				environment: value.environment.trim(),
			},
			createGitHubRepo: value.createGitHubRepo,
		};
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
