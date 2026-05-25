# Implementation Plan: Angular Monolith Production Readiness

## Overview

This plan transforms the angular-monolith and ionic-mobile Scriban template sets from empty placeholder stubs into fully functional, production-ready Angular 21 standalone application scaffolds. Implementation proceeds incrementally: engine enhancement first, then workspace config, bootstrap, core infrastructure, mobile-specific templates, and finally tests. All code is C# (engine/tests) and Scriban templates producing TypeScript/Angular output.

## Tasks

- [x] 1. Engine enhancement: skip empty rendered templates
  - [x] 1.1 Add empty-content skip logic to FileGenerator
    - In `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Generation/FileGenerator.cs`, after the `templateEngine.Render(template, model)` call, add a check: if `string.IsNullOrWhiteSpace(rendered)` then `continue` to skip emitting the file
    - This enables templates to use Scriban `{{ if }}` guards that produce empty output when conditions aren't met (e.g., `platform.service.ts` only emitted for web+mobile)
    - _Requirements: 12.1, 12.3_

  - [ ]* 1.2 Write unit test for empty file skip behavior
    - Add a test in `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Generation/` verifying that `FileGenerator.GenerateAsync` does not include files whose rendered content is empty/whitespace
    - _Requirements: 12.1_

- [x] 2. Angular workspace configuration templates
  - [x] 2.1 Implement `angular.json.sbn` for angular-monolith
    - Replace the existing `{ "version": 1 }` stub with a complete Angular CLI workspace descriptor
    - Include `projects` object keyed by `{{ model.manifest.service_name }}`
    - Define `build` target using `@angular-devkit/build-angular:application` with production configuration (optimization, sourceMap: false, outputHashing: "all", namedChunks: false)
    - Define `serve`, `test`, and `lint` architect targets
    - Include `budgets` array with warning/error thresholds
    - Include `fileReplacements` for environment switching
    - Add Scriban conditional for Ionic-specific build config when `ui_targets` contains "mobile"
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6, 6.3, 13.3_

  - [x] 2.2 Implement `package.json.sbn` for angular-monolith
    - Replace the existing stub with complete npm package manifest
    - Include all required `dependencies`: @angular/core, @angular/common, @angular/compiler, @angular/platform-browser, @angular/platform-browser-dynamic, @angular/router, @angular/animations, rxjs, zone.js, tslib
    - Include all required `devDependencies`: @angular/cli, @angular/compiler-cli, @angular-devkit/build-angular, typescript, karma, karma-chrome-launcher, karma-coverage, karma-jasmine, karma-jasmine-html-reporter, jasmine-core, eslint, @angular-eslint/*, @typescript-eslint/*
    - Include `scripts`: start, build, test, lint
    - Add Scriban conditional for Ionic/Capacitor dependencies when `ui_targets` contains "mobile"
    - Pin all @angular/* packages to `^21.0.0`
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 2.5, 13.2_

  - [x] 2.3 Implement `tsconfig.json.sbn` for angular-monolith
    - Replace the existing `{ "compilerOptions": {} }` stub with strict TypeScript configuration
    - Set `strict: true`, `target: "ES2022"`, `module: "ES2022"`, `moduleResolution: "bundler"`, `lib: ["ES2022", "dom"]`
    - Set `forceConsistentCasingInFileNames: true`, `noFallthroughCasesInSwitch: true`
    - _Requirements: 3.1, 3.2, 3.3_

  - [x] 2.4 Create `tsconfig.app.json.sbn` for angular-monolith
    - Create new file extending root tsconfig with application-specific `files` (main.ts) and `include` patterns (src/**/*.ts)
    - _Requirements: 3.4_

