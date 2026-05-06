# Pervaxis Forge — Solution Structure
**Reference architecture for both BFF and UI teams**

**Version:** 1.0  
**Date:** May 5, 2026  
**Classification:** Internal

> This document is the Day 1 reference for both teams.  
> BFF team works in `pervaxis-forge-api` repo.  
> UI team works in `pervaxis-forge-launchpad` repo.  
> These are separate repos, separate teams, running in parallel.

---

## Table of Contents
1. [BFF Repo — `pervaxis-forge-api`](#1-bff-repo--pervaxis-forge-api)
2. [UI Repo — `pervaxis-forge-launchpad`](#2-ui-repo--pervaxis-forge-launchpad)
3. [Key Conventions](#3-key-conventions)

---

## 1. BFF Repo — `pervaxis-forge-api`

### 1.1 Repository Root

```
pervaxis-forge-api/
├── Pervaxis.Forge.slnx               ← solution file
├── Directory.Build.props             ← compiler settings for all projects
├── nuget.config                      ← package feeds (no Pervaxis.Core)
├── .gitignore
├── README.md
├── src/
│   ├── Pervaxis.Forge.Engine/        ← pure generation library
│   └── Pervaxis.Forge.Api/           ← HTTP API + orchestration
└── tests/
    ├── Pervaxis.Forge.Engine.Tests/
    └── Pervaxis.Forge.Api.Tests/
```

### 1.2 Directory.Build.props

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <LangVersion>latest</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

---

### 1.3 Pervaxis.Forge.Engine

**No dependencies outside .NET 10 BCL + Scriban. Zero Genesis/Canvas/Core references.**

```
Pervaxis.Forge.Engine/
├── Pervaxis.Forge.Engine.csproj
│
├── Manifest/                         ← input model
│   ├── ForgeManifest.cs
│   ├── ServiceType.cs
│   ├── DatabaseConfig.cs
│   ├── QueueConfig.cs
│   ├── ApiConfig.cs
│   ├── AngularConfig.cs
│   └── ManifestMetadata.cs
│
├── Naming/                           ← naming convention derivation
│   ├── NamingConvention.cs           ← static class, all transform methods
│   └── DerivedNames.cs               ← record holding all derived name strings
│
├── Validation/                       ← manifest validation
│   ├── ManifestValidator.cs
│   └── ValidationResult.cs
│
├── Generation/                       ← core generation orchestration
│   ├── PrintGenerator.cs             ← main entry point: manifest → ZIP bytes
│   ├── FileGenerator.cs              ← renders single template to string
│   ├── ZipPackager.cs                ← packages file dictionary → ZIP bytes
│   └── TemplateModelBuilder.cs       ← builds TemplateModel from manifest
│
├── Templating/                       ← Scriban engine wrapper
│   ├── ITemplateEngine.cs
│   ├── ScribanTemplateEngine.cs
│   ├── TemplateLoader.cs             ← loads embedded .sbn resources
│   └── TemplateModel.cs             ← the object passed into every template
│
├── Modules/                          ← Genesis + Canvas module metadata
│   ├── GenesisModules.cs             ← 8 modules as constants + metadata
│   ├── CanvasModules.cs              ← 14 modules as constants + metadata
│   ├── GenesisModuleMetadata.cs
│   └── CanvasModuleMetadata.cs
│
└── Templates/                        ← embedded Scriban templates (.sbn files)
    ├── rest-api/
    │   ├── manifest.json.sbn
    │   ├── SPEC.md.sbn
    │   ├── README.md.sbn
    │   ├── Dockerfile.sbn
    │   ├── docker-compose.localstack.yml.sbn
    │   ├── csproj.sbn
    │   ├── Program.cs.sbn
    │   ├── ServiceCollectionExtensions.cs.sbn
    │   ├── appsettings.json.sbn
    │   ├── appsettings.Development.json.sbn
    │   ├── Controller.cs.sbn
    │   ├── IService.cs.sbn
    │   ├── Request.cs.sbn
    │   ├── Response.cs.sbn
    │   ├── tests.csproj.sbn
    │   ├── TestBase.cs.sbn
    │   ├── .github/
    │   │   └── workflows/
    │   │       └── build-test.yml.sbn
    │   └── .claude/
    │       └── CLAUDE.md.sbn
    ├── angular-shell/
    │   ├── manifest.json.sbn
    │   ├── SPEC.md.sbn
    │   ├── README.md.sbn
    │   ├── package.json.sbn
    │   ├── angular.json.sbn
    │   ├── tsconfig.json.sbn
    │   ├── app.component.ts.sbn
    │   ├── app.routes.ts.sbn
    │   ├── app.config.ts.sbn
    │   └── .claude/
    │       └── CLAUDE.md.sbn
    ├── angular-microfrontend/
    │   ├── manifest.json.sbn
    │   ├── SPEC.md.sbn
    │   ├── README.md.sbn
    │   ├── module.ts.sbn
    │   ├── routing.module.ts.sbn
    │   ├── component.ts.sbn
    │   ├── component.html.sbn
    │   ├── api.service.ts.sbn
    │   ├── index.ts.sbn
    │   └── .claude/
    │       └── CLAUDE.md.sbn
    ├── terraform/
    │   ├── main.tf.sbn
    │   ├── variables.tf.sbn
    │   └── outputs.tf.sbn
    └── cdk/
        ├── Program.cs.sbn
        └── InfrastructureStack.cs.sbn
```

**`.csproj` — embedded resources config:**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>Pervaxis.Forge.Engine</AssemblyName>
    <RootNamespace>Pervaxis.Forge.Engine</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Scriban" Version="5.*" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="Templates\**\*.sbn" />
  </ItemGroup>
</Project>
```

---

### 1.4 Pervaxis.Forge.Api

```
Pervaxis.Forge.Api/
├── Pervaxis.Forge.Api.csproj
├── Program.cs                        ← app bootstrap, DI registration, endpoint mapping
├── appsettings.json
├── appsettings.Development.json
│
├── Endpoints/                        ← Minimal API endpoint definitions
│   ├── VerticalEndpoints.cs          ← /api/v1/verticals/*
│   ├── GenerationEndpoints.cs        ← /api/v1/generate, /api/v1/validate
│   └── ModuleEndpoints.cs            ← /api/v1/modules, /api/v1/canvas-modules
│
├── Services/                         ← business logic + external integrations
│   ├── IVerticalService.cs
│   ├── VerticalService.cs            ← enrollment, CRUD, credential encryption
│   ├── IVerticalConnectivityValidator.cs
│   ├── VerticalConnectivityValidator.cs  ← STS dry-run + GitHub org check
│   ├── IAwsDeploymentService.cs
│   ├── AwsDeploymentService.cs       ← assumes IAM role, creates resources
│   ├── IGitHubService.cs
│   └── GitHubService.cs              ← create repo, branch protection, push commit
│
├── Data/                             ← EF Core
│   ├── ForgeDbContext.cs
│   ├── Entities/
│   │   ├── Vertical.cs
│   │   ├── VerticalCloudConfig.cs
│   │   ├── VerticalSourceControlConfig.cs
│   │   ├── VerticalTechDefaults.cs
│   │   ├── GenerationLog.cs
│   │   └── DeploymentOutput.cs
│   └── Migrations/                   ← EF Core generated migrations
│
└── Models/                           ← request/response DTOs
    ├── Requests/
    │   ├── VerticalEnrollmentRequest.cs
    │   ├── UpdateVerticalRequest.cs
    │   ├── GenerationRequest.cs
    │   └── BatchGenerationRequest.cs
    └── Responses/
        ├── VerticalResponse.cs
        ├── VerticalSummaryResponse.cs
        ├── ConnectivityValidationResponse.cs
        ├── GenerationResult.cs
        └── BatchGenerationResult.cs
```

**`.csproj` — key packages:**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <ItemGroup>
    <!-- Engine -->
    <ProjectReference Include="..\Pervaxis.Forge.Engine\Pervaxis.Forge.Engine.csproj" />

    <!-- Database -->
    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.*" />
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="10.*" />
    <PackageReference Include="Microsoft.AspNetCore.DataProtection" Version="10.*" />

    <!-- AWS -->
    <PackageReference Include="AWSSDK.SecurityToken" Version="*" />
    <PackageReference Include="AWSSDK.ElastiCache" Version="*" />
    <PackageReference Include="AWSSDK.RDS" Version="*" />
    <PackageReference Include="AWSSDK.S3" Version="*" />
    <PackageReference Include="AWSSDK.SQS" Version="*" />
    <PackageReference Include="AWSSDK.SecretsManager" Version="*" />

    <!-- GitHub -->
    <PackageReference Include="Octokit" Version="*" />
    <PackageReference Include="LibGit2Sharp" Version="*" />
  </ItemGroup>
</Project>
```

---

### 1.5 Test Projects

```
Pervaxis.Forge.Engine.Tests/
├── Pervaxis.Forge.Engine.Tests.csproj
├── Naming/
│   └── NamingConventionTests.cs      ← 50+ tests
├── Validation/
│   └── ManifestValidatorTests.cs     ← 30+ tests
├── Generation/
│   ├── PrintGeneratorTests.cs        ← integration: manifest → ZIP
│   └── ZipPackagerTests.cs
├── Templating/
│   ├── ScribanTemplateEngineTests.cs
│   └── TemplateLoaderTests.cs
└── Modules/
    ├── GenesisModulesTests.cs
    └── CanvasModulesTests.cs

Pervaxis.Forge.Api.Tests/
├── Pervaxis.Forge.Api.Tests.csproj
├── Endpoints/
│   ├── VerticalEndpointsTests.cs     ← integration tests, real PostgreSQL
│   ├── GenerationEndpointsTests.cs
│   └── ModuleEndpointsTests.cs
└── Services/
    ├── VerticalServiceTests.cs       ← unit tests, mocked DB
    ├── VerticalConnectivityValidatorTests.cs
    ├── AwsDeploymentServiceTests.cs  ← integration tests, LocalStack
    └── GitHubServiceTests.cs         ← integration tests, test GitHub org
```

**Test `.csproj` — key packages:**

```xml
<ItemGroup>
  <PackageReference Include="xunit" Version="*" />
  <PackageReference Include="xunit.runner.visualstudio" Version="*" />
  <PackageReference Include="FluentAssertions" Version="*" />
  <PackageReference Include="Moq" Version="*" />
  <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.*" />
  <PackageReference Include="Testcontainers.PostgreSql" Version="*" />  <!-- disposable PG -->
</ItemGroup>
```

---

## 2. UI Repo — `pervaxis-forge-launchpad`

### 2.1 Repository Root

```
pervaxis-forge-launchpad/
├── package.json
├── nx.json                           ← Nx workspace config
├── tsconfig.base.json
├── .eslintrc.json
├── .gitignore
├── README.md
└── apps/
    └── launchpad/                    ← single Angular 21 app
```

---

### 2.2 Angular App Structure

```
apps/launchpad/
├── project.json                      ← Nx project config
├── tsconfig.json
├── tsconfig.app.json
└── src/
    ├── index.html
    ├── main.ts
    ├── styles.scss                   ← global styles + Angular Material theme
    │
    ├── environments/
    │   ├── environment.ts            ← { apiBaseUrl, useMockApi: true }
    │   └── environment.prod.ts       ← { apiBaseUrl, useMockApi: false }
    │
    └── app/
        ├── app.component.ts          ← root shell (router-outlet + top nav)
        ├── app.component.html
        ├── app.component.scss
        ├── app.config.ts             ← provideRouter, provideHttpClient, etc.
        ├── app.routes.ts             ← top-level route definitions
        │
        ├── core/                     ← singleton services, guards, interceptors
        │   ├── auth/
        │   │   ├── auth.service.ts
        │   │   └── auth.guard.ts     ← forgeAuthGuard (forge-admin role)
        │   │
        │   ├── api/                  ← all HTTP services
        │   │   ├── vertical-api.service.ts
        │   │   ├── mock-vertical-api.service.ts   ← week 1 mock
        │   │   ├── generation-api.service.ts
        │   │   └── mock-generation-api.service.ts ← week 1 mock
        │   │
        │   └── models/               ← TypeScript interfaces matching API contracts
        │       ├── vertical.model.ts
        │       ├── enrollment.model.ts
        │       ├── cloud-provider.model.ts
        │       ├── source-control.model.ts
        │       ├── generation.model.ts
        │       └── module.model.ts
        │
        ├── features/                 ← one folder per route/feature
        │   │
        │   ├── vertical-dashboard/   ← route: /
        │   │   ├── vertical-dashboard.component.ts
        │   │   ├── vertical-dashboard.component.html
        │   │   └── vertical-dashboard.component.scss
        │   │
        │   ├── vertical-enrollment/  ← route: /verticals/enroll
        │   │   ├── vertical-enrollment.component.ts    ← stepper wrapper
        │   │   ├── vertical-enrollment.component.html
        │   │   ├── enrollment.state.ts                 ← Signal-based wizard state
        │   │   └── steps/
        │   │       ├── vertical-identity-step/
        │   │       │   ├── vertical-identity-step.component.ts
        │   │       │   ├── vertical-identity-step.component.html
        │   │       │   └── vertical-identity-step.component.scss
        │   │       ├── cloud-provider-step/
        │   │       │   ├── cloud-provider-step.component.ts
        │   │       │   ├── cloud-provider-step.component.html
        │   │       │   └── cloud-provider-step.component.scss
        │   │       ├── source-control-step/
        │   │       │   ├── source-control-step.component.ts
        │   │       │   ├── source-control-step.component.html
        │   │       │   └── source-control-step.component.scss
        │   │       ├── tech-defaults-step/
        │   │       │   ├── tech-defaults-step.component.ts
        │   │       │   ├── tech-defaults-step.component.html
        │   │       │   └── tech-defaults-step.component.scss
        │   │       └── review-enroll-step/
        │   │           ├── review-enroll-step.component.ts
        │   │           ├── review-enroll-step.component.html
        │   │           └── review-enroll-step.component.scss
        │   │
        │   ├── vertical-workspace/   ← route: /verticals/:slug
        │   │   ├── vertical-workspace.component.ts
        │   │   ├── vertical-workspace.component.html
        │   │   ├── vertical-workspace.component.scss
        │   │   └── vertical-settings/
        │   │       ├── vertical-settings.component.ts   ← slide-out panel
        │   │       ├── vertical-settings.component.html
        │   │       └── vertical-settings.component.scss
        │   │
        │   ├── service-generation/   ← route: /verticals/:slug/generate
        │   │   ├── generation-wizard.component.ts       ← stepper wrapper
        │   │   ├── generation-wizard.component.html
        │   │   ├── generation.state.ts                  ← Signal-based wizard state
        │   │   └── steps/
        │   │       ├── service-identity-step/
        │   │       │   ├── service-identity-step.component.ts
        │   │       │   ├── service-identity-step.component.html
        │   │       │   └── service-identity-step.component.scss
        │   │       ├── module-selection-step/
        │   │       │   ├── module-selection-step.component.ts
        │   │       │   ├── module-selection-step.component.html
        │   │       │   └── module-selection-step.component.scss
        │   │       ├── database-queues-step/
        │   │       │   ├── database-queues-step.component.ts
        │   │       │   ├── database-queues-step.component.html
        │   │       │   └── database-queues-step.component.scss
        │   │       ├── infrastructure-step/
        │   │       │   ├── infrastructure-step.component.ts
        │   │       │   ├── infrastructure-step.component.html
        │   │       │   └── infrastructure-step.component.scss
        │   │       ├── github-config-step/
        │   │       │   ├── github-config-step.component.ts
        │   │       │   ├── github-config-step.component.html
        │   │       │   └── github-config-step.component.scss
        │   │       └── preview-generate-step/
        │   │           ├── preview-generate-step.component.ts
        │   │           ├── preview-generate-step.component.html
        │   │           └── preview-generate-step.component.scss
        │   │
        │   └── unauthorized/         ← route: /unauthorized
        │       └── unauthorized.component.ts
        │
        └── shared/                   ← reusable components, pipes, directives
            ├── components/
            │   ├── naming-preview/
            │   │   ├── naming-preview.component.ts      ← live derived names panel
            │   │   ├── naming-preview.component.html
            │   │   └── naming-preview.component.scss
            │   ├── provider-card/
            │   │   ├── provider-card.component.ts       ← AWS/Azure/GCP card
            │   │   └── provider-card.component.html
            │   └── module-card/
            │       ├── module-card.component.ts         ← Genesis/Canvas module card
            │       └── module-card.component.html
            ├── pipes/
            │   └── mask-secret.pipe.ts                  ← masks ARN / tokens in review
            └── directives/
                └── kebab-case-validator.directive.ts    ← reused across both wizards
```

---

### 2.3 Key Files Explained

**`app.routes.ts`**
```typescript
export const routes: Routes = [
  {
    path: '',
    component: AppComponent,
    canActivate: [forgeAuthGuard],
    children: [
      { path: '', loadComponent: () => import('./features/vertical-dashboard/...) },
      { path: 'verticals/enroll', loadComponent: () => import('./features/vertical-enrollment/...) },
      { path: 'verticals/:slug', loadComponent: () => import('./features/vertical-workspace/...) },
      { path: 'verticals/:slug/generate', loadComponent: () => import('./features/service-generation/...) },
    ]
  },
  { path: 'unauthorized', loadComponent: () => import('./features/unauthorized/...) },
  { path: '**', redirectTo: '' }
];
```

**`enrollment.state.ts`** — wizard state via Signals
```typescript
export interface EnrollmentState {
  identity: VerticalIdentityForm | null;
  cloudProvider: CloudProviderForm | null;
  sourceControl: SourceControlForm | null;
  techDefaults: TechDefaultsForm | null;
}

export const enrollmentState = signal<EnrollmentState>({
  identity: null,
  cloudProvider: null,
  sourceControl: null,
  techDefaults: null
});
```

**`environment.ts`** — mock API flag
```typescript
export const environment = {
  production: false,
  apiBaseUrl: 'http://localhost:5000',
  useMockApi: true   // ← flip to false on May 10 (integration day)
};
```

---

### 2.4 `package.json` — key dependencies

```json
{
  "dependencies": {
    "@angular/core": "21.2.9",
    "@angular/material": "21.2.9",
    "@angular/cdk": "21.2.9",
    "@angular/router": "21.2.9",
    "@angular/forms": "21.2.9",
    "@angular/common": "21.2.9"
  },
  "devDependencies": {
    "@nx/angular": "22.7.0",
    "@nx/workspace": "22.7.0",
    "typescript": "~5.9.2",
    "jasmine-core": "~5.1.0",
    "karma": "~6.4.0",
    "cypress": "^13.0.0"
  }
}
```

**No Canvas libraries in Launchpad.** Canvas is only referenced in the *generated* Angular templates (Shell + MFE), not in Forge itself.

---

## 3. Key Conventions

### 3.1 Naming

| Thing | Convention | Example |
|---|---|---|
| .NET projects | `Pervaxis.Forge.<Layer>` | `Pervaxis.Forge.Engine` |
| .NET namespaces | Match project name | `Pervaxis.Forge.Engine.Naming` |
| .NET interfaces | `I` prefix | `ITemplateEngine` |
| .NET records (requests) | `*Request` / `*Response` | `VerticalEnrollmentRequest` |
| Angular components | `kebab-case` folders, `PascalCase` class | `vertical-dashboard/VerticalDashboardComponent` |
| Angular services | `*.service.ts` | `vertical-api.service.ts` |
| Angular state files | `*.state.ts` | `enrollment.state.ts` |
| Angular models | `*.model.ts` | `vertical.model.ts` |
| Scriban templates | `*.sbn` | `Program.cs.sbn` |

### 3.2 Dependency Rules

```
Engine          → no external dependencies (Scriban only)
Api             → references Engine only; no Pervaxis.Core/Genesis
Launchpad       → no Canvas libraries; Angular Material only
Generated code  → can reference anything (Genesis, Canvas, etc.)
```

### 3.3 Folder Rules

- One class per file in .NET (no exceptions)
- One component per folder in Angular (`.ts` + `.html` + `.scss` together)
- Feature folders in Angular are lazy-loaded routes — never imported directly
- Shared components go in `shared/` only if used by 2+ features

### 3.4 Configuration Files

| File | Purpose |
|---|---|
| `Directory.Build.props` | Compiler settings applied to all .NET projects automatically |
| `nuget.config` | Package feed config — explicitly excludes Pervaxis.Core feed |
| `environment.ts` | Angular env config — `useMockApi` flag controls mock vs real API |
| `appsettings.Development.json` | LocalStack endpoints for local dev |

---

**End of Solution Structure**

*Pervaxis Forge v1.0 — Clarivex Technologies © 2026*
