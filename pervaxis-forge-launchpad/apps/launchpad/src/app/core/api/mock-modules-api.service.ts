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

import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay } from 'rxjs/operators';

import { CanvasModule, GenesisModule } from '../models/generation.model';
import { IModulesApiService } from './modules-api.service';

@Injectable({
	providedIn: 'root',
})
export class MockModulesApiService implements IModulesApiService {
	private readonly modules: GenesisModule[] = [
		{ name: 'Caching', label: 'Caching', description: 'Distributed caching via Redis' },
		{ name: 'Messaging', label: 'Messaging', description: 'Event-driven messaging via SQS/SNS' },
		{ name: 'FileStorage', label: 'File Storage', description: 'Object storage via S3' },
		{ name: 'Search', label: 'Search', description: 'Full-text search via OpenSearch' },
		{ name: 'Notifications', label: 'Notifications', description: 'Email, SMS and push notifications' },
		{ name: 'Workflow', label: 'Workflow', description: 'Step-based workflow via AWS Step Functions' },
		{ name: 'AIAssistance', label: 'AI Assistance', description: 'Generative AI via Bedrock' },
		{ name: 'Reporting', label: 'Reporting', description: 'Data exports and scheduled reports' },
	];

	private readonly canvasModules: CanvasModule[] = [
		{ id: 'shell', name: 'Shell', label: 'Shell' },
		{ id: 'layout', name: 'Layout', label: 'Layout' },
		{ id: 'navigation', name: 'Navigation', label: 'Navigation' },
		{ id: 'auth', name: 'Auth', label: 'Auth' },
		{ id: 'workspace', name: 'Workspace', label: 'Workspace' },
		{ id: 'settings', name: 'Settings', label: 'Settings' },
		{ id: 'profile', name: 'Profile', label: 'Profile' },
		{ id: 'notifications', name: 'Notifications', label: 'Notifications' },
		{ id: 'search', name: 'Search', label: 'Search' },
		{ id: 'dashboard', name: 'Dashboard', label: 'Dashboard' },
		{ id: 'reports', name: 'Reports', label: 'Reports' },
		{ id: 'analytics', name: 'Analytics', label: 'Analytics' },
		{ id: 'admin', name: 'Admin', label: 'Admin' },
		{ id: 'support', name: 'Support', label: 'Support' },
	];

	listModules(): Observable<GenesisModule[]> {
		return of(this.modules).pipe(delay(300));
	}

	listCanvasModules(): Observable<CanvasModule[]> {
		return of(this.canvasModules).pipe(delay(300));
	}
}
