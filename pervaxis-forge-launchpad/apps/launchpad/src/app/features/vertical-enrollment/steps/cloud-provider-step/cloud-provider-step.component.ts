// Copyright © Clarivex Technologies. All rights reserved.

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';

@Component({
	selector: 'forge-cloud-provider-step',
	standalone: true,
	imports: [
		ReactiveFormsModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
		MatButtonModule,
		MatStepperModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<form [formGroup]="formGroup()" class="form-grid">
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
				@if (hasError('awsAccountId', 'required')) {
					<mat-error>AWS account id is required.</mat-error>
				}
				@if (hasError('awsAccountId', 'pattern')) {
					<mat-error>AWS account id must be exactly 12 digits.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline" class="full-width">
				<mat-label>IAM Role ARN</mat-label>
				<input matInput type="text" formControlName="iamRoleArn" />
				@if (hasError('iamRoleArn', 'required')) {
					<mat-error>IAM role ARN is required.</mat-error>
				}
				@if (hasError('iamRoleArn', 'pattern')) {
					<mat-error>Enter a valid IAM role ARN.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Default Region</mat-label>
				<input matInput type="text" formControlName="defaultRegion" />
				@if (hasError('defaultRegion', 'required')) {
					<mat-error>Default region is required.</mat-error>
				}
			</mat-form-field>
			<div class="actions full-width">
				<button mat-button matStepperPrevious>Back</button>
				<button mat-raised-button color="primary" matStepperNext [disabled]="formGroup().invalid">
					Next
				</button>
			</div>
		</form>
	`,
	styles: [
		`
			.form-grid {
				display: grid;
				grid-template-columns: repeat(2, minmax(0, 1fr));
				gap: 1.15rem;
			}

			.form-grid mat-form-field {
				margin-bottom: 0.5rem;
			}

			.full-width {
				grid-column: 1 / -1;
			}

			.actions {
				display: flex;
				justify-content: flex-end;
				gap: 0.5rem;
			}
		`,
	],
})
export class CloudProviderStepComponent {
	readonly formGroup = input.required<FormGroup>();

	control(name: string): AbstractControl | null {
		return this.formGroup().get(name);
	}

	hasError(name: string, errorCode: string): boolean {
		const control = this.control(name);
		return !!control && control.hasError(errorCode) && (control.touched || control.dirty);
	}
}