- [x] 3. Checkpoint - Ensure workspace config templates are correct
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Application bootstrap templates (angular-monolith)
  - [x] 4.1 Implement `main.tstemplate.sbn`
    - Create `apps/__APP__/src/main.tstemplate.sbn` with `bootstrapApplication(AppComponent, appConfig)` call
    - Include error logging fallback in the catch block
    - _Requirements: 4.1_

  - [x] 4.2 Implement `app.config.tstemplate.sbn`
    - Create `apps/__APP__/src/app/app.config.tstemplate.sbn` exporting `ApplicationConfig`
    - Include `provideRouter(appRoutes)`, `provideHttpClient(withInterceptors([authInterceptor, errorInterceptor]))`, `provideAnimationsAsync()`
    - Register `{ provide: ErrorHandler, useClass: GlobalErrorHandler }`
    - Add Scriban conditional for `provideIonicAngular()` when `ui_targets` contains "mobile"
    - _Requirements: 4.2, 7.2, 8.3, 9.3_

  - [x] 4.3 Implement `app.component.tstemplate.sbn`
    - Create `apps/__APP__/src/app/app.component.tstemplate.sbn` with valid `@Component` decorator
    - Set `selector: '{{ model.manifest.component_prefix }}-root'`, `standalone: true`
    - Import `RouterOutlet` (+ `IonApp, IonRouterOutlet` conditionally for mobile)
    - Use `templateUrl` pointing to external template
    - _Requirements: 4.3, 4.4_

  - [x] 4.4 Implement `app.component.html.sbn`
    - Create `apps/__APP__/src/app/app.component.html.sbn`
    - Standard HTML layout with `<router-outlet>`
    - Add Scriban conditional for Ionic layout (`<ion-app><ion-router-outlet>`) when `ui_targets` contains "mobile"
    - _Requirements: 4.5, 12.4_

  - [x] 4.5 Implement `app.routes.tstemplate.sbn`
    - Create `apps/__APP__/src/app/app.routes.tstemplate.sbn`
    - Define root route (redirect to dashboard), protected route with `authGuard`, and wildcard `**` catch-all
    - _Requirements: 9.1, 9.2_

- [x] 5. HTML entry point and environment templates (angular-monolith)
  - [x] 5.1 Implement `index.html.sbn`
    - Create `apps/__APP__/src/index.html.sbn` with valid HTML5 doctype
    - Include `<html lang="en">`, `<head>`, `<base href="/">`, viewport meta, CSP meta tag
    - Include app root element `<{{ model.manifest.component_prefix }}-root>`
    - Add Scriban conditional for Ionic viewport/status bar meta tags when `ui_targets` contains "mobile"
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 5.6_

  - [x] 5.2 Implement environment templates
    - Create `apps/__APP__/src/environments/environment.tstemplate.sbn` with `production: false` and `apiUrl` placeholder
    - Create `apps/__APP__/src/environments/environment.prod.tstemplate.sbn` with `production: true` and `apiUrl` placeholder
    - _Requirements: 6.1, 6.2_

  - [x] 5.3 Implement `styles.scss.sbn`
    - Create `apps/__APP__/src/styles.scss.sbn` with CSS reset, `:root` design tokens (primary color, font family, spacing, border radius)
    - Add Scriban conditional for Ionic theme variables and CSS utilities when `ui_targets` contains "mobile"
    - _Requirements: 14.1, 14.2, 14.3_

- [x] 6. Core infrastructure templates (angular-monolith)
  - [x] 6.1 Implement `global-error-handler.tstemplate.sbn`
    - Create `apps/__APP__/src/app/core/handlers/global-error-handler.tstemplate.sbn`
    - Implement Angular `ErrorHandler` interface, delegate to `LoggerService`
    - Suppress stack traces in production mode
    - _Requirements: 7.1, 7.3_

  - [x] 6.2 Implement `auth.interceptor.tstemplate.sbn`
    - Create `apps/__APP__/src/app/core/interceptors/auth.interceptor.tstemplate.sbn`
    - Implement `HttpInterceptorFn` that clones requests with Authorization header placeholder
    - _Requirements: 8.1_

  - [x] 6.3 Implement `error.interceptor.tstemplate.sbn`
    - Create `apps/__APP__/src/app/core/interceptors/error.interceptor.tstemplate.sbn`
    - Implement `HttpInterceptorFn` that catches 401/403/500 errors, routes to error handling
    - _Requirements: 8.2_

  - [x] 6.4 Implement `auth.guard.tstemplate.sbn`
    - Create `apps/__APP__/src/app/core/guards/auth.guard.tstemplate.sbn`
    - Implement `CanActivateFn` that checks auth state and redirects unauthenticated users
    - _Requirements: 9.1_

  - [x] 6.5 Implement `logger.service.tstemplate.sbn`
    - Create `apps/__APP__/src/app/core/services/logger.service.tstemplate.sbn`
    - Injectable service with `debug`, `info`, `warn`, `error` methods
    - Suppress debug/info in production using environment flag
    - _Requirements: 10.1, 10.2, 10.3_

  - [x] 6.6 Implement `platform.service.tstemplate.sbn` (web+mobile only)
    - Create `apps/__APP__/src/app/core/services/platform.service.tstemplate.sbn`
    - Wrap entire content in Scriban `{{ if model.manifest.ui_targets | array.contains "mobile" }}` guard so it renders empty (and is skipped by engine) for web-only
    - Detect Capacitor native context via `Capacitor.isNativePlatform()`
    - _Requirements: 12.1_

