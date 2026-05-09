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

import { TestBed } from '@angular/core/testing';

import { HttpRequestTrackerService } from './http-request-tracker.service';

describe('HttpRequestTrackerService', () => {
	let service: HttpRequestTrackerService;

	beforeEach(() => {
		TestBed.configureTestingModule({});
		service = TestBed.inject(HttpRequestTrackerService);
	});

	it('tracks loading while requests are active', () => {
		expect(service.isLoading()).toBe(false);

		service.beginRequest();

		expect(service.isLoading()).toBe(true);

		service.endRequest();

		expect(service.isLoading()).toBe(false);
	});

	it('never decrements below zero', () => {
		service.endRequest();

		expect(service.isLoading()).toBe(false);
	});
});
