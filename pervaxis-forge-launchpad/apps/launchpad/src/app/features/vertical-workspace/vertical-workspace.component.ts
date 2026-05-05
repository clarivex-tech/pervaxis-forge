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
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
	selector: 'forge-vertical-workspace',
	standalone: true,
	imports: [RouterLink],
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<section>
			<h2>Vertical Workspace</h2>
			<p>Selected vertical: <strong>{{ slug }}</strong></p>
			<p>Workspace details are planned in Phase 1.</p>
			<a [routerLink]="['/verticals', slug, 'generate']">Open Generation Wizard</a>
		</section>
	`,
})
export class VerticalWorkspaceComponent {
	private readonly route = inject(ActivatedRoute);

	get slug(): string {
		return this.route.snapshot.paramMap.get('slug') ?? 'unknown';
	}
}
