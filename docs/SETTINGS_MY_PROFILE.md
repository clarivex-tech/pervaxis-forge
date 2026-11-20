# Settings / My Profile delivery

## Baseline and scope

Implementation is on `feature/ui-profile-settings`, cut from fetched `origin/develop` at `56d281a3e9aa8a68e352b59c5b1bc190610bd33b` (API self-profile merge). Delivery is confined to this feature branch; no merge or deployment is included. The shared checkout, live services, AWS and Downloads were not changed.

This adds Settings to reusable Angular-monolith **web admin-shell** prints, including default empty UI targets and explicit web targets. Combined web/mobile retains its existing shell; standalone Ionic templates are untouched. There is no Canvas rewrite, Material migration, backend change, password UI or profile editing.

## Delivered behavior

- Header Account menu offers Settings and direct Sign out. Tenant/user administration remains separate and role protected.
- Settings is a lazy platform route protected by the existing authentication guard. It intentionally omits tenantGuard so identity and Appearance remain available when application tenant admission rejects `/me`.
- My Profile displays name/email only from the existing AuthService sign-in profile. Missing values display "Not provided"; displayName and API extras cannot supply identity attributes.
- Application access uses the existing authenticated ApiService `GET /api/v1/me`, accepting `{ subject, tenant: { id, name }, roles: string[] }`. The subject must match the signed-in identity. Empty roles remain empty, role case is preserved, and failed requests never substitute token tenant/roles.
- Loading, refresh/retry, network failure, malformed/mismatched responses, 401 and 403 have explicit states. Error handling does not assume an MVC response body or expose backend bodies.
- Context is component-scoped and held only in memory. SessionStateRegistry resets, component destruction, request epochs and subject/tenant snapshots prevent late responses from restoring stale context. Existing PKCE, refresh, interceptors, guards and backend contracts are retained.
- Appearance uses the existing ThemeService and browser-local System/Light/Dark preference. No new identity-provider calls, OAuth scopes or IAM permissions are introduced.

Application roles describe the accepted token and admitted tenant context, not live Cognito group membership or revocation. Navigation visibility is not authorization; existing guards and backend enforcement remain authoritative.

## Production CSS correction

Production browser verification found that Angular's default critical-CSS optimization emitted a stylesheet loader using an inline `onload` handler. The existing strict CSP blocked that handler: PrimeNG changed themes while the full custom stylesheet never loaded. Web admin-shell production configuration now disables critical-CSS inlining while retaining script/style minification and the existing CSP. Combined-target optimization is unchanged. The tradeoff is a normal blocking stylesheet rather than critical-CSS inlining.

The account-menu control uses a unique bound ID, explicit expanded/menu semantics and PrimeNG keyboard behavior. Skip and section links retain the Settings route and move focus correctly. The context refresh control has an explicit accessible label, excluding the PrimeIcons font glyph from its accessible name.

## Ownership

- [Settings templates and focused tests](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/apps/__APP__/src/app/platform/features/settings/)
- [Platform route registration](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/apps/__APP__/src/app/platform/platform.routes.tstemplate.sbn)
- [Shell and account menu](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/apps/__APP__/src/app/platform/shell/)
- [Production build configuration](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/angular.json.sbn)
- [Generator regression tests](../pervaxis-forge-api/tests/Pervaxis.Forge.Engine.Tests/Generation/SettingsGenerationTests.cs)
- [Receiving-print documentation](../pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/SETTINGS.md.sbn)

Domain contracts, preparation scripts and domain registration remain unchanged. Settings belongs to platform infrastructure. A copied example domain still registers as a lazy route and builds alongside Settings.

## Verification

Verified using disposable rendered prints inside the isolated worktree, with normal bootstrap/runtime configuration restored after browser fixtures:

- Web production build and lint pass; Settings remains a separate lazy chunk (approximately 11.6 kB raw). No budget warnings.
- Generated Angular tests: **103 passed**, including read-only identity/context separation, malformed responses, error statuses, reset/destruction/owner changes, request races, menu semantics, focus and existing auth/admin tests. Headless Edge emitted a launcher shutdown warning after reporting success; command exited zero.
- Domain architecture tests: **24 passed**, including copy/removal, reserved route, dependency/config/target validation and boundaries. A copied example domain was included in the web production build.
- Full Engine test suite: **154 passed** (including generator and API portability integration tests). Focused generator/auth/admin/provisioning tests: **23 passed**, covering default/explicit web, combined-target exclusion, unchanged admin/domain registration, scope restrictions, CSP-compatible production configuration and the existing rejection of monolith generation without Cognito configuration.
- Combined-target production build passes with its existing shell and optimization. Native device compilation was not performed.
- Production browser smoke passes using local identity/API fixtures and the real ApiService/auth HTTP interceptors: administrator `clv`/`PlatformAdmin`, ordinary user `Philosophy`/empty roles, missing identity fields, subject mismatch, raw-body 401/403, network failure/retry and pending response after sign-out. All 20 recorded application requests were authenticated `GET /api/v1/me`; no write or identity-provider endpoint was used.
- Keyboard smoke covers account-menu entry/Escape/focus return, Settings selection, direct sign-out, skip link, section links and theme selection/persistence. Layout checked at 1440, 390 and 320 px without horizontal overflow. Axe WCAG 2 A/AA and 2.1 AA checks reported zero violations for tested menu, light/dark and narrow states. Mobile screenshot was also visually reviewed.

Final commit review aligned the context button's explicit accessible name with its visible Loading/Refresh/Load/Retry label and removed a trailing blank line. The 23 focused generator/auth/admin checks passed again afterward; the full Angular/browser verification above preceded this label-only refinement.

Run generated-print checks through supported npm scripts, which also prepare domain registration: `npm run build:prod`, `npm run lint`, `npm test -- --watch=false --browsers=ChromeHeadless`, `npm run test:architecture`.

These checks do not establish live deployment connectivity, current Cognito app-client attributes/permissions, real browser assistive-technology behavior or native device behavior. No live AWS or backend mutation was performed. After deployment through the normal release process, operator acceptance should verify real sign-in, `/me` context, sign-out and theme persistence for an administrator and ordinary user. No new scopes are needed for this implementation.
