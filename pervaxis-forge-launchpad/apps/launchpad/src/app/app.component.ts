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

import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { HttpRequestTrackerService } from '@core/http/http-request-tracker.service';
import { AppLayoutComponent } from '@layout/app-layout.component';

@Component({
	selector: 'forge-root',
	standalone: true,
	imports: [CommonModule, MatProgressBarModule, AppLayoutComponent],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		@if (isLoading()) {
			<div class="global-loading-shell" aria-live="polite" aria-busy="true">
				<mat-progress-bar mode="indeterminate"></mat-progress-bar>
			</div>
		}

		<forge-app-layout></forge-app-layout>
	`,
	styles: [
		`
			.global-loading-shell {
				position: fixed;
				top: 0;
				left: 0;
				right: 0;
				z-index: 2000;
			}
		`,
	],
})
export class AppComponent {
	private readonly requestTracker = inject(HttpRequestTrackerService);

	readonly isLoading = this.requestTracker.isLoading;
}
