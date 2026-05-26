# Implementation Plan: MFE Shell Production Readiness

## Overview

This plan upgrades the `angular-shell` Scriban template set so that generated MFE shell prints are immediately buildable, testable, and deployable with Angular 21, Jest, ESLint, CI pipelines, and Pervaxis Canvas integration. Implementation proceeds incrementally: engine enhancement (Canvas version pinning) first, then workspace configuration, application bootstrap and core infrastructure, CI/documentation, styles/assets, and finally tests. All code is C# (engine/tests) and Scriban templates producing TypeScript/Angular output.

## Tasks

- [x] 1. Engine enhancement: Canvas version pinning
  - [x] 1.1 Add `Version` property to `SelectedCanvasModule` and `GetVersion()` to `CanvasModules`
    - In `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templating/SelectedCanvasModule.cs`, add `public required string Version { get; init; }` to the record
    - In `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Modules/CanvasModules.cs`, add `public static string GetVersion(string moduleName) => "^0.1.0";` method
    - In `TemplateModelBuilder` (or wherever `SelectedCanvasModule` instances are constructed), set `Version = CanvasModules.GetVersion(moduleName)` for each module
    - If version resolution fails for an unknown module, log a warning via `ILogger` and default to `^0.1.0`
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 1.2 Expose `version` in Scriban template context
    - In the `ScribanTemplateEngine` or model-building code where `selected_canvas_modules` is mapped to `ScriptObject`, add `["version"] = m.Version` alongside existing `name`, `package_name`, `import_name`
    - _Requirements: 1.2_

