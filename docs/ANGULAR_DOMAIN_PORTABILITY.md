# Angular monolith domain portability V1

Implemented on `feature/angular-domain-portability`, cut directly from fetched `origin/develop` at `6429b79` (merged admin-shell PR #221). Work uses the isolated UI worktree; the shared checkout is untouched.

## Ownership and behavior

The Angular monolith [templates](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/) now emit:

| Location inside the generated app | Owner / purpose |
| --- | --- |
| `platform/shell/` | Print-owned responsive PrimeNG shell and navigation |
| `platform/core/` | Print-owned Cognito PKCE, tenant/session state, guards, HTTP and infrastructure |
| `platform/features/` | Print-owned login, callback, access-denied, starter landing page, administration and not-found pages |
| `platform/shared/` | Print-owned shared controls/utilities |
| `platform/contracts/v1/` | Stable public source facade at `@forge/ui-contracts/v1` |
| `platform/domains/` | Host route/navigation/security adapters; contains no business implementation |
| `domains/<id>/` | Application-owned business manifest/options/routes/services/models/components/tests |
| `.generated/domain-registry.ts` | Disposable deterministic composition artifact outside platform |
| Root component/config/routes | Minimal forwarding exports for Angular tooling |

No business domain is installed by default. The old host-coupled `features/_reference` is replaced by an opt-in [DomainExample](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/DeveloperReference/DomainExample/README.md.sbn), copied unchanged as `domains/work-items`. Existing applications need a one-time conversion of legacy feature imports to V1 contracts; moving old source paths alone is insufficient.

The polished PrimeNG shell, tenant/user administration, existing route URLs, Cognito PKCE, refresh synchronization, session reset and stale-response protections remain. No Canvas rewrite, Material migration, backend contract, profile/password UI/API or TenantAdmin delegation is included. Future Settings stays platform-owned and is not implemented here.

## Registration and compatibility

A domain owns `domain.json`, `domain.config.json` and `routes.ts` exporting `DOMAIN_ROUTES`. It declares its stable ID, display name/icon, contract major, security mode, supported targets, permissions, npm package ranges and required public option keys. Routes/services/models/tests stay inside that folder, use host-independent selectors and API paths, and import platform facilities only through the public facade.

The facade supplies typed HTTP, identity signals, session-reset registration and route-scoped public options. Host adapters are type-checked against those interfaces. Domain stores must clear state on reset/destruction and invalidate or cancel outstanding work; the example includes a late-response regression test. Domain routes in the supported Cognito/tenant host require sign-in, a valid tenant and all declared permissions before lazy matching and child activation. The same manifest filters navigation. PlatformAdmin tenant administration remains tenant-exempt and separately role-guarded. Backend authorization remains authoritative.

Compatibility requires the same contract major, Angular major, security/tenancy assumptions, all receiving targets and installed package APIs. Required public options must be present. New package installation or environment-specific configuration is an explicit prerequisite, as in the [REST portability pattern](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/rest-api/DeveloperReference/Portability.md.sbn); it is not manual platform registration. Options are bundled publicly and cannot contain secrets. Cross-domain implementation imports are prohibited; separately versioned shared packages are the extension point.

The current monolith validator requires Cognito and either web or web+mobile. The reserved public template branch retains anonymous behavior, but this change does not enable new manifest combinations.

## Angular-specific tradeoff

Normal npm build, production build, test, lint and start commands prepare domains before invoking Angular. A Node scanner validates manifests/import boundaries and writes literal lazy imports in ordinal folder order. The development wrapper also detects folder/options changes. No feature code executes during discovery. No handwritten platform, root wiring, route-list or navigation-list edits are needed for a compatible copy.

Raw `ng build/serve/test` bypass preparation and are documented as low-level commands; use the generated npm scripts or run `npm run domains:prepare` first. This is the deliberate build-time integration point, not a runtime plugin loader. Angular's application builder requires analyzable imports; its internal Vite development server is not a public `import.meta.glob` contract. See [Angular build-system guidance](https://angular.dev/tools/cli/build-system-migration) and [lazy route definitions](https://angular.dev/guide/routing/define-routes).

Preparation rejects private/cross-domain imports, private type imports, triple-slash path escapes, unowned business sources, escaping component resources, nonliteral imports, mismatched IDs/reserved routes, versions, targets and security modes. It uses the TypeScript parser/resolver, not text-only import matching. These are architecture checks for trusted application source, not a hostile-code sandbox.

## Verification completed

- Entire engine suite: **150 passed**. After final template adjustments, **18 focused ownership/auth/admin/provisioning tests passed** again.
- API generation service suite: **12 passed**; changes there only update expected generated paths.
- Generated architecture suite: **17 passed**, including deliberately invalid domain fixtures.
- Source and differently named receiving print: **78 headless Edge tests passed each**. Final receiving run passed after the last adapter/lint adjustments. Tests include PKCE/session behavior, tenant/permission guards, administration stale responses, domain lazy routes/options/navigation, permission updates and copied service reset handling.
- Receiving print lint: **passed, no errors/warnings**. Portable domain selector rules use a stable `domain-` prefix instead of the generated host prefix.
- Actual unchanged domain-folder copy from `source-web` to `fresh-web`: both production AOT builds passed; copied file hashes matched; every receiving platform file hash stayed unchanged. The generated [verify-copy script](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/scripts/verify-copy.mjs.sbn) makes that proof repeatable.
- Actual local Edge smoke on the receiving shell: generated navigation via keyboard opened the copied lazy route and displayed its data; active-link state, theme persistence, drawer Escape/focus restoration, and no horizontal overflow at 1440/390/320px passed with no page errors. Identity/data were mocked only in disposable bootstrap wiring; platform/domain files stayed unchanged and normal bootstrap was restored afterward.
- Combined web+mobile monolith production build passed, preserving the existing target-selection/template behavior. Separate Ionic templates are untouched. No native Android/iOS device build is claimed.

No live Cognito/backend invitation, CRUD or AWS operation was rerun. Auth/admin verification here is deterministic browser tests and local mocked smoke; previously live-verified backend contracts were not changed. No deployment or merge was performed.

## Upgrade acceptance

Generate a fresh compatible print, compare its public contract/security/target and package metadata, copy the domain folder unchanged, reconcile declared dependencies/public configuration, then run production build, architecture tests, domain/platform tests and lint. Verify route/navigation registration and authorization in the receiving app. Keep fresh platform files and deployment-specific auth settings. For an actual deployment, repeat sign-in/refresh/sign-out and tenant-isolated API acceptance in the normal staging environment.
