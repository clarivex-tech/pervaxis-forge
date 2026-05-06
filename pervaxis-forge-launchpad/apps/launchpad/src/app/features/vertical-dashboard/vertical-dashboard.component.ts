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

import { ChangeDetectionStrategy, Component } from '@angular/core';
import { DatePipe } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

import {
	IVerticalApiService,
	VERTICAL_API_SERVICE,
} from '../../core/api/vertical-api.service';
import { VerticalSummaryResponse } from '../../core/models/vertical.model';
import { inject, signal } from '@angular/core';

@Component({
	selector: 'forge-vertical-dashboard',
	standalone: true,
	imports: [
		DatePipe,
		MatButtonModule,
		MatCardModule,
		MatChipsModule,
		MatProgressSpinnerModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<section class="dashboard-shell">
			<header class="dashboard-header">
				<div>
					<p class="crumbs">Console / <span>Business Verticals</span></p>
					<h1>Business Verticals</h1>
				</div>
				<button mat-raised-button color="primary" type="button" (click)="goToEnrollment()">
					+ Enroll New Vertical
				</button>
			</header>

			@if (isLoading()) {
				<div class="status-wrap">
					<mat-spinner diameter="36" />
					<p>Loading verticals...</p>
				</div>
			} @else if (errorMessage()) {
				<mat-card class="status-card">
					<h2>Unable to load verticals</h2>
					<p>{{ errorMessage() }}</p>
					<button mat-stroked-button type="button" (click)="loadVerticals()">Retry</button>
				</mat-card>
			} @else if (verticals().length === 0) {
				<mat-card class="empty-state">
					<div class="empty-illustration" aria-hidden="true">◌</div>
					<h2>No verticals enrolled</h2>
					<p>
						Get started by enrolling your first business vertical to manage infrastructure deployments and service generation.
					</p>
					<button mat-raised-button color="primary" type="button" (click)="goToEnrollment()">
						Enroll Your First Vertical
					</button>
				</mat-card>
			} @else {
				<div class="card-grid">
					@for (vertical of verticals(); track vertical.id) {
						<mat-card class="vertical-card">
							<div class="card-head">
								<mat-chip class="slug-chip">{{ vertical.slug }}</mat-chip>
							</div>

							<h3>{{ vertical.displayName }}</h3>

							<div class="provider-chips">
								<mat-chip class="chip-aws">{{ vertical.cloudProvider }}</mat-chip>
								<mat-chip class="chip-github">{{ vertical.sourceControl }}</mat-chip>
							</div>

							<div class="stats">
								<p>{{ vertical.serviceCount ?? 0 }} services generated</p>
								<p>
									Last generated:
									@if (vertical.lastGeneratedAt) {
										{{ vertical.lastGeneratedAt | date:'medium' }}
									} @else {
										Never
									}
								</p>
							</div>

							<button mat-raised-button color="primary" type="button" (click)="openVertical(vertical.slug)">
								Open
							</button>
						</mat-card>
					}
				</div>
			}
		</section>
	`,
	styles: [
		`
			.dashboard-shell {
				padding: var(--spacing-3);
			}

			.dashboard-header {
				display: flex;
				justify-content: space-between;
				align-items: end;
				gap: var(--spacing-2);
				margin-bottom: var(--spacing-3);
			}

			.crumbs {
				color: var(--color-on-surface-variant);
				margin: 0;
				font-size: 0.875rem;
			}

			.crumbs span {
				color: var(--color-primary);
				font-weight: 600;
			}

			h1 {
				margin: 0.25rem 0 0;
			}

			.status-wrap {
				display: grid;
				justify-items: center;
				gap: var(--spacing-2);
				padding: var(--spacing-6) var(--spacing-2);
			}

			.status-card {
				padding: var(--spacing-3);
				display: grid;
				gap: var(--spacing-2);
			}

			.empty-state {
				display: grid;
				justify-items: center;
				text-align: center;
				gap: var(--spacing-2);
				padding: var(--spacing-6) var(--spacing-3);
			}

			.empty-state h2,
			.status-card h2 {
				margin: 0;
			}

			.empty-state p,
			.status-card p {
				margin: 0;
				max-width: 42rem;
				color: var(--color-on-surface-variant);
			}

			.empty-illustration {
				width: 5rem;
				height: 5rem;
				display: grid;
				place-items: center;
				border-radius: var(--shape-full);
				border: 2px dashed var(--color-outline-variant);
				font-size: 2rem;
				color: var(--color-primary);
			}

			.card-grid {
				display: grid;
				grid-template-columns: repeat(auto-fit, minmax(18rem, 1fr));
				gap: var(--spacing-2);
			}

			.vertical-card {
				display: grid;
				gap: var(--spacing-2);
				height: 100%;
			}

			.card-head {
				display: flex;
				justify-content: space-between;
				align-items: center;
			}

			.slug-chip {
				background: color-mix(in srgb, var(--color-primary) 12%, transparent);
				color: var(--color-primary);
				font-weight: 700;
			}

			h3 {
				margin: 0;
			}

			.provider-chips {
				display: flex;
				gap: 0.5rem;
				flex-wrap: wrap;
			}

			.stats {
				display: grid;
				gap: 0.25rem;
			}

			.stats p {
				margin: 0;
				color: var(--color-on-surface-variant);
			}

			@media (max-width: 900px) {
				.dashboard-header {
					align-items: start;
					flex-direction: column;
				}
			}
		`,
	],
})
export class VerticalDashboardComponent {
	private readonly router = inject(Router);
	private readonly verticalApiService = inject(
		VERTICAL_API_SERVICE,
	) as IVerticalApiService;

	readonly verticals = signal<VerticalSummaryResponse[]>([]);
	readonly isLoading = signal<boolean>(true);
	readonly errorMessage = signal<string | null>(null);

	constructor() {
		this.loadVerticals();
	}

	loadVerticals(): void {
		this.isLoading.set(true);
		this.errorMessage.set(null);

		this.verticalApiService
			.listVerticals()
			.pipe(finalize(() => this.isLoading.set(false)))
			.subscribe({
				next: (verticals: VerticalSummaryResponse[]) => {
					this.verticals.set(verticals);
				},
				error: () => {
					this.errorMessage.set('Please retry in a moment.');
				},
			});
	}

	goToEnrollment(): void {
		void this.router.navigate(['/enroll']);
	}

	openVertical(slug: string): void {
		void this.router.navigate(['/verticals', slug]);
	}
}
