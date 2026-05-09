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

import { CloudProviderConfig } from './cloud-provider.model';

export type SourceControlPlatform = 'GitHub' | 'GitLab' | 'Bitbucket';
export type RepoVisibility = 'private' | 'internal' | 'public';

export interface SourceControlConfig {
	platform: SourceControlPlatform;
	gitHubOrg: string;
	accessToken: string;
	defaultVisibility: RepoVisibility;
	defaultBranchProtection: boolean;
}

export interface VerticalTechDefaults {
	environments: string[];
	defaultEnvironment: string;
	generateTerraform: boolean;
	generateCdk: boolean;
	defaultDbEngine: string | null;
}

export interface VerticalEnrollmentRequest {
	slug: string;
	displayName: string;
	description: string;
	ownerTeam: string;
	ownerEmail: string;
	componentPrefix: string;
	cloudProvider: CloudProviderConfig;
	sourceControl: SourceControlConfig;
	techDefaults: VerticalTechDefaults;
}

export interface UpdateVerticalRequest {
	displayName: string;
	description: string;
	ownerTeam: string;
	ownerEmail: string;
	techDefaults: VerticalTechDefaults;
}
