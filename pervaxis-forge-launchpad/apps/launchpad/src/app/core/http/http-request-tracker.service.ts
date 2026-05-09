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

import { computed, Injectable, signal } from '@angular/core';

@Injectable({
	providedIn: 'root',
})
export class HttpRequestTrackerService {
	private readonly activeRequestCount = signal(0);

	readonly isLoading = computed(() => this.activeRequestCount() > 0);

	beginRequest(): void {
		this.activeRequestCount.update((count) => count + 1);
	}

	endRequest(): void {
		this.activeRequestCount.update((count) => Math.max(0, count - 1));
	}
}
