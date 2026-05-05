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
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
	selector: 'forge-root',
	standalone: true,
	imports: [RouterOutlet, RouterLink, RouterLinkActive],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<header>
			<h1>Pervaxis Forge Launchpad</h1>
			<nav>
				<a routerLink="/" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }">Dashboard</a>
				<a routerLink="/verticals/enroll" routerLinkActive="active">Enroll Vertical</a>
			</nav>
		</header>

		<main>
			<router-outlet />
		</main>
	`,
	styles: [
		`
			:host {
				display: block;
				padding: 1.25rem;
			}

			header {
				margin-bottom: 1rem;
			}

			nav {
				display: flex;
				gap: 1rem;
			}

			.active {
				font-weight: 700;
			}
		`,
	],
})
export class AppComponent {}
