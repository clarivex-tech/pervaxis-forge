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
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import {
	ChangeDetectionStrategy,
	Component,
	EventEmitter,
	Input,
	Output,
	inject,
} from '@angular/core';
import { filter, map } from 'rxjs';

@Component({
	selector: 'forge-app-header',
	standalone: true,
	imports: [
		CommonModule,
		MatToolbarModule,
		MatButtonModule,
		MatIconModule,
		MatMenuModule,
		MatDividerModule,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-toolbar color="primary" class="app-header">
			<button
				mat-icon-button
				aria-label="Toggle navigation"
				(click)="toggleSidenav.emit()"
				class="menu-btn"
			>
				<mat-icon>menu</mat-icon>
			</button>

			<h1 class="app-title">Pervaxis Forge</h1>

			<div class="breadcrumb" aria-label="Breadcrumb">
				<span class="breadcrumb-item">Forge</span>
				@for (item of breadcrumbItems; track item) {
					<span class="breadcrumb-separator">/</span>
					<span class="breadcrumb-item">{{ item }}</span>
				}
			</div>

			<span class="spacer"></span>

			<button mat-icon-button aria-label="Open user menu" [matMenuTriggerFor]="userMenu">
				<mat-icon>account_circle</mat-icon>
			</button>
		</mat-toolbar>

		<mat-menu #userMenu="matMenu">
			<button mat-menu-item type="button">
				<mat-icon>person</mat-icon>
				<span>Profile</span>
			</button>
			<button mat-menu-item type="button">
				<mat-icon>settings</mat-icon>
				<span>Settings</span>
			</button>
			<mat-divider></mat-divider>
			<button mat-menu-item type="button">
				<mat-icon>logout</mat-icon>
				<span>Logout</span>
			</button>
		</mat-menu>
	`,
	styles: [
		`
			.app-header {
				position: sticky;
				top: 0;
				z-index: 100;
				box-shadow: var(--elevation-1);
			}

			.menu-btn {
				margin-right: 1rem;
			}

			.app-title {
				margin: 0;
				font-size: 1.5rem;
				font-weight: 600;
				color: #ffffff;
			}

			.breadcrumb {
				display: flex;
				align-items: center;
				gap: 0.5rem;
				margin-left: 2rem;
				font-size: 0.875rem;
				color: rgba(255, 255, 255, 0.8);
			}

			.breadcrumb-item {
				white-space: nowrap;
				overflow: hidden;
				text-overflow: ellipsis;
				max-width: 200px;
			}

			.breadcrumb-separator {
				opacity: 0.7;
			}

			.spacer {
				flex: 1 1 auto;
			}

			@media (max-width: 768px) {
				.breadcrumb {
					display: none;
				}

				.app-title {
					font-size: 1.25rem;
				}
			}
		`,
	],
})
export class AppHeaderComponent {
	@Input() sidenavOpened = false;
	@Output() toggleSidenav = new EventEmitter<void>();

	breadcrumbItems: string[] = [];

	private readonly router = inject(Router);
	private readonly activatedRoute = inject(ActivatedRoute);

	constructor() {
		this.router.events
			.pipe(
				filter((event) => event instanceof NavigationEnd),
				map(() => this.buildBreadcrumb(this.activatedRoute.root))
			)
			.subscribe((breadcrumbs) => {
				this.breadcrumbItems = breadcrumbs;
			});
	}

	private buildBreadcrumb(route: ActivatedRoute, breadcrumbs: string[] = []): string[] {
		const label = route.snapshot.data['breadcrumb'];
		const hasPath = route.snapshot.url.length > 0;

		if (hasPath && label) {
			breadcrumbs.push(label);
		}

		return route.firstChild ? this.buildBreadcrumb(route.firstChild, breadcrumbs) : breadcrumbs;
	}
}
