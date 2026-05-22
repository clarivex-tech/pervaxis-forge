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

import { HttpInterceptorFn } from '@angular/common/http';

import { environment } from '@env/environment';

export const apiKeyInterceptor: HttpInterceptorFn = (request, next) => {
	if (environment.apiKey && request.url.startsWith(environment.apiBaseUrl)) {
		const cloned = request.clone({
			setHeaders: {
				'X-Api-Key': environment.apiKey,
			},
		});
		return next(cloned);
	}

	return next(request);
};
