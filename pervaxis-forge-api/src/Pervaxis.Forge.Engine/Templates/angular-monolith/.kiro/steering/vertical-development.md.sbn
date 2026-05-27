---
inclusion: auto
---
# Vertical Development Conventions

When creating or modifying feature modules in this workspace:

- Use `inject()` function, never constructor injection
- Use `ChangeDetectionStrategy.OnPush` on all components
- Use `ApiService` for HTTP calls (never raw HttpClient)
- Put features in `features/{domain}/`
- Follow the structure in `features/_reference/`
- Write a `.spec.ts` for every `.ts` file
- Use typed interfaces, never `any` or `unknown`
- Apply `authGuard` on all protected routes
- Use CSS custom properties from `styles.scss`
- Export public API from `index.ts` barrel file
- Use signals for component state
- Keep components standalone with explicit imports
