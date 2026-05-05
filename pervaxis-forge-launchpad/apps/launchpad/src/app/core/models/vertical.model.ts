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
	SourceControlConfig,
	SourceControlPlatform,
	VerticalTechDefaults,
} from './enrollment.model';
import { CloudProvider, CloudProviderConfig } from './cloud-provider.model';

export interface VerticalSummaryResponse {
	id: string;
	slug: string;
	displayName: string;
	cloudProvider: CloudProvider;
	sourceControl: SourceControlPlatform;
	githubOrg: string;
	environments: string[];
	enrolledAt: string;
}

export interface VerticalResponse extends VerticalSummaryResponse {
	description: string;
	ownerTeam: string;
	ownerEmail: string;
	cloudProviderConfig: CloudProviderConfig;
	sourceControlConfig: SourceControlConfig;
	techDefaults: VerticalTechDefaults;
}

export interface AwsConnectivityStatus {
	success: boolean;
	accountId: string;
}

export interface GitHubConnectivityStatus {
	success: boolean;
	org: string;
}

export interface ConnectivityValidationResponse {
	awsConnectivity: AwsConnectivityStatus;
	githubConnectivity: GitHubConnectivityStatus;
}
