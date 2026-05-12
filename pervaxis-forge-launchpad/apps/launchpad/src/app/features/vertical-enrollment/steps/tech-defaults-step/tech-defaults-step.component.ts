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
	selector: 'forge-tech-defaults-step',
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
			<mat-form-field appearance="outline" class="full-width">
				<mat-label>Environments (comma separated)</mat-label>
				<input matInput type="text" formControlName="environmentsCsv" />
				@if (hasError('environmentsCsv', 'required')) {
					<mat-error>At least one environment is required.</mat-error>
				}
				@if (hasError('environmentsCsv', 'pattern')) {
					<mat-error
						>Use lowercase names separated by commas, for example: test,accp,prod.</mat-error
					>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Default Environment</mat-label>
				<input matInput type="text" formControlName="defaultEnvironment" />
				@if (hasError('defaultEnvironment', 'required')) {
					<mat-error>Default environment is required.</mat-error>
				}
				@if (hasError('defaultEnvironment', 'pattern')) {
					<mat-error>Use lowercase letters, numbers, or hyphens.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Default DB Engine</mat-label>
				<mat-select formControlName="defaultDbEngine">
					<mat-option value="">None</mat-option>
					<mat-option value="postgresql">PostgreSQL</mat-option>
					<mat-option value="mysql">MySQL</mat-option>
				</mat-select>
			</mat-form-field>
			<mat-checkbox formControlName="generateTerraform">Generate Terraform</mat-checkbox>
			<mat-checkbox formControlName="generateCdk">Generate CDK</mat-checkbox>
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
export class TechDefaultsStepComponent {
	readonly formGroup = input.required<FormGroup>();

	control(name: string): AbstractControl | null {
		return this.formGroup().get(name);
	}

	hasError(name: string, errorCode: string): boolean {
		const control = this.control(name);
		return !!control && control.hasError(errorCode) && (control.touched || control.dirty);
	}
}
