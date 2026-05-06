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
import { MatCardModule } from '@angular/material/card';

@Component({
	selector: 'forge-settings',
	standalone: true,
	imports: [MatCardModule],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<section class="page-section">
			<h1>Settings</h1>
			<mat-card class="coming-soon-card">
				<mat-card-header>
					<mat-card-title>Coming Soon</mat-card-title>
				</mat-card-header>
				<mat-card-content>
					<p>Settings will be available soon.</p>
				</mat-card-content>
			</mat-card>
		</section>
	`,
	styles: [
		`
			.page-section {
				max-width: 1200px;
			}

			h1 {
				font-size: 32px;
				font-weight: 700;
				color: var(--color-on-surface);
				margin: 0 0 1.5rem 0;
			}

			.coming-soon-card {
				background-color: var(--color-surface-container-low);
				border: 1px solid var(--color-outline-variant);
			}
		`,
	],
})
export class SettingsComponent {}
