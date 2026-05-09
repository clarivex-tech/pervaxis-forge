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

import { Routes } from '@angular/router';

import { forgeAuthGuard } from './core/auth/auth.guard';

export const appRoutes: Routes = [
	{
		path: '',
		canActivate: [forgeAuthGuard],
		children: [
			{
				path: '',
				data: { breadcrumb: 'Dashboard' },
				loadComponent: () =>
					import('./features/vertical-dashboard/vertical-dashboard.component').then(
						(m) => m.VerticalDashboardComponent
					),
			},
			{
				path: 'verticals',
				children: [
					{
						path: 'enroll',
						data: { breadcrumb: 'Enroll Vertical' },
						loadComponent: () =>
							import('./features/vertical-enrollment/vertical-enrollment.component').then(
								(m) => m.VerticalEnrollmentComponent
							),
					},
					{
						path: ':slug',
						data: { breadcrumb: 'Workspace' },
						loadComponent: () =>
							import('./features/vertical-workspace/vertical-workspace.component').then(
								(m) => m.VerticalWorkspaceComponent
							),
					},
					{
						path: ':slug/generate',
						data: { breadcrumb: 'Generate Services' },
						loadComponent: () =>
							import('./features/service-generation/generation-wizard.component').then(
								(m) => m.GenerationWizardComponent
							),
					},
				],
			},
		],
	},
	{
		path: 'unauthorized',
		loadComponent: () =>
			import('./features/unauthorized/unauthorized.component').then((m) => m.UnauthorizedComponent),
	},
	{
		path: '**',
		redirectTo: '',
	},
];