- [x] 2. Workspace configuration templates
  - [x] 2.1 Upgrade `package.json.sbn` to Angular 21 with Jest and ESLint deps
    - Replace all `@angular/*` dependencies with `^21.0.0`
    - Replace `@angular/cli` and `@angular-devkit/build-angular` with `^21.0.0`
    - Add `@angular/compiler-cli`: `^21.0.0` to devDependencies
    - Update `typescript` to `~5.8.0`, `zone.js` to `~0.15.0`, `@nx/angular` to `^21.0.0`
    - Add Jest devDependencies: `jest`, `@nx/jest`, `jest-preset-angular`, `@types/jest`, `ts-jest`
    - Add ESLint devDependencies: `eslint`, `@angular-eslint/builder`, `@angular-eslint/eslint-plugin`, `@angular-eslint/eslint-plugin-template`, `@angular-eslint/template-parser`, `@typescript-eslint/eslint-plugin`, `@typescript-eslint/parser`
    - Remove any Karma/Jasmine references
    - Change Canvas module version from `"*"` to `"{{ m.version }}"`
    - _Requirements: 1.1, 2.5, 2.6, 10.1, 10.2, 10.3, 10.4, 10.5, 14.2_

  - [x] 2.2 Upgrade `angular.json.sbn` with Jest executor, lint target, and fileReplacements
    - Replace `test` target from `@angular-devkit/build-angular:karma` to `@nx/jest:jest` with `jestConfig` option pointing to `apps/{service_name}/jest.config.ts`
    - Add `lint` target using `@angular-eslint/builder:lint` executor with `lintFilePatterns`
    - Add `fileReplacements` to production build configuration substituting `environment.ts` with `environment.prod.ts`
    - Keep `build` target as `@angular-devkit/build-angular:application` and `serve` as `@angular-devkit/build-angular:dev-server`
    - _Requirements: 2.1, 6.3, 14.3, 15.2_

  - [x] 2.3 Confirm `nx.json.sbn` alignment
    - Verify `generators` section declares `"unitTestRunner": "jest"` (already present)
    - Verify `namedInputs.production` excludes `**/*.spec.ts` and `tsconfig.spec.json` (already present)
    - Ensure no Karma references exist anywhere in the file
    - _Requirements: 2.2, 15.1, 15.3, 15.4_

  - [x] 2.4 Update `tsconfig.spec.json.sbn` for Jest types
    - Change `"types": ["jasmine"]` to `"types": ["jest"]` (or add `"jest"` if types array doesn't exist)
    - _Requirements: 2.3_

  - [x] 2.5 Create `apps/__APP__/jest.config.ts.sbn`
    - Create new file at `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/apps/__APP__/jest.config.ts.sbn`
    - Configure with `@nx/jest/presets/jest-preset`, `jest-preset-angular` setup, Angular-specific transforms
    - Use `{{ model.manifest.service_name }}` for `displayName`
    - _Requirements: 2.4_

  - [x] 2.6 Create `eslint.config.mjs.sbn`
    - Create new file at `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/eslint.config.mjs.sbn`
    - Flat config format with `@angular-eslint` and `@typescript-eslint` rules
    - Configure for both TypeScript and Angular template files
    - Enforce component prefix `{{ model.manifest.component_prefix }}`
    - _Requirements: 14.1, 14.4_

- [x] 3. Checkpoint - Ensure workspace configuration is correct
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Application bootstrap and core infrastructure
  - [x] 4.1 Update `app.config.tstemplate.sbn` with interceptors and ErrorHandler
    - Import `ErrorHandler` from `@angular/core`
    - Import `withInterceptors` instead of `withInterceptorsFromDi` from `@angular/common/http`
    - Import `authInterceptor` and `errorInterceptor` from `./core/interceptors/`
    - Import `GlobalErrorHandler` from `./core/handlers/global-error-handler`
    - Change `provideHttpClient(withInterceptorsFromDi())` to `provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))`
    - Add `{ provide: ErrorHandler, useClass: GlobalErrorHandler }` to providers
    - _Requirements: 8.3, 9.2_

  - [x] 4.2 Update `app.routes.tstemplate.sbn` with landing component and wildcard
    - Import `HomeComponent` from `./home.component`
    - Define root path `''` → `redirectTo: 'home'` with `pathMatch: 'full'`
    - Define `'home'` route with `component: HomeComponent`
    - Define wildcard `'**'` → `redirectTo: 'home'`
    - _Requirements: 7.1, 7.2, 7.3_

  - [x] 4.3 Create `home.component.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/home.component.tstemplate.sbn`
    - Minimal standalone component with selector `{{ model.manifest.component_prefix }}-home`
    - Simple template with heading placeholder
    - _Requirements: 7.2_

  - [x] 4.4 Create `auth.interceptor.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/core/interceptors/auth.interceptor.tstemplate.sbn`
    - Implement `HttpInterceptorFn` that clones requests with `Authorization: Bearer <token>` placeholder
    - Include TODO comment for auth service injection
    - _Requirements: 8.1_

  - [x] 4.5 Create `error.interceptor.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/core/interceptors/error.interceptor.tstemplate.sbn`
    - Implement `HttpInterceptorFn` using `catchError` in RxJS pipeline
    - Handle 401, 403, 5xx errors with logging and re-throw
    - _Requirements: 8.2_

  - [x] 4.6 Create `global-error-handler.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/core/handlers/global-error-handler.tstemplate.sbn`
    - Implement Angular `ErrorHandler` interface
    - In production: log error message only (suppress stack traces)
    - In development: log full error with stack trace
    - Use `environment.production` flag for mode detection
    - _Requirements: 9.1, 9.3_

  - [x] 4.7 Update `main.tstemplate.sbn` with structured error handling
    - Replace bare `console.error(err)` in bootstrap catch with structured logging call
    - _Requirements: 9.4_

  - [x] 4.8 Create environment files
    - Create `apps/__APP__/src/environments/environment.tstemplate.sbn` with `production: false`, `apiUrl: 'http://localhost:4200/api'`
    - Create `apps/__APP__/src/environments/environment.prod.tstemplate.sbn` with `production: true`, `apiUrl: '/api'`
    - _Requirements: 6.1, 6.2_

- [x] 5. Checkpoint - Ensure bootstrap and core infrastructure are correct
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. CI pipelines, documentation, and remaining templates
  - [x] 6.1 Create `.github/workflows/pr-check.yml.sbn`
    - Create new file at `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/.github/workflows/pr-check.yml.sbn`
    - Trigger on `pull_request` to `main` and `develop`
    - Steps: checkout (fetch-depth: 0), setup Node 20.x, cache node_modules (hash of package-lock.json), `npm ci`, `nx lint`, `nx test --coverage`, `nx build --configuration production`
    - _Requirements: 4.1, 4.2, 4.5_

  - [x] 6.2 Create `.github/workflows/deploy.yml.sbn`
    - Create new file at `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/.github/workflows/deploy.yml.sbn`
    - Trigger on `push` to `main` and `develop`, plus `workflow_dispatch`
    - Same validation steps as pr-check + placeholder deployment step
    - _Requirements: 4.3, 4.4, 4.5_

  - [x] 6.3 Rewrite `.claude/guides/ci-sonarcloud-setup.md.sbn` for Angular/Nx
    - Replace .NET-specific content with Angular/Nx CI documentation
    - Reference `sonar-scanner` (JS/TS scanner), `lcov` coverage format, `sonar.typescript.lcov.reportPaths`
    - Remove all references to `dotnet sonarscanner`, `.csproj`, `opencover`, `dotnet test`
    - _Requirements: 5.1, 5.2, 5.3_

  - [x] 6.4 Update `README.md.sbn` to reference Angular 21
    - Replace Angular 18 references with Angular 21
    - Ensure README documents Nx workspace commands (`nx build`, `nx test`, `nx lint`)
    - _Requirements: 10.7_

  - [x] 6.5 Update `manifest.json.sbn` with `$schema` reference
    - Add `"$schema": "./manifest.schema.json"` property to the manifest template
    - _Requirements: 13.1_

  - [x] 6.6 Create `manifest.schema.json.sbn`
    - Create new file at `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-shell/manifest.schema.json.sbn`
    - Define JSON Schema with required fields: `product`, `verticalSlug`, `serviceName`, `serviceType`, `componentPrefix`, `canvasModules`
    - Include type constraints and descriptions for each field
    - _Requirements: 13.2, 13.3_

  - [x] 6.7 Update `styles.scss.sbn` with CSS reset and design tokens
    - Add CSS reset: `box-sizing: border-box`, margin/padding normalization on `*`, `html`, `body`
    - Add `:root` design tokens: `--pvx-color-primary`, `--pvx-font-family`, `--pvx-spacing-*`, `--pvx-border-radius`
    - Add body defaults: font-family, line-height, color
    - _Requirements: 12.1, 12.2_

  - [x] 6.8 Add `favicon.ico` binary asset
    - Add a minimal `favicon.ico` file at `apps/__APP__/src/favicon.ico`
    - Verify `angular.json` assets array already references it (it does)
    - _Requirements: 11.1, 11.2_

- [x] 7. Baseline unit test spec templates
  - [x] 7.1 Create `app.component.spec.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/app.component.spec.tstemplate.sbn`
    - Test that `AppComponent` creates successfully
    - Test that it renders a `<router-outlet>`
    - _Requirements: 3.1_

  - [x] 7.2 Create `app.config.spec.tstemplate.sbn`
    - Create new file at `apps/__APP__/src/app/app.config.spec.tstemplate.sbn`
    - Test that `appConfig.providers` is non-empty
    - Test that providers include router and HTTP client providers
    - _Requirements: 3.2, 3.3_

- [x] 8. Checkpoint - Ensure all template files are in place
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 9. Property-based tests (FsCheck)
  - [ ] 9.1 Set up FsCheck and shared test infrastructure for angular-shell
    - Add `<PackageReference Include="FsCheck.Xunit" Version="3.*" />` to `Pervaxis.Forge.Engine.Tests.csproj` (if not already present from monolith spec)
    - Create shared test helper/generator for random `ForgeManifest` instances with valid Nx project names (lowercase, alphanumeric, hyphens, 3-30 chars), component prefixes (2-4 lowercase letters), products (3-20 chars), and 0-5 Canvas modules
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/` directory
    - _Requirements: 1.1, 2.1, 10.1_

  - [ ]* 9.2 Write property test: Canvas dependency version pinning (Property 1)
    - **Property 1: Canvas dependency version pinning**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/CanvasVersionPropertyTests.cs`
    - For any set of `selected_canvas_modules` containing one or more modules, the rendered `package.json` SHALL list each Canvas module dependency with a version string matching `^X.Y.Z` (not `*`)
    - **Validates: Requirements 1.1, 1.2**

  - [ ]* 9.3 Write property test: Jest-only test configuration (Property 2)
    - **Property 2: Jest-only test configuration**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/JestConfigPropertyTests.cs`
    - For any valid template model, rendered `angular.json` test target SHALL use `@nx/jest:jest`, rendered `tsconfig.spec.json` SHALL contain `"jest"` in types, and neither SHALL contain `karma`, `jasmine`, or `@types/jasmine`
    - **Validates: Requirements 2.1, 2.3, 2.6, 15.1, 15.3**

  - [ ]* 9.4 Write property test: Required devDependencies present (Property 3)
    - **Property 3: Required devDependencies present**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/DevDependenciesPropertyTests.cs`
    - For any valid template model, rendered `package.json` SHALL include all required Jest and ESLint devDependencies
    - **Validates: Requirements 2.5, 14.2**

  - [ ]* 9.5 Write property test: Production fileReplacements configuration (Property 4)
    - **Property 4: Production fileReplacements configuration**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/FileReplacementsPropertyTests.cs`
    - For any valid `service_name`, rendered `angular.json` production config SHALL contain `fileReplacements` substituting `environment.ts` with `environment.prod.ts`
    - **Validates: Requirements 6.3**

  - [ ]* 9.6 Write property test: Route redirect target consistency (Property 5)
    - **Property 5: Route redirect target consistency**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/RouteConsistencyPropertyTests.cs`
    - For any valid template model, rendered `app.routes.ts` root path redirect target SHALL match the `path` of another route definition in the same file
    - **Validates: Requirements 7.1**

  - [ ]* 9.7 Write property test: app.config.ts provider registration (Property 6)
    - **Property 6: app.config.ts provider registration**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/AppConfigPropertyTests.cs`
    - For any valid template model, rendered `app.config.ts` SHALL call `withInterceptors` (not `withInterceptorsFromDi`) and SHALL register `ErrorHandler` with `useClass: GlobalErrorHandler`
    - **Validates: Requirements 8.3, 9.2**

  - [ ]* 9.8 Write property test: Angular 21 version consistency (Property 7)
    - **Property 7: Angular 21 version consistency**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/Angular21VersionPropertyTests.cs`
    - For any valid template model, rendered `package.json` SHALL pin all `@angular/*` packages to `^21.0.0` and `@nx/angular` to `^21.0.0`
    - **Validates: Requirements 10.1, 10.2, 10.5**

  - [ ]* 9.9 Write property test: Documentation references Angular 21 (Property 8)
    - **Property 8: Documentation references Angular 21**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/DocumentationPropertyTests.cs`
    - For any valid template model, rendered `README.md` SHALL contain "Angular 21" and SHALL NOT contain "Angular 18"
    - **Validates: Requirements 10.7**

  - [ ]* 9.10 Write property test: CI guide excludes .NET tooling (Property 9)
    - **Property 9: CI guide excludes .NET tooling**
    - In same file as Property 8 or separate `CiGuidePropertyTests.cs`
    - For any valid template model, rendered `.claude/guides/ci-sonarcloud-setup.md` SHALL NOT contain `dotnet sonarscanner`, `.csproj`, `opencover`, and SHALL contain `sonar-scanner` and `lcov`
    - **Validates: Requirements 5.1, 5.2, 5.3**

  - [ ]* 9.11 Write property test: Correct Nx/Angular executors (Property 10)
    - **Property 10: Correct Nx/Angular executors**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/ExecutorPropertyTests.cs`
    - For any valid `service_name`, rendered `angular.json` SHALL use `@angular-devkit/build-angular:application` for build, `@angular-devkit/build-angular:dev-server` for serve, and `@angular-eslint/builder:lint` for lint
    - **Validates: Requirements 14.3, 15.2**

- [ ] 10. Unit tests (example-based)
  - [ ]* 10.1 Write angular-shell workspace configuration tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/WorkspaceConfigTests.cs`
    - Verify `jest.config.ts` structure (preset, setup files, transform config)
    - Verify `tsconfig.spec.json` types array contains `"jest"`
    - Verify `nx.json` generators declare `unitTestRunner: "jest"`
    - Verify `nx.json` namedInputs.production excludes spec files
    - Verify ESLint config imports correct plugins
    - _Requirements: 2.2, 2.3, 2.4, 14.1, 15.1, 15.4_

  - [ ]* 10.2 Write angular-shell bootstrap and infrastructure tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/BootstrapTests.cs`
    - Verify `app.config.ts` calls `withInterceptors` (not `withInterceptorsFromDi`)
    - Verify interceptor function signatures (`HttpInterceptorFn`)
    - Verify global error handler implements `ErrorHandler` interface
    - Verify route structure (root redirect, landing component, wildcard)
    - Verify environment file structure (production flag, apiUrl)
    - _Requirements: 6.1, 6.2, 7.1, 7.2, 7.3, 8.1, 8.2, 8.3, 9.1, 9.2_

  - [ ]* 10.3 Write angular-shell CI and documentation tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularShell/CiWorkflowTests.cs`
    - Verify CI workflow triggers and step names
    - Verify Node.js 20.x and cache configuration
    - Verify manifest schema defines required fields
    - Verify CSS reset and design tokens in styles.scss
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.5, 12.1, 12.2, 13.2_

- [ ] 11. Integration tests
  - [ ]* 11.1 Write end-to-end angular-shell print generation test
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Integration/AngularShellGenerationTests.cs`
    - Generate a complete print via `PrintGenerator` with `ServiceType.AngularShell`
    - Verify all expected files are present in the generated output
    - Verify no Karma/Jasmine files are emitted
    - Verify JSON files parse without errors
    - Verify Canvas module versions are pinned (not `*`)
    - _Requirements: 1.1, 2.6, 3.3, 10.1_

- [x] 12. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document using FsCheck
- Unit tests validate specific examples and edge cases
- The implementation language is C# for engine code/tests and Scriban templates producing TypeScript/Angular output
- Templates use `.sbn` extension, `.tstemplate.sbn` → `.ts` file mapping, and `__APP__` directory placeholder per existing Forge Engine conventions
- The `nx.json.sbn` already has correct `generators` and `namedInputs` configuration — task 2.3 is a verification step


## Deferred Items (Convention / Polish — Not Blockers)

The following items were raised in the final review but are NOT blockers for scaffold handoff:

- [ ]* 13. Add `ChangeDetectionStrategy.OnPush` to app.component.ts and home.component.ts (convention enforcement)
- [ ]* 14. Improve app.config.spec.ts with real provider assertions instead of count check
- [ ]* 15. Clean up auth.interceptor.ts TODO — wire to CanvasAuthModule or leave clean stub
- [ ]* 16. Add Canvas auth redirect on 401 in error.interceptor.ts
- [ ]* 17. Document deploy.yml placeholder in README.md
