# Pervaxis Forge — Technical Specification
**For Engineers, Architects, and Technical Leads**

**Version:** 1.1  
**Date:** May 5, 2026  
**Author:** Anand Jayaseelan  
**Company:** Clarivex Technologies  
**Classification:** Confidential

---

## Table of Contents
1. [System Architecture](#1-system-architecture)
2. [Technology Stack](#2-technology-stack)
3. [Vertical Enrollment](#3-vertical-enrollment)
4. [Component Design](#4-component-design)
5. [Infrastructure Provisioning](#5-infrastructure-provisioning)
6. [Secrets Management](#6-secrets-management)
7. [GitHub Integration](#7-github-integration)
8. [Database Schema](#8-database-schema)
9. [Template System](#9-template-system)
10. [Naming Convention Engine](#10-naming-convention-engine)
11. [Security & Access Control](#11-security--access-control)
12. [API Reference](#12-api-reference)
13. [Extension Points](#13-extension-points)

---

## 1. System Architecture

### 1.1 High-Level Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Forge.Launchpad                           │
│                   (Angular 18 Web UI)                        │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Vertical Dashboard  (landing page)                    │  │
│  │  ┌──────────────┐  ┌──────────────────────────────┐   │  │
│  │  │  Enrolled    │  │   + Enroll New Vertical       │   │  │
│  │  │  Verticals   │  │   (5-step wizard)             │   │  │
│  │  └──────────────┘  └──────────────────────────────┘   │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────────────┐  │
│  │  Service Generation Wizard  (vertical selected)        │  │
│  │  ┌────────────┐  ┌────────────┐  ┌──────────────────┐ │  │
│  │  │ Multi-Svc  │  │  Genesis   │  │  Canvas Module   │ │  │
│  │  │  Wizard    │  │  Modules   │  │  Selection       │ │  │
│  │  └────────────┘  └────────────┘  └──────────────────┘ │  │
│  └────────────────────────────────────────────────────────┘  │
└──────────────────┬───────────────────────────────────────────┘
                   │ HTTPS/REST
┌──────────────────▼───────────────────────────────────────────┐
│              Pervaxis.Forge.Api                              │
│              (ASP.NET Core Minimal API)                      │
│                                                              │
│  ┌───────────────┐  ┌──────────────┐  ┌────────────────┐   │
│  │  Vertical     │  │  Generation  │  │  AWS SDK       │   │
│  │  Enrollment   │  │  Endpoints   │  │  Integration   │   │
│  │  Endpoints    │  │              │  │                │   │
│  └───────────────┘  └──────────────┘  └────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  GitHub API Integration                              │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │            Credential Store (PostgreSQL)             │   │
│  │   - Vertical Registry                               │   │
│  │   - AWS Account Registry                            │   │
│  │   - Source Control Registry                         │   │
│  │   - Audit Logs                                      │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────┬───────────────────────────────────────────┘
                   │ Direct Library Reference
┌──────────────────▼───────────────────────────────────────────┐
│           Pervaxis.Forge.Engine                              │
│           (.NET 10 Class Library)                            │
│                                                              │
│  ┌───────────────┐  ┌──────────────┐  ┌─────────────────┐  │
│  │   Manifest    │  │   Naming     │  │   Template      │  │
│  │   Parser &    │  │   Convention │  │   Engine        │  │
│  │   Validator   │  │   Derivation │  │   (Scriban)     │  │
│  └───────────────┘  └──────────────┘  └─────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │         Embedded Template Resources                  │   │
│  │   - rest-api/ (18 .sbn files)                       │   │
│  │   - angular-shell/ (9 .sbn files)                   │   │
│  │   - angular-microfrontend/ (9 .sbn files)           │   │
│  │   - terraform/ (resource templates)                 │   │
│  │   - cdk/ (C# CDK templates)                         │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────────────────────────────────────────────┘

External Integrations:
┌─────────────┐  ┌──────────────┐  ┌─────────────────────────┐
│   AWS SDK   │  │  GitHub API  │  │  AWS Secrets Manager    │
│   (Direct)  │  │  (REST)      │  │  (Runtime secrets)      │
└─────────────┘  └──────────────┘  └─────────────────────────┘
```

### 1.2 Core Principles

1. **Vertical-First**: All service generation happens within a registered vertical. No vertical = no generation. Vertical context (cloud, GitHub, environments) is resolved once at enrollment, not per-generation.

2. **Engine Independence**: `Pervaxis.Forge.Engine` has ZERO dependencies on `Pervaxis.Core` or `Pervaxis.Genesis`. It knows module names as strings only.

3. **Deterministic Generation**: No AI, no randomness. Same manifest.json = identical output, every time.

4. **Polyrepo by Default**: Each generated service = separate GitHub repository under the vertical's GitHub org.

5. **Admin-Only Access**: Forge is an internal platform tool, not exposed to all developers.

6. **Hybrid Infrastructure**: Generate IaC templates AND optionally deploy resources immediately using the vertical's enrolled AWS account.

---

## 2. Technology Stack

### 2.1 Backend (Forge.Engine + Forge.Api)

| Component | Technology | Version | Purpose |
|---|---|---|---|
| Runtime | .NET | 10.0 | Core execution environment |
| Template Engine | Scriban | 5.x | Deterministic text generation |
| API Framework | ASP.NET Core Minimal API | .NET 10 | HTTP endpoints |
| JSON Serialization | System.Text.Json | .NET 10 | manifest.json parsing |
| AWS SDK | AWSSDK.* | Latest | Direct AWS resource creation |
| GitHub API Client | Octokit | Latest | Repository creation, configuration |
| Database | Entity Framework Core | .NET 10 | Credential + vertical store persistence |
| Database Provider | Npgsql.EntityFrameworkCore.PostgreSQL | Latest | PostgreSQL driver |
| Secrets | AWS Secrets Manager SDK | Latest | Runtime secret storage |
| ZIP Packaging | System.IO.Compression | .NET 10 | Print packaging |
| Testing | xUnit + FluentAssertions | Latest | Unit and integration tests |

**Compiler Settings:**
- `<Nullable>enable</Nullable>` — All projects
- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — All projects
- `<LangVersion>latest</LangVersion>` — All projects

### 2.2 Frontend (Forge.Launchpad)

| Component | Technology | Version |
|---|---|---|
| Framework | Angular | 18.x |
| UI Components | Angular Material | 18.x |
| Build System | Nx | 19.x |
| State Management | Angular Signals | 18.x |
| HTTP Client | Angular HttpClient | 18.x |
| Styling | SCSS | - |
| TypeScript | TypeScript | 5.4+ |

**No Ionic, No Pervaxis.Mobile dependency** — Launchpad is desktop-web only.

### 2.3 Infrastructure-as-Code Generation

| Format | Technology | Purpose |
|---|---|---|
| Terraform | HCL | Multi-cloud, industry standard |
| AWS CDK | C# (.NET 10) | Type-safe, .NET team familiarity |

Both formats generated for every print. Teams choose their preference.

---

## 3. Vertical Enrollment

### 3.1 Overview

Vertical Enrollment is the prerequisite for all service generation. A vertical represents a business domain (e.g., Clarivolt, ClariFrost) and owns its cloud account, source control configuration, environment names, and naming namespace.

The Launchpad landing page is the **Vertical Dashboard** — a list of enrolled verticals with a "Enroll New Vertical" entry point. Until at least one vertical is enrolled, service generation is unavailable.

### 3.2 Enrollment Wizard Steps

#### Step 1: Vertical Identity

| Field | Type | Rules | Example |
|---|---|---|---|
| `slug` | string | kebab-case, unique, immutable after enrollment | `clarivolt` |
| `displayName` | string | Free text | `Clarivolt` |
| `description` | string | Free text | `Sales upload and validation platform` |
| `ownerTeam` | string | Free text | `Clarivolt Platform Team` |
| `ownerEmail` | email | Valid email | `team@clarivex.tech` |

`slug` becomes the product prefix for all services generated within this vertical (e.g., `clarivolt/intake-service`).

#### Step 2: Cloud Provider

Provider list displayed as cards. All except AWS are locked (visually present, grayed out with "Coming Soon").

| Provider | Status |
|---|---|
| **AWS** | Available — select this |
| Azure | Coming Soon |
| GCP | Coming Soon |

**AWS-specific fields (shown after selecting AWS):**

| Field | Type | Rules | Example |
|---|---|---|---|
| `awsAccountId` | string | 12-digit numeric | `123456789012` |
| `iamRoleArn` | string | Valid ARN format | `arn:aws:iam::123456789012:role/ForgeDeploymentRole` |
| `defaultRegion` | select | AWS region list | `us-east-1` |

> No raw access keys. Forge assumes the IAM role at generation time. The role must have permissions for the AWS services the vertical intends to use (RDS, S3, SQS, ElastiCache, Secrets Manager).

#### Step 3: Source Control

Platform list displayed as cards. All except GitHub are locked.

| Platform | Status |
|---|---|
| **GitHub** | Available — selected by default |
| GitLab | Coming Soon |
| Azure DevOps | Coming Soon |

**GitHub-specific fields:**

| Field | Type | Rules | Example |
|---|---|---|---|
| `githubOrg` | string | GitHub org slug | `clarivex-tech` |
| `accessToken` | string (secret) | PAT with `repo`, `admin:org` scopes | `ghp_...` |
| `defaultVisibility` | select | Private / Public | `Private` |
| `defaultBranchProtection` | bool | Enable by default | `true` |

Access token is validated on entry (live API call to verify org access).

#### Step 4: Technical Defaults

| Field | Type | Default | Notes |
|---|---|---|---|
| `environments` | string[] | `["test", "accp", "prod"]` | Editable list, order matters |
| `defaultEnvironment` | select | `test` | Used for "Deploy Now" |
| `generateTerraform` | bool | `true` | Include Terraform IaC by default |
| `generateCdk` | bool | `true` | Include CDK IaC by default |
| `defaultDbEngine` | select | PostgreSQL / None | Pre-select for new services |

#### Step 5: Review & Enroll

Summary of all entered details. IAM Role ARN and GitHub token are masked. "Enroll Vertical" button triggers:
1. Validate all fields server-side
2. Verify AWS IAM role reachability (STS AssumeRole dry-run)
3. Verify GitHub org access (Octokit org membership check)
4. Persist to database (vertical + aws_account + source_control records)
5. Redirect to Vertical Dashboard

### 3.3 Post-Enrollment: Vertical Dashboard

The landing page shows all enrolled verticals as cards:

```
┌─────────────────────────────────────────────────────────┐
│  Pervaxis Forge                                         │
│                                                         │
│  ┌─────────────────┐  ┌─────────────────┐  ┌────────┐  │
│  │  Clarivolt      │  │  ClariFrost     │  │   +    │  │
│  │  AWS · GitHub   │  │  AWS · GitHub   │  │  New   │  │
│  │  12 services    │  │  0 services     │  │        │  │
│  │  [Open]         │  │  [Open]         │  │        │  │
│  └─────────────────┘  └─────────────────┘  └────────┘  │
└─────────────────────────────────────────────────────────┘
```

Clicking a vertical opens the **Vertical Workspace** — a scoped view showing that vertical's services and a "Generate New Services" entry point.

### 3.4 API Endpoints — Vertical Enrollment

```
POST   /api/v1/verticals                    Enroll a new vertical
GET    /api/v1/verticals                    List all enrolled verticals
GET    /api/v1/verticals/{slug}             Get vertical details
PUT    /api/v1/verticals/{slug}             Update mutable fields
DELETE /api/v1/verticals/{slug}             Unenroll (soft delete)
POST   /api/v1/verticals/{slug}/validate    Validate cloud + source control connectivity
```

### 3.5 VerticalEnrollmentRequest Model

```csharp
public record VerticalEnrollmentRequest
{
    public required string Slug { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string OwnerTeam { get; init; }
    public required string OwnerEmail { get; init; }

    public required CloudProviderConfig CloudProvider { get; init; }
    public required SourceControlConfig SourceControl { get; init; }
    public required VerticalTechDefaults TechDefaults { get; init; }
}

public record CloudProviderConfig
{
    public required string Provider { get; init; } // "AWS"
    public required string AwsAccountId { get; init; }
    public required string IamRoleArn { get; init; }
    public required string DefaultRegion { get; init; }
}

public record SourceControlConfig
{
    public required string Platform { get; init; } // "GitHub"
    public required string GitHubOrg { get; init; }
    public required string AccessToken { get; init; }
    public required string DefaultVisibility { get; init; }
    public bool DefaultBranchProtection { get; init; }
}

public record VerticalTechDefaults
{
    public List<string> Environments { get; init; } = ["test", "accp", "prod"];
    public string DefaultEnvironment { get; init; } = "test";
    public bool GenerateTerraform { get; init; } = true;
    public bool GenerateCdk { get; init; } = true;
    public string? DefaultDbEngine { get; init; }
}
```

---

## 4. Component Design

### 4.1 Pervaxis.Forge.Engine

**Responsibilities:**
- Parse and validate `manifest.json`
- Derive all naming conventions from product + name
- Load embedded Scriban templates
- Execute template expansion with normalized model
- Package output as ZIP binary

**Key Classes:**

```csharp
namespace Pervaxis.Forge.Engine.Manifest;

public record ForgeManifest
{
    public required string Product { get; init; }
    public required string Name { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string Version { get; init; }
    public required ServiceType Type { get; init; }
    public List<string> GenesisModules { get; init; } = new();
    public DatabaseConfig? Database { get; init; }
    public List<QueueConfig> Queues { get; init; } = new();
    public ApiConfig? Api { get; init; }
    public AngularConfig? Angular { get; init; }
    public ManifestMetadata Metadata { get; init; } = new();
}

public enum ServiceType
{
    RestApi,
    Grpc,
    Worker,
    AngularShell,
    AngularMicrofrontend
}
```

```csharp
namespace Pervaxis.Forge.Engine.Naming;

public static class NamingConvention
{
    public static DerivedNames DeriveAllNames(
        string product,
        string name,
        ServiceType type)
    {
        return type switch
        {
            ServiceType.RestApi => DeriveDotNetNames(product, name),
            ServiceType.AngularShell => DeriveAngularShellNames(product, name),
            ServiceType.AngularMicrofrontend => DeriveAngularMfeNames(product, name),
            _ => throw new NotImplementedException($"Type {type} not implemented")
        };
    }

    private static DerivedNames DeriveDotNetNames(string product, string name)
    {
        var productPascal = ToPascalCase(product);
        var namePascal = ToPascalCase(name);
        var nameStripped = StripServiceSuffix(name);
        var firstSegment = GetFirstSegment(name);

        return new DerivedNames
        {
            Namespace = $"{productPascal}.{namePascal}",
            ProjectFile = $"{productPascal}.{namePascal}.csproj",
            SolutionFile = $"{productPascal}.{namePascal}.slnx",
            TestProject = $"{productPascal}.{namePascal}.Tests",
            DockerImage = $"{product}/{name}",
            EcsTaskName = $"{product}-{name}",
            ApiBaseRoute = $"/api/v1/{nameStripped}",
            SqsPrefix = $"{product}.{firstSegment}",
            CachePrefix = $"{product}:{firstSegment}:",
            DatabaseSchema = firstSegment,
            FolderName = name,
            GitHubRepoPath = $"services/{name}/"
        };
    }
}
```

```csharp
namespace Pervaxis.Forge.Engine.Generation;

public class PrintGenerator
{
    private readonly ITemplateEngine _templateEngine;
    private readonly IFileGenerator _fileGenerator;
    private readonly IZipPackager _zipPackager;

    public async Task<byte[]> GenerateAsync(
        ForgeManifest manifest,
        string cloudProvider,
        CancellationToken cancellationToken = default)
    {
        var model = TemplateModelBuilder.Build(manifest, cloudProvider);
        var names = NamingConvention.DeriveAllNames(
            manifest.Product, manifest.Name, manifest.Type);
        model.Names = names;

        var templates = await LoadTemplatesAsync(manifest.Type);
        var files = new Dictionary<string, string>();

        foreach (var template in templates)
        {
            var content = await _templateEngine.RenderAsync(template, model);
            var path = ResolvePath(template.Name, names);
            files[path] = content;
        }

        return await _zipPackager.PackageAsync(files, cancellationToken);
    }
}
```

**No external dependencies.** Engine is pure .NET 10 + Scriban.

### 4.2 Pervaxis.Forge.Api

**Responsibilities:**
- Expose HTTP endpoints for vertical enrollment, generation, validation, metadata
- Orchestrate AWS resource creation via SDK using the vertical's enrolled IAM role
- Orchestrate GitHub repository creation via Octokit using the vertical's enrolled token
- Store verticals, AWS credentials, and audit logs in PostgreSQL
- Return generated ZIP or deployment results

**Key Endpoints:**

```csharp
// POST /api/v1/verticals — enroll a new vertical
// POST /api/v1/generate — generate single service (verticalSlug in request)
// POST /api/v1/generate/batch — generate multiple services within a vertical
// POST /api/v1/validate — validate manifest without generating
// GET  /api/v1/modules — list Genesis modules
// GET  /api/v1/canvas-modules — list Canvas modules
// GET  /api/v1/verticals — list all verticals
```

When generating, the API resolves the vertical's AWS account and GitHub token from the database — the caller only provides `verticalSlug`.

### 4.3 Pervaxis.Forge.Launchpad

**Responsibilities:**
- Vertical Dashboard (landing page with enrolled verticals)
- Vertical Enrollment wizard (5 steps)
- Vertical Workspace (scoped service generation per vertical)
- Multi-service generation wizard (6 steps, within selected vertical)
- Real-time naming preview

**Navigation Flow:**

```
/                         → Vertical Dashboard
/verticals/enroll         → Enrollment Wizard
/verticals/:slug          → Vertical Workspace
/verticals/:slug/generate → Service Generation Wizard
```

---

## 5. Infrastructure Provisioning

### 5.1 Dual-Output Strategy

For every service that selects AWS modules, Forge generates:

1. **Terraform Templates** (`.tf` files)
2. **AWS CDK Code** (C# `.cs` files)
3. **Optionally: Direct AWS Deployment** (via SDK, using vertical's IAM role)

> **V2 Roadmap — Forge-Managed Infrastructure:** In V2, Forge becomes the exclusive provisioning path for all cloud resources. No developer creates resources via the AWS console or runs `terraform apply` manually. Forge executes the full Terraform lifecycle internally and enforces this via IAM guardrails scoped to the vertical's role. See [Section 13.4 — Forge-Managed Infrastructure (V2)](#134-forge-managed-infrastructure-v2) for the full design.

### 5.2 Resource Naming Convention

```
{environment}-{product}-{service}-{resource-type}

Examples:
  test-clarivolt-intake-cache           (ElastiCache)
  test-clarivolt-intake-db              (RDS PostgreSQL)
  test-clarivolt-intake-files           (S3 Bucket)
  test-clarivolt-intake-queue-submitted (SQS Queue)
```

`product` comes from the vertical slug. `environment` comes from the vertical's enrolled environment list.

### 5.3 Generated Terraform Structure

```hcl
# infrastructure/terraform/main.tf

variable "environment" {
  description = "Environment name (test, accp, prod)"
  type        = string
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "us-east-1"
}

# ElastiCache Redis (if Caching.AWS selected)
resource "aws_elasticache_cluster" "cache" {
  cluster_id           = "${var.environment}-clarivolt-intake-cache"
  engine               = "redis"
  node_type            = "cache.t3.micro"
  num_cache_nodes      = 1
  parameter_group_name = "default.redis7"
  engine_version       = "7.0"
  port                 = 6379
}

# RDS PostgreSQL (if Database.postgresql specified)
resource "aws_db_instance" "db" {
  identifier             = "${var.environment}-clarivolt-intake-db"
  engine                 = "postgres"
  engine_version         = "16.1"
  instance_class         = "db.t3.micro"
  allocated_storage      = 20
  db_name                = "intake"
  username               = "postgres"
  password               = random_password.db_password.result
  skip_final_snapshot    = true
}

# S3 Bucket (if FileStorage.AWS selected)
resource "aws_s3_bucket" "files" {
  bucket = "${var.environment}-clarivolt-intake-files"
}

# SQS Queues (from manifest.queues)
resource "aws_sqs_queue" "intake_submitted" {
  name                      = "${var.environment}-clarivolt-intake-submitted"
  delay_seconds             = 0
  max_message_size          = 262144
  message_retention_seconds = 345600
  receive_wait_time_seconds = 0
}
```

### 5.4 Generated AWS CDK Structure

```csharp
// infrastructure/cdk/Program.cs

var app = new App();
new IntakeServiceStack(app, "IntakeServiceStack", new StackProps
{
    Env = new Amazon.CDK.Environment
    {
        Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
        Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
    }
});
app.Synth();
```

### 5.5 Direct AWS Deployment (Deploy Now)

Uses the vertical's enrolled IAM role (assumed via STS) — no per-request credentials needed:

```csharp
public async Task<DeploymentResult> DeployAsync(
    ForgeManifest manifest,
    string verticalSlug,
    string environment)
{
    var vertical = await _verticalService.GetAsync(verticalSlug);
    var awsCredentials = await AssumeRoleAsync(vertical.IamRoleArn);

    // Deploy resources using assumed role credentials
    // Store connection strings in Secrets Manager under vertical's account
}
```

---

## 6. Secrets Management

### 6.1 Secret Types & Storage

| Secret Type | Storage Location | Access Pattern |
|---|---|---|
| **Runtime Secrets** (DB connections, API keys) | AWS Secrets Manager (vertical's account) | Application reads at startup |
| **CI/CD Credentials** (AWS deploy keys, Docker tokens) | GitHub Secrets (vertical's repo) | GitHub Actions workflows only |
| **Vertical Credentials** (IAM Role ARN, GitHub token) | Forge PostgreSQL Database | Encrypted at rest, API layer only |

### 6.2 Runtime Secret Path

```
{environment}/{vertical-slug}/{service-name}/config

Example:
  test/clarivolt/intake-service/config
```

### 6.3 GitHub Secrets

Forge creates GitHub Secrets scoped to the vertical's GitHub org:

```csharp
var secrets = new Dictionary<string, string>
{
    ["AWS_ACCOUNT_ID"] = vertical.AwsAccountId,
    ["AWS_REGION"] = vertical.DefaultRegion,
    ["AWS_ROLE_ARN"] = vertical.IamRoleArn,
    ["ENVIRONMENT"] = environment
};
```

---

## 7. GitHub Integration

### 7.1 Repository Creation

Uses the vertical's enrolled GitHub token and org:

```csharp
public async Task<Repository> CreateRepositoryAsync(
    Vertical vertical,
    ForgeManifest manifest,
    byte[] zipContent)
{
    var repo = await _gitHubClient.Repository.Create(
        vertical.GitHubOrg,
        new NewRepository(manifest.Name)
        {
            Description = manifest.Description,
            Private = vertical.DefaultVisibility == "Private",
            AutoInit = false
        });

    await ConfigureBranchProtectionAsync(repo.FullName, vertical);
    await ConfigureGitHubSecretsAsync(repo.FullName, vertical, manifest);
    await PushInitialCommitAsync(repo.FullName, zipContent, manifest);

    return repo;
}
```

### 7.2 Generated GitHub Actions Workflow

```yaml
name: Build and Test

on:
  pull_request:
    branches: [ main, develop ]
  push:
    branches: [ main, develop ]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore --configuration Release
    - name: Test
      run: dotnet test --no-build --configuration Release --verbosity normal --collect:"XPlat Code Coverage"
```

---

## 8. Database Schema

### 8.1 Forge Credential & Vertical Store (PostgreSQL)

```sql
-- Verticals (business domains)
CREATE TABLE verticals (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    slug VARCHAR(100) NOT NULL UNIQUE,
    display_name VARCHAR(255) NOT NULL,
    description TEXT,
    owner_team VARCHAR(255) NOT NULL,
    owner_email VARCHAR(255) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Cloud provider configuration per vertical
CREATE TABLE vertical_cloud_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vertical_id UUID NOT NULL REFERENCES verticals(id) ON DELETE CASCADE,
    provider VARCHAR(50) NOT NULL DEFAULT 'AWS',
    aws_account_id VARCHAR(12),
    iam_role_arn TEXT,              -- encrypted at rest
    default_region VARCHAR(50) NOT NULL DEFAULT 'us-east-1',
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (vertical_id, provider)
);

-- Source control configuration per vertical
CREATE TABLE vertical_source_control_configs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vertical_id UUID NOT NULL REFERENCES verticals(id) ON DELETE CASCADE,
    platform VARCHAR(50) NOT NULL DEFAULT 'GitHub',
    github_org VARCHAR(255),
    access_token TEXT,              -- encrypted at rest
    default_visibility VARCHAR(20) NOT NULL DEFAULT 'Private',
    default_branch_protection BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (vertical_id, platform)
);

-- Technical defaults per vertical
CREATE TABLE vertical_tech_defaults (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vertical_id UUID NOT NULL REFERENCES verticals(id) ON DELETE CASCADE UNIQUE,
    environments TEXT[] NOT NULL DEFAULT '{test,accp,prod}',
    default_environment VARCHAR(50) NOT NULL DEFAULT 'test',
    generate_terraform BOOLEAN NOT NULL DEFAULT true,
    generate_cdk BOOLEAN NOT NULL DEFAULT true,
    default_db_engine VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Generation audit log
CREATE TABLE generation_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    vertical_id UUID NOT NULL REFERENCES verticals(id),
    manifest JSONB NOT NULL,
    service_count INT NOT NULL,
    infrastructure_deployed BOOLEAN NOT NULL DEFAULT false,
    github_repos_created BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Per-service deployment outputs
CREATE TABLE deployment_outputs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    generation_log_id UUID NOT NULL REFERENCES generation_logs(id) ON DELETE CASCADE,
    service_name VARCHAR(255) NOT NULL,
    resource_type VARCHAR(100) NOT NULL,
    resource_name VARCHAR(500) NOT NULL,
    endpoint_or_arn TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_verticals_slug ON verticals(slug);
CREATE INDEX idx_generation_logs_vertical ON generation_logs(vertical_id);
CREATE INDEX idx_generation_logs_created_at ON generation_logs(created_at DESC);
CREATE INDEX idx_deployment_outputs_generation ON deployment_outputs(generation_log_id);
```

### 8.2 Entity Framework Core Models

```csharp
public class Vertical
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public required string OwnerTeam { get; set; }
    public required string OwnerEmail { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public VerticalCloudConfig? CloudConfig { get; set; }
    public VerticalSourceControlConfig? SourceControlConfig { get; set; }
    public VerticalTechDefaults? TechDefaults { get; set; }
    public ICollection<GenerationLog> GenerationLogs { get; set; } = new List<GenerationLog>();
}

public class VerticalCloudConfig
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public required string Provider { get; set; }
    public string? AwsAccountId { get; set; }
    public string? IamRoleArn { get; set; }    // stored encrypted
    public required string DefaultRegion { get; set; }

    public Vertical Vertical { get; set; } = null!;
}

public class VerticalSourceControlConfig
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public required string Platform { get; set; }
    public string? GitHubOrg { get; set; }
    public string? AccessToken { get; set; }   // stored encrypted
    public required string DefaultVisibility { get; set; }
    public bool DefaultBranchProtection { get; set; }

    public Vertical Vertical { get; set; } = null!;
}
```

---

## 9. Template System

### 9.1 Scriban Engine Wrapper

```csharp
public class TemplateEngine : ITemplateEngine
{
    public async Task<string> RenderAsync(string templateContent, object model)
    {
        var template = Template.Parse(templateContent, "template");

        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            throw new TemplateException($"Template parsing failed:\n{errors}");
        }

        var context = new TemplateContext
        {
            StrictVariables = true,
            MemberRenamer = member => member.Name
        };

        context.PushGlobal(new ScriptObject { { "model", model } });

        return await template.RenderAsync(context);
    }
}
```

### 9.2 Template Model Structure

```csharp
public class TemplateModel
{
    public required ForgeManifest Manifest { get; init; }
    public required DerivedNames Names { get; init; }
    public required string CloudProvider { get; init; }  // "AWS" | "Azure" | "GCP" — from the vertical
    public required List<GenesisModuleMetadata> SelectedModules { get; init; }
    public required List<CanvasModuleMetadata> SelectedCanvasModules { get; init; }
    public DatabaseConfig? Database { get; init; }
    public List<QueueConfig> Queues { get; init; } = new();
    public string CurrentYear { get; init; } = DateTime.UtcNow.Year.ToString();
}

// TemplateModelBuilder.Build resolves cloud-specific package names using GenesisModules:
//   GenesisModuleMetadata.PackageName = GenesisModules.GetPackageName(module, cloudProvider)
//   GenesisModuleMetadata.DiExtensionName = GenesisModules.GetDiExtensionName(module, cloudProvider)
```

### 9.3 Sample Template: `Program.cs.sbn`

`model.cloud_provider` is the vertical's enrolled cloud (`"AWS"`, `"Azure"`, `"GCP"`).
`module.package_name` and `module.di_extension_name` are pre-resolved by `TemplateModelBuilder`
using `GenesisModules.GetPackageName` / `GetDiExtensionName` so the template stays declarative.

```scriban
// Auto-generated by Pervaxis.Forge
// Product: {{ model.manifest.product }}
// Service: {{ model.manifest.name }}
// Cloud:   {{ model.cloud_provider }}
// Generated: {{ date.now }}

using {{ model.names.namespace }};
using {{ model.names.namespace }}.Extensions;
{{ for module in model.selected_modules }}
using {{ module.package_name }};
{{ end }}

var builder = WebApplication.CreateBuilder(args);

builder.Services.Add{{ model.names.pascal }}Service(builder.Configuration);

{{ for module in model.selected_modules }}
builder.Services.{{ module.di_extension_name }}(
    builder.Configuration.GetSection("{{ module.name }}"));
{{ end }}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### 9.4 Template Location

```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\rest-api\**\*.sbn" />
  <EmbeddedResource Include="Templates\angular-shell\**\*.sbn" />
  <EmbeddedResource Include="Templates\angular-microfrontend\**\*.sbn" />
  <EmbeddedResource Include="Templates\terraform\**\*.sbn" />
  <EmbeddedResource Include="Templates\cdk\**\*.sbn" />
</ItemGroup>
```

---

## 10. Naming Convention Engine

### 10.1 Transformation Functions

```csharp
public static class NamingConvention
{
    /// "intake-service" → "IntakeService"
    public static string ToPascalCase(string kebab)
    {
        return string.Join("", kebab.Split('-')
            .Select(segment => char.ToUpper(segment[0]) + segment[1..]));
    }

    /// "intake-service" → "intake"
    public static string StripServiceSuffix(string name)
    {
        return name.EndsWith("-service") ? name[..^8] : name;
    }

    /// "intake-service" → "intake"
    public static string GetFirstSegment(string name)
    {
        var index = name.IndexOf('-');
        return index == -1 ? name : name[..index];
    }

    /// "clarivolt" → "clv"
    public static string GetComponentPrefix(string product)
    {
        return product.Length >= 3 ? product[..3].ToLower() : product.ToLower();
    }
}
```

### 10.2 Validation Rules

```csharp
public class ManifestValidator
{
    private static readonly Regex KebabCaseRegex =
        new(@"^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public ValidationResult Validate(ForgeManifest manifest)
    {
        var errors = new List<string>();

        if (!KebabCaseRegex.IsMatch(manifest.Product))
            errors.Add("Product must be kebab-case (lowercase, hyphens only)");

        if (!KebabCaseRegex.IsMatch(manifest.Name))
            errors.Add("Name must be kebab-case (lowercase, hyphens only)");

        if (manifest.Type is ServiceType.RestApi or ServiceType.Grpc or ServiceType.Worker)
        {
            if (!manifest.Name.EndsWith("-service"))
                errors.Add(".NET services must have names ending with '-service'");
        }

        if (manifest.Type == ServiceType.AngularMicrofrontend)
        {
            if (manifest.Name.EndsWith("-service"))
                errors.Add("Angular microfrontends cannot have names ending with '-service'");
        }

        var validModules = GenesisModules.GetAllNames();
        var invalidModules = manifest.GenesisModules
            .Where(m => !validModules.Contains(m))
            .ToList();

        if (invalidModules.Any())
            errors.Add($"Invalid Genesis modules: {string.Join(", ", invalidModules)}");

        return new ValidationResult { IsValid = errors.Count == 0, Errors = errors };
    }
}
```

---

## 11. Security & Access Control

### 11.1 Admin-Only Access

```typescript
export const forgeAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);

  if (!authService.isAuthenticated())
    return authService.login(state.url);

  if (!authService.hasRole('forge-admin'))
    return inject(Router).createUrlTree(['/unauthorized']);

  return true;
};
```

**Roles:**
- `forge-admin` — Full access: enroll verticals, generate services, deploy infrastructure
- `forge-viewer` — Read-only: view enrolled verticals and generation logs

### 11.2 Credential Encryption

IAM Role ARNs and GitHub tokens stored in PostgreSQL are encrypted using ASP.NET Core Data Protection:

```csharp
public class VerticalService
{
    private readonly IDataProtector _protector;

    public VerticalService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("VerticalCredentials");
    }

    public async Task<Vertical> EnrollAsync(VerticalEnrollmentRequest request)
    {
        var cloudConfig = new VerticalCloudConfig
        {
            Provider = request.CloudProvider.Provider,
            AwsAccountId = request.CloudProvider.AwsAccountId,
            IamRoleArn = _protector.Protect(request.CloudProvider.IamRoleArn),
            DefaultRegion = request.CloudProvider.DefaultRegion
        };

        var sourceControlConfig = new VerticalSourceControlConfig
        {
            Platform = request.SourceControl.Platform,
            GitHubOrg = request.SourceControl.GitHubOrg,
            AccessToken = _protector.Protect(request.SourceControl.AccessToken),
            DefaultVisibility = request.SourceControl.DefaultVisibility,
            DefaultBranchProtection = request.SourceControl.DefaultBranchProtection
        };

        // persist vertical + configs
    }
}
```

### 11.3 Audit Logging

Every generation is logged to `generation_logs` with the vertical ID, manifest snapshot, and operator identity.

---

## 12. API Reference

### 12.1 Vertical Enrollment Endpoints

#### POST /api/v1/verticals
Enroll a new vertical.

**Request:** `VerticalEnrollmentRequest` (see Section 3.5)

**Response 201 Created:**
```json
{
  "id": "uuid",
  "slug": "clarivolt",
  "displayName": "Clarivolt",
  "cloudProvider": "AWS",
  "sourceControl": "GitHub",
  "githubOrg": "clarivex-tech",
  "environments": ["test", "accp", "prod"],
  "enrolledAt": "2026-05-06T09:00:00Z"
}
```

#### POST /api/v1/verticals/{slug}/validate
Validate cloud and source control connectivity without enrolling.

**Response 200 OK:**
```json
{
  "awsConnectivity": { "success": true, "accountId": "123456789012" },
  "githubConnectivity": { "success": true, "org": "clarivex-tech" }
}
```

---

### 12.2 Service Generation Endpoints

#### POST /api/v1/generate
Generate a single service within an enrolled vertical.

**Request:**
```json
{
  "verticalSlug": "clarivolt",
  "name": "intake-service",
  "displayName": "Intake Service",
  "description": "Accepts daily sales uploads",
  "version": "1.0.0",
  "type": "rest-api",
  "genesisModules": ["FileStorage", "Messaging"],
  "database": { "engine": "postgresql", "schema": "intake" },
  "queues": [{ "name": "clarivolt.intake.submitted", "role": "publish" }],
  "metadata": {
    "deployInfrastructure": true,
    "pushToGitHub": true,
    "environment": "test"
  }
}
```

**Response 200 OK:**
```
Content-Type: application/zip
Content-Disposition: attachment; filename="intake-service.zip"
```

#### POST /api/v1/generate/batch
Generate multiple services within a vertical.

**Request:**
```json
{
  "verticalSlug": "clarivolt",
  "services": [
    { "name": "intake-service", ... },
    { "name": "validation-service", ... }
  ]
}
```

---

### 12.3 Module Metadata Endpoints

```
GET /api/v1/modules          List all Genesis modules
GET /api/v1/canvas-modules   List all Canvas modules
```

---

## 13. Extension Points

### 13.1 New Cloud Providers

The template system is already cloud-agnostic. `TemplateModel.CloudProvider` flows from the vertical into every `.sbn` file — package names and DI extension names resolve automatically via `GenesisModules.GetPackageName` / `GetDiExtensionName`.

To add Azure:
1. Add `azure` option to enrollment wizard (remove "Coming Soon")
2. Implement `AzureDeploymentService : IDeploymentService` for direct provisioning
3. Add `AzureAccountConfig` fields to `VerticalCloudConfig` (subscription ID, tenant ID, managed identity)
4. Add Azure-specific Terraform/CDK templates under `Templates/terraform/` and `Templates/cdk/`
5. Publish `Pervaxis.Genesis.*.Azure` NuGet packages (Genesis repo work, not Forge)

No changes to `PrintGenerator`, `TemplateModel`, or any `.sbn` template — the `{{ model.cloud_provider }}` substitution handles it automatically.

### 13.2 New Source Control Platforms

To add GitLab:
1. Add `gitlab` option to enrollment wizard
2. Implement `GitLabService : ISourceControlService`
3. Add `GitLabConfig` to `VerticalSourceControlConfig`

### 13.3 New Service Types

To add `graphql` generation type:
1. Create `Templates/graphql/` folder with `.sbn` files
2. Add `ServiceType.Graphql` enum value
3. Implement `DeriveGraphqlNames()` in `NamingConvention`
4. Register in `PrintGenerator.LoadTemplatesAsync()`

### 13.4 Forge-Managed Infrastructure (V2)

**Status:** Planned — not in V1 scope

**Goal:** Forge becomes the single control plane for all cloud resource provisioning across every vertical. No developer creates, modifies, or destroys resources via the cloud console or by running IaC tooling manually. Infrastructure consistency and policy compliance are enforced structurally, not by convention.

#### Architecture

```
Forge API  →  Terraform Execution Engine  →  AWS / Azure / GCP
                     ↑
         State stored in Forge-managed S3 bucket
         Locks managed via DynamoDB
         Audit written to generation_logs
```

#### New API Endpoints

```
POST   /api/v1/infrastructure/apply                          Apply IaC for a service + environment
POST   /api/v1/infrastructure/destroy                        Tear down resources for a service + environment
GET    /api/v1/infrastructure/{vertical}/{service}/{env}     Current resource state + endpoints
GET    /api/v1/infrastructure/{vertical}/{service}/{env}/drift  Plan diff — declared vs. actual
```

#### Terraform Module Strategy (Decision Pending)

Generated Terraform templates will reference reusable modules rather than raw provider resources. The `terraform-aws-modules` community registry is the leading candidate (covers ElastiCache, RDS, S3, SQS, VPC, IAM). Final decision — community modules vs. internal custom modules — to be made before Phase 4 begins, weighing:

- Security posture and supply-chain risk of community modules
- Coverage of Pervaxis-specific resource configurations
- Versioning and upgrade cadence

#### IAM Guardrails

To make Forge the exclusive provisioning path, the vertical's `ForgeDeploymentRole` will be constrained by a `ForgeResourcePolicy`:

- Denies `Create*` / `Delete*` actions on AWS services unless the STS session carries the `forge:managed=true` request tag
- Forge sets this tag on every `AssumeRole` call
- Policy template generated as `iam-policy.tf.sbn` at enrollment time; applied to the vertical's AWS account by Cloud Ops
- Once applied, console-based resource creation is blocked for that vertical's account

#### Multi-Cloud

The same Terraform execution engine supports Azure and GCP by swapping providers — the `cloudProvider` field already flows through `TemplateModel` from V1. Terraform module candidates per cloud:

| Cloud | Provider | Module Candidate |
|---|---|---|
| AWS | `hashicorp/aws` | `terraform-aws-modules/*` (TBD) |
| Azure | `hashicorp/azurerm` | `Azure/` registry modules (TBD) |
| GCP | `hashicorp/google` | `terraform-google-modules/*` (TBD) |

#### Prerequisites Before Scheduling

1. Terraform module decision (community vs. internal)
2. Terraform binary strategy (bundled CLI vs. sidecar container vs. Terraform Cloud API)
3. S3 state bucket + DynamoDB lock table provisioned per Forge environment
4. IAM policy template reviewed by Cloud Ops and Security
5. V2 UI scope defined for infrastructure management screens

---

## Appendix: Technology Decisions

### Why Scriban over Razor?
- No runtime compilation — faster, lower memory
- Simpler — text templates, not C# code
- Portable — no ASP.NET dependency

### Why Vertical-First Architecture?
- Scales cleanly to N verticals without credential re-entry
- Isolates cloud accounts per business domain (security boundary)
- Enables per-vertical audit trails

### Why Both Terraform + CDK?
- Terraform — Multi-cloud, industry standard, declarative HCL
- CDK — Type-safe C#, compile-time checks, .NET team familiarity

### Why PostgreSQL for Credential Store?
- Relational integrity — verticals → cloud configs → generation logs
- JSONB support — flexible manifest storage
- EF Core — first-class .NET integration

### Why Polyrepo over Monorepo?
- Independent deployment — each service has own CI/CD
- Clear ownership — one team, one repo
- Reduced blast radius — changes isolated to single service

---

**End of Technical Specification**

*Pervaxis Forge v1.1 — Clarivex Technologies © 2026*
