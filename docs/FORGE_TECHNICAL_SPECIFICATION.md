# Pervaxis Forge — Technical Specification
**For Engineers, Architects, and Technical Leads**

**Version:** 1.0  
**Date:** May 4, 2026  
**Author:** Anand Jayaseelan  
**Company:** Clarivex Technologies  
**Classification:** Confidential

---

## Table of Contents
1. [System Architecture](#1-system-architecture)
2. [Technology Stack](#2-technology-stack)
3. [Component Design](#3-component-design)
4. [Infrastructure Provisioning](#4-infrastructure-provisioning)
5. [Secrets Management](#5-secrets-management)
6. [GitHub Integration](#6-github-integration)
7. [Database Schema](#7-database-schema)
8. [Template System](#8-template-system)
9. [Naming Convention Engine](#9-naming-convention-engine)
10. [Security & Access Control](#10-security--access-control)
11. [API Reference](#11-api-reference)
12. [Extension Points](#12-extension-points)

---

## 1. System Architecture

### 1.1 High-Level Architecture

```
┌──────────────────────────────────────────────────────────────┐
│                    Forge.Launchpad                           │
│                   (Angular 18 Web UI)                        │
│                                                              │
│  ┌────────────┐  ┌────────────┐  ┌──────────────────────┐  │
│  │ Multi-Svc  │  │  Genesis   │  │  Canvas Module       │  │
│  │  Wizard    │  │  Module    │  │  Selection           │  │
│  │            │  │  Selection │  │                      │  │
│  └────────────┘  └────────────┘  └──────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │     Infrastructure Deployment Controls               │  │
│  │     (Deploy Now / Generate IaC Templates)            │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────┬───────────────────────────────────────────┘
                   │ HTTPS/REST
┌──────────────────▼───────────────────────────────────────────┐
│              Pervaxis.Forge.Api                              │
│              (ASP.NET Core Minimal API)                      │
│                                                              │
│  ┌────────────────┐  ┌──────────────┐  ┌────────────────┐  │
│  │   Generation   │  │  AWS SDK     │  │   GitHub API   │  │
│  │   Endpoints    │  │  Integration │  │   Integration  │  │
│  └────────────────┘  └──────────────┘  └────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │            Credential Store (PostgreSQL)             │  │
│  │   - AWS Account Registry                            │  │
│  │   - Organization Mappings                           │  │
│  │   - Audit Logs                                      │  │
│  └──────────────────────────────────────────────────────┘  │
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
│  ┌──────────────────────────────────────────────────────┐  │
│  │         Embedded Template Resources                  │  │
│  │   - rest-api/ (18 .sbn files)                       │  │
│  │   - angular-shell/ (9 .sbn files)                   │  │
│  │   - angular-microfrontend/ (9 .sbn files)           │  │
│  │   - terraform/ (resource templates)                 │  │
│  │   - cdk/ (C# CDK templates)                         │  │
│  └──────────────────────────────────────────────────────┘  │
└──────────────────────────────────────────────────────────────┘

External Integrations:
┌─────────────┐  ┌──────────────┐  ┌─────────────────────────┐
│   AWS SDK   │  │  GitHub API  │  │  AWS Secrets Manager    │
│   (Direct)  │  │  (REST)      │  │  (Runtime secrets)      │
└─────────────┘  └──────────────┘  └─────────────────────────┘
```

### 1.2 Core Principles

1. **Engine Independence**: `Pervaxis.Forge.Engine` has ZERO dependencies on `Pervaxis.Core` or `Pervaxis.Genesis`. It knows module names as strings only.

2. **Deterministic Generation**: No AI, no randomness. Same manifest.json = identical output, every time.

3. **Polyrepo by Default**: Each generated service = separate GitHub repository.

4. **Admin-Only Access**: Forge is an internal platform tool, not exposed to all developers.

5. **Hybrid Infrastructure**: Generate IaC templates AND optionally deploy resources immediately.

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
| Database | Entity Framework Core | .NET 10 | Credential store persistence |
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

## 3. Component Design

### 3.1 Pervaxis.Forge.Engine

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

    // ... Angular naming methods
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
        CancellationToken cancellationToken = default)
    {
        // 1. Normalize manifest (set defaults)
        var model = TemplateModelBuilder.Build(manifest);

        // 2. Derive all names
        var names = NamingConvention.DeriveAllNames(
            manifest.Product, 
            manifest.Name, 
            manifest.Type);

        model.Names = names;

        // 3. Load templates for service type
        var templates = await LoadTemplatesAsync(manifest.Type);

        // 4. Generate all files
        var files = new Dictionary<string, string>();
        foreach (var template in templates)
        {
            var content = await _templateEngine.RenderAsync(template, model);
            var path = ResolvePath(template.Name, names);
            files[path] = content;
        }

        // 5. Package as ZIP
        return await _zipPackager.PackageAsync(files, cancellationToken);
    }
}
```

**No external dependencies.** Engine is pure .NET 10 + Scriban.

### 3.2 Pervaxis.Forge.Api

**Responsibilities:**
- Expose HTTP endpoints for generation, validation, metadata
- Orchestrate AWS resource creation via SDK
- Orchestrate GitHub repository creation via Octokit
- Store AWS credentials and audit logs in PostgreSQL
- Return generated ZIP or deployment results

**Key Endpoints:**

```csharp
// POST /api/v1/generate
app.MapPost("/api/v1/generate", async (
    ForgeManifest manifest,
    PrintGenerator generator,
    IAwsDeploymentService? awsService,
    IGitHubService? gitHubService) =>
{
    // 1. Generate print
    var zipBytes = await generator.GenerateAsync(manifest);

    // 2. If deploy requested, create AWS resources
    if (manifest.Metadata.DeployInfrastructure)
    {
        var awsResources = await awsService!.DeployAsync(manifest);
        // Store connection strings in AWS Secrets Manager
    }

    // 3. If GitHub push requested, create repos
    if (manifest.Metadata.PushToGitHub)
    {
        await gitHubService!.CreateRepositoryAsync(manifest, zipBytes);
    }

    return Results.Bytes(zipBytes, "application/zip", 
        $"{manifest.Name}.zip");
});

// POST /api/v1/generate/batch
app.MapPost("/api/v1/generate/batch", async (
    List<ForgeManifest> manifests,
    PrintGenerator generator,
    IAwsDeploymentService awsService,
    IGitHubService gitHubService) =>
{
    var results = new List<GenerationResult>();

    foreach (var manifest in manifests)
    {
        // Same logic as single generation
        // Collect results
    }

    return Results.Ok(results);
});

// POST /api/v1/validate
app.MapPost("/api/v1/validate", (ForgeManifest manifest) =>
{
    var validator = new ManifestValidator();
    var result = validator.Validate(manifest);
    
    if (result.IsValid)
    {
        var names = NamingConvention.DeriveAllNames(
            manifest.Product, manifest.Name, manifest.Type);
        return Results.Ok(new { valid = true, derivedNames = names });
    }

    return Results.BadRequest(new { valid = false, errors = result.Errors });
});

// GET /api/v1/modules
app.MapGet("/api/v1/modules", () =>
{
    return Results.Ok(GenesisModules.GetAll());
});

// GET /api/v1/canvas-modules
app.MapGet("/api/v1/canvas-modules", () =>
{
    return Results.Ok(CanvasModules.GetAll());
});
```

### 3.3 Pervaxis.Forge.Launchpad

**Responsibilities:**
- Multi-service wizard UI (6 steps)
- Real-time naming preview
- Genesis/Canvas module selection
- Infrastructure deployment controls
- Batch generation interface

**Step Flow:**

```
Step 1: Service Count & Identity
  ┌─────────────────────────────────────┐
  │ How many services?  [5]             │
  │                                     │
  │ Service 1:                          │
  │   Product: [clarivolt_____]        │
  │   Name: [intake-service_____]      │
  │   Type: ☑ BFF  ☐ MFE              │
  │                                     │
  │ Service 2:                          │
  │   Product: [clarivolt_____]        │
  │   Name: [validation-service___]    │
  │   Type: ☑ BFF  ☐ MFE              │
  │                                     │
  │ [+ Add Service]                     │
  └─────────────────────────────────────┘

Step 2: Module Selection (Per Service)
  ┌─────────────────────────────────────┐
  │ intake-service — Genesis Modules    │
  │                                     │
  │ ☑ FileStorage.AWS                  │
  │ ☑ Messaging.AWS                    │
  │ ☐ Caching.AWS                      │
  │ ☐ Search.AWS                       │
  │ ...                                 │
  │                                     │
  │ [< Previous] [Next >]               │
  └─────────────────────────────────────┘

Step 3: Database & Queues
  (Per service configuration)

Step 4: Infrastructure Deployment
  ┌─────────────────────────────────────┐
  │ AWS Deployment Options              │
  │                                     │
  │ ☑ Deploy infrastructure now        │
  │   AWS Account: [clarivolt-prod ▼]  │
  │   Environment: [test_____]         │
  │                                     │
  │ ☑ Generate IaC templates           │
  │   Formats: ☑ Terraform  ☑ CDK     │
  │                                     │
  │ [< Previous] [Next >]               │
  └─────────────────────────────────────┘

Step 5: GitHub Configuration
  ┌─────────────────────────────────────┐
  │ ☑ Create GitHub repositories       │
  │   Organization: [clarivex-tech ▼]  │
  │   Visibility: ⚫ Private            │
  │                                     │
  │ ☑ Configure branch protection      │
  │ ☑ Add GitHub Secrets               │
  │                                     │
  │ [< Previous] [Next >]               │
  └─────────────────────────────────────┘

Step 6: Preview & Generate
  ┌─────────────────────────────────────┐
  │ 5 Services Ready                    │
  │                                     │
  │ ✓ clarivolt-intake-service         │
  │ ✓ clarivolt-validation-service     │
  │ ✓ clarivolt-credits-service        │
  │ ✓ clarivolt-obligations-service    │
  │ ✓ clarivolt-filing-service         │
  │                                     │
  │ Infrastructure: Deploy to AWS       │
  │ Repositories: Create 5 repos        │
  │                                     │
  │ [Generate All Services]             │
  └─────────────────────────────────────┘
```

**Real-Time Naming Preview:**

As user types in Step 1, show live preview:

```typescript
// Component logic
export class ServiceIdentityComponent {
  product = signal('');
  name = signal('');
  type = signal<ServiceType>('rest-api');

  derivedNames = computed(() => {
    if (!this.product() || !this.name()) return null;
    
    return this.namingService.deriveNames(
      this.product(),
      this.name(),
      this.type()
    );
  });
}
```

```html
<!-- Template -->
<div class="naming-preview" *ngIf="derivedNames()">
  <h3>Derived Names (Live Preview)</h3>
  <dl>
    <dt>Namespace:</dt>
    <dd>{{ derivedNames().namespace }}</dd>
    
    <dt>Docker Image:</dt>
    <dd>{{ derivedNames().dockerImage }}</dd>
    
    <dt>API Route:</dt>
    <dd>{{ derivedNames().apiBaseRoute }}</dd>
    
    <dt>GitHub Repo:</dt>
    <dd>clarivex-tech/{{ derivedNames().folderName }}</dd>
  </dl>
</div>
```

---

## 4. Infrastructure Provisioning

### 4.1 Dual-Output Strategy

For every service that selects AWS modules, Forge generates:

1. **Terraform Templates** (`.tf` files)
2. **AWS CDK Code** (C# `.cs` files)
3. **Optionally: Direct AWS Deployment** (via SDK)

### 4.2 Resource Naming Convention

```
{environment}-{product}-{service}-{resource-type}

Examples:
  test-clarivolt-intake-cache          (ElastiCache)
  test-clarivolt-intake-db             (RDS PostgreSQL)
  test-clarivolt-intake-files          (S3 Bucket)
  test-clarivolt-intake-queue-submitted (SQS Queue)
```

**Environment** is provided at deployment time, not generation time.

### 4.3 Generated Terraform Structure

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

# Outputs - connection strings
output "cache_endpoint" {
  value = aws_elasticache_cluster.cache.cache_nodes[0].address
}

output "db_endpoint" {
  value = aws_db_instance.db.endpoint
}

output "s3_bucket_name" {
  value = aws_s3_bucket.files.bucket
}

output "queue_url_submitted" {
  value = aws_sqs_queue.intake_submitted.url
}
```

### 4.4 Generated AWS CDK Structure

```csharp
// infrastructure/cdk/Program.cs

using Amazon.CDK;
using Amazon.CDK.AWS.ElastiCache;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SQS;

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

public class IntakeServiceStack : Stack
{
    public IntakeServiceStack(Construct scope, string id, IStackProps props) 
        : base(scope, id, props)
    {
        var environment = this.Node.TryGetContext("environment")?.ToString() 
            ?? "test";

        // ElastiCache Redis
        var cache = new CfnCacheCluster(this, "Cache", new CfnCacheClusterProps
        {
            ClusterId = $"{environment}-clarivolt-intake-cache",
            Engine = "redis",
            CacheNodeType = "cache.t3.micro",
            NumCacheNodes = 1
        });

        // RDS PostgreSQL
        var db = new DatabaseInstance(this, "Database", new DatabaseInstanceProps
        {
            Engine = DatabaseInstanceEngine.Postgres(new PostgresInstanceEngineProps
            {
                Version = PostgresEngineVersion.VER_16_1
            }),
            InstanceIdentifier = $"{environment}-clarivolt-intake-db",
            InstanceType = InstanceType.Of(InstanceClass.BURSTABLE3, InstanceSize.MICRO),
            AllocatedStorage = 20,
            DatabaseName = "intake"
        });

        // S3 Bucket
        var bucket = new Bucket(this, "FilesBucket", new BucketProps
        {
            BucketName = $"{environment}-clarivolt-intake-files"
        });

        // SQS Queue
        var queue = new Queue(this, "SubmittedQueue", new QueueProps
        {
            QueueName = $"{environment}-clarivolt-intake-submitted"
        });

        // Outputs
        new CfnOutput(this, "CacheEndpoint", new CfnOutputProps
        {
            Value = cache.AttrRedisEndpointAddress
        });

        new CfnOutput(this, "DbEndpoint", new CfnOutputProps
        {
            Value = db.DbInstanceEndpointAddress
        });

        new CfnOutput(this, "S3BucketName", new CfnOutputProps
        {
            Value = bucket.BucketName
        });

        new CfnOutput(this, "QueueUrl", new CfnOutputProps
        {
            Value = queue.QueueUrl
        });
    }
}
```

### 4.5 Direct AWS Deployment (Deploy Now)

When admin selects "Deploy Now" in Launchpad:

```csharp
public class AwsDeploymentService : IAwsDeploymentService
{
    private readonly IAmazonElastiCache _elasticache;
    private readonly IAmazonRDS _rds;
    private readonly IAmazonS3 _s3;
    private readonly IAmazonSQS _sqs;
    private readonly IAmazonSecretsManager _secretsManager;

    public async Task<DeploymentResult> DeployAsync(
        ForgeManifest manifest,
        string environment,
        string awsAccountId)
    {
        var result = new DeploymentResult();

        // Deploy each selected Genesis module
        if (manifest.GenesisModules.Contains("Caching.AWS"))
        {
            var cacheEndpoint = await DeployElastiCacheAsync(
                manifest, environment);
            result.Resources.Add("cache_endpoint", cacheEndpoint);
        }

        if (manifest.Database?.Engine == "postgresql")
        {
            var dbEndpoint = await DeployRdsAsync(
                manifest, environment);
            result.Resources.Add("db_endpoint", dbEndpoint);
        }

        if (manifest.GenesisModules.Contains("FileStorage.AWS"))
        {
            var bucketName = await DeployS3BucketAsync(
                manifest, environment);
            result.Resources.Add("s3_bucket", bucketName);
        }

        // Deploy SQS queues
        foreach (var queue in manifest.Queues)
        {
            var queueUrl = await DeploySqsQueueAsync(
                manifest, environment, queue.Name);
            result.Resources.Add($"queue_{queue.Name}", queueUrl);
        }

        // Store all connection strings in AWS Secrets Manager
        await StoreSecretsAsync(manifest, environment, result.Resources);

        return result;
    }

    private async Task StoreSecretsAsync(
        ForgeManifest manifest,
        string environment,
        Dictionary<string, string> resources)
    {
        var secretName = $"{environment}/{manifest.Product}/{manifest.Name}/config";
        var secretValue = JsonSerializer.Serialize(resources);

        await _secretsManager.CreateSecretAsync(new CreateSecretRequest
        {
            Name = secretName,
            SecretString = secretValue,
            Description = $"Auto-generated by Forge for {manifest.Name}"
        });
    }
}
```

**Result:** All connection strings stored in AWS Secrets Manager at:
```
{environment}/{product}/{service}/config
```

Generated `Program.cs` reads from Secrets Manager at startup:

```csharp
// Auto-generated in Program.cs

var secretName = $"{environment}/clarivolt/intake-service/config";
var secretResponse = await secretsClient.GetSecretValueAsync(
    new GetSecretValueRequest { SecretId = secretName });

var config = JsonSerializer.Deserialize<Dictionary<string, string>>(
    secretResponse.SecretString);

builder.Configuration.AddInMemoryCollection(config!);
```

---

## 5. Secrets Management

### 5.1 Secret Types & Storage

| Secret Type | Storage Location | Access Pattern |
|---|---|---|
| **Runtime Secrets** (DB connections, API keys) | AWS Secrets Manager | Application reads at startup |
| **CI/CD Credentials** (AWS deploy keys, Docker tokens) | GitHub Secrets | GitHub Actions workflows only |
| **Forge Admin Credentials** (AWS account access) | Forge PostgreSQL Database | Encrypted at rest, API layer only |

### 5.2 Runtime Secret Retrieval

**Generated in `Program.cs`:**

```csharp
// Pervaxis.Forge-generated secret loading
var environment = builder.Configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

if (environment != "Development")
{
    var secretsClient = new AmazonSecretsManagerClient();
    var secretName = $"{environment}/{builder.Configuration["Product"]}/{builder.Configuration["ServiceName"]}/config";
    
    var secretResponse = await secretsClient.GetSecretValueAsync(
        new GetSecretValueRequest { SecretId = secretName });
    
    var runtimeConfig = JsonSerializer.Deserialize<Dictionary<string, string>>(
        secretResponse.SecretString);
    
    builder.Configuration.AddInMemoryCollection(runtimeConfig!);
}
```

**Local Development:**
- Uses `appsettings.Development.json` with LocalStack endpoints
- No AWS Secrets Manager required locally

**Deployed Environments (test, accp, prod):**
- Reads from AWS Secrets Manager on startup
- Falls back to environment variables if secret not found

### 5.3 GitHub Secrets

Forge automatically creates GitHub Secrets for CI/CD:

```csharp
public async Task ConfigureGitHubSecretsAsync(
    string repoFullName,
    ForgeManifest manifest,
    string environment)
{
    var secrets = new Dictionary<string, string>
    {
        ["AWS_ACCOUNT_ID"] = manifest.Metadata.AwsAccountId,
        ["AWS_REGION"] = "us-east-1",
        ["AWS_ROLE_ARN"] = $"arn:aws:iam::{manifest.Metadata.AwsAccountId}:role/GitHubActionsDeployRole",
        ["ENVIRONMENT"] = environment
    };

    foreach (var (key, value) in secrets)
    {
        await _gitHubClient.Repository.Actions.Secrets.CreateOrUpdate(
            repoFullName, key, new UpsertRepositorySecret
            {
                EncryptedValue = EncryptSecret(value, publicKey),
                KeyId = publicKey.KeyId
            });
    }
}
```

---

## 6. GitHub Integration

### 6.1 Repository Creation

```csharp
public async Task<Repository> CreateRepositoryAsync(
    string org,
    ForgeManifest manifest,
    byte[] zipContent)
{
    // 1. Create repository
    var repo = await _gitHubClient.Repository.Create(org, new NewRepository(manifest.Name)
    {
        Description = manifest.Description,
        Private = true,
        AutoInit = false
    });

    // 2. Configure branch protection
    await ConfigureBranchProtectionAsync(repo.FullName);

    // 3. Add GitHub Secrets
    await ConfigureGitHubSecretsAsync(repo.FullName, manifest, "test");

    // 4. Extract ZIP and push initial commit
    await PushInitialCommitAsync(repo.FullName, zipContent, manifest);

    return repo;
}

private async Task ConfigureBranchProtectionAsync(string repoFullName)
{
    var protection = new BranchProtectionSettingsUpdate(
        new BranchProtectionRequiredStatusChecksUpdate(true, new[] { "build-and-test" }),
        new BranchProtectionRequiredReviewsUpdate(true, true, 1),
        null,
        true
    );

    await _gitHubClient.Repository.Branch.UpdateBranchProtection(
        repoFullName, "main", protection);
}

private async Task PushInitialCommitAsync(
    string repoFullName,
    byte[] zipContent,
    ForgeManifest manifest)
{
    // Extract ZIP to temp directory
    var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    ZipFile.ExtractToDirectory(new MemoryStream(zipContent), tempDir);

    // Git operations
    using var repo = new LibGit2Sharp.Repository(LibGit2Sharp.Repository.Init(tempDir));
    
    // Stage all files
    Commands.Stage(repo, "*");
    
    // Create commit
    var signature = new Signature("Pervaxis Forge", "forge@clarivex.tech", DateTimeOffset.Now);
    repo.Commit($"Initial scaffold from Pervaxis Forge\n\n{manifest.Description}", 
        signature, signature);
    
    // Push to GitHub
    var remote = repo.Network.Remotes.Add("origin", 
        $"https://github.com/{repoFullName}.git");
    
    var pushOptions = new PushOptions
    {
        CredentialsProvider = (url, usernameFromUrl, types) => 
            new UsernamePasswordCredentials
            {
                Username = _gitHubToken,
                Password = string.Empty
            }
    };
    
    repo.Network.Push(remote, "refs/heads/main", pushOptions);
    
    // Cleanup
    Directory.Delete(tempDir, true);
}
```

### 6.2 GitHub Actions Workflow (Generated)

**`.github/workflows/build-test.yml`:**

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
    
    - name: Upload coverage to Codecov
      uses: codecov/codecov-action@v3
      with:
        files: ./coverage.cobertura.xml
```

**`.github/workflows/deploy.yml`** (nice-to-have for v1):

```yaml
name: Deploy to AWS

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@v4
      with:
        role-to-assume: ${{ secrets.AWS_ROLE_ARN }}
        aws-region: ${{ secrets.AWS_REGION }}
    
    - name: Login to Amazon ECR
      id: login-ecr
      uses: aws-actions/amazon-ecr-login@v2
    
    - name: Build and push Docker image
      env:
        ECR_REGISTRY: ${{ steps.login-ecr.outputs.registry }}
        IMAGE_TAG: ${{ github.sha }}
      run: |
        docker build -t $ECR_REGISTRY/clarivolt/intake-service:$IMAGE_TAG .
        docker push $ECR_REGISTRY/clarivolt/intake-service:$IMAGE_TAG
    
    - name: Deploy to ECS
      run: |
        aws ecs update-service \
          --cluster clarivolt-cluster \
          --service intake-service \
          --force-new-deployment
```

---

## 7. Database Schema

### 7.1 Forge Credential Store (PostgreSQL)

**Tables:**

```sql
CREATE TABLE organizations (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL UNIQUE,
    display_name VARCHAR(255) NOT NULL,
    github_org VARCHAR(255),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE aws_accounts (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(id) ON DELETE CASCADE,
    account_name VARCHAR(255) NOT NULL,
    account_id VARCHAR(12) NOT NULL,
    iam_role_arn VARCHAR(500) NOT NULL,
    region VARCHAR(50) NOT NULL DEFAULT 'us-east-1',
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (organization_id, account_name)
);

CREATE TABLE generation_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    organization_id UUID NOT NULL REFERENCES organizations(id),
    aws_account_id UUID REFERENCES aws_accounts(id),
    manifest JSONB NOT NULL,
    service_count INT NOT NULL,
    infrastructure_deployed BOOLEAN NOT NULL DEFAULT false,
    github_repos_created BOOLEAN NOT NULL DEFAULT false,
    created_by VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE deployment_outputs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    generation_log_id UUID NOT NULL REFERENCES generation_logs(id) ON DELETE CASCADE,
    service_name VARCHAR(255) NOT NULL,
    resource_type VARCHAR(100) NOT NULL, -- 'elasticache', 'rds', 's3', 'sqs', etc.
    resource_name VARCHAR(500) NOT NULL,
    endpoint_or_arn TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_generation_logs_org ON generation_logs(organization_id);
CREATE INDEX idx_generation_logs_created_at ON generation_logs(created_at DESC);
CREATE INDEX idx_deployment_outputs_generation ON deployment_outputs(generation_log_id);
```

**Sample Data:**

```sql
INSERT INTO organizations (name, display_name, github_org) VALUES
('clarivex-tech', 'Clarivex Technologies', 'clarivex-tech');

INSERT INTO aws_accounts (organization_id, account_name, account_id, iam_role_arn, region)
SELECT id, 'clarivolt-production', '123456789012', 
       'arn:aws:iam::123456789012:role/ForgeDeploymentRole', 'us-east-1'
FROM organizations WHERE name = 'clarivex-tech';

INSERT INTO aws_accounts (organization_id, account_name, account_id, iam_role_arn, region)
SELECT id, 'clarifrost-production', '987654321098', 
       'arn:aws:iam::987654321098:role/ForgeDeploymentRole', 'us-east-1'
FROM organizations WHERE name = 'clarivex-tech';
```

### 7.2 Entity Framework Core Models

```csharp
public class Organization
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string DisplayName { get; set; }
    public string? GitHubOrg { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<AwsAccount> AwsAccounts { get; set; } = new List<AwsAccount>();
}

public class AwsAccount
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public required string AccountName { get; set; }
    public required string AccountId { get; set; }
    public required string IamRoleArn { get; set; }
    public required string Region { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
}

public class GenerationLog
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid? AwsAccountId { get; set; }
    public required JsonDocument Manifest { get; set; }
    public int ServiceCount { get; set; }
    public bool InfrastructureDeployed { get; set; }
    public bool GitHubReposCreated { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Organization Organization { get; set; } = null!;
    public AwsAccount? AwsAccount { get; set; }
    public ICollection<DeploymentOutput> DeploymentOutputs { get; set; } = new List<DeploymentOutput>();
}

public class DeploymentOutput
{
    public Guid Id { get; set; }
    public Guid GenerationLogId { get; set; }
    public required string ServiceName { get; set; }
    public required string ResourceType { get; set; }
    public required string ResourceName { get; set; }
    public string? EndpointOrArn { get; set; }
    public DateTime CreatedAt { get; set; }

    public GenerationLog GenerationLog { get; set; } = null!;
}
```

---

## 8. Template System

### 8.1 Scriban Engine Wrapper

```csharp
public class TemplateEngine : ITemplateEngine
{
    public async Task<string> RenderAsync(
        string templateContent,
        object model)
    {
        var template = Template.Parse(templateContent, "template");
        
        if (template.HasErrors)
        {
            var errors = string.Join("\n", template.Messages.Select(m => m.Message));
            throw new TemplateException($"Template parsing failed:\n{errors}");
        }

        var context = new TemplateContext
        {
            StrictVariables = true, // Fail on undefined variables
            MemberRenamer = member => member.Name // PascalCase preserved
        };
        
        context.PushGlobal(new ScriptObject
        {
            { "model", model }
        });

        var result = await template.RenderAsync(context);
        return result;
    }
}
```

### 8.2 Template Model Structure

```csharp
public class TemplateModel
{
    public required ForgeManifest Manifest { get; init; }
    public required DerivedNames Names { get; init; }
    public required List<GenesisModuleMetadata> SelectedModules { get; init; }
    public required List<CanvasModuleMetadata> SelectedCanvasModules { get; init; }
    public DatabaseConfig? Database { get; init; }
    public List<QueueConfig> Queues { get; init; } = new();
    public string CurrentYear { get; init; } = DateTime.UtcNow.Year.ToString();
}
```

### 8.3 Sample Template: `Program.cs.sbn`

```scriban
// Auto-generated by Pervaxis.Forge
// Product: {{ model.manifest.product }}
// Service: {{ model.manifest.name }}
// Generated: {{ date.now }}

using {{ model.names.namespace }};
using {{ model.names.namespace }}.Extensions;
{{ for module in model.selected_modules }}
using Pervaxis.Genesis.{{ module.name }};
{{ end }}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.Add{{ model.names.pascal }}Service(builder.Configuration);

// Add Genesis modules
{{ for module in model.selected_modules }}
builder.Services.AddGenesis{{ module.name }}(
    builder.Configuration.GetSection("{{ module.name }}"));
{{ end }}

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
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

**Output for `clarivolt/intake-service` with `Messaging.AWS` + `FileStorage.AWS`:**

```csharp
// Auto-generated by Pervaxis.Forge
// Product: clarivolt
// Service: intake-service
// Generated: 2026-05-04T10:30:00Z

using Clarivolt.IntakeService;
using Clarivolt.IntakeService.Extensions;
using Pervaxis.Genesis.Messaging.AWS;
using Pervaxis.Genesis.FileStorage.AWS;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddIntakeService(builder.Configuration);

// Add Genesis modules
builder.Services.AddGenesisMessaging(
    builder.Configuration.GetSection("Messaging"));
builder.Services.AddGenesisFileStorage(
    builder.Configuration.GetSection("FileStorage"));

// Add controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
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

### 8.4 Template Location

All templates embedded as resources in `Pervaxis.Forge.Engine.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\rest-api\**\*.sbn" />
  <EmbeddedResource Include="Templates\angular-shell\**\*.sbn" />
  <EmbeddedResource Include="Templates\angular-microfrontend\**\*.sbn" />
  <EmbeddedResource Include="Templates\terraform\**\*.sbn" />
  <EmbeddedResource Include="Templates\cdk\**\*.sbn" />
</ItemGroup>
```

**Loading at runtime:**

```csharp
public async Task<string> LoadTemplateAsync(string path)
{
    var assembly = typeof(PrintGenerator).Assembly;
    var resourceName = $"Pervaxis.Forge.Engine.Templates.{path.Replace('/', '.')}";
    
    using var stream = assembly.GetManifestResourceStream(resourceName);
    if (stream == null)
        throw new TemplateNotFoundException(path);
    
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync();
}
```

---

## 9. Naming Convention Engine

### 9.1 Transformation Functions

```csharp
public static class NamingConvention
{
    /// <summary>
    /// Convert kebab-case to PascalCase
    /// "intake-service" → "IntakeService"
    /// </summary>
    public static string ToPascalCase(string kebab)
    {
        return string.Join("", kebab.Split('-')
            .Select(segment => char.ToUpper(segment[0]) + segment[1..]));
    }

    /// <summary>
    /// Remove trailing "-service" suffix
    /// "intake-service" → "intake"
    /// </summary>
    public static string StripServiceSuffix(string name)
    {
        return name.EndsWith("-service") 
            ? name[..^8] 
            : name;
    }

    /// <summary>
    /// Extract first segment before first hyphen
    /// "intake-service" → "intake"
    /// </summary>
    public static string GetFirstSegment(string name)
    {
        var index = name.IndexOf('-');
        return index == -1 ? name : name[..index];
    }

    /// <summary>
    /// Generate component prefix from product name
    /// "clarivolt" → "clv"
    /// </summary>
    public static string GetComponentPrefix(string product)
    {
        return product.Length >= 3 
            ? product[..3].ToLower() 
            : product.ToLower();
    }
}
```

### 9.2 Validation Rules

```csharp
public class ManifestValidator
{
    private static readonly Regex KebabCaseRegex = 
        new(@"^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public ValidationResult Validate(ForgeManifest manifest)
    {
        var errors = new List<string>();

        // Product validation
        if (!KebabCaseRegex.IsMatch(manifest.Product))
            errors.Add("Product must be kebab-case (lowercase, hyphens only)");

        // Name validation
        if (!KebabCaseRegex.IsMatch(manifest.Name))
            errors.Add("Name must be kebab-case (lowercase, hyphens only)");

        // .NET services must end with "-service"
        if (manifest.Type is ServiceType.RestApi or ServiceType.Grpc or ServiceType.Worker)
        {
            if (!manifest.Name.EndsWith("-service"))
                errors.Add(".NET services must have names ending with '-service'");
        }

        // Angular microfrontends must NOT end with "-service"
        if (manifest.Type == ServiceType.AngularMicrofrontend)
        {
            if (manifest.Name.EndsWith("-service"))
                errors.Add("Angular microfrontends cannot have names ending with '-service'");
        }

        // Genesis module validation
        var validModules = GenesisModules.GetAllNames();
        var invalidModules = manifest.GenesisModules
            .Where(m => !validModules.Contains(m))
            .ToList();
        
        if (invalidModules.Any())
            errors.Add($"Invalid Genesis modules: {string.Join(", ", invalidModules)}");

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

---

## 10. Security & Access Control

### 10.1 Admin-Only Access

Forge.Launchpad requires authentication:

```typescript
// auth.guard.ts
export const forgeAuthGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  
  if (!authService.isAuthenticated()) {
    return authService.login(state.url);
  }
  
  if (!authService.hasRole('forge-admin')) {
    // Redirect to unauthorized page
    return inject(Router).createUrlTree(['/unauthorized']);
  }
  
  return true;
};
```

**Role-Based Access:**
- `forge-admin` — Full access to all Forge features
- `forge-viewer` — Read-only access to generation logs and audit trails

### 10.2 AWS Credential Security

AWS credentials stored in PostgreSQL are encrypted at rest:

```csharp
public class AwsAccountService
{
    private readonly IDataProtector _protector;

    public AwsAccountService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("AwsAccountCredentials");
    }

    public async Task<AwsAccount> CreateAsync(AwsAccountDto dto)
    {
        var account = new AwsAccount
        {
            AccountName = dto.AccountName,
            AccountId = dto.AccountId,
            IamRoleArn = _protector.Protect(dto.IamRoleArn), // Encrypted
            Region = dto.Region
        };

        await _context.AwsAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public string GetDecryptedRoleArn(AwsAccount account)
    {
        return _protector.Unprotect(account.IamRoleArn);
    }
}
```

### 10.3 Audit Logging

Every generation logged to `generation_logs` table:

```csharp
public async Task LogGenerationAsync(
    Guid organizationId,
    List<ForgeManifest> manifests,
    bool infrastructureDeployed,
    bool gitHubReposCreated,
    string createdBy)
{
    var log = new GenerationLog
    {
        OrganizationId = organizationId,
        Manifest = JsonDocument.Parse(
            JsonSerializer.Serialize(manifests)),
        ServiceCount = manifests.Count,
        InfrastructureDeployed = infrastructureDeployed,
        GitHubReposCreated = gitHubReposCreated,
        CreatedBy = createdBy,
        CreatedAt = DateTime.UtcNow
    };

    await _context.GenerationLogs.AddAsync(log);
    await _context.SaveChangesAsync();
}
```

---

## 11. API Reference

### 11.1 Endpoints

#### POST /api/v1/generate

**Description:** Generate a single service print

**Request:**
```json
{
  "product": "clarivolt",
  "name": "intake-service",
  "displayName": "Intake Service",
  "description": "Accepts daily sales uploads",
  "version": "1.0.0",
  "type": "rest-api",
  "genesisModules": ["FileStorage.AWS", "Messaging.AWS"],
  "database": {
    "engine": "postgresql",
    "schema": "intake"
  },
  "queues": [
    { "name": "clarivolt.intake.submitted", "role": "publish" }
  ],
  "metadata": {
    "deployInfrastructure": true,
    "pushToGitHub": true,
    "awsAccountId": "123456789012",
    "gitHubOrg": "clarivex-tech"
  }
}
```

**Response (200 OK):**
```
Content-Type: application/zip
Content-Disposition: attachment; filename="intake-service.zip"

[ZIP binary data]
```

**Response (400 Bad Request):**
```json
{
  "errors": [
    "Name must be kebab-case and end with -service for .NET types"
  ]
}
```

---

#### POST /api/v1/generate/batch

**Description:** Generate multiple services at once

**Request:**
```json
[
  {
    "product": "clarivolt",
    "name": "intake-service",
    ...
  },
  {
    "product": "clarivolt",
    "name": "validation-service",
    ...
  }
]
```

**Response (200 OK):**
```json
{
  "results": [
    {
      "serviceName": "intake-service",
      "success": true,
      "gitHubRepo": "clarivex-tech/intake-service",
      "awsResources": {
        "cache_endpoint": "test-clarivolt-intake-cache.abc123.cache.amazonaws.com",
        "db_endpoint": "test-clarivolt-intake-db.abc123.us-east-1.rds.amazonaws.com"
      }
    },
    {
      "serviceName": "validation-service",
      "success": true,
      ...
    }
  ]
}
```

---

#### POST /api/v1/validate

**Description:** Validate a manifest without generating

**Request:** Same as `/generate`

**Response (200 OK):**
```json
{
  "valid": true,
  "derivedNames": {
    "namespace": "Clarivolt.IntakeService",
    "projectFile": "Clarivolt.IntakeService.csproj",
    "dockerImage": "clarivolt/intake-service",
    "apiBaseRoute": "/api/v1/intake",
    ...
  }
}
```

**Response (400 Bad Request):**
```json
{
  "valid": false,
  "errors": [
    "Product must be kebab-case",
    "Invalid Genesis module: InvalidModule.AWS"
  ]
}
```

---

#### GET /api/v1/modules

**Description:** List all available Genesis modules

**Response (200 OK):**
```json
[
  {
    "id": "FileStorage.AWS",
    "displayName": "File Storage (AWS S3)",
    "description": "S3 — file uploads, downloads, presigned URLs",
    "nugetPackage": "Pervaxis.Genesis.FileStorage.AWS",
    "version": "1.0.0",
    "iamPermissions": ["s3:PutObject", "s3:GetObject", "s3:DeleteObject"]
  },
  ...
]
```

---

#### GET /api/v1/canvas-modules

**Description:** List all available Canvas Angular modules

**Response (200 OK):**
```json
[
  {
    "id": "canvas-platform-http",
    "layer": "platform",
    "displayName": "HTTP Services",
    "description": "HTTP client with interceptors and error handling",
    "npmPackage": "@pervaxis/canvas-platform-http",
    "version": "1.0.0"
  },
  ...
]
```

---

#### GET /api/v1/organizations

**Description:** List all organizations with AWS accounts

**Response (200 OK):**
```json
[
  {
    "id": "uuid",
    "name": "clarivex-tech",
    "displayName": "Clarivex Technologies",
    "gitHubOrg": "clarivex-tech",
    "awsAccounts": [
      {
        "id": "uuid",
        "accountName": "clarivolt-production",
        "accountId": "123456789012",
        "region": "us-east-1",
        "isActive": true
      }
    ]
  }
]
```

---

## 12. Extension Points

### 12.1 Custom Template Types

To add a new generation target (e.g., `graphql`):

1. Create template folder: `Templates/graphql/`
2. Add `.sbn` files for all generated files
3. Implement naming derivation in `NamingConvention.DeriveGraphqlNames()`
4. Add `ServiceType.Graphql` enum value
5. Register in `PrintGenerator.LoadTemplatesAsync()`

No other changes required — engine is extensible by design.

### 12.2 Custom Genesis Modules

To add a 9th Genesis module:

1. Update `GenesisModules.cs`:
```csharp
public static class GenesisModules
{
    public const string NewModule = "NewModule.AWS";
    
    public static IReadOnlyList<GenesisModuleMetadata> GetAll()
    {
        return new[]
        {
            // ... existing modules
            new GenesisModuleMetadata
            {
                Id = NewModule,
                DisplayName = "New Module (AWS XYZ)",
                Description = "...",
                NugetPackage = "Pervaxis.Genesis.NewModule.AWS"
            }
        };
    }
}
```

2. Update templates to handle the new module in DI wiring

No database migrations, no API changes — metadata-driven.

### 12.3 Multi-Cloud Support (Future)

Current design supports future Azure/GCP extensions:

```csharp
public enum CloudProvider
{
    AWS,
    Azure,
    GCP
}

// manifest.json
{
  "cloudProvider": "azure",
  "genesisModules": ["FileStorage.Azure", "Messaging.Azure"]
}
```

Templates would switch based on `cloudProvider` field. Infrastructure provisioning would use Azure SDK instead of AWS SDK.

---

## Appendix: Technology Decisions

### Why Scriban over Razor?
- **No runtime compilation** — faster, lower memory
- **Simpler** — text templates, not C# code
- **Portable** — no ASP.NET dependency

### Why Both Terraform + CDK?
- **Terraform** — Multi-cloud, industry standard, declarative HCL
- **CDK** — Type-safe C#, compile-time checks, .NET team familiarity
- Teams choose preference; Forge generates both

### Why PostgreSQL for Credential Store?
- **Relational integrity** — organizations → AWS accounts → deployment logs
- **JSONB support** — flexible manifest storage
- **Industry standard** — familiar to all teams
- **EF Core support** — first-class .NET integration

### Why Polyrepo over Monorepo?
- **Independent deployment** — each service has own CI/CD
- **Clear ownership** — one team, one repo
- **Reduced blast radius** — changes isolated to single service
- **Easier security boundaries** — repo-level access control

---

**End of Technical Specification**

*Pervaxis Forge v1.0 — Clarivex Technologies © 2026*
