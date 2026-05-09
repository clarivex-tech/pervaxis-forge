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

import { configureStandaloneComponent } from '../../../testing/component-testbed';
import { UnauthorizedComponent } from './unauthorized.component';

describe('UnauthorizedComponent', () => {
	beforeEach(async () => {
		await configureStandaloneComponent(UnauthorizedComponent, [provideRouter([])]);
	});

	it('renders the access denied message', () => {
		const fixture = TestBed.createComponent(UnauthorizedComponent);
		fixture.detectChanges();

		const textContent = fixture.nativeElement.textContent as string;

		expect(textContent).toContain('Access Denied');
		expect(textContent).toContain('Return to Dashboard');
	});
});
