# Reference Module

> **This is a reference module for learning and copying. Do NOT commit this folder.**

## Purpose

This `_reference/` folder is a complete, working example of a vertical feature module.
It demonstrates every pattern you need: models, services, components, routing, mocking, and testing.

## How to Use

1. **Copy** this entire `_reference/` folder
2. **Rename** the copy to your domain name (e.g., `assets/`, `orders/`, `inspections/`)
3. **Find and replace** `sample` → your domain name (e.g., `asset`)
4. **Find and replace** `Sample` → your domain name PascalCase (e.g., `Asset`)
5. **Customize** the model interface to match your API contract
6. **Update** `app.routes.ts` to add your new feature routes
7. **Delete** this `_reference/` folder when you're done (it's in `.gitignore` anyway)

## What's Inside

| File | Purpose |
|------|---------|
| `models/sample.model.ts` | Domain interfaces and DTOs |
| `services/sample.service.ts` | API calls via ApiService |
| `services/sample.service.spec.ts` | Service unit tests |
| `pages/sample-list/` | List view with table |
| `pages/sample-form/` | Create/Edit form with validation |
| `pages/sample-detail/` | Detail view for a single item |
| `mocks/sample-mock.interceptor.ts` | Dev-only mock HTTP interceptor |
| `constants/sample.constants.ts` | Route paths, API paths, permissions |
| `routes/sample.routes.ts` | Lazy-loaded routes with guards |
| `index.ts` | Barrel exports |

## Conventions

- Use `inject()` — never constructor injection
- Use `ChangeDetectionStrategy.OnPush` on all components
- Use `ApiService` for HTTP — never raw `HttpClient`
- Use signals for component state
- Keep components standalone
- Write a `.spec.ts` for every `.ts` file
