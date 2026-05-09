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

import { Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';

type ProviderList = Parameters<typeof TestBed.configureTestingModule>[0]['providers'];

export async function configureStandaloneComponent<T>(
	component: Type<T>,
	providers: ProviderList = []
): Promise<void> {
	TestBed.resetTestingModule();
	TestBed.configureTestingModule({
		imports: [component],
		providers,
	});

	await TestBed.compileComponents();
}
