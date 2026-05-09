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

import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

import { NotificationService } from '@core/notifications/notification.service';

export const errorInterceptor: HttpInterceptorFn = (request, next) => {
	const router = inject(Router);
	const notifications = inject(NotificationService);

	return next(request).pipe(
		catchError((error: unknown) => {
			if (!(error instanceof HttpErrorResponse)) {
				notifications.showError('An unexpected error occurred.');
				return throwError(() => error);
			}

			if (error.status === 401 || error.status === 403) {
				notifications.showError('Your session does not allow this action.');
				void router.navigate(['/unauthorized'], {
					queryParams: { returnUrl: router.url },
				});
				return throwError(() => error);
			}

			if (error.status === 0) {
				notifications.showError(
					'Unable to reach the Forge API. Check the network or backend status.'
				);
				return throwError(() => error);
			}

			const message =
				error.status >= 500
					? 'Forge hit a server error. Please retry in a moment.'
					: 'Forge could not process that request.';

			notifications.showError(message);
			return throwError(() => error);
		})
	);
};
