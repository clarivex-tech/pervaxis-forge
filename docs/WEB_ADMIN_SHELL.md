# Generated web administration shell

The Angular monolith web-only print owns this PrimeNG layout. Ng-Matero informed the layout direction; no Ng-Matero code, Material shell, packages, demo authentication, or charts are included.

## Ownership and reuse

Local Canvas implementation was inspected in the sibling pervaxis-canvas repository: shell/layout exports a standalone ShellLayoutComponent, NavigationComponent, NavItem inputs and CSS customization tokens. Those libraries are not currently consumed by the monolith. This change keeps the reusable source in Forge's angular-monolith templates and does not rewrite Canvas or introduce a dependency on its auth/routing services. Features continue to use lazy routes, existing API services, guards and SessionStateRegistry.

The default navigation includes Dashboard and a PlatformAdmin-only Tenants and users link. Route guards and the backend remain authoritative. Tenant selection in administration does not change the authenticated tenant. Profile/password endpoints and TenantAdmin delegation are outside this change.

## Web behavior

- Persistent desktop navigation, collapsible to labeled icons; modal PrimeNG Drawer below 1024px, closed on navigation, resize and session reset.
- Skip link, main landmark, current-page links, labeled controls, visible focus, scrollable users table and reduced-motion support.
- System/light/dark appearance preference stored separately from auth in pervaxis.appearance. PrimeNG and custom colors share the document dark-mode class. No identity or tenant data is persisted by appearance settings.
- Existing create/rename/lifecycle actions, invitations, resend and pagination use unchanged backend contracts. Administration state reset and late-response epoch checks remain in place.

## Target boundary

The renderer's explicit web_admin_layout flag enables these changes only for a Monolithic manifest with uiTargets [web]. Mobile and combined output retain baseline templates. The existing renderer does not expose manifest.ui_targets even though older templates reference it; fixing that existing native-target behavior is a separate task because changing it would alter generated output broadly.

## Verification

Generator regression tests cover layout selection, unchanged component dependencies, protected admin routing and reset/epoch preservation. Render a web print and run its production build and generated Karma tests. The generated root tests use mock identity/services; they do not establish live Cognito or AWS behavior. Use a local mock administration API for responsive and keyboard checks. Keep the verified auth implementation unchanged; repeat live acceptance through the normal deployment process before release.

Combined-target output was compared to develop baseline for the shell, administration and global styles after whitespace normalization. Native UI was not redesigned or device-certified.

### Verification performed for this change

- Rendered a web monolith, combined web/mobile monolith and separate Ionic print.
- Web production build passed within existing bundle/style budgets (about 739 kB initial, 173 kB estimated transfer).
- All 81 generated browser tests passed in headless Edge, including authentication/session, guards, administration, appearance and shell tests.
- Generator suite passed 83 existing generation checks; 14 focused checks also passed after adding two web/combined layout regressions.
- Local Playwright checks with mock identity and administration services passed at 1440px and 390px: tenant selection, loading users, theme selection, no page-width overflow, drawer focus containment, Escape dismissal and focus restoration.
- Combined shell, configuration, administration and global styles matched develop baseline with whitespace normalized.

Live Cognito/invitation delivery and native device behavior were not reverified. No live AWS changes were made.
