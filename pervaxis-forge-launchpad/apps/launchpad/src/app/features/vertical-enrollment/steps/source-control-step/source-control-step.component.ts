// Copyright © Clarivex Technologies. All rights reserved.

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatStepperModule } from '@angular/material/stepper';

@Component({
	selector: 'forge-source-control-step',
	standalone: true,
	imports: [
		ReactiveFormsModule,
		MatFormFieldModule,
		MatInputModule,
		MatSelectModule,
		MatCheckboxModule,
		MatButtonModule,
		MatStepperModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<form [formGroup]="formGroup()" class="form-grid">
			<mat-form-field appearance="outline">
				<mat-label>Platform</mat-label>
				<mat-select formControlName="platform">
					<mat-option value="GitHub">GitHub</mat-option>
				</mat-select>
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>GitHub Organization</mat-label>
				<input matInput type="text" formControlName="gitHubOrg" />
				@if (hasError('gitHubOrg', 'required')) {
					<mat-error>GitHub organization is required.</mat-error>
				}
				@if (hasError('gitHubOrg', 'pattern')) {
					<mat-error>Use a valid GitHub organization name.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline" class="full-width">
				<mat-label>Access Token</mat-label>
				<input matInput type="password" formControlName="accessToken" />
				@if (hasError('accessToken', 'required')) {
					<mat-error>Access token is required.</mat-error>
				}
				@if (hasError('accessToken', 'minlength')) {
					<mat-error>Access token looks too short.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Visibility</mat-label>
				<mat-select formControlName="defaultVisibility">
					<mat-option value="Private">Private</mat-option>
					<mat-option value="Public">Public</mat-option>
				</mat-select>
			</mat-form-field>
			<mat-checkbox formControlName="defaultBranchProtection"
				>Default branch protection</mat-checkbox
			>
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
		`,
	],
})
export class SourceControlStepComponent {
	readonly formGroup = input.required<FormGroup>();

	control(name: string): AbstractControl | null {
		return this.formGroup().get(name);
	}

	hasError(name: string, errorCode: string): boolean {
		const control = this.control(name);
		return !!control && control.hasError(errorCode) && (control.touched || control.dirty);
	}
}
