# Workspace CRUDA reference UI

Web Angular monolith prints with Cognito include a Workspaces screen at `/workspace/workspaces`. It is the reusable list, create, read, update, delete and audit (CRUDA) pattern for future domain modules. It is UI-first and runs on explicit mock data. No Workspaces API, database or AWS resource exists.

## Where it lives

Templates: `pervaxis-forge-api/src/Pervaxis.Forge.Engine/Templates/angular-monolith/apps/__APP__/src/app/domains/workspace/workspaces/`

| Folder | Contents |
|---|---|
| `contracts/` | Record, draft, member, audit and view-state types |
| `constants/` | Field limits, page sizes, statuses, plans, toast copy |
| `sources/` | `WorkspaceRecordsData` port, mock source, unconfigured source, route providers |
| `state/` | Route-scoped Signals state for list, filters, selection, drawer, delete and toasts |
| `components/` | AG Grid Community grid and row actions, status badge, drawer, form, detail tabs, delete dialog, mock preview controls |
| `pages/` | The Workspaces page |

The folder is named `sources/` rather than `data/` because the generator drops every `/Data/` template path when a manifest has no database.

The existing Workspace Tasks screen (`tasks/`) is unchanged. It keeps its live API integration, and it does not import anything from `workspaces/`.

## Mock mode

- Mock mode is selected only when `domain.config.json` contains `"workspaceRecordsMode": "mock"`. The template sets that value.
- Any other value selects `UnconfiguredWorkspaceRecordsData`. It fails visibly with reference `ERR-WS-UNCONFIGURED`.
- Mock data is never a fallback for API or authentication failures.
- In the UI, the following are labelled "mock" and exist only in the browser session: records, Members, the Audit timeline, export, audit-log export and restore after delete.
- When the mock source is active, the page shows a mock banner and review controls. The controls switch the list between data, loading, empty, error and access denied. They can also force the next mutation to fail.

To add a live backend later, provide a `WorkspaceRecordsData` implementation in `sources/workspace-records.providers.ts`. Components and state do not change.

## Platform and package changes

- `package.json` and the domain's `domain.json` declare `ag-grid-angular` and `ag-grid-community` `~36.3.0` (MIT). This applies to web admin prints only. Only `AllCommunityModule` is registered. No AG Grid Enterprise, Ng-Matero or Canvas is used.
- Domain manifests (UI contract V1) accept an optional `navigationItems` list of `{ label, icon, path }`. It is additive: when it is present, the sidebar shows one link per item instead of the single `navigationPath` link. `scripts/domain-tool.mjs` validates it. The Workspace domain uses it to show "My Workspace" (Tasks) and "Workspaces (mock)".
- The grid uses `themeQuartz`, with colors taken from the shared `--color-*` and `--p-*` tokens, so light and dark themes follow the app.

## Behaviour covered

- **Page:** header with breadcrumb, title, description and New workspace.
- **Summary cards:** totals, active, suspended and members.
- **Toolbar:** search, status and plan filters, removable chips, Export (mock) and Refresh.
- **Grid:** sorting, multi-row selection (header box selects the current page), status badges, row Edit and Delete, pagination with a page-size selector, and Enter to open a row.
- **Bulk bar:** Suspend, Export (mock), Delete and Clear selection.
- **Create/Edit drawer:**
  - Grouped fieldsets with required markers and hints.
  - Inline validation and duplicate-code checks.
  - An error summary that takes focus and links to its fields.
  - A Saving state, and server errors with a reference code.
- **Read drawer:** Overview, Members (mock) and Audit timeline (mock) tabs.
- **Delete dialog:**
  - A single delete types the workspace code when it has members.
  - A bulk delete lists names and types `DELETE` when members exist.
  - After deleting, a mock Restore is offered.
- **Toasts:** they use the same verb as the action, such as "Workspace created", "Changes saved", "Workspaces suspended", "Workspace deleted" and "Workspaces deleted". Failures read "Couldn't …", with a reference code.
- **States:** loading skeleton, empty, no results, error with reference code, and access denied.
- **Accessibility:**
  - Keyboard grid navigation and visible focus.
  - Esc closes the drawer and dialog.
  - Focus returns to the opener, or to the heading after a delete.
  - Reduced motion is respected, and the layout is responsive down to 390px.

## Verification

- **Generator:** `WebAdminLayoutGenerationTests.WorkspaceCrudaReferenceStaysInsideDomainAndWebOnly` checks:
  - the file set and dependencies;
  - explicit mock mode;
  - that the Tasks screen does not use mock data;
  - that no Enterprise, Ng-Matero or platform imports appear;
  - that combined web/mobile prints exclude the screen.
- **Generated print:** `npm run build`, `npm run lint`, `npm test` (Karma, including new state and page specs) and `npm run test:architecture`.
- **Browser:** a local Playwright/Edge pass with a fake sign-in, at 1440px and 390px, in light and dark themes, and with reduced motion.

Live Cognito, a live Workspaces API and native targets were not exercised. None of them is part of this change.
