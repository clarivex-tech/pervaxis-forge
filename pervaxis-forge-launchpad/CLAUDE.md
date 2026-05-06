# Pervaxis Forge — Launchpad

## Project Context

Forge Launchpad is an **admin-only Angular 21 web application** for enrolling business verticals and generating production-ready service scaffolds. It is part of Pervaxis Forge — an internal platform tool at Clarivex Technologies.

Before writing any code, read:
- `docs/FORGE_SOLUTION_STRUCTURE.md` — folder layout, naming conventions
- `docs/FORGE_BLUEPRINT_UI.md` — phase-by-phase implementation plan
- `docs/FORGE_TECHNICAL_SPECIFICATION.md` — API contracts and data models

---

## Architecture

### Application Structure
```
core/         Singleton services, guards, interceptors — never imported by features directly
features/     One folder per route — lazy-loaded, self-contained
shared/       Components, pipes, directives used by 2+ features only
environments/ environment.ts (mock API) and environment.prod.ts (real API)
```

### State Management
- Use **Angular Signals** exclusively — `signal()`, `computed()`, `effect()`
- No NgRx, no BehaviorSubject for UI state
- Wizard state lives in dedicated `*.state.ts` files using typed Signals
- Persist wizard state to `sessionStorage` so page refresh does not lose progress

### Routing
- All feature routes are **lazy-loaded** — always use `loadComponent()` or `loadChildren()`
- Never eagerly import a feature component into `app.config.ts`
- Route guards use the functional `CanActivateFn` pattern — no class-based guards

### HTTP
- All API calls go through services in `core/api/`
- `environment.useMockApi` flag toggles between `MockVerticalApiService` and `VerticalApiService` — zero code change to swap
- Mock services must implement the same interface as real services
- Debounce user-triggered API calls (slug uniqueness check, naming preview) at 300ms

---

## Technology Constraints

| Concern | Choice | Do NOT use |
|---|---|---|
| UI components | Angular Material 21.2.9 | Tailwind, Bootstrap, custom CSS frameworks |
| State | Angular Signals | NgRx, BehaviorSubject, Redux |
| Build | Nx — use `npx nx` commands | `ng` CLI directly |
| Testing (unit) | Karma + Jasmine | Vitest, Jest |
| Testing (E2E) | Cypress | Playwright |
| Styling | SCSS + Angular Material theming | Tailwind, Emotion, styled-components |
| Icons | Angular Material icons (`mat-icon`) | FontAwesome, custom SVG sprites |
| Platform libraries | None in Launchpad | Canvas libraries (Canvas is for generated code only) |

---

## File Header

Every `.ts` file must start with this exact license header — no exceptions:

```typescript
/**
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */
```

---

## TypeScript Standards

- **Strict mode is on** — no `any`, no implicit `any`, no non-null assertion (`!`) unless genuinely impossible to be null
- Define TypeScript interfaces for every API request and response in `core/models/`
- Use `readonly` on Signal inputs and immutable state shapes
- Prefer `interface` over `type` for object shapes; use `type` only for unions and primitives
- Always type HTTP observable returns: `this.http.get<VerticalResponse[]>(...)`

---

## Component Standards

- **Standalone components only** — no `NgModule` declarations (except where Nx scaffolding requires it)
- One component per folder — `.ts`, `.html`, `.scss` co-located
- Use `OnPush` change detection on all components
- Keep templates clean — no complex logic in HTML; extract to `computed()` signals or component methods
- Use Angular Material Stepper for all wizards — do not build custom stepper
- All form fields must use `MatFormField` + `MatInput` — no raw `<input>` elements

---

## Accessibility

- All form inputs must have `<mat-label>` and `aria-describedby` pointing to error messages
- Buttons must have descriptive labels — no icon-only buttons without `aria-label`
- Wizard steps must be keyboard-navigable
- Colour contrast must meet WCAG 2.2 AA — use Angular Material's theming system, do not override colours inline
- All provider/module selection cards must be reachable and selectable via keyboard

---

## Forms & Validation

- Use **Reactive Forms** (`FormGroup`, `FormControl`) — no Template-driven forms
- Define validators as pure functions in the component or a `validators.ts` file
- Kebab-case validation: reuse `KebabCaseValidatorDirective` from `shared/directives/`
- Show errors only when the control is `touched` or the form has been submitted
- Debounce async validators (slug uniqueness) — do not fire on every keystroke

---

## Testing

- **Coverage target: 85%+** on all components and services
- Test every Signal-based computed value independently
- Test error states explicitly — do not only test the happy path
- Mock all HTTP calls using `HttpClientTestingModule` — no real API calls in unit tests
- E2E tests (Cypress) cover: full enrollment wizard, single service generation, batch generation

---

## Phase 0 Focus (Week 1 — May 6-10)

The immediate goal is the **Vertical Enrollment Wizard** — 5 steps:
1. Vertical Identity (slug, display name, owner)
2. Cloud Provider (AWS selected, Azure/GCP disabled)
3. Source Control (GitHub selected, others disabled)
4. Technical Defaults (environments, IaC preferences)
5. Review & Enroll

All HTTP calls use `MockVerticalApiService` this week.  
On **May 10**, swap `useMockApi` to `false` in `environment.ts` to connect to the real BFF API.

The BFF team will share the Swagger JSON (API contract) by **May 8** — verify mock response shapes match before integration day.

---

## Nx Commands

```bash
npx nx serve launchpad          # dev server
npx nx build launchpad          # production build
npx nx test launchpad           # unit tests
npx nx lint launchpad           # lint
npx nx e2e launchpad-e2e        # Cypress E2E
npx nx test launchpad --watch   # watch mode
```

---

## Mock API Toggle

```typescript
// environment.ts — week 1
export const environment = {
  apiBaseUrl: 'http://localhost:5000',
  useMockApi: true   // ← flip to false on May 10
};
```

No other changes needed — the DI configuration in `app.config.ts` reads this flag and provides the correct service class automatically.
