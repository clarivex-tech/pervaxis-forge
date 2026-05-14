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

import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';

import {
	BatchGenerationRequest,
	GenerationExecutionResult,
	GenerationRequest,
	GenerationAuditEntry,
	RecentGenerationsResponse,
	ValidationPreviewResult,
} from '../models/generation.model';
import { IGenerationApiService } from './generation-api.service';

@Injectable({
	providedIn: 'root',
})
export class MockGenerationApiService implements IGenerationApiService {
	private readonly auditLog: Map<string, GenerationAuditEntry[]> = new Map([
		[
			'clarivolt',
			[
				{
					id: 'gen-clarivolt-001',
					verticalSlug: 'clarivolt',
					timestamp: '2026-05-05T20:00:00Z',
					operator: 'alice@clarivex.com',
					status: 'success',
					serviceCount: 3,
					serviceNames: ['intake-service', 'reporting-service', 'analytics-dashboard'],
					results: [
						{
							name: 'intake-service',
							status: 'success',
							namespace: 'clarivolt/intake-service',
							dockerImage: 'clarivex/clarivolt-intake-service:1.0.0',
							apiRoute: '/clarivolt/intake-service',
							githubRepo: 'clarivex-tech/clarivolt-intake-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-intake-service.git',
							schemaName: 'clarivolt_intake_service',
							zipUrl: 'https://forge.clarivex.tech/download/clarivolt-intake-service.zip',
						},
						{
							name: 'reporting-service',
							status: 'success',
							namespace: 'clarivolt/reporting-service',
							dockerImage: 'clarivex/clarivolt-reporting-service:1.0.0',
							apiRoute: '/clarivolt/reporting-service',
							githubRepo: 'clarivex-tech/clarivolt-reporting-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-reporting-service.git',
							schemaName: 'clarivolt_reporting_service',
							zipUrl: 'https://forge.clarivex.tech/download/clarivolt-reporting-service.zip',
						},
						{
							name: 'analytics-dashboard',
							status: 'success',
							namespace: 'clarivolt/analytics-dashboard',
							dockerImage: 'clarivex/clarivolt-analytics-dashboard:1.0.0',
							apiRoute: '/clarivolt/analytics-dashboard',
							githubRepo: 'clarivex-tech/clarivolt-analytics-dashboard',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-analytics-dashboard.git',
						},
					],
				},
				{
					id: 'gen-clarivolt-002',
					verticalSlug: 'clarivolt',
					timestamp: '2026-05-04T15:30:00Z',
					operator: 'bob@clarivex.com',
					status: 'partial-failure',
					serviceCount: 2,
					serviceNames: ['payment-service', 'notification-service'],
					results: [
						{
							name: 'payment-service',
							status: 'success',
							namespace: 'clarivolt/payment-service',
							dockerImage: 'clarivex/clarivolt-payment-service:1.0.0',
							apiRoute: '/clarivolt/payment-service',
							githubRepo: 'clarivex-tech/clarivolt-payment-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-payment-service.git',
							schemaName: 'clarivolt_payment_service',
							zipUrl: 'https://forge.clarivex.tech/download/clarivolt-payment-service.zip',
						},
						{
							name: 'notification-service',
							status: 'failed',
							namespace: 'clarivolt/notification-service',
							dockerImage: 'clarivex/clarivolt-notification-service:1.0.0',
							apiRoute: '/clarivolt/notification-service',
							githubRepo: 'clarivex-tech/clarivolt-notification-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-notification-service.git',
							errorMessage: 'GitHub repository creation failed: organization rate limit exceeded',
						},
					],
				},
				{
					id: 'gen-clarivolt-003',
					verticalSlug: 'clarivolt',
					timestamp: '2026-05-03T11:45:00Z',
					operator: 'alice@clarivex.com',
					status: 'success',
					serviceCount: 1,
					serviceNames: ['audit-service'],
					results: [
						{
							name: 'audit-service',
							status: 'success',
							namespace: 'clarivolt/audit-service',
							dockerImage: 'clarivex/clarivolt-audit-service:1.0.0',
							apiRoute: '/clarivolt/audit-service',
							githubRepo: 'clarivex-tech/clarivolt-audit-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/clarivolt-audit-service.git',
							schemaName: 'clarivolt_audit_service',
							zipUrl: 'https://forge.clarivex.tech/download/clarivolt-audit-service.zip',
						},
					],
				},
			],
		],
		[
			'stellarlink',
			[
				{
					id: 'gen-stellarlink-001',
					verticalSlug: 'stellarlink',
					timestamp: '2026-05-05T21:49:00Z',
					operator: 'charlie@clarivex.com',
					status: 'success',
					serviceCount: 5,
					serviceNames: [
						'telemetry-service',
						'command-service',
						'tracking-service',
						'analytics-service',
						'dashboard-ui',
					],
					results: [
						{
							name: 'telemetry-service',
							status: 'success',
							namespace: 'stellarlink/telemetry-service',
							dockerImage: 'clarivex/stellarlink-telemetry-service:2.0.0',
							apiRoute: '/stellarlink/telemetry-service',
							githubRepo: 'clarivex-tech/stellarlink-telemetry-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/stellarlink-telemetry-service.git',
							schemaName: 'stellarlink_telemetry_service',
							zipUrl: 'https://forge.clarivex.tech/download/stellarlink-telemetry-service.zip',
						},
						{
							name: 'command-service',
							status: 'success',
							namespace: 'stellarlink/command-service',
							dockerImage: 'clarivex/stellarlink-command-service:2.0.0',
							apiRoute: '/stellarlink/command-service',
							githubRepo: 'clarivex-tech/stellarlink-command-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/stellarlink-command-service.git',
							schemaName: 'stellarlink_command_service',
							zipUrl: 'https://forge.clarivex.tech/download/stellarlink-command-service.zip',
						},
						{
							name: 'tracking-service',
							status: 'success',
							namespace: 'stellarlink/tracking-service',
							dockerImage: 'clarivex/stellarlink-tracking-service:2.0.0',
							apiRoute: '/stellarlink/tracking-service',
							githubRepo: 'clarivex-tech/stellarlink-tracking-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/stellarlink-tracking-service.git',
							schemaName: 'stellarlink_tracking_service',
							zipUrl: 'https://forge.clarivex.tech/download/stellarlink-tracking-service.zip',
						},
						{
							name: 'analytics-service',
							status: 'success',
							namespace: 'stellarlink/analytics-service',
							dockerImage: 'clarivex/stellarlink-analytics-service:2.0.0',
							apiRoute: '/stellarlink/analytics-service',
							githubRepo: 'clarivex-tech/stellarlink-analytics-service',
							gitCloneUrl: 'https://github.com/clarivex-tech/stellarlink-analytics-service.git',
							schemaName: 'stellarlink_analytics_service',
							zipUrl: 'https://forge.clarivex.tech/download/stellarlink-analytics-service.zip',
						},
						{
							name: 'dashboard-ui',
							status: 'success',
							namespace: 'stellarlink/dashboard-ui',
							dockerImage: 'clarivex/stellarlink-dashboard-ui:2.0.0',
							apiRoute: '/stellarlink/dashboard-ui',
							githubRepo: 'clarivex-tech/stellarlink-dashboard-ui',
							gitCloneUrl: 'https://github.com/clarivex-tech/stellarlink-dashboard-ui.git',
						},
					],
				},
			],
		],
		[
			'neofinance',
			[
				{
					id: 'gen-neofinance-001',
					verticalSlug: 'neofinance',
					timestamp: '2026-05-04T22:15:00Z',
					operator: 'diana@clarivex.com',
					status: 'success',
					serviceCount: 2,
					serviceNames: ['lending-engine', 'risk-calculator'],
					results: [
						{
							name: 'lending-engine',
							status: 'success',
							namespace: 'neofinance/lending-engine',
							dockerImage: 'clarivex/neofinance-lending-engine:3.0.0',
							apiRoute: '/neofinance/lending-engine',
							githubRepo: 'clarivex-tech/neofinance-lending-engine',
							gitCloneUrl: 'https://github.com/clarivex-tech/neofinance-lending-engine.git',
							schemaName: 'neofinance_lending_engine',
							zipUrl: 'https://forge.clarivex.tech/download/neofinance-lending-engine.zip',
						},
						{
							name: 'risk-calculator',
							status: 'success',
							namespace: 'neofinance/risk-calculator',
							dockerImage: 'clarivex/neofinance-risk-calculator:3.0.0',
							apiRoute: '/neofinance/risk-calculator',
							githubRepo: 'clarivex-tech/neofinance-risk-calculator',
							gitCloneUrl: 'https://github.com/clarivex-tech/neofinance-risk-calculator.git',
							schemaName: 'neofinance_risk_calculator',
							zipUrl: 'https://forge.clarivex.tech/download/neofinance-risk-calculator.zip',
						},
					],
				},
			],
		],
	]);

