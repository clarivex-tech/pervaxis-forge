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
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { importProvidersFrom } from '@angular/core';
import { MatSnackBarModule } from '@angular/material/snack-bar';

import { environment } from '@env/environment';
import { errorInterceptor } from '@core/http/interceptors/error.interceptor';
import { loadingInterceptor } from '@core/http/interceptors/loading.interceptor';
import { requestLoggingInterceptor } from '@core/http/interceptors/request-logging.interceptor';
import { appRoutes } from './app.routes';
import { VERTICAL_API_SERVICE, VerticalApiService } from '@core/api/vertical-api.service';
import { MockVerticalApiService } from '@core/api/mock-vertical-api.service';
import { GENERATION_API_SERVICE, GenerationApiService } from '@core/api/generation-api.service';
import { MockGenerationApiService } from '@core/api/mock-generation-api.service';
import { MODULES_API_SERVICE, ModulesApiService } from '@core/api/modules-api.service';
import { MockModulesApiService } from '@core/api/mock-modules-api.service';

export const appConfig: ApplicationConfig = {
	providers: [
		provideRouter(appRoutes),
		provideHttpClient(
			withInterceptors([requestLoggingInterceptor, loadingInterceptor, errorInterceptor])
		),
		provideAnimations(),
		importProvidersFrom(MatSnackBarModule),
		{
			provide: VERTICAL_API_SERVICE,
			useClass: environment.useMockApi ? MockVerticalApiService : VerticalApiService,
		},
		{
			provide: GENERATION_API_SERVICE,
			useClass: environment.useMockApi ? MockGenerationApiService : GenerationApiService,
		},
		{
			provide: MODULES_API_SERVICE,
			useClass: environment.useMockApi ? MockModulesApiService : ModulesApiService,
		},
	],
};

