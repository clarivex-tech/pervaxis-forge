// Copyright © Clarivex Technologies. All rights reserved.

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatStepperModule } from '@angular/material/stepper';

@Component({
	selector: 'forge-vertical-identity-step',
	standalone: true,
	imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatStepperModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<form [formGroup]="formGroup()" class="form-grid">
			<mat-form-field appearance="outline">
				<mat-label>Slug</mat-label>
				<input matInput type="text" formControlName="slug" />
				@if (hasError('slug', 'required')) {
					<mat-error>Slug is required.</mat-error>
				}
				@if (hasError('slug', 'pattern')) {
					<mat-error>Use lowercase letters, numbers, and hyphens only.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Display Name</mat-label>
				<input matInput type="text" formControlName="displayName" />
				@if (hasError('displayName', 'required')) {
					<mat-error>Display name is required.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline" class="full-width">
				<mat-label>Description</mat-label>
				<textarea matInput rows="3" formControlName="description"></textarea>
				@if (hasError('description', 'required')) {
					<mat-error>Description is required.</mat-error>
				}
				@if (hasError('description', 'minlength')) {
					<mat-error>Description must be at least 10 characters.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Owner Team</mat-label>
				<input matInput type="text" formControlName="ownerTeam" />
				@if (hasError('ownerTeam', 'required')) {
					<mat-error>Owner team is required.</mat-error>
				}
			</mat-form-field>
			<mat-form-field appearance="outline">
				<mat-label>Owner Email</mat-label>
				<input matInput type="email" formControlName="ownerEmail" />
				@if (hasError('ownerEmail', 'required')) {
					<mat-error>Owner email is required.</mat-error>
				}
				@if (hasError('ownerEmail', 'email')) {
					<mat-error>Enter a valid email address.</mat-error>
				}
			</mat-form-field>
			<div class="actions full-width">
				<button mat-raised-button color="primary" matStepperNext [disabled]="formGroup().invalid">Next</button>
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
			}
		`,
	],
})
export class VerticalIdentityStepComponent {
	readonly formGroup = input.required<FormGroup>();

	control(name: string): AbstractControl | null {
		return this.formGroup().get(name);
	}

	hasError(name: string, errorCode: string): boolean {
		const control = this.control(name);
		return !!control && control.hasError(errorCode) && (control.touched || control.dirty);
	}
}
