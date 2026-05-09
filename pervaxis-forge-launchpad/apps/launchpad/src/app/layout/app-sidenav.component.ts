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

import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ChangeDetectionStrategy, Component, EventEmitter, Output } from '@angular/core';

interface NavItem {
	label: string;
	path: string;
	icon: string;
	description?: string;
}

@Component({
	selector: 'forge-app-sidenav',
	standalone: true,
	imports: [RouterLink, RouterLinkActive, MatIconModule, MatListModule, MatDividerModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<div class="sidenav-wrapper">
			<div class="sidenav-header">
				<h2 class="sidenav-title">Forge</h2>
				<p class="sidenav-subtitle">Launchpad</p>
			</div>

			<mat-divider></mat-divider>

			<mat-nav-list class="nav-list">
				@for (item of navItems; track item.path) {
					<a
						mat-list-item
						[routerLink]="item.path"
						routerLinkActive="active"
						[routerLinkActiveOptions]="{ exact: true }"
						class="nav-item"
						(click)="navigate.emit()"
					>
						<mat-icon matListItemIcon>{{ item.icon }}</mat-icon>
						<div matListItemTitle>{{ item.label }}</div>
						@if (item.description) {
							<div matListItemLine class="nav-description">{{ item.description }}</div>
						}
					</a>
				}
			</mat-nav-list>

			<div class="sidenav-footer">
				<mat-divider></mat-divider>
				<div class="footer-content">
					<p class="footer-label">Documentation</p>
					<a href="https://clarivex.tech/docs" target="_blank" rel="noreferrer" class="footer-link">
						<mat-icon>help_outline</mat-icon>
						<span>Help & Docs</span>
					</a>
				</div>
			</div>
		</div>
	`,
	styles: [
		`
			:host {
				display: flex;
				flex-direction: column;
				height: 100%;
			}

			.sidenav-wrapper {
				display: flex;
				flex-direction: column;
				height: 100%;
				overflow-y: auto;
			}

			.sidenav-header {
				padding: 1.5rem 1rem;
				text-align: center;
			}

			.sidenav-title {
				margin: 0;
				font-size: 1.5rem;
				font-weight: 700;
				background: linear-gradient(135deg, var(--color-primary), var(--color-secondary));
				-webkit-background-clip: text;
				-webkit-text-fill-color: transparent;
				background-clip: text;
			}

			.sidenav-subtitle {
				margin: 0.25rem 0 0;
				font-size: 0.875rem;
				color: var(--color-on-surface-variant);
				font-weight: 500;
			}

			.nav-list {
				flex: 1;
				padding: 1rem 0;
			}

			.nav-item {
				margin: 0.25rem 0.5rem;
				border-radius: 0.5rem;
				text-decoration: none;
				transition: background-color 200ms ease;
			}

			.nav-item:hover {
				background-color: var(--color-surface-container-high);
			}

			.nav-item.active {
				background-color: var(--color-primary-container);
				color: var(--color-on-primary-container);
			}

			.nav-item.active mat-icon {
				color: var(--color-on-primary-container);
			}

			.nav-description {
				font-size: 0.75rem;
				opacity: 0.7;
				margin-top: 0.25rem;
			}

			.sidenav-footer {
				padding: 1rem 0.5rem 0;
				margin-top: auto;
				border-top: 1px solid var(--color-outline-variant);
			}

			.footer-content {
				padding: 1rem;
			}

			.footer-label {
				margin: 0 0 0.5rem;
				font-size: 0.75rem;
				font-weight: 600;
				color: var(--color-on-surface-variant);
				text-transform: uppercase;
				letter-spacing: 0.5px;
			}

			.footer-link {
				display: flex;
				align-items: center;
				gap: 0.75rem;
				padding: 0.5rem 0.75rem;
				color: var(--color-primary);
				text-decoration: none;
				border-radius: 0.25rem;
				font-size: 0.875rem;
				transition: background-color 200ms ease;
			}

			.footer-link:hover {
				background-color: var(--color-primary-container);
			}

			.footer-link mat-icon {
				font-size: 1.25rem;
				width: 1.25rem;
				height: 1.25rem;
			}

			@media (max-width: 768px) {
				.sidenav-header {
					padding: 1rem;
				}

				.nav-description {
					display: none;
				}
			}
		`,
	],
})
export class AppSidenavComponent {
	@Output() navigate = new EventEmitter<void>();

	readonly navItems: NavItem[] = [
		{
			label: 'Dashboard',
			path: '/',
			icon: 'dashboard',
			description: 'View your verticals',
		},
		{
			label: 'Enroll Vertical',
			path: '/verticals/enroll',
			icon: 'add_circle_outline',
			description: 'New vertical',
		},
	];
}