- [x] 7. Checkpoint - Ensure angular-monolith templates compile correctly
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Build tooling and documentation templates (angular-monolith)
  - [x] 8.1 Implement `eslint.config.mjs.sbn`
    - Create `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/eslint.config.mjs.sbn`
    - Flat config format with `@angular-eslint` and `@typescript-eslint` rules
    - Configure for both TypeScript and Angular template files
    - _Requirements: 13.1_

  - [x] 8.2 Implement `.gitignore.sbn`
    - Create `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/.gitignore.sbn`
    - Exclude node_modules/, dist/, .angular/, coverage/, IDE files
    - Add Scriban conditional for android/, ios/, Capacitor artifacts when `ui_targets` contains "mobile"
    - _Requirements: 16.1, 16.2_

  - [x] 8.3 Implement `README.md.sbn`
    - Replace existing stub with comprehensive README template
    - Include project name, description, prerequisites, install/start/build/test/lint commands, project structure
    - Interpolate `{{ model.manifest.service_name }}` and `{{ model.manifest.product }}`
    - _Requirements: 15.1_

  - [x] 8.4 Implement `SPEC.md.sbn`
    - Replace existing stub with architectural specification template
    - Include service type, target platforms, component prefix, architectural decisions
    - Interpolate `{{ model.manifest.service_name }}`, `{{ model.manifest.component_prefix }}`, `{{ model.manifest.ui_targets }}`
    - _Requirements: 15.2_

  - [x] 8.5 Implement test configuration (`app.component.spec.tstemplate.sbn`)
    - Create `apps/__APP__/src/app/app.component.spec.tstemplate.sbn`
    - Basic test validating root component creates and renders with correct selector
    - _Requirements: 17.1, 17.2_

