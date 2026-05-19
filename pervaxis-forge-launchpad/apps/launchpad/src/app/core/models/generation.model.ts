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

export type GenerationStatus = 'pending' | 'in-progress' | 'success' | 'failed' | 'partial-failure';

export interface GenesisModule {
	name: string;
	label: string;
	description: string;
}

export interface CanvasModule {
	id: string;
	name: string;
	label: string;
}

export interface GenerationDatabaseConfig {
	engine: string;
	schema: string;
}

export interface GenerationQueueConfig {
	name: string;
	role: 'publish' | 'subscribe';
}

export interface GenerationMetadata {
	deployInfrastructure: boolean;
	pushToGitHub: boolean;
	environment: string;
}

export interface EnterpriseScaffoldOptions {
	authenticationEnabled?: boolean;
	secretsManagementEnabled?: boolean;
	resilienceEnabled?: boolean;
	rateLimitingEnabled?: boolean;
	auditLoggingEnabled?: boolean;
	piiClassificationEnabled?: boolean;
	outputCachingEnabled?: boolean;
}

export interface GenerationRequest {
	verticalSlug: string;
	name: string;
	displayName: string;
	description: string;
	version: string;
	type: 'RestApi' | 'GraphQL' | 'Grpc' | 'AngularShell' | 'AngularMfe';
	genesisModules: string[];
	canvasModules?: string[];
	database: GenerationDatabaseConfig | null;
	createGitHubRepo: boolean;
	enterprise?: EnterpriseScaffoldOptions;
}

export interface ValidationPreviewResult {
	isValid: boolean;
	errors: string[];
	serviceName: string | null;
	namespace: string | null;
	projectName: string | null;
}

export interface GenerationExecutionResult {
	zipBlob: Blob;
	fileName: string;
	gitHubRepoUrl: string | null;
	generatedServiceName: string | null;
	generatedVertical: string | null;
	generationTimestamp: string | null;
}

export interface ServiceGenerationRequest {
	name: string;
	type: 'BFF' | 'MFE';
	modules?: string[];
	databaseEngine?: string | null;
	queues?: QueueConfig[];
}

export interface QueueConfig {
	name: string;
	role: 'publish' | 'subscribe';
}

export interface BatchGenerationRequest {
	verticalSlug: string;
	services: ServiceGenerationRequest[];
	deployInfrastructure: boolean;
	environment: string;
	generateTerraform: boolean;
	generateCdk: boolean;
	createGitHubRepos: boolean;
	githubVisibility: string;
	githubBranchProtection: boolean;
	githubSecrets: boolean;
}

export interface ServiceGenerationResult {
	name: string;
	status: 'success' | 'failed';
	namespace: string;
	dockerImage: string;
	apiRoute: string;
	githubRepo: string;
	gitCloneUrl: string;
	schemaName?: string;
	zipUrl?: string;
	errorMessage?: string;
}

export interface GenerationAuditEntry {
	id: string;
	verticalSlug: string;
	timestamp: string;
	operator: string;
	status: GenerationStatus;
	serviceCount: number;
	serviceNames: string[];
	results: ServiceGenerationResult[];
}

export interface RecentGenerationsResponse {
	generations: GenerationAuditEntry[];
}
