# Domain Library Conventions — Pervaxis Canvas

Coding rules that every domain/print developer must follow when building
application-layer modules on top of the Canvas platform libraries.

---

## Naming

| Item | Convention | Example |
|---|---|---|
| Library name | `canvas-domain-<noun>` | `canvas-domain-customer` |
| npm package | `@pervaxis/canvas-domain-<noun>` | `@pervaxis/canvas-domain-customer` |
| Lib directory | `libs/domain/<noun>/` | `libs/domain/customer/` |
| Component selector | `<noun>-<role>` | `customer-list`, `customer-form` |
| Service class | `<Noun>ApiService` | `CustomerApiService` |
| Store | `<Noun>Store` | `CustomerStore` |
| Page class | `<Noun><Role>Page` | `CustomerListPage` |
| Route const | `<NOUN>_ROUTES` | `CUSTOMER_ROUTES` |
| i18n file | `en.<noun>.json` | `en.customer.json` |
| i18n key prefix | `<noun>.<section>.<key>` | `customer.list.title` |

---

## File Structure

```
libs/domain/<noun>/
├── src/
│   ├── lib/
│   │   ├── models/           # Interfaces only — no classes
│   │   ├── services/         # One ApiService per domain noun
│   │   ├── state/            # One NgRx Signals store per domain noun
│   │   ├── pages/
│   │   │   ├── <noun>-list/  # Grid/table view
│   │   │   ├── <noun>-detail/# Read-only single record view
│   │   │   └── <noun>-form/  # Create + Edit form (single component)
│   │   └── routes/           # ROUTES constant only — no logic
│   ├── assets/
│   │   └── i18n/
│   │       └── en.<noun>.json
│   └── index.ts              # Barrel export
├── project.json
├── package.json              # private: true
└── ... (tsconfig / vite)
```

---

## TypeScript

- **No `any`** — TypeScript strict mode is enforced globally
- **No non-null assertion** (`!`) in templates — use `@if`, optional chaining, or default values
- **Separate interfaces for list projections** — never expose the full record in grid `rowData`
- **Separate DTO interfaces** — `CreateCustomerDto` must not extend `Customer`
- **Export all public types** via `src/index.ts` barrel

---

## Angular Components

- **Standalone always** — `standalone: true` on every component/directive/pipe
- **OnPush always** — `changeDetection: ChangeDetectionStrategy.OnPush`
- **`inject()` not constructor** — use `inject(Service)` in the class body
- **No template logic** — compute values in the class; keep templates declarative
- **Selector prefix matches domain** — `customer-*` for the customer domain

---

## State Management

- **One store per domain noun** — `CustomerStore` owns all customer state
- **Always set `loading: true` before async calls** — prevents stale UI
- **Always reset `error: null` at start of each action** — clears previous errors
- **Use `patchState`** — never mutate state objects directly
- **Use `firstValueFrom()`** — never `.subscribe()` inside a store method
- **Re-export the store type** — `export type CustomerStoreType = InstanceType<typeof CustomerStore>`

---

## HTTP

- **Use Angular's `HttpClient`** — not Fetch, Axios, or custom wrappers
- **Build `HttpParams` programmatically** — never string-concatenate query strings
- **Do NOT call `provideCanvasHttp()`** in domain libs — shell provides it
- **Do NOT add retry/timeout logic** — Canvas interceptors handle it globally
- **Type all responses** — `this.#http.get<MyType>(...)`

---

## Permissions

- **Gate UI elements with `*hasPermission`** — not with `ngIf` and manual checks
- **Gate routes with `permissionGuard`** — always declare `data: { permissions: [...] }`
- **Always also include `authGuard`** — permission guard does not redirect to login
- **Permission strings follow the pattern** `<resource>:<action>`, e.g. `customers:write`

---

## Internationalisation

- **All user-visible strings in `en.<domain>.json`** — no hardcoded strings in templates
- **Use the `transloco` pipe** — `{{ 'customer.list.title' | transloco }}`
- **Key hierarchy** — `domain.section.key` e.g. `customer.form.submit`
- **Status/enum labels** — use a key per value: `customer.status.active`

---

## Testing

- **90%+ line coverage** — enforced in CI via `vitest --coverage`
- **API service** — use `HttpTestingController`; test each method and each optional param
- **Store** — mock the ApiService with `vi.fn()`; test success AND error paths for each method
- **Page components** — stub heavy imports (TranslocoPipe, ag-Grid) using `TestBed.overrideComponent`
- **Permission tests** — set `AuthContextService` context to test show/hide of guarded elements
- **No `NO_ERRORS_SCHEMA`** — always provide stubs for unknown elements

---

## What NOT to do

| ❌ Avoid | ✅ Instead |
|---|---|
| `constructor(private svc: Service)` | `private readonly svc = inject(Service)` |
| `ChangeDetectionStrategy.Default` | `ChangeDetectionStrategy.OnPush` |
| `.subscribe()` in store methods | `await firstValueFrom(obs$)` |
| `any` types | Typed interfaces |
| Hardcoded labels in templates | `transloco` pipe with i18n keys |
| `ngIf` for permission checks | `*hasPermission` directive |
| `NgModule` declarations | `standalone: true` + imports array |
| Duplicate model interfaces | Reference `@pervaxis/canvas-mfe-contracts` |
| `provideCanvasHttp()` in domain | Shell-level concern only |
