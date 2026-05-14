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

import { CloudProviderConfig } from '../../core/models/cloud-provider.model';
import {
	SourceControlConfig,
	VerticalEnrollmentRequest,
	VerticalTechDefaults,
} from '../../core/models/enrollment.model';

export interface VerticalIdentityState {
	slug: string;
	displayName: string;
	description: string;
	ownerTeam: string;
	ownerEmail: string;
	componentPrefix: string;
}

export interface EnrollmentConnectivityState {
	isChecking: boolean;
	cloudValid: boolean | null;
	sourceControlValid: boolean | null;
	lastCheckedAt: string | null;
	errorMessage: string | null;
}

export interface EnrollmentSubmitState {
	isSubmitting: boolean;
	submitError: string | null;
	submitSuccess: boolean;
}

export interface EnrollmentState {
	currentStepIndex: number;
	identity: VerticalIdentityState;
	cloudProvider: CloudProviderConfig;
	sourceControl: SourceControlConfig;
	techDefaults: VerticalTechDefaults;
	connectivity: EnrollmentConnectivityState;
	submit: EnrollmentSubmitState;
}

export const initialEnrollmentState: EnrollmentState = {
	currentStepIndex: 0,
	identity: {
		slug: '',
		displayName: '',
		description: '',
		ownerTeam: '',
		ownerEmail: '',
		componentPrefix: '',
	},
	cloudProvider: {
		provider: 'AWS',
		awsAccountId: '',
		iamRoleArn: '',
		defaultRegion: 'ap-south-1',
	},
	sourceControl: {
		platform: 'GitHub',
		gitHubOrg: '',
		accessToken: '',
		defaultVisibility: 'Private',
		defaultBranchProtection: true,
	},
	techDefaults: {
		environments: ['test', 'accp', 'prod'],
		defaultEnvironment: 'test',
		generateTerraform: true,
		generateCdk: true,
		defaultDbEngine: null,
	},
	connectivity: {
		isChecking: false,
		cloudValid: null,
		sourceControlValid: null,
		lastCheckedAt: null,
		errorMessage: null,
	},
	submit: {
		isSubmitting: false,
		submitError: null,
		submitSuccess: false,
	},
};

export function toEnrollmentRequest(state: EnrollmentState): VerticalEnrollmentRequest {
	return {
		slug: state.identity.slug,
		displayName: state.identity.displayName,
		description: state.identity.description,
		ownerTeam: state.identity.ownerTeam,
		ownerEmail: state.identity.ownerEmail,
		componentPrefix: state.identity.componentPrefix,
		cloudProvider: state.cloudProvider,
		sourceControl: state.sourceControl,
		techDefaults: state.techDefaults,
	};
}
