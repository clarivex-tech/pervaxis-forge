// Copyright © Clarivex Technologies. All rights reserved.

import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatStepperModule } from '@angular/material/stepper';
import { EnrollmentState } from '../../enrollment.state';

@Component({
	selector: 'forge-review-enroll-step',
	standalone: true,
	imports: [CommonModule, MatButtonModule, MatStepperModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<section class="summary-grid">
			<h4 class="full-width">Identity</h4>
			<p><strong>Slug:</strong> {{ state().identity.slug }}</p>
			<p><strong>Display Name:</strong> {{ state().identity.displayName }}</p>
			<p><strong>Component Prefix:</strong> {{ state().identity.componentPrefix }}</p>
			<p class="full-width"><strong>Description:</strong> {{ state().identity.description }}</p>
			<p><strong>Owner Team:</strong> {{ state().identity.ownerTeam }}</p>
			<p><strong>Owner Email:</strong> {{ state().identity.ownerEmail }}</p>

			<h4 class="full-width">Cloud Provider</h4>
			<p><strong>Provider:</strong> {{ state().cloudProvider.provider }}</p>
			<p><strong>Account Id:</strong> {{ state().cloudProvider.awsAccountId }}</p>
			<p class="full-width"><strong>IAM Role ARN:</strong> {{ maskedIamRoleArn() }}</p>
			<p><strong>Region:</strong> {{ state().cloudProvider.defaultRegion }}</p>

			<h4 class="full-width">Source Control</h4>
			<p><strong>Platform:</strong> {{ state().sourceControl.platform }}</p>
			<p><strong>GitHub Org:</strong> {{ state().sourceControl.gitHubOrg }}</p>
			<p class="full-width"><strong>Access Token:</strong> {{ maskedAccessToken() }}</p>
			<p><strong>Visibility:</strong> {{ state().sourceControl.defaultVisibility }}</p>
			<p>
				<strong>Branch Protection:</strong>
				{{ state().sourceControl.defaultBranchProtection ? 'Enabled' : 'Disabled' }}
			</p>

			<h4 class="full-width">Technical Defaults</h4>
			<p class="full-width">
				<strong>Environments:</strong> {{ state().techDefaults.environments.join(', ') }}
			</p>
			<p><strong>Default Environment:</strong> {{ state().techDefaults.defaultEnvironment }}</p>
			<p>
				<strong>Generate Terraform:</strong>
				{{ state().techDefaults.generateTerraform ? 'Yes' : 'No' }}
			</p>
			<p><strong>Generate CDK:</strong> {{ state().techDefaults.generateCdk ? 'Yes' : 'No' }}</p>
			<p>
				<strong>Default DB Engine:</strong> {{ state().techDefaults.defaultDbEngine ?? 'Not set' }}
			</p>

			<div class="actions full-width">
				<button mat-button type="button" matStepperPrevious>Back</button>
				<button
					mat-stroked-button
					type="button"
					(click)="validateConnectivity.emit()"
					[disabled]="state().connectivity.isChecking"
				>
					Validate Connectivity
				</button>
				<button
					mat-raised-button
					color="primary"
					type="button"
					(click)="enrollVertical.emit()"
					[disabled]="state().submit.isSubmitting"
				>
					Enroll Vertical
				</button>
			</div>

			@if (state().connectivity.errorMessage) {
				<p class="full-width error">{{ state().connectivity.errorMessage }}</p>
			}
			@if (state().connectivity.lastCheckedAt) {
				<p class="full-width success">
					Connectivity checked at {{ state().connectivity.lastCheckedAt | date: 'short' }}.
					AWS: {{ state().connectivity.cloudValid ? 'OK' : 'Failed' }}, GitHub:
					{{ state().connectivity.sourceControlValid ? 'OK' : 'Failed' }}
				</p>
			}
			@if (state().submit.submitError) {
				<p class="full-width error">{{ state().submit.submitError }}</p>
			}
			@if (state().submit.submitSuccess) {
				<p class="full-width success">Vertical enrolled successfully.</p>
			}
		</section>
	`,
	styles: [
		`
			.summary-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 0.75rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.actions {
				display: flex;
				justify-content: flex-end;
				gap: 0.5rem;
			}

			.error {
				color: #b3261e;
			}

			.success {
				color: #0f766e;
			}
		`,
	],
})
export class ReviewEnrollStepComponent {
	readonly state = input.required<EnrollmentState>();
	readonly maskedIamRoleArn = input.required<string>();
	readonly maskedAccessToken = input.required<string>();

	readonly validateConnectivity = output<void>();
	readonly enrollVertical = output<void>();
}
