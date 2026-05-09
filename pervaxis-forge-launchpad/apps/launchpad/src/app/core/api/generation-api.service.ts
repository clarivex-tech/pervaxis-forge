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

import { environment } from '@env/environment';
import {
	BatchGenerationRequest,
	GenerationAuditEntry,
	RecentGenerationsResponse,
} from '../models/generation.model';

export interface IGenerationApiService {
	generateBatch(request: BatchGenerationRequest): Observable<GenerationAuditEntry>;
	getRecentGenerations(verticalSlug: string, limit?: number): Observable<RecentGenerationsResponse>;
	getGenerationAudit(verticalSlug: string, generationId: string): Observable<GenerationAuditEntry>;
}

export const GENERATION_API_SERVICE = new InjectionToken<IGenerationApiService>(
	'GENERATION_API_SERVICE'
);

@Injectable({
	providedIn: 'root',
})
export class GenerationApiService implements IGenerationApiService {
	private readonly baseUrl = `${environment.apiBaseUrl}/generate`;
	private readonly http = inject(HttpClient);

	generateBatch(request: BatchGenerationRequest): Observable<GenerationAuditEntry> {
		return this.http.post<GenerationAuditEntry>(`${this.baseUrl}/batch`, request);
	}

	getRecentGenerations(verticalSlug: string, limit = 10): Observable<RecentGenerationsResponse> {
		return this.http.get<RecentGenerationsResponse>(
			`${this.baseUrl}/audit/${verticalSlug}?limit=${limit}`
		);
	}

	getGenerationAudit(verticalSlug: string, generationId: string): Observable<GenerationAuditEntry> {
		return this.http.get<GenerationAuditEntry>(`${this.baseUrl}/audit/${verticalSlug}/${generationId}`);
	}
}
