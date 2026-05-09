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

import { HttpEventType, HttpInterceptorFn } from '@angular/common/http';
import { tap } from 'rxjs';

import { environment } from '@env/environment';

export const requestLoggingInterceptor: HttpInterceptorFn = (request, next) => {
	if (!environment.enableHttpLogging) {
		return next(request);
	}

	const startedAt = performance.now();
	console.info(`[HTTP] ${request.method} ${request.urlWithParams}`);

	return next(request).pipe(
		tap({
			next: (event) => {
				if (event.type === HttpEventType.Response) {
					const duration = Math.round(performance.now() - startedAt);
					console.info(
						`[HTTP] ${request.method} ${request.urlWithParams} ${event.status} (${duration}ms)`
					);
				}
			},
			error: (error: { status?: number }) => {
				const duration = Math.round(performance.now() - startedAt);
				console.error(
					`[HTTP] ${request.method} ${request.urlWithParams} ${error.status ?? 'ERR'} (${duration}ms)`
				);
			},
		})
	);
};