- [x] 9. Ionic-mobile template set
  - [x] 9.1 Implement `angular.json.sbn` for ionic-mobile
    - Replace existing stub with complete Angular CLI workspace descriptor for Ionic project
    - Include Ionic-specific build configuration, project keyed by `{{ model.manifest.service_name }}`
    - Define build, serve, test, lint targets
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5_

  - [x] 9.2 Implement `package.json.sbn` for ionic-mobile
    - Replace existing stub with complete npm manifest including Angular + Ionic + Capacitor dependencies
    - Include @ionic/angular, @capacitor/core, @capacitor/cli, @capacitor/android, @capacitor/ios
    - Pin @angular/* to ^19.0.0, include scripts
    - _Requirements: 2.1, 2.2, 2.3, 2.5, 11.5_

  - [x] 9.3 Implement `tsconfig.json.sbn` and `tsconfig.app.json.sbn` for ionic-mobile
    - Replace existing stub with strict TypeScript configuration (same as angular-monolith)
    - Create tsconfig.app.json extending root
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [x] 9.4 Implement `capacitor.config.tstemplate.sbn`
    - Create `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/ionic-mobile/capacitor.config.tstemplate.sbn`
    - Configure `appId` derived from product + service_name, `appName`, `webDir` pointing to build output
    - _Requirements: 11.1_

  - [x] 9.5 Implement Ionic bootstrap templates
    - Create `src/main.tstemplate.sbn` with `bootstrapApplication` including `provideIonicAngular()`
    - Create `src/app/app.config.tstemplate.sbn` with Ionic providers
    - Replace existing `src/app/app.component.tstemplate.sbn` with Ionic root component (`<ion-app><ion-router-outlet>`)
    - Create `src/app/app.component.html.sbn` with Ionic layout
    - Replace existing `src/app/app.routes.tstemplate.sbn` with proper routing
    - _Requirements: 11.2, 11.3, 11.4_

  - [x] 9.6 Implement Ionic HTML entry point and styles
    - Create `src/index.html.sbn` with Ionic viewport meta tags, Capacitor script references
    - Create `src/styles.scss.sbn` with Ionic CSS utilities and theme variables
    - _Requirements: 11.4, 14.1, 14.2, 14.3_

  - [x] 9.7 Implement Ionic core infrastructure templates
    - Create `src/app/core/handlers/global-error-handler.tstemplate.sbn`
    - Create `src/app/core/interceptors/auth.interceptor.tstemplate.sbn`
    - Create `src/app/core/interceptors/error.interceptor.tstemplate.sbn`
    - Create `src/app/core/guards/auth.guard.tstemplate.sbn`
    - Create `src/app/core/services/logger.service.tstemplate.sbn`
    - _Requirements: 7.1, 7.2, 7.3, 8.1, 8.2, 8.3, 9.1, 9.2, 10.1, 10.2, 10.3_

  - [x] 9.8 Implement Ionic build tooling and documentation
    - Create `eslint.config.mjs.sbn`, `.gitignore.sbn` (with android/, ios/ exclusions)
    - Replace `README.md.sbn` and `SPEC.md.sbn` stubs with comprehensive templates
    - Create `src/app/app.component.spec.tstemplate.sbn`
    - _Requirements: 13.1, 13.2, 15.1, 15.2, 16.1, 16.2, 17.1, 17.2_

- [x] 10. Checkpoint - Ensure all templates are in place
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 11. Property-based tests (FsCheck)
  - [ ] 11.1 Add FsCheck package reference to test project
    - Add `<PackageReference Include="FsCheck.Xunit" Version="3.*" />` to `Pervaxis.Forge.Engine.Tests.csproj`
    - Create shared test helper/generator for random `ForgeManifest` instances with valid Angular project names, component prefixes, products, and ui_targets combinations
    - _Requirements: 1.1, 2.5, 4.4, 11.1, 15.1_

  - [ ]* 11.2 Write property test: Workspace project keyed by service_name (Property 1)
    - **Property 1: Workspace project keyed by service_name**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/AngularJsonPropertyTests.cs`
    - For any valid service_name, rendered angular.json SHALL contain a projects object with key matching service_name, and that project SHALL contain build, serve, test, lint targets
    - **Validates: Requirements 1.1, 1.2**

  - [ ]* 11.3 Write property test: Mobile-conditional content correlation (Property 2)
    - **Property 2: Mobile-conditional content correlation**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/MobileConditionalPropertyTests.cs`
    - For any template model, mobile-specific content SHALL be present if and only if ui_targets contains "mobile"
    - **Validates: Requirements 1.6, 2.4, 4.5, 5.6, 12.1, 12.2, 12.3, 12.4, 14.3, 16.2**

  - [ ]* 11.4 Write property test: Angular package version consistency (Property 3)
    - **Property 3: Angular package version consistency**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/PackageJsonPropertyTests.cs`
    - For any rendered package.json, all @angular/* packages SHALL share the same major version prefix
    - **Validates: Requirements 2.5**

  - [ ]* 11.5 Write property test: Component prefix interpolation (Property 4)
    - **Property 4: Component prefix interpolation**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/ComponentPrefixPropertyTests.cs`
    - For any valid component_prefix, all rendered files referencing the root component selector SHALL use `{component_prefix}-root`
    - **Validates: Requirements 4.3, 4.4, 5.5, 17.2**

  - [ ]* 11.6 Write property test: Capacitor config model binding (Property 5)
    - **Property 5: Capacitor config model binding**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/IonicMobile/CapacitorConfigPropertyTests.cs`
    - For any valid product and service_name, rendered capacitor.config.ts SHALL have appId derived from product + service_name, and appName containing service_name
    - **Validates: Requirements 11.1**

  - [ ]* 11.7 Write property test: Documentation model variable interpolation (Property 6)
    - **Property 6: Documentation model variable interpolation**
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/DocumentationPropertyTests.cs`
    - For any valid template model, rendered README.md SHALL contain service_name and product, rendered SPEC.md SHALL contain service_name, component_prefix, and ui_targets
    - **Validates: Requirements 15.1, 15.2**

- [ ] 12. Unit tests (example-based)
  - [ ]* 12.1 Write angular-monolith structure tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/AngularJsonStructureTests.cs`
    - Verify production build config values (optimization, sourceMap, outputHashing, namedChunks)
    - Verify bundle budget thresholds, fileReplacements, required script entries
    - Verify TypeScript compiler options (strict, target, module, moduleResolution)
    - _Requirements: 1.3, 1.4, 1.5, 2.3, 3.1, 3.2, 3.3_

  - [ ]* 12.2 Write angular-monolith bootstrap tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/BootstrapTests.cs`
    - Verify main.ts calls bootstrapApplication
    - Verify app.config.ts includes required providers (provideRouter, provideHttpClient, provideAnimationsAsync, ErrorHandler)
    - Verify app.component.ts has valid @Component decorator with standalone: true
    - Verify route structure (root, guarded, wildcard)
    - _Requirements: 4.1, 4.2, 4.3, 7.2, 8.3, 9.2, 9.3_

  - [ ]* 12.3 Write angular-monolith security tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/AngularMonolith/SecurityTests.cs`
    - Verify HTML structure (doctype, head, body, base href, viewport, CSP meta tag)
    - Verify environment file structure (production flag, apiUrl)
    - Verify error handler implements ErrorHandler interface
    - Verify interceptor function signatures
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 6.1, 6.2, 7.1, 8.1, 8.2_

  - [ ]* 12.4 Write ionic-mobile structure and bootstrap tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/IonicMobile/IonicBootstrapTests.cs`
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Templates/IonicMobile/IonicStructureTests.cs`
    - Verify Ionic-specific bootstrap (provideIonicAngular, ion-app, ion-router-outlet)
    - Verify capacitor.config.ts structure
    - Verify Ionic viewport meta tags in index.html
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5_

- [ ] 13. Integration tests
  - [ ]* 13.1 Write end-to-end print generation tests
    - Create `pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Integration/PrintGenerationTests.cs`
    - Generate complete prints via `PrintGenerator` for all three flavors (web, mobile, web+mobile)
    - Extract ZIP contents and verify all expected files are present
    - Verify no empty files are emitted (engine enhancement working)
    - Verify web+mobile has platform.service.ts and capacitor.config.ts, web-only does not
    - _Requirements: 1.1, 2.1, 4.1, 11.1, 12.1_

- [x] 14. Final checkpoint - Ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation
- Property tests validate universal correctness properties from the design document using FsCheck
- Unit tests validate specific examples and edge cases
- The implementation language is C# for engine code/tests and Scriban templates producing TypeScript/Angular output
- Templates use `.sbn` extension, `.tstemplate.sbn` → `.ts` file mapping, and `__APP__` directory placeholder per existing Forge Engine conventions


## Deferred Items (Nice-to-Have / Vertical-Specific)

The following items were raised in the final production readiness review but are NOT blockers for scaffold handoff. They are intentionally left as extension points for vertical teams or future platform iterations:

- [ ]* 15. Real IdP integration (Cognito/Auth0/Azure AD) — vertical-specific, AuthService contract is provided
- [ ]* 16. Wire ErrorReporter to a real service (Sentry/Datadog/CloudWatch RUM) — vertical-specific
- [ ]* 17. Remote log shipping in LoggerService (CloudWatch/Datadog/ELK) — infra-specific
- [ ]* 18. CD pipeline (Docker push, env promotion dev→staging→prod) — infra-specific
- [ ]* 19. Core service unit tests (LoadingService, LoggerService, AuthService, interceptors)
- [ ]* 20. E2E test setup (Playwright or Cypress harness)
- [ ]* 21. Route preloading strategy (PreloadAllModules or custom)
- [ ]* 22. CSP header in nginx (supplement meta tag)
- [ ]* 23. Health check endpoint stub for Kubernetes/load balancers
- [ ]* 24. CI coverage threshold gate
