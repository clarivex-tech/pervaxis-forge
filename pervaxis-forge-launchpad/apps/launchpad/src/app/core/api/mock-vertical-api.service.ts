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
import { Observable, of } from 'rxjs';

import { UpdateVerticalRequest, VerticalEnrollmentRequest } from '../models/enrollment.model';
import {
	ConnectivityValidationResponse,
	VerticalResponse,
	VerticalSummaryResponse,
} from '../models/vertical.model';
import { IVerticalApiService } from './vertical-api.service';

@Injectable({
	providedIn: 'root',
})
export class MockVerticalApiService implements IVerticalApiService {
	private readonly verticals: VerticalResponse[] = [
		{
			id: 'vertical-clarivolt',
			slug: 'clarivolt',
			displayName: 'Clarivolt Energy',
			description: 'Energy analytics and operations.',
			ownerTeam: 'Digital Platforms',
			ownerEmail: 'platforms@clarivex.com',
			cloudProvider: 'AWS',
			sourceControl: 'GitHub',
			githubOrg: 'clarivex-tech',
			environments: ['test', 'accp', 'prod'],
			enrolledAt: '2026-05-05T09:00:00Z',
			serviceCount: 12,
			lastGeneratedAt: '2026-05-05T20:00:00Z',
			cloudProviderConfig: {
				provider: 'AWS',
				awsAccountId: '123456789012',
				iamRoleArn: 'arn:aws:iam::123456789012:role/ForgeVerticalRole',
				defaultRegion: 'ap-south-1',
			},
			sourceControlConfig: {
				platform: 'GitHub',
				gitHubOrg: 'clarivex-tech',
				accessToken: '***',
				defaultVisibility: 'private',
				defaultBranchProtection: true,
			},
			techDefaults: {
				environments: ['test', 'accp', 'prod'],
				defaultEnvironment: 'test',
				generateTerraform: true,
				generateCdk: true,
				defaultDbEngine: null,
			},
		},
		{
			id: 'vertical-stellarlink',
			slug: 'stellarlink',
			displayName: 'StellarLink Satellite',
			description: 'Satellite fleet command and telemetry platform.',
			ownerTeam: 'Orbital Systems',
			ownerEmail: 'orbital@clarivex.com',
			cloudProvider: 'AWS',
			sourceControl: 'GitHub',
			githubOrg: 'clarivex-tech',
			environments: ['test', 'accp', 'prod'],
			enrolledAt: '2026-05-04T06:20:00Z',
			serviceCount: 45,
			lastGeneratedAt: '2026-05-05T21:49:00Z',
			cloudProviderConfig: {
				provider: 'AWS',
				awsAccountId: '223344556677',
				iamRoleArn: 'arn:aws:iam::223344556677:role/ForgeVerticalRole',
				defaultRegion: 'eu-west-1',
			},
			sourceControlConfig: {
				platform: 'GitHub',
				gitHubOrg: 'clarivex-tech',
				accessToken: '***',
				defaultVisibility: 'private',
				defaultBranchProtection: true,
			},
			techDefaults: {
				environments: ['test', 'accp', 'prod'],
				defaultEnvironment: 'test',
				generateTerraform: true,
				generateCdk: true,
				defaultDbEngine: null,
			},
		},
		{
			id: 'vertical-neofinance',
			slug: 'neofinance',
			displayName: 'NeoFinance Core',
			description: 'Core lending and risk processing platform.',
			ownerTeam: 'Finance Core',
			ownerEmail: 'finance-core@clarivex.com',
			cloudProvider: 'AWS',
			sourceControl: 'GitHub',
			githubOrg: 'clarivex-tech',
			environments: ['test', 'accp', 'prod'],
			enrolledAt: '2026-05-03T11:30:00Z',
			serviceCount: 8,
			lastGeneratedAt: '2026-05-04T22:15:00Z',
			cloudProviderConfig: {
				provider: 'AWS',
				awsAccountId: '334455667788',
				iamRoleArn: 'arn:aws:iam::334455667788:role/ForgeVerticalRole',
				defaultRegion: 'ap-south-1',
			},
			sourceControlConfig: {
				platform: 'GitHub',
				gitHubOrg: 'clarivex-tech',
				accessToken: '***',
				defaultVisibility: 'private',
				defaultBranchProtection: true,
			},
			techDefaults: {
				environments: ['test', 'accp', 'prod'],
				defaultEnvironment: 'test',
				generateTerraform: true,
				generateCdk: true,
				defaultDbEngine: null,
			},
		},
	];

	enrollVertical(request: VerticalEnrollmentRequest): Observable<VerticalSummaryResponse> {
		const created: VerticalResponse = {
			id: `vertical-${request.slug}`,
			slug: request.slug,
			displayName: request.displayName,
			description: request.description,
			ownerTeam: request.ownerTeam,
			ownerEmail: request.ownerEmail,
			cloudProvider: request.cloudProvider.provider,
			sourceControl: request.sourceControl.platform,
			githubOrg: request.sourceControl.gitHubOrg,
			environments: request.techDefaults.environments,
			enrolledAt: new Date().toISOString(),
			serviceCount: 0,
			cloudProviderConfig: request.cloudProvider,
			sourceControlConfig: request.sourceControl,
			techDefaults: request.techDefaults,
		};

		this.verticals.push(created);
		return of(this.toSummary(created));
	}

	listVerticals(): Observable<VerticalSummaryResponse[]> {
		return of(this.verticals.map((vertical) => this.toSummary(vertical)));
	}

	getVertical(slug: string): Observable<VerticalResponse> {
		const existing = this.verticals.find((vertical) => vertical.slug === slug);

		return of(existing ?? this.verticals[0]);
	}

	updateVertical(slug: string, request: UpdateVerticalRequest): Observable<VerticalResponse> {
		const existing = this.verticals.find((vertical) => vertical.slug === slug);

		if (!existing) {
			return of(this.verticals[0]);
		}

		existing.displayName = request.displayName;
		existing.description = request.description;
		existing.ownerTeam = request.ownerTeam;
		existing.ownerEmail = request.ownerEmail;
		existing.environments = request.techDefaults.environments;
		existing.techDefaults = request.techDefaults;

		return of(existing);
	}

	unenrollVertical(slug: string): Observable<void> {
		const index = this.verticals.findIndex((vertical) => vertical.slug === slug);

		if (index >= 0) {
			this.verticals.splice(index, 1);
		}

		return of(void 0);
	}

	validateConnectivity(_slug: string): Observable<ConnectivityValidationResponse> {
		return of({
			awsConnectivity: {
				success: true,
				accountId: '123456789012',
			},
			gitHubConnectivity: {
				success: true,
				org: 'clarivex-tech',
			},
		});
	}

	private toSummary(vertical: VerticalResponse): VerticalSummaryResponse {
		return {
			id: vertical.id,
			slug: vertical.slug,
			displayName: vertical.displayName,
			cloudProvider: vertical.cloudProvider,
			sourceControl: vertical.sourceControl,
			githubOrg: vertical.githubOrg,
			environments: vertical.environments,
			enrolledAt: vertical.enrolledAt,
			serviceCount: vertical.serviceCount,
			lastGeneratedAt: vertical.lastGeneratedAt,
		};
	}
}
