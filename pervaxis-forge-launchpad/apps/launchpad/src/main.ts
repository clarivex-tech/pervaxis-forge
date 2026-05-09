// Copyright © Clarivex Technologies. All rights reserved.

import { bootstrapApplication } from '@angular/platform-browser';

import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';

void bootstrapApplication(AppComponent, appConfig).catch((error: unknown) => {
	// Keep bootstrap failures visible in browser console during early setup.
	console.error(error);
});
