# Print Developer Guide — Building Domain Modules with Pervaxis Canvas

This guide shows print developers (application-layer engineers) how to build domain
modules that follow Canvas conventions. The reference implementation is
`libs/domain/customer/` — read it alongside this guide.

---

## 1. What Is a Domain Module?

A domain module owns one bounded context of business logic. For the `customer` domain
that means:

| Layer | File | Purpose |
|---|---|---|
| Models | `models/customer.model.ts` | TypeScript interfaces — no runtime code |
| API service | `services/customer-api.service.ts` | HTTP CRUD via `HttpClient` |
| Store | `state/customer.store.ts` | NgRx Signals reactive state |
| Pages | `pages/*/customer-*.page.ts` | Route components (List, Detail, Form) |
| Routes | `routes/customer.routes.ts` | Lazy route config with guards |
| i18n | `assets/i18n/en.customer.json` | All user-visible strings |

---

## 2. Generating the Library Scaffold

```bash
# From the workspace root
nx g @nx/angular:library \
  --name=canvas-domain-<your-domain> \
  --directory=libs/domain/<your-domain> \
  --standalone \
  --unitTestRunner=vitest-analog
```

Then add the path alias to `tsconfig.base.json`:

```json
"@pervaxis/canvas-domain-<your-domain>": [
  "./libs/domain/<your-domain>/src/index.ts"
]
```

And register the lcov path in `sonar-project.properties`:

```
coverage/libs/domain/<your-domain>/lcov.info,\
```

---

## 3. Domain Models

Define all TypeScript interfaces in `models/<domain>.model.ts`. Use `interface` for
data shapes and `type` for unions (e.g. status enums).

```typescript
// ✅ Full record returned by the API
export interface Customer { id: string; code: string; name: string; ... }

// ✅ Lightweight projection for list/grid views
export interface CustomerListItem { id: string; code: string; name: string; ... }

// ✅ Separate DTOs for create and update
export interface CreateCustomerDto { code: string; name: string; ... }
export interface UpdateCustomerDto { name?: string; ... }
```

**Rules:**
- No `any` — TypeScript strict mode is enforced
- Keep models flat; avoid circular references
- Export all interfaces from `src/index.ts`

---

## 4. API Service Pattern

Use Angular's `HttpClient` (not a wrapper). The Canvas HTTP interceptors (retry,
timeout, correlation ID, error normalisation) are already active at the shell level.

```typescript
@Injectable({ providedIn: 'root' })
export class CustomerApiService {
  readonly #http = inject(HttpClient);

  list(filter?: CustomerFilter): Observable<CustomerPage> {
    let params = new HttpParams();
    if (filter?.search) params = params.set('search', filter.search);
    return this.#http.get<CustomerPage>('/api/customers', { params });
  }

  getById(id: string): Observable<Customer> {
    return this.#http.get<Customer>(`/api/customers/${id}`);
  }

  create(dto: CreateCustomerDto): Observable<Customer> {
    return this.#http.post<Customer>('/api/customers', dto);
  }

  update(id: string, dto: UpdateCustomerDto): Observable<Customer> {
    return this.#http.patch<Customer>(`/api/customers/${id}`, dto);
  }

  remove(id: string): Observable<void> {
    return this.#http.delete<void>(`/api/customers/${id}`);
  }
}
```

**Rules:**
- Always type the response generic: `get<MyType>(...)`
- Build `HttpParams` programmatically — never string-concatenate query strings
- Do NOT call `provideCanvasHttp()` inside the domain — it is a shell concern

---

## 5. NgRx Signals Store Pattern

```typescript
interface CustomerState {
  customers: CustomerListItem[];
  selected: Customer | null;
  loading: boolean;
  error: string | null;
}

export const CustomerStore = signalStore(
  { providedIn: 'root' },
  withState<CustomerState>({ customers: [], selected: null, loading: false, error: null }),
  withComputed(({ customers }) => ({
    hasCustomers: computed(() => customers().length > 0),
  })),
  withMethods((store, api = inject(CustomerApiService)) => ({
    async loadAll(): Promise<void> {
      patchState(store, { loading: true, error: null });
      try {
        const result = await firstValueFrom(api.list());
        patchState(store, { customers: result.items, loading: false });
      } catch (err) {
        patchState(store, { loading: false, error: String(err) });
      }
    },
  }))
);
```

**Rules:**
- Always set `loading: true` before the async call and `loading: false` after
- Always set `error: null` at the start of each action
- Always catch errors and store them in `error`
- Use `firstValueFrom()` — never `.subscribe()` inside the store
- Inject services via the second parameter of `withMethods`: `api = inject(ApiService)`

---

## 6. Page Components

### List Page

