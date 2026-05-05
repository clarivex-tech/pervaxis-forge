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

import { CommonModule } from '@angular/common';
import {
	ChangeDetectionStrategy,
	Component,
	effect,
	inject,
	signal,
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';

import {
	IVerticalApiService,
	VERTICAL_API_SERVICE,
} from '../../core/api/vertical-api.service';
import {
	EnrollmentState,
	initialEnrollmentState,
	toEnrollmentRequest,
} from './enrollment.state';
import { ConnectivityValidationResponse, VerticalSummaryResponse } from '../../core/models/vertical.model';

@Component({
	selector: 'forge-vertical-enrollment',
	standalone: true,
	imports: [
		CommonModule,
		ReactiveFormsModule,
		MatStepperModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
		MatCheckboxModule,
		MatButtonModule,
		MatCardModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-card>
			<h2>Vertical Enrollment Wizard</h2>
			<mat-stepper [linear]="true" [selectedIndex]="currentStep()" (selectionChange)="onStepSelection($event.selectedIndex)">
				<mat-step [stepControl]="identityForm" label="Vertical Identity">
					<form [formGroup]="identityForm" class="form-grid">
						<mat-form-field appearance="outline">
							<mat-label>Slug</mat-label>
							<input matInput type="text" formControlName="slug" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Display Name</mat-label>
							<input matInput type="text" formControlName="displayName" />
						</mat-form-field>
						<mat-form-field appearance="outline" class="full-width">
							<mat-label>Description</mat-label>
							<textarea matInput rows="3" formControlName="description"></textarea>
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Owner Team</mat-label>
							<input matInput type="text" formControlName="ownerTeam" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Owner Email</mat-label>
							<input matInput type="email" formControlName="ownerEmail" />
						</mat-form-field>
						<div class="actions full-width">
							<button mat-raised-button color="primary" matStepperNext (click)="syncStateFromForms()">Next</button>
						</div>
					</form>
				</mat-step>

				<mat-step [stepControl]="cloudForm" label="Cloud Provider">
					<form [formGroup]="cloudForm" class="form-grid">
						<mat-form-field appearance="outline">
							<mat-label>Provider</mat-label>
							<mat-select formControlName="provider">
								<mat-option value="AWS">AWS</mat-option>
								<mat-option value="Azure" disabled>Azure (coming soon)</mat-option>
								<mat-option value="GCP" disabled>GCP (coming soon)</mat-option>
							</mat-select>
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>AWS Account Id</mat-label>
							<input matInput type="text" formControlName="awsAccountId" />
						</mat-form-field>
						<mat-form-field appearance="outline" class="full-width">
							<mat-label>IAM Role ARN</mat-label>
							<input matInput type="text" formControlName="iamRoleArn" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Default Region</mat-label>
							<input matInput type="text" formControlName="defaultRegion" />
						</mat-form-field>
						<div class="actions full-width">
							<button mat-button matStepperPrevious (click)="syncStateFromForms()">Back</button>
							<button mat-raised-button color="primary" matStepperNext (click)="syncStateFromForms()">Next</button>
						</div>
					</form>
				</mat-step>

				<mat-step [stepControl]="sourceControlForm" label="Source Control">
					<form [formGroup]="sourceControlForm" class="form-grid">
						<mat-form-field appearance="outline">
							<mat-label>Platform</mat-label>
							<mat-select formControlName="platform">
								<mat-option value="GitHub">GitHub</mat-option>
							</mat-select>
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>GitHub Organization</mat-label>
							<input matInput type="text" formControlName="gitHubOrg" />
						</mat-form-field>
						<mat-form-field appearance="outline" class="full-width">
							<mat-label>Access Token</mat-label>
							<input matInput type="password" formControlName="accessToken" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Visibility</mat-label>
							<mat-select formControlName="defaultVisibility">
								<mat-option value="private">private</mat-option>
								<mat-option value="internal">internal</mat-option>
								<mat-option value="public">public</mat-option>
							</mat-select>
						</mat-form-field>
						<mat-checkbox formControlName="defaultBranchProtection">Default branch protection</mat-checkbox>
						<div class="actions full-width">
							<button mat-button matStepperPrevious (click)="syncStateFromForms()">Back</button>
							<button mat-raised-button color="primary" matStepperNext (click)="syncStateFromForms()">Next</button>
						</div>
					</form>
				</mat-step>

				<mat-step [stepControl]="techDefaultsForm" label="Tech Defaults">
					<form [formGroup]="techDefaultsForm" class="form-grid">
						<mat-form-field appearance="outline" class="full-width">
							<mat-label>Environments (comma separated)</mat-label>
							<input matInput type="text" formControlName="environmentsCsv" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Default Environment</mat-label>
							<input matInput type="text" formControlName="defaultEnvironment" />
						</mat-form-field>
						<mat-form-field appearance="outline">
							<mat-label>Default DB Engine</mat-label>
							<input matInput type="text" formControlName="defaultDbEngine" />
						</mat-form-field>
						<mat-checkbox formControlName="generateTerraform">Generate Terraform</mat-checkbox>
						<mat-checkbox formControlName="generateCdk">Generate CDK</mat-checkbox>
						<div class="actions full-width">
							<button mat-button matStepperPrevious (click)="syncStateFromForms()">Back</button>
							<button mat-raised-button color="primary" matStepperNext (click)="syncStateFromForms()">Next</button>
						</div>
					</form>
				</mat-step>

				<mat-step label="Review & Enroll">
					<div class="form-grid">
						<pre class="full-width">{{ state() | json }}</pre>
						<div class="actions full-width">
							<button mat-stroked-button type="button" (click)="validateConnectivity()" [disabled]="state().connectivity.isChecking">
								Validate Connectivity
							</button>
							<button mat-raised-button color="primary" type="button" (click)="enrollVertical()" [disabled]="state().submit.isSubmitting">
								Enroll Vertical
							</button>
						</div>
						<p class="full-width" *ngIf="state().connectivity.errorMessage">{{ state().connectivity.errorMessage }}</p>
						<p class="full-width" *ngIf="state().submit.submitError">{{ state().submit.submitError }}</p>
						<p class="full-width" *ngIf="state().submit.submitSuccess">Vertical enrolled successfully.</p>
					</div>
				</mat-step>
			</mat-stepper>
		</mat-card>
	`,
	styles: [
		`
			mat-card {
				max-width: 64rem;
				margin: 0 auto;
			}

			.form-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 0.75rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.actions {
				display: flex;
				gap: 0.5rem;
				justify-content: flex-end;
			}
		`,
	],
})
export class VerticalEnrollmentComponent {
	private readonly fb = inject(FormBuilder);
	private readonly router = inject(Router);
	private readonly verticalApiService = inject(
		VERTICAL_API_SERVICE,
	) as IVerticalApiService;

	readonly state = signal<EnrollmentState>(initialEnrollmentState);
	private readonly storageKey = 'forge.verticalEnrollment.state';
	private readonly persistState = effect(() => {
		sessionStorage.setItem(this.storageKey, JSON.stringify(this.state()));
	});

	readonly identityForm = this.fb.nonNullable.group({
		slug: ['', Validators.required],
		displayName: ['', Validators.required],
		description: ['', Validators.required],
		ownerTeam: ['', Validators.required],
		ownerEmail: ['', [Validators.required, Validators.email]],
	});

	readonly cloudForm = this.fb.nonNullable.group({
		provider: ['AWS'],
		awsAccountId: ['', Validators.required],
		iamRoleArn: ['', Validators.required],
		defaultRegion: ['ap-south-1', Validators.required],
	});

	readonly sourceControlForm = this.fb.nonNullable.group({
		platform: ['GitHub'],
		gitHubOrg: ['', Validators.required],
		accessToken: ['', Validators.required],
		defaultVisibility: ['private'],
		defaultBranchProtection: [true],
	});

	readonly techDefaultsForm = this.fb.nonNullable.group({
		environmentsCsv: ['test,accp,prod', Validators.required],
		defaultEnvironment: ['test', Validators.required],
		generateTerraform: [true],
		generateCdk: [true],
		defaultDbEngine: [''],
	});

	constructor() {
		this.restoreStateFromSessionStorage();
	}

	currentStep(): number {
		return this.state().currentStepIndex;
	}

	onStepSelection(index: number): void {
		this.syncStateFromForms();
		this.state.update((current: EnrollmentState) => ({
			...current,
			currentStepIndex: index,
		}));
	}

	validateConnectivity(): void {
		this.syncStateFromForms();
		const slug = this.state().identity.slug;

		this.state.update((current: EnrollmentState) => ({
			...current,
			connectivity: {
				...current.connectivity,
				isChecking: true,
				errorMessage: null,
			},
		}));

		this.verticalApiService.validateConnectivity(slug).subscribe({
			next: (response: ConnectivityValidationResponse) => {
				this.state.update((current: EnrollmentState) => ({
					...current,
					connectivity: {
						isChecking: false,
						cloudValid: response.awsConnectivity.success,
						sourceControlValid: response.githubConnectivity.success,
						lastCheckedAt: new Date().toISOString(),
						errorMessage: null,
					},
				}));
			},
			error: () => {
				this.state.update((current: EnrollmentState) => ({
					...current,
					connectivity: {
						...current.connectivity,
						isChecking: false,
						errorMessage: 'Connectivity validation failed.',
					},
				}));
			},
		});
	}

	enrollVertical(): void {
		this.syncStateFromForms();

		this.state.update((current: EnrollmentState) => ({
			...current,
			submit: {
				...current.submit,
				isSubmitting: true,
				submitError: null,
			},
		}));

		this.verticalApiService.enrollVertical(toEnrollmentRequest(this.state())).subscribe({
			next: (response: VerticalSummaryResponse) => {
				this.state.update((current: EnrollmentState) => ({
					...current,
					submit: {
						isSubmitting: false,
						submitError: null,
						submitSuccess: true,
					},
				}));

				void this.router.navigate(['/verticals', response.slug]);
			},
			error: () => {
				this.state.update((current: EnrollmentState) => ({
					...current,
					submit: {
						...current.submit,
						isSubmitting: false,
						submitError: 'Enrollment failed. Please retry.',
					},
				}));
			},
		});
	}

	private syncStateFromForms(): void {
		const environments = this.techDefaultsForm
			.controls
			.environmentsCsv
			.value.split(',')
			.map((value: string) => value.trim())
			.filter((value: string) => value.length > 0);

		this.state.update((current: EnrollmentState) => ({
			...current,
			identity: { ...this.identityForm.getRawValue() },
			cloudProvider: { ...this.cloudForm.getRawValue() },
			sourceControl: { ...this.sourceControlForm.getRawValue() },
			techDefaults: {
				environments,
				defaultEnvironment: this.techDefaultsForm.controls.defaultEnvironment.value,
				generateTerraform: this.techDefaultsForm.controls.generateTerraform.value,
				generateCdk: this.techDefaultsForm.controls.generateCdk.value,
				defaultDbEngine:
					this.techDefaultsForm.controls.defaultDbEngine.value.trim().length > 0
						? this.techDefaultsForm.controls.defaultDbEngine.value.trim()
						: null,
			},
		}));
	}

	private restoreStateFromSessionStorage(): void {
		const persisted = sessionStorage.getItem(this.storageKey);
		if (!persisted) {
			return;
		}

		try {
			const restored = JSON.parse(persisted) as EnrollmentState;
			this.state.set(restored);
			this.identityForm.patchValue(restored.identity);
			this.cloudForm.patchValue(restored.cloudProvider);
			this.sourceControlForm.patchValue(restored.sourceControl);
			this.techDefaultsForm.patchValue({
				environmentsCsv: restored.techDefaults.environments.join(','),
				defaultEnvironment: restored.techDefaults.defaultEnvironment,
				generateTerraform: restored.techDefaults.generateTerraform,
				generateCdk: restored.techDefaults.generateCdk,
				defaultDbEngine: restored.techDefaults.defaultDbEngine ?? '',
			});
		} catch {
			sessionStorage.removeItem(this.storageKey);
		}
	}
}
