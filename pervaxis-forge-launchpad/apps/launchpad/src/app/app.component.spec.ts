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

import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';

import { HttpRequestTrackerService } from '@core/http/http-request-tracker.service';
import { AppComponent } from './app.component';
import { configureStandaloneComponent } from '../testing/component-testbed';

describe('AppComponent', () => {
	beforeEach(async () => {
		await configureStandaloneComponent(AppComponent, [provideRouter([])]);
	});

	it('shows the global loading bar when requests are active', () => {
		const tracker = TestBed.inject(HttpRequestTrackerService);
		const fixture = TestBed.createComponent(AppComponent);

		tracker.beginRequest();
		fixture.detectChanges();

		expect(fixture.nativeElement.querySelector('mat-progress-bar')).not.toBeNull();
	});
});