	validateManifest(request: GenerationRequest): Observable<ValidationPreviewResult> {
		const normalizedName = request.name.trim();
		const isValid = normalizedName.length >= 3 && !!request.verticalSlug;

		return of({
			isValid,
			errors: isValid ? [] : ['Service name must be at least 3 characters long.'],
			serviceName: normalizedName || null,
			namespace: isValid ? `${request.verticalSlug}.${normalizedName}` : null,
			projectName: isValid ? `${normalizedName}.csproj` : null,
		}).pipe(delay(300));
	}

	generateService(request: GenerationRequest): Observable<GenerationExecutionResult> {
		const content = `Generated scaffold for ${request.name} in ${request.verticalSlug}`;
		const zipBlob = new Blob([content], { type: 'application/zip' });

		return of({
			zipBlob,
			fileName: `${request.name}-scaffold.zip`,
			gitHubRepoUrl: request.createGitHubRepo
				? `https://github.com/clarivex-tech/${request.name}`
				: null,
		}).pipe(delay(600));
	}

	generateBatch(request: BatchGenerationRequest): Observable<GenerationAuditEntry> {
		const audit: GenerationAuditEntry = {
			id: `gen-${request.verticalSlug}-${Date.now()}`,
			verticalSlug: request.verticalSlug,
			timestamp: new Date().toISOString(),
			operator: 'user@clarivex.com',
			status: 'success',
			serviceCount: request.services.length,
			serviceNames: request.services.map((s) => s.name),
			results: request.services.map((service) => ({
				name: service.name,
				status: 'success' as const,
				namespace: `${request.verticalSlug}/${service.name}`,
				dockerImage: `clarivex/${request.verticalSlug}-${service.name}:1.0.0`,
				apiRoute: `/${request.verticalSlug}/${service.name}`,
				githubRepo: `clarivex-tech/${request.verticalSlug}-${service.name}`,
				gitCloneUrl: `https://github.com/clarivex-tech/${request.verticalSlug}-${service.name}.git`,
				schemaName:
					service.type === 'BFF' ? `${request.verticalSlug}_${service.name}` : undefined,
				zipUrl: `https://forge.clarivex.tech/download/${request.verticalSlug}-${service.name}.zip`,
			})),
		};

		const existing = this.auditLog.get(request.verticalSlug) ?? [];
		existing.unshift(audit);
		this.auditLog.set(request.verticalSlug, existing);

		return of(audit).pipe(delay(600));
	}

	getRecentGenerations(verticalSlug: string, limit = 10): Observable<RecentGenerationsResponse> {
		const generations = (this.auditLog.get(verticalSlug) ?? []).slice(0, limit);

		return of({ generations }).pipe(delay(400));
	}

	getGenerationAudit(verticalSlug: string, generationId: string): Observable<GenerationAuditEntry> {
		const generations = this.auditLog.get(verticalSlug) ?? [];
		const audit = generations.find((g) => g.id === generationId);

		return of(audit ?? generations[0]).pipe(delay(300));
	}
}
