/**
 ******************************************************************************
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
 ******************************************************************************
 */

import {
	ChangeDetectionStrategy,
	Component,
	input,
	output,
	signal,
	effect,
	inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { VerticalResponse } from '@core/models/vertical.model';
import { VERTICAL_API_SERVICE } from '@core/api/vertical-api.service';
import { UpdateVerticalRequest } from '@core/models/enrollment.model';

@Component({
	selector: 'forge-vertical-settings-panel',
	standalone: true,
	imports: [
		CommonModule,
		ReactiveFormsModule,
		MatButtonModule,
		MatIconModule,
		MatInputModule,
		MatFormFieldModule,
		MatDividerModule,
		MatSlideToggleModule,
	],
	templateUrl: './vertical-settings-panel.component.html',
	styleUrls: ['./vertical-settings-panel.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerticalSettingsPanelComponent {
	readonly vertical = input<VerticalResponse | null>(null);
	readonly slug = input<string>('');
	readonly close = output<void>();

	private readonly fb = inject(FormBuilder);
	private readonly verticalApiService = inject(VERTICAL_API_SERVICE);
	private readonly router = inject(Router);

	readonly showTokenInput = signal(false);
	readonly isSaving = signal(false);
	readonly showUnenrollConfirm = signal(false);

	readonly editForm = this.fb.group({
		displayName: ['', Validators.required],
		description: [''],
		ownerTeam: ['', Validators.required],
		ownerEmail: ['', [Validators.required, Validators.email]],
	});

	readonly tokenForm = this.fb.group({
		newToken: ['', Validators.required],
	});

	constructor() {
		effect(() => {
			const vertical = this.vertical();
			if (vertical) {
				this.editForm.patchValue({
					displayName: vertical.displayName,
					description: vertical.description,
					ownerTeam: vertical.ownerTeam,
					ownerEmail: vertical.ownerEmail,
				});
			}
		});
	}

	onSaveSettings(): void {
		if (!this.editForm.valid) {
			return;
		}

		this.isSaving.set(true);
		const updateRequest: UpdateVerticalRequest = {
			displayName: this.editForm.get('displayName')?.value || '',
			description: this.editForm.get('description')?.value || '',
			ownerTeam: this.editForm.get('ownerTeam')?.value || '',
			ownerEmail: this.editForm.get('ownerEmail')?.value || '',
			techDefaults: this.vertical()?.techDefaults || {
				environments: [],
				defaultEnvironment: 'test',
				generateTerraform: true,
				generateCdk: true,
				defaultDbEngine: null,
			},
		};

		this.verticalApiService.updateVertical(this.slug(), updateRequest).subscribe({
			next: () => {
				this.isSaving.set(false);
				this.close.emit();
			},
			error: () => {
				this.isSaving.set(false);
			},
		});
	}

	onRotateToken(): void {
		this.tokenForm.reset();
		this.showTokenInput.set(false);
	}

	onUnenrollConfirm(): void {
		this.isSaving.set(true);
		this.verticalApiService.unenrollVertical(this.slug()).subscribe({
			next: () => {
				this.isSaving.set(false);
				this.showUnenrollConfirm.set(false);
				this.close.emit();
				void this.router.navigateByUrl('/');
			},
			error: () => {
				this.isSaving.set(false);
			},
		});
	}

	toggleTokenInput(): void {
		this.showTokenInput.set(!this.showTokenInput());
		if (!this.showTokenInput()) {
			this.tokenForm.reset();
		}
	}

	toggleUnenrollConfirm(): void {
		this.showUnenrollConfirm.set(!this.showUnenrollConfirm());
	}

	closePanel(): void {
		this.close.emit();
	}
}
