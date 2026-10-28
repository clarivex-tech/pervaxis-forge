# BFF UI Architecture Contract
**Monolithic Angular + Ionic Mobile + Existing MFE Support**

**Status:** Proposed contract
**Scope:** Forge Launchpad BFF / Engine
**Goal:** Extend Forge so it can describe and generate:
- Monolithic Angular web apps
- Ionic mobile apps
- Combined web + mobile variants from the same domain
- Existing Angular Shell and Angular MFE outputs

## 1. Summary

The current Forge contract is centered on a single `ServiceType` per generation request and assumes Angular UI generation means either:
- `AngularShell`
- `AngularMfe`

This contract adds a first-class UI capability model:
- `web`
- `mobile`
- `both`
- `mfe`

The BFF should treat monolithic Angular and Ionic as distinct output targets, not as special cases of MFE.

## 2. Current Code Paths

### 2.1 MFE shell / remote logic

The current MFE-specific generation seams are:
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/PrintGenerator.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/PrintGenerator.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/TemplateModelBuilder.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/TemplateModelBuilder.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Naming/NamingConvention.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Naming/NamingConvention.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Validation/ManifestValidator.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Validation/ManifestValidator.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-microfrontend/`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-microfrontend/)

The routing decision is currently in `PrintGenerator.ResolveTemplateRoot(...)`.

### 2.2 Generation API seams

The request/response and business logic live in:
- [`pervaxis-forge-api/src/Pervaxis.Forge.Api/Models/Requests/GenerationRequest.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Api/Models/Requests/GenerationRequest.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Api/Models/Requests/ServiceGenerationSpec.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Api/Models/Requests/ServiceGenerationSpec.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Api/Services/GenerationService.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Api/Services/GenerationService.cs)
- [`pervaxis-forge-api/src/Pervaxis.Forge.Api/Endpoints/GenerationEndpoints.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Api/Endpoints/GenerationEndpoints.cs)

## 3. Required File Changes

### 3.1 Engine

1. Add new UI service types in [`ServiceType.cs`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Manifest/ServiceType.cs).
2. Extend [`PrintGenerator.ResolveTemplateRoot(...)`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/PrintGenerator.cs) to route to new template roots.
3. Extend [`TemplateModelBuilder`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/TemplateModelBuilder.cs) and [`NamingConvention`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Naming/NamingConvention.cs) for any new monolith/mobile naming rules.
4. Extend [`ManifestValidator`](/C:/Anand/Clarivex/Pervaxis/Code/pervaxis-forge/pervaxis-forge-api/src/Pervaxis.Forge.Engine/Validation/ManifestValidator.cs) with new type-specific validation.
5. Add new template roots for monolithic Angular and Ionic mobile scaffolds.

### 3.2 API

1. Extend the generation request model with UI target selection.
2. Add a config/capabilities endpoint so Launchpad can query which UI targets are enabled.
3. Keep `POST /api/v1/generate` as the actual generation endpoint.
4. If one request generates multiple UI outputs, make the response explicit about each artifact returned.

## 4. ServiceType Updates

### 4.1 Proposed enum additions

```csharp
public enum ServiceType
{
    RestApi = 1,
    GraphQL = 2,
    Grpc = 3,
    AngularShell = 5,
    AngularMfe = 6,
    AngularMonolith = 7,
    IonicMobile = 8
}
```

### 4.2 Meaning

- `RestApi` - backend API scaffold
- `GraphQL` - backend GraphQL scaffold
- `Grpc` - backend gRPC scaffold
- `AngularShell` - MFE shell host
- `AngularMfe` - MFE remote
- `AngularMonolith` - standalone Angular app
- `IonicMobile` - Ionic/Capacitor mobile app scaffold

## 5. Request / Response Contracts

## 5.1 Capability discovery

### `GET /api/v1/capabilities`

Use this for the Launchpad wizard to understand what UI targets are available.

#### Response

```json
{
  "uiTargets": [
    {
      "key": "web",
      "label": "Monolithic Web App",
      "serviceTypes": ["AngularMonolith"]
    },
    {
      "key": "mobile",
      "label": "Ionic Mobile App",
      "serviceTypes": ["IonicMobile"]
    },
    {
      "key": "mfe",
      "label": "Microfrontend",
      "serviceTypes": ["AngularShell", "AngularMfe"]
    }
  ],
  "backendTargets": [
    "RestApi",
    "GraphQL",
    "Grpc"
  ]
}
```

## 5.2 Generation request

### `POST /api/v1/generate`

#### Request

```json
{
  "verticalSlug": "clarivolt",
  "name": "customer-service",
  "displayName": "Customer Service",
  "description": "Customer domain scaffold",
  "version": "1.0.0",
  "type": "RestApi",
  "uiTargets": ["web", "mobile"],
  "genesisModules": ["Messaging", "Caching"],
  "canvasModules": ["Workspace", "Shell"],
  "database": {
    "engine": "postgresql",
    "schema": "customer"
  },
  "createGitHubRepo": true,
  "metadata": {
    "owner": "Platform",
    "team": "Customer Experience"
  }
}
```

#### Notes

- `type` remains the primary backend scaffold type.
- `uiTargets` is optional.
- If `uiTargets` is omitted, the generation behaves as it does today.
- If `uiTargets` contains `web`, `mobile`, or `both`, the generator emits the corresponding UI artifacts in addition to backend artifacts.

#### Response

```json
{
  "serviceName": "customer-service",
  "verticalSlug": "clarivolt",
  "generatedAt": "2026-05-22T15:04:05Z",
  "gitHubRepoUrl": "https://github.com/clarivex-tech/customer-service",
  "artifacts": [
    {
      "target": "backend",
      "serviceType": "RestApi",
      "zipEntry": "customer-service-backend.zip"
    },
    {
      "target": "web",
      "serviceType": "AngularMonolith",
      "zipEntry": "customer-service-web.zip"
    },
    {
      "target": "mobile",
      "serviceType": "IonicMobile",
      "zipEntry": "customer-service-mobile.zip"
    }
  ]
}
```

## 5.3 Validation request

### `POST /api/v1/generate/validate`

#### Request

Same shape as `POST /api/v1/generate`.

#### Response

```json
{
  "isValid": true,
  "errors": [],
  "serviceName": "customer-service",
  "namespace": "Clarivolt.Customer",
  "projectName": "Pervaxis.Clarivolt.Customer",
  "uiTargets": [
    {
      "key": "web",
      "isValid": true,
      "derivedNames": {
        "componentName": "ClarivoltCustomerAppComponent",
        "routePath": "customer"
      }
    },
    {
      "key": "mobile",
      "isValid": true,
      "derivedNames": {
        "componentName": "ClarivoltCustomerAppComponent",
        "routePath": "customer"
      }
    }
  ]
}
```

## 5.4 Batch generation

### `POST /api/v1/generate/batch`

If batch generation is retained, the batch request should accept the same `uiTargets` shape per service.

#### Response

```json
{
  "verticalSlug": "clarivolt",
  "totalServices": 3,
  "succeededCount": 3,
  "failedCount": 0,
  "results": [
    {
      "serviceName": "customer-service",
      "status": "Succeeded",
      "artifacts": ["backend", "web", "mobile"]
    }
  ]
}
```

## 6. Template Folder Structure

### 6.1 Existing

- `Templates/rest-api/`
- `Templates/angular-shell/`
- `Templates/angular-microfrontend/`
- `Templates/graphql/`
- `Templates/grpc/`

### 6.2 New

- `Templates/angular-monolith/`
- `Templates/ionic-mobile/`

### 6.3 Recommended substructure

#### `Templates/angular-monolith/`

- `manifest.json.sbn`
- `README.md.sbn`
- `SPEC.md.sbn`
- `package.json.sbn`
- `angular.json.sbn`
- `tsconfig.json.sbn`
- `apps/__APP__/src/app/app.component.tstemplate.sbn`
- `apps/__APP__/src/app/app.routes.tstemplate.sbn`
- `apps/__APP__/src/main.tstemplate.sbn`
- `apps/__APP__/src/styles.scss.sbn`
- `apps/__APP__/src/index.html.sbn`
- `.claude/CLAUDE.md.sbn`

#### `Templates/ionic-mobile/`

- `manifest.json.sbn`
- `README.md.sbn`
- `SPEC.md.sbn`
- `package.json.sbn`
- `ionic.config.json.sbn`
- `angular.json.sbn`
- `tsconfig.json.sbn`
- `src/app/app.component.tstemplate.sbn`
- `src/app/app.routes.tstemplate.sbn`
- `src/app/services/api.service.tstemplate.sbn`
- `src/app/models/*.tstemplate.sbn`
- `.claude/CLAUDE.md.sbn`

## 7. Validation Rules

### 7.1 Existing rules remain

- REST/GraphQL/gRPC service names must end with `-service`
- Angular shell names must end with `-shell`
- Angular MFE names must not end with `-shell` or `-service`

### 7.2 New monolith rules

- `AngularMonolith` names must not end with `-shell`
- `AngularMonolith` names must not end with `-service` unless the team explicitly wants service-style naming for web apps
- `AngularMonolith` should validate as a standalone UI app, not a backend service

### 7.3 New mobile rules

- `IonicMobile` names must not end with `-service`
- `IonicMobile` should derive a mobile-friendly app name and route/app id
- `IonicMobile` should reject backend-only assumptions such as `genesisModules` unless explicitly shared domain logic is enabled

### 7.4 Combined web + mobile

- If `uiTargets` contains both `web` and `mobile`, the request must include a shared domain name and compatible UI name roots.
- Shared backend APIs should be generated once and referenced by both UI outputs.
- The generator should fail fast if the requested UI target combination cannot be mapped to compatible templates.

## 8. Recommended API Implementation

### 8.1 Request model

Add:

```csharp
public List<string> UiTargets { get; init; } = [];
```

to the generation request model.

### 8.2 Manifest model

Add a normalized UI capability block to the manifest:

```csharp
public IReadOnlyList<string> UiTargets { get; init; } = [];
```

### 8.3 Generator routing

Replace the single-template-root model with:
- one backend root
- zero or more UI roots

This avoids forcing `AngularMonolith` and `IonicMobile` into the MFE pipeline.

## 9. Example Contract Matrix

| UI choice | `uiTargets` | Generated UI root | Backend required? |
|---|---|---|---|
| Monolithic web only | `["web"]` | `Templates/angular-monolith/` | No |
| Mobile only | `["mobile"]` | `Templates/ionic-mobile/` | No |
| Web + mobile | `["web", "mobile"]` | Both | Usually yes |
| MFE shell + remotes | `["mfe"]` | `Templates/angular-shell/` + `Templates/angular-microfrontend/` | Yes |

## 10. Suggested Endpoints

Required:
- `GET /api/v1/capabilities`
- `POST /api/v1/generate`
- `POST /api/v1/generate/validate`

Optional, if the product team wants richer discovery:
- `GET /api/v1/ui-targets`
- `GET /api/v1/template-roots`
- `GET /api/v1/generation/schema`

## 11. Implementation Note

The current code already has a hard boundary between:
- manifest validation
- template selection
- file rendering
- ZIP packaging

That is the right place to introduce the new UI targets. Do not implement monolithic/mobile support by overloading the existing MFE templates.

