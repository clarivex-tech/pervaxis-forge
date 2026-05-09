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

import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

import { AppHeaderComponent } from '@layout/app-header.component';
import { AppSidenavComponent } from '@layout/app-sidenav.component';

@Component({
	selector: 'forge-app-layout',
	standalone: true,
	imports: [
		CommonModule,
		RouterOutlet,
		MatSidenavModule,
		MatButtonModule,
		MatIconModule,
		AppHeaderComponent,
		AppSidenavComponent,
	],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<mat-sidenav-container class="sidenav-container">
			<!-- Sidenav Drawer -->
			<mat-sidenav
				#sidenav
				class="sidenav"
				[mode]="sidenavMode()"
				[opened]="sidenavOpened()"
				(openedChange)="sidenavOpened.set($event)"
			>
				<forge-app-sidenav (navigate)="sidenav.close()"></forge-app-sidenav>
			</mat-sidenav>

			<!-- Main Content -->
			<mat-sidenav-content>
				<!-- Header -->
				<forge-app-header
					(toggleSidenav)="sidenav.toggle()"
					[sidenavOpened]="sidenavOpened()"
				></forge-app-header>

				<!-- Page Content -->
				<main class="main-content">
					<router-outlet></router-outlet>
				</main>
			</mat-sidenav-content>
		</mat-sidenav-container>
	`,
	styles: [
		`
			:host {
				display: flex;
				height: 100vh;
			}

			.sidenav-container {
				width: 100%;
				height: 100%;
			}

			.sidenav {
				width: 256px;
			}

			.main-content {
				flex: 1;
				overflow-y: auto;
				padding: 2rem;
				background-color: var(--color-background);
			}

			@media (max-width: 768px) {
				.main-content {
					padding: 1rem;
				}
			}
		`,
	],
})
export class AppLayoutComponent {
	private readonly breakpointObserver = inject(BreakpointObserver);

	sidenavMode = signal<'side' | 'over'>('side');
	sidenavOpened = signal(true);

	constructor() {
		// Responsive sidenav: switch to 'over' mode on mobile
		this.breakpointObserver
			.observe([Breakpoints.Handset, Breakpoints.Tablet])
			.subscribe((result) => {
				if (result.matches) {
					this.sidenavMode.set('over');
					this.sidenavOpened.set(false);
				} else {
					this.sidenavMode.set('side');
					this.sidenavOpened.set(true);
				}
			});
	}
}
