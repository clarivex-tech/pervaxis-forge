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
import { RouterOutlet } from '@angular/router';
import { AppHeaderComponent } from './app-header.component';
import { AppSidenavComponent } from './app-sidenav.component';

@Component({
	selector: 'forge-app-layout',
	standalone: true,
	imports: [RouterOutlet, AppHeaderComponent, AppSidenavComponent],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<forge-app-header></forge-app-header>
		<forge-app-sidenav></forge-app-sidenav>
		<main class="main-content">
			<router-outlet></router-outlet>
		</main>
	`,
	styles: [
		`
			:host {
				display: block;
			}

			.main-content {
				margin-left: 256px;
				margin-top: 64px;
				padding: 1.5rem;
				min-height: 100vh;
				background-color: var(--color-background);
			}
		`,
	],
})
export class AppLayoutComponent {}
