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

import { inject, Injectable, InjectionToken } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { environment } from '@env/environment';
import { CanvasModule, GenesisModule } from '../models/generation.model';

interface ModuleApiDto {
	id: string;
	displayName: string;
	iamPermissions: string;
}

interface CanvasModuleApiDto {
	id: string;
	displayName: string;
}

export interface IModulesApiService {
	listModules(): Observable<GenesisModule[]>;
	listCanvasModules(): Observable<CanvasModule[]>;
}

export const MODULES_API_SERVICE = new InjectionToken<IModulesApiService>('MODULES_API_SERVICE');

@Injectable({
	providedIn: 'root',
})
export class ModulesApiService implements IModulesApiService {
	private readonly baseUrl = `${environment.apiBaseUrl}/modules`;
	private readonly canvasBaseUrl = `${environment.apiBaseUrl}/canvas-modules`;
	private readonly http = inject(HttpClient);

	listModules(): Observable<GenesisModule[]> {
		return this.http.get<ModuleApiDto[]>(this.baseUrl).pipe(
			map(items => items.map(item => ({
				name: item.id,
				label: item.displayName,
				description: item.iamPermissions,
			})))
		);
	}

	listCanvasModules(): Observable<CanvasModule[]> {
		return this.http.get<CanvasModuleApiDto[]>(this.canvasBaseUrl).pipe(
			map(items => items.map(item => ({
				id: item.id,
				name: item.displayName,
				label: item.displayName,
			})))
		);
	}
}
