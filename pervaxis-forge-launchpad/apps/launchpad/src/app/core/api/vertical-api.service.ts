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

import {
	UpdateVerticalRequest,
	VerticalEnrollmentRequest,
} from '../models/enrollment.model';
import {
	ConnectivityValidationResponse,
	VerticalResponse,
	VerticalSummaryResponse,
} from '../models/vertical.model';

export interface IVerticalApiService {
	enrollVertical(
		request: VerticalEnrollmentRequest,
	): Observable<VerticalSummaryResponse>;
	listVerticals(): Observable<VerticalSummaryResponse[]>;
	getVertical(slug: string): Observable<VerticalResponse>;
	updateVertical(
		slug: string,
		request: UpdateVerticalRequest,
	): Observable<VerticalResponse>;
	validateConnectivity(slug: string): Observable<ConnectivityValidationResponse>;
}

export const VERTICAL_API_SERVICE = new InjectionToken<IVerticalApiService>(
	'VERTICAL_API_SERVICE',
);

@Injectable({
	providedIn: 'root',
})
export class VerticalApiService implements IVerticalApiService {
	private readonly baseUrl = '/api/v1/verticals';
	private readonly http = inject(HttpClient);

	enrollVertical(
		request: VerticalEnrollmentRequest,
	): Observable<VerticalSummaryResponse> {
		return this.http.post<VerticalSummaryResponse>(this.baseUrl, request);
	}

	listVerticals(): Observable<VerticalSummaryResponse[]> {
		return this.http.get<VerticalSummaryResponse[]>(this.baseUrl);
	}

	getVertical(slug: string): Observable<VerticalResponse> {
		return this.http.get<VerticalResponse>(`${this.baseUrl}/${slug}`);
	}

	updateVertical(
		slug: string,
		request: UpdateVerticalRequest,
	): Observable<VerticalResponse> {
		return this.http.put<VerticalResponse>(`${this.baseUrl}/${slug}`, request);
	}

	validateConnectivity(slug: string): Observable<ConnectivityValidationResponse> {
		return this.http.post<ConnectivityValidationResponse>(
			`${this.baseUrl}/${slug}/validate`,
			{},
		);
	}
}
