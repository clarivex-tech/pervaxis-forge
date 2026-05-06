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
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
	selector: 'forge-app-sidenav',
	standalone: true,
	imports: [RouterLink, RouterLinkActive, MatIconModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<aside class="sidenav-shell">
			<div class="sidenav-header">
				<h2 class="sidenav-title">Forge Engine</h2>
				<p class="sidenav-subtitle">Management Console</p>
			</div>

			<nav class="sidenav-nav">
				<a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }" class="nav-item">
					<mat-icon class="nav-icon">cloud_queue</mat-icon>
					<span class="nav-label">Infrastructure</span>
				</a>
				<a routerLink="/deployments" routerLinkActive="active" class="nav-item">
					<mat-icon class="nav-icon">rocket_launch</mat-icon>
					<span class="nav-label">Deployments</span>
				</a>
				<a routerLink="/monitoring" routerLinkActive="active" class="nav-item">
					<mat-icon class="nav-icon">analytics</mat-icon>
					<span class="nav-label">Monitoring</span>
				</a>
				<a routerLink="/security" routerLinkActive="active" class="nav-item">
					<mat-icon class="nav-icon">security</mat-icon>
					<span class="nav-label">Security</span>
				</a>
				<a routerLink="/clusters" routerLinkActive="active" class="nav-item">
					<mat-icon class="nav-icon">hub</mat-icon>
					<span class="nav-label">Clusters</span>
				</a>
				<a routerLink="/settings" routerLinkActive="active" class="nav-item">
					<mat-icon class="nav-icon">settings</mat-icon>
					<span class="nav-label">Settings</span>
				</a>
			</nav>

			<div class="system-health">
				<div class="health-header">
					<span class="health-title">System Health</span>
					<span class="health-dot"></span>
				</div>
				<p class="health-status">99.9% Uptime</p>
			</div>
		</aside>
	`,
	styles: [
		`
			.sidenav-shell {
				position: fixed;
				left: 0;
				top: 64px;
				width: 256px;
				height: calc(100vh - 64px);
				background-color: var(--color-surface-container-low);
				border-right: 1px solid var(--color-outline-variant);
				display: flex;
				flex-direction: column;
				padding: 1rem;
				z-index: 40;
				overflow-y: auto;
			}

			.sidenav-header {
				margin-bottom: 2rem;
				padding: 0.5rem;
			}

			.sidenav-title {
				margin: 0;
				font-size: 16px;
				font-weight: 700;
				color: var(--color-primary);
				letter-spacing: 0.15px;
			}

			.sidenav-subtitle {
				margin: 0;
				font-size: 12px;
				color: var(--color-on-surface-variant);
				opacity: 0.7;
			}

			.sidenav-nav {
				flex: 1;
				display: flex;
				flex-direction: column;
				gap: 0.25rem;
			}

			.nav-item {
				display: flex;
				align-items: center;
				gap: 0.75rem;
				padding: 0.75rem 1rem;
				color: var(--color-on-surface-variant);
				text-decoration: none;
				border-radius: 0.75rem;
				transition: all 200ms;
				font-size: 14px;
				font-weight: 500;

				&:hover {
					background-color: var(--color-surface-container-highest);
				}

				&.active {
					background-color: var(--color-secondary-container);
					color: var(--color-on-secondary-container);
				}
			}

			.nav-icon {
				font-size: 20px;
				width: 20px;
				height: 20px;
			}

			.nav-label {
				font-size: 14px;
			}

			.system-health {
				margin-top: auto;
				padding: 1rem;
				background-color: rgba(103, 80, 164, 0.1);
				border: 1px solid rgba(103, 80, 164, 0.2);
				border-radius: 0.75rem;
			}

			.health-header {
				display: flex;
				align-items: center;
				justify-content: space-between;
				margin-bottom: 0.5rem;
			}

			.health-title {
				font-size: 12px;
				font-weight: 700;
				color: var(--color-primary);
			}

			.health-dot {
				display: inline-block;
				width: 8px;
				height: 8px;
				border-radius: 9999px;
				background-color: var(--color-success);
			}

			.health-status {
				margin: 0;
				font-size: 14px;
				font-weight: 500;
				color: var(--color-primary);
				letter-spacing: 0.1px;
			}
		`,
	],
})
export class AppSidenavComponent {}
