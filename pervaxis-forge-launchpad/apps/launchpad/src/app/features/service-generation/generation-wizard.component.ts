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
import { ActivatedRoute } from '@angular/router';

@Component({
	selector: 'forge-generation-wizard',
	standalone: true,
	changeDetection: ChangeDetectionStrategy.OnPush,
	template: `
		<section>
			<h2>Service Generation Wizard</h2>
			<p>Vertical context: <strong>{{ slug }}</strong></p>
			<p>Phase 2 implementation is queued after dashboard/workspace completion.</p>
		</section>
	`,
})
export class GenerationWizardComponent {
	private readonly route = inject(ActivatedRoute);

	get slug(): string {
		return this.route.snapshot.paramMap.get('slug') ?? 'unknown';
	}
}
