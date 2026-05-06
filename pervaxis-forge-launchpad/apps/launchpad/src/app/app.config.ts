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

import { ApplicationConfig } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';

import { appRoutes } from './app.routes';
import {
	VERTICAL_API_SERVICE,
	VerticalApiService,
} from './core/api/vertical-api.service';
import { MockVerticalApiService } from './core/api/mock-vertical-api.service';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
	providers: [
		provideRouter(appRoutes),
		provideHttpClient(),
		provideAnimations(),
		{
			provide: VERTICAL_API_SERVICE,
			useClass: environment.useMockApi ? MockVerticalApiService : VerticalApiService,
		},
	],
};
