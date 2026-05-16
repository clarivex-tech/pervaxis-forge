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

import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatStepperModule } from '@angular/material/stepper';

import { IVerticalApiService, VERTICAL_API_SERVICE } from '../../core/api/vertical-api.service';
import { EnrollmentState, initialEnrollmentState, toEnrollmentRequest } from './enrollment.state';
import {
	ConnectivityValidationResponse,
	VerticalSummaryResponse,
} from '../../core/models/vertical.model';
import { CloudProvider } from '../../core/models/cloud-provider.model';
import {
	DefaultDbEngine,
	RepoVisibility,
	SourceControlPlatform,
} from '../../core/models/enrollment.model';
import { VerticalIdentityStepComponent } from './steps/vertical-identity-step/vertical-identity-step.component';
import { CloudProviderStepComponent } from './steps/cloud-provider-step/cloud-provider-step.component';
import { SourceControlStepComponent } from './steps/source-control-step/source-control-step.component';
import { TechDefaultsStepComponent } from './steps/tech-defaults-step/tech-defaults-step.component';
import { ReviewEnrollStepComponent } from './steps/review-enroll-step/review-enroll-step.component';

@Component({
	selector: 'forge-vertical-enrollment',
	standalone: true,
	imports: [
		ReactiveFormsModule,
		MatStepperModule,
		MatCardModule,
		VerticalIdentityStepComponent,
		CloudProviderStepComponent,
		SourceControlStepComponent,
		TechDefaultsStepComponent,
		ReviewEnrollStepComponent,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-card>
			<h2>Vertical Enrollment Wizard</h2>
			<mat-stepper
				[linear]="true"
				[selectedIndex]="currentStep()"
				(selectionChange)="onStepSelection($event.selectedIndex)"
			>
				<mat-step [stepControl]="identityForm" label="Vertical Identity">
					<forge-vertical-identity-step [formGroup]="identityForm" />
				</mat-step>

				<mat-step [stepControl]="cloudForm" label="Cloud Provider">
					<forge-cloud-provider-step [formGroup]="cloudForm" />
				</mat-step>

				<mat-step [stepControl]="sourceControlForm" label="Source Control">
					<forge-source-control-step [formGroup]="sourceControlForm" />
				</mat-step>

				<mat-step [stepControl]="techDefaultsForm" label="Tech Defaults">
					<forge-tech-defaults-step [formGroup]="techDefaultsForm" />
				</mat-step>

				<mat-step label="Review & Enroll">
					<forge-review-enroll-step
						[state]="state()"
						[maskedIamRoleArn]="maskedIamRoleArn()"
						[maskedAccessToken]="maskedAccessToken()"
						(validateConnectivity)="validateConnectivity()"
						(enrollVertical)="enrollVertical()"
					/>
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
		`,
	],
})
export class VerticalEnrollmentComponent {
	private readonly fb = inject(FormBuilder);
	private readonly router = inject(Router);
	private readonly verticalApiService = inject(VERTICAL_API_SERVICE) as IVerticalApiService;

	readonly state = signal<EnrollmentState>(initialEnrollmentState);
	private readonly storageKey = 'forge.verticalEnrollment.state';
	private readonly persistState = effect(() => {
		sessionStorage.setItem(this.storageKey, JSON.stringify(this.state()));
	});

	readonly identityForm = this.fb.nonNullable.group({
		slug: ['', [Validators.required, Validators.pattern(/^[a-z0-9]+(?:-[a-z0-9]+)*$/)]],
		displayName: ['', Validators.required],
		description: ['', [Validators.required, Validators.minLength(10)]],
		ownerTeam: ['', Validators.required],
		ownerEmail: ['', [Validators.required, Validators.email]],
		componentPrefix: ['', [Validators.required, Validators.pattern(/^[A-Z]{2,5}$/)]],
	});

	readonly cloudForm = this.fb.nonNullable.group({
		provider: ['AWS'],
		awsAccountId: ['', [Validators.required, Validators.pattern(/^\d{12}$/)]],
		iamRoleArn: [
			'',
			[Validators.required, Validators.pattern(/^arn:aws:iam::\d{12}:role\/[\w+=,.@\-_/]+$/)],
		],
		defaultRegion: ['ap-south-1', Validators.required],
	});

	readonly sourceControlForm = this.fb.nonNullable.group({
		platform: ['GitHub'],
		gitHubOrg: [
			'',
			[Validators.required, Validators.pattern(/^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$/)],
		],
		accessToken: ['', [Validators.required, Validators.minLength(8)]],
		defaultVisibility: ['Private'],
		defaultBranchProtection: [true],
	});

	readonly techDefaultsForm = this.fb.nonNullable.group({
		environmentsCsv: [
			'test,accp,prod',
			[Validators.required, Validators.pattern(/^[a-z0-9-]+(?:\s*,\s*[a-z0-9-]+)*$/)],
		],
		defaultEnvironment: ['test', [Validators.required, Validators.pattern(/^[a-z0-9-]+$/)]],
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

	maskedIamRoleArn(): string {
		return this.maskSecret(this.state().cloudProvider.iamRoleArn);
	}

	maskedAccessToken(): string {
		return this.maskSecret(this.state().sourceControl.accessToken);
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
		const slug = this.state().identity.slug.trim();

		if (!slug) {
			this.state.update((current: EnrollmentState) => ({
				...current,
				connectivity: {
					...current.connectivity,
					errorMessage: 'Provide a valid slug before validating connectivity.',
				},
			}));
			return;
		}

		this.state.update((current: EnrollmentState) => ({
			...current,
			connectivity: {
				isChecking: true,
				cloudValid: null,
				sourceControlValid: null,
				lastCheckedAt: null,
				errorMessage: null,
			},
		}));

		this.verticalApiService.validateConnectivity(slug).subscribe({
			next: (response: ConnectivityValidationResponse) => {
				const errors = [
					response.awsConnectivity.errorMessage,
					response.gitHubConnectivity.errorMessage,
				].filter((value): value is string => !!value && value.trim().length > 0);

				this.state.update((current: EnrollmentState) => ({
					...current,
					connectivity: {
						isChecking: false,
						cloudValid: response.awsConnectivity.success,
						sourceControlValid: response.gitHubConnectivity.success,
						lastCheckedAt: new Date().toISOString(),
						errorMessage: errors.length > 0 ? errors.join(' ') : null,
					},
				}));
			},
			error: () => {
				this.state.update((current: EnrollmentState) => ({
					...current,
					connectivity: {
						...current.connectivity,
						isChecking: false,
						errorMessage:
							'Connectivity validation failed. Enroll the vertical first or verify the slug.',
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
				this.resetEnrollmentDraft();

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
		const cloudFormValue = this.cloudForm.getRawValue();
		const sourceControlFormValue = this.sourceControlForm.getRawValue();
		const techDefaultsValue = this.techDefaultsForm.getRawValue();
		const selectedDefaultDbEngine = techDefaultsValue.defaultDbEngine.trim();

		const environments = this.techDefaultsForm.controls.environmentsCsv.value
			.split(',')
			.map((value: string) => value.trim())
			.filter((value: string) => value.length > 0);

		this.state.update((current: EnrollmentState) => ({
			...current,
			identity: { ...this.identityForm.getRawValue() },
			cloudProvider: {
				...cloudFormValue,
				provider: cloudFormValue.provider as CloudProvider,
			},
			sourceControl: {
				...sourceControlFormValue,
				platform: sourceControlFormValue.platform as SourceControlPlatform,
				defaultVisibility: sourceControlFormValue.defaultVisibility as RepoVisibility,
			},
			techDefaults: {
				environments,
				defaultEnvironment: techDefaultsValue.defaultEnvironment,
				generateTerraform: techDefaultsValue.generateTerraform,
				generateCdk: techDefaultsValue.generateCdk,
				defaultDbEngine:
					selectedDefaultDbEngine === 'postgresql' || selectedDefaultDbEngine === 'mysql'
						? (selectedDefaultDbEngine as DefaultDbEngine)
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

	private resetEnrollmentDraft(): void {
		this.state.set(initialEnrollmentState);
		this.identityForm.reset({
			slug: '',
			displayName: '',
			description: '',
			ownerTeam: '',
			ownerEmail: '',
			componentPrefix: '',
		});
		this.cloudForm.reset({
			provider: 'AWS',
			awsAccountId: '',
			iamRoleArn: '',
			defaultRegion: 'ap-south-1',
		});
		this.sourceControlForm.reset({
			platform: 'GitHub',
			gitHubOrg: '',
			accessToken: '',
			defaultVisibility: 'Private',
			defaultBranchProtection: true,
		});
		this.techDefaultsForm.reset({
			environmentsCsv: 'test,accp,prod',
			defaultEnvironment: 'test',
			generateTerraform: true,
			generateCdk: true,
			defaultDbEngine: '',
		});
		sessionStorage.removeItem(this.storageKey);
	}

	private maskSecret(value: string): string {
		if (!value) {
			return 'Not provided';
		}

		if (value.length <= 6) {
			return '*'.repeat(value.length);
		}

		const prefix = value.slice(0, 3);
		const suffix = value.slice(-3);
		return `${prefix}${'*'.repeat(Math.max(value.length - 6, 4))}${suffix}`;
	}
}
