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
import { map, Observable } from 'rxjs';

import { environment } from '@env/environment';
import {
	BatchGenerationRequest,
	GenerationExecutionResult,
	GenerationRequest,
	GenerationAuditEntry,
	RecentGenerationsResponse,
	ValidationPreviewResult,
} from '../models/generation.model';

export interface IGenerationApiService {
	validateManifest(request: GenerationRequest): Observable<ValidationPreviewResult>;
	generateService(request: GenerationRequest): Observable<GenerationExecutionResult>;
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

	validateManifest(request: GenerationRequest): Observable<ValidationPreviewResult> {
		return this.http.post<ValidationPreviewResult>(`${this.baseUrl}/validate`, request);
	}

	generateService(request: GenerationRequest): Observable<GenerationExecutionResult> {
		return this.http
			.post(`${this.baseUrl}`, request, {
				observe: 'response',
				responseType: 'blob',
			})
			.pipe(
				map((response) => {
					const contentDisposition = response.headers.get('content-disposition') ?? '';
					const fileNameMatch = /filename="?([^\";]+)"?/i.exec(contentDisposition);
					const headerServiceName = response.headers.get('X-Generation-Service-Name');
					const fileName = fileNameMatch?.[1] ?? `${headerServiceName ?? request.name}-scaffold.zip`;

					return {
						zipBlob: response.body ?? new Blob(),
						fileName,
						gitHubRepoUrl: response.headers.get('X-Generation-GitHub-Url'),
						generatedServiceName: headerServiceName,
						generatedVertical: response.headers.get('X-Generation-Vertical'),
						generationTimestamp: response.headers.get('X-Generation-Timestamp'),
					};
				})
			);
	}

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
