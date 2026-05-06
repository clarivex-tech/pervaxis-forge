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

import { Component } from '@angular/core';
import { AppLayoutComponent } from './layout/app-layout.component';

@Component({
	selector: 'forge-root',
	standalone: true,
	imports: [AppLayoutComponent],
	template: `<forge-app-layout></forge-app-layout>`,
	styles: [],
})
export class AppComponent {}
