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

import {
	ChangeDetectionStrategy,
	Component,
	inject,
	signal,
	computed,
	effect,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';

import { VERTICAL_API_SERVICE } from '@core/api/vertical-api.service';
import { GENERATION_API_SERVICE } from '@core/api/generation-api.service';
import { VerticalResponse } from '@core/models/vertical.model';
import { GenerationAuditEntry, GeneratedServiceRecord } from '@core/models/generation.model';
import { VerticalSettingsPanelComponent } from './vertical-settings/vertical-settings-panel.component';

@Component({
	selector: 'forge-vertical-workspace',
	standalone: true,
	imports: [
		CommonModule,
		RouterLink,
		MatButtonModule,
		MatIconModule,
		MatChipsModule,
		MatTableModule,
		MatProgressSpinnerModule,
		MatTooltipModule,
		MatBadgeModule,
		MatDividerModule,
		VerticalSettingsPanelComponent,
	],
	templateUrl: './vertical-workspace.component.html',
	styleUrls: ['./vertical-workspace.component.scss'],
	changeDetection: ChangeDetectionStrategy.OnPush,
})
export class VerticalWorkspaceComponent {
	private readonly route = inject(ActivatedRoute);
	private readonly verticalApiService = inject(VERTICAL_API_SERVICE);
	private readonly generationApiService = inject(GENERATION_API_SERVICE);

	readonly slug = signal<string>('');
	readonly vertical = signal<VerticalResponse | null>(null);
	readonly recentGenerations = signal<GenerationAuditEntry[]>([]);
	readonly generatedServices = signal<GeneratedServiceRecord[]>([]);
	readonly isLoadingVertical = signal(false);
	readonly isLoadingGenerations = signal(false);
	readonly isLoadingServices = signal(false);
	readonly regeneratingServiceId = signal<string | null>(null);
	readonly showSettingsPanel = signal(false);

	readonly displayColumns = ['timestamp', 'operator', 'services', 'status'];
	readonly serviceColumns = ['name', 'type', 'generatedAt', 'generatedBy', 'actions'];

	readonly hasData = computed(() => this.vertical() !== null);
	readonly serviceCount = computed(() => this.vertical()?.serviceCount ?? 0);
	readonly lastGeneratedAt = computed(() => this.vertical()?.lastGeneratedAt);

	constructor() {
		effect(() => {
			const slug = this.route.snapshot.paramMap.get('slug');
			if (slug) {
				this.slug.set(slug);
				this.loadVertical(slug);
				this.loadRecentGenerations(slug);
				this.loadGeneratedServices(slug);
			}
		});
	}

	private loadVertical(slug: string): void {
		this.isLoadingVertical.set(true);
		this.verticalApiService.getVertical(slug).subscribe({
			next: (vertical) => {
				this.vertical.set(vertical);
				this.isLoadingVertical.set(false);
			},
			error: () => {
				this.isLoadingVertical.set(false);
			},
		});
	}

	private loadRecentGenerations(slug: string): void {
		this.isLoadingGenerations.set(true);
		this.generationApiService.getRecentGenerations(slug, 10).subscribe({
			next: (response) => {
				this.recentGenerations.set(response.generations);
				this.isLoadingGenerations.set(false);
			},
			error: () => {
				this.isLoadingGenerations.set(false);
			},
		});
	}

	private loadGeneratedServices(slug: string): void {
		this.isLoadingServices.set(true);
		this.generationApiService.listGeneratedServices(slug).subscribe({
			next: (services) => {
				this.generatedServices.set(services);
				this.isLoadingServices.set(false);
			},
			error: () => {
				this.isLoadingServices.set(false);
			},
		});
	}

	regenerateService(serviceId: string): void {
		this.regeneratingServiceId.set(serviceId);
		this.generationApiService.regenerateService(this.slug(), serviceId).subscribe({
			next: (result) => {
				this.downloadZip(result.zipBlob, result.fileName);
				this.regeneratingServiceId.set(null);
			},
			error: () => {
				this.regeneratingServiceId.set(null);
			},
		});
	}

	private downloadZip(blob: Blob, fileName: string): void {
		const url = URL.createObjectURL(blob);
		const anchor = document.createElement('a');
		anchor.href = url;
		anchor.download = fileName;
		anchor.click();
		URL.revokeObjectURL(url);
	}

	openSettings(): void {
		this.showSettingsPanel.set(true);
	}

	closeSettings(): void {
		this.showSettingsPanel.set(false);
	}

	getStatusChipColor(status: string): string {
		switch (status) {
			case 'success':
				return 'accent';
			case 'failed':
				return 'warn';
			case 'partial-failure':
				return 'warn';
			default:
				return 'primary';
		}
	}

	formatDate(dateString: string): string {
		return new Date(dateString).toLocaleDateString('en-US', {
			month: 'short',
			day: 'numeric',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit',
		});
	}
}
