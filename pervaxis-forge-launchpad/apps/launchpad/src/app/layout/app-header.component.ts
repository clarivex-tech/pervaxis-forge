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
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
	selector: 'forge-app-header',
	standalone: true,
	imports: [RouterLink, RouterLinkActive, MatButtonModule, MatIconModule, MatInputModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<header class="header-shell">
			<div class="header-left">
				<h1 class="logo">Pervaxis Forge Launchpad</h1>
				<nav class="header-nav">
					<a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }" class="nav-link">
						Verticals
					</a>
					<a routerLink="/resources" routerLinkActive="active" class="nav-link">
						Resources
					</a>
					<a routerLink="/pipeline" routerLinkActive="active" class="nav-link">
						Pipeline
					</a>
				</nav>
			</div>

			<div class="header-right">
				<div class="search-box">
					<mat-icon class="search-icon">search</mat-icon>
					<input type="text" placeholder="Search services..." class="search-input" />
				</div>
				<button mat-raised-button class="create-service-btn">Create Service</button>
				<div class="header-icons">
					<button mat-icon-button class="icon-btn">
						<mat-icon>notifications</mat-icon>
					</button>
					<button mat-icon-button class="icon-btn">
						<mat-icon>help_outline</mat-icon>
					</button>
				</div>
				<img alt="Avatar" class="avatar-img" src="https://lh3.googleusercontent.com/aida-public/AB6AXuAssbOBAnGdbhkirgqkRG0ox3zvocWiXzZ2Vm0xqvEAj5f6o2RCV8k4nCnoWF1bds1pOzFPffWblXfuGoW_IaPsSVF3SONYnOI26EecIJBoSsmNbvABfbRFOuueCzXU65UY61c42PufaVGSC11MYy77RMwPY596HpwO_VlwUt0RLzQkO-CX8RfuG3cgjyZTS4AOeyokwUsim8Rg1AVdGUAWH9WNNlOzV3A7-2dWdJm6stQVbPjeeVXu2H65WknDOxcvD9JdL0sBTg" />
			</div>
		</header>
	`,
	styles: [
		`
			.header-shell {
				position: fixed;
				top: 0;
				left: 0;
				right: 0;
				z-index: 50;
				height: 64px;
				background-color: var(--color-surface-container);
				box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
				display: flex;
				justify-content: space-between;
				align-items: center;
				padding: 0 var(--spacing-3);
				border-bottom: 1px solid var(--color-outline-variant);
			}

			.header-left {
				display: flex;
				align-items: center;
				gap: var(--spacing-2);
			}

			.logo {
				margin: 0;
				font-size: 18px;
				font-weight: 900;
				color: var(--color-primary);
				letter-spacing: -0.25px;
			}

			.header-nav {
				display: none;
				gap: var(--spacing-2);
				margin-left: var(--spacing-3);
			}

			@media (min-width: 768px) {
				.header-nav {
					display: flex;
				}
			}

			.nav-link {
				color: var(--color-on-surface-variant);
				text-decoration: none;
				font-weight: 500;
				font-size: 14px;
				padding: 0.5rem 0.5rem;
				border-radius: 0.5rem;
				transition: all 200ms;
				border-bottom: 2px solid transparent;

				&:hover {
					background-color: var(--color-surface-container-high);
				}

				&.active {
					color: var(--color-primary);
					border-bottom-color: var(--color-primary);
				}
			}

			.header-right {
				display: flex;
				align-items: center;
				gap: var(--spacing-1);
			}

			.search-box {
				display: none;
				position: relative;
			}

			@media (min-width: 640px) {
				.search-box {
					display: flex;
					align-items: center;
				}
			}

			.search-icon {
				position: absolute;
				left: var(--spacing-1);
				top: 50%;
				transform: translateY(-50%);
				color: var(--color-on-surface-variant);
				font-size: 20px;
				width: 20px;
				height: 20px;
			}

			.search-input {
				background-color: var(--color-surface-container-high);
				border: none;
				border-radius: 9999px;
				padding: 0.5rem 1rem 0.5rem 2.5rem;
				font-size: 14px;
				width: 256px;
				color: var(--color-on-surface);

				&:focus {
					outline: none;
					box-shadow: 0 0 0 2px var(--color-primary);
				}

				&::placeholder {
					color: var(--color-on-surface-variant);
				}
			}

			.create-service-btn {
				background-color: var(--color-primary-container);
				color: var(--color-on-primary-container);
				border-radius: 9999px;
				font-size: 14px;
				padding: 0.5rem 1rem;
				text-transform: none;
				box-shadow: none;

				&:hover {
					opacity: 0.9;
				}
			}

			.header-icons {
				display: flex;
				gap: 0.5rem;
				color: var(--color-on-surface-variant);
			}

			.icon-btn {
				color: var(--color-on-surface-variant);
				border-radius: 9999px;

				&:hover {
					background-color: var(--color-surface-container-high);
				}
			}

			.avatar-img {
				width: 32px;
				height: 32px;
				border-radius: 9999px;
				background-color: var(--color-surface-variant);
			}
		`,
	],
})
export class AppHeaderComponent {}