```typescript
@Component({
  selector: 'customer-list',
  standalone: true,
  imports: [TranslocoPipe, HasPermissionDirective, PageComponent, DataViewComponent, CanvasGridComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <canvas-page [title]="'customer.list.title' | transloco">
      <div canvas-page-actions>
        <button *hasPermission="'customers:write'" (click)="navigateToCreate()">
          {{ 'customer.action.create' | transloco }}
        </button>
      </div>
      <canvas-data-view [loading]="store.loading()" [empty]="!store.hasCustomers()">
        <canvas-grid [rowData]="store.customers()" [columnDefs]="columnDefs"
          (rowClicked)="onRowClicked($event)" />
      </canvas-data-view>
    </canvas-page>
  `,
})
export class CustomerListPage implements OnInit {
  protected readonly store = inject(CustomerStore);
  private readonly router = inject(Router);

  protected readonly columnDefs: ColDef<CustomerListItem>[] = [
    { field: 'code' }, { field: 'name' }, { field: 'email' },
  ];

  ngOnInit() { void this.store.loadAll(); }

  protected onRowClicked(e: RowClickedEvent<CustomerListItem>): void {
    if (e.data?.id) void this.router.navigate(['/customers', e.data.id]);
  }
}
```

### Form Page — create vs edit

Detect edit vs create from route params. Use `@if (!store.loading())` to gate the
form so it only renders once the customer data is loaded:

```typescript
protected readonly formSchema = computed((): FormSchema => {
  const c = this.store.selected(); // null for create, Customer for edit
  return { fields: [{ key: 'name', type: 'text', defaultValue: c?.name ?? '' }] };
});
```

---

## 7. Permission Integration

### Structural directive (template-level)

```html
<!-- Show button only for users with customers:write -->
<button *hasPermission="'customers:write'">Create</button>

<!-- Require ALL permissions -->
<div *hasPermission="['customers:read', 'reports:export']">Export</div>
```

### Route guard (navigation-level)

```typescript
{
  path: 'create',
  canActivate: [authGuard, permissionGuard],
  data: { permissions: ['customers:write'] },
  loadComponent: () => import('./customer-form.page').then(m => m.CustomerFormPage),
}
```

---

## 8. Internationalisation (i18n)

All user-visible strings must use the `transloco` pipe — never hardcode labels.

**1. Create the translation file:**

```json
// src/assets/i18n/en.<domain>.json
{
  "customer": {
    "list": { "title": "Customers" },
    "action": { "create": "Create Customer" }
  }
}
```

**2. Use in templates:**

```html
<h1>{{ 'customer.list.title' | transloco }}</h1>
<button>{{ 'customer.action.create' | transloco }}</button>
```

**3. Register with the shell's `provideCanvasI18n()`:**

The shell resolves translation files from `assets/i18n/`. Name your file
`en.<domain>.json` to keep them discoverable and avoid key collisions.

---

## 9. Unit Testing

### API service

Use `HttpTestingController` to intercept requests:

```typescript
it('sends GET /api/customers', () => {
  service.list().subscribe();
  const req = http.expectOne('/api/customers');
  expect(req.request.method).toBe('GET');
  req.flush(MOCK_PAGE);
});
```

### Store

Mock the API service with `vi.fn()`:

```typescript
const mockApi = { list: vi.fn().mockReturnValue(of(MOCK_PAGE)) };
TestBed.configureTestingModule({
  providers: [CustomerStore, { provide: CustomerApiService, useValue: mockApi }],
});
const store = TestBed.inject(CustomerStore);
await store.loadAll();
expect(store.customers()).toEqual([MOCK_ITEM]);
```

### Page components

Override heavy imports (TranslocoPipe, ag-Grid, etc.) with stub components
to keep tests fast:

```typescript
@Pipe({ name: 'transloco', standalone: true, pure: true })
class MockTranslocoPipe implements PipeTransform { transform = (k: string) => k; }

TestBed.configureTestingModule({ imports: [CustomerListPage], providers: [...] })
  .overrideComponent(CustomerListPage, {
    remove: { imports: [TranslocoPipe] },
    add: { imports: [MockTranslocoPipe] },
  });
```

---

## 10. Reference Implementation

The complete working example lives at `libs/domain/customer/`. Read it for:

| Topic | File |
|---|---|
| Domain models | `src/lib/models/customer.model.ts` |
| HTTP CRUD | `src/lib/services/customer-api.service.ts` |
| Signals store | `src/lib/state/customer.store.ts` |
| List page | `src/lib/pages/customer-list/customer-list.page.ts` |
| Detail page | `src/lib/pages/customer-detail/customer-detail.page.ts` |
| Form page | `src/lib/pages/customer-form/customer-form.page.ts` |
| Route config | `src/lib/routes/customer.routes.ts` |
| i18n file | `src/assets/i18n/en.customer.json` |
| API tests | `src/lib/services/customer-api.service.spec.ts` |
| Store tests | `src/lib/state/customer.store.spec.ts` |
| Page tests | `src/lib/pages/*/customer-*.page.spec.ts` |
