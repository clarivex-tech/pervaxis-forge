# Pervaxis Forge Launchpad — Stitch Design Prompt

**Purpose:** Use this prompt with Google Stitch to generate screen designs for the Forge Launchpad UI.  
**Target:** 7 screens covering the full Vertical Enrollment flow and core workspace views.  
**Design System:** Material Design 3 + Angular Material 3 components.

> **Run order:**
> 1. **Prompt A (Design Tokens)** — run this first. Share output with engineering to wire into Angular Material theme.
> 2. **Prompt B (Screen Designs)** — run this second, after tokens are confirmed. Reference the token output so screens are consistent.

---

## Prompt A — Design Token Specification (Run First)

Generate a complete Material Design 3 design token specification for an internal web admin tool called **Pervaxis Forge Launchpad**. This is a desktop-only tool used by platform engineers at Clarivex Technologies.

Output the following token groups in a structured format (JSON or design token table):

### Brand Identity
- Product name: Pervaxis Forge Launchpad
- Brand: Clarivex Technologies
- Personality: Professional, technical, trustworthy — not consumer-facing
- Primary accent: Clarivex purple (suggest closest Material purple 600 equivalent)

### Colour Tokens
Provide hex values for the full Material Design 3 colour role set:

**Primary palette:**
- `primary`, `on-primary`, `primary-container`, `on-primary-container`

**Secondary palette** (for chips, tags, secondary actions):
- `secondary`, `on-secondary`, `secondary-container`, `on-secondary-container`

**Tertiary palette** (for status indicators — success, warning):
- `tertiary`, `on-tertiary`, `tertiary-container`, `on-tertiary-container`

**Error palette:**
- `error`, `on-error`, `error-container`, `on-error-container`

**Surface & background:**
- `background`, `on-background`
- `surface`, `on-surface`, `surface-variant`, `on-surface-variant`
- `outline`, `outline-variant`
- `surface-container-lowest`, `surface-container-low`, `surface-container`, `surface-container-high`, `surface-container-highest`

**Semantic colours** (for Forge-specific use):
- AWS chip colour (orange family)
- GitHub chip colour (dark/neutral)
- Success status chip (green family)
- Failed status chip (error/red family)
- "Coming Soon" disabled chip (neutral grey)

### Typography Tokens
Font family: Google Sans (primary), Roboto (fallback)

Provide the full Material Design 3 type scale:

| Role | Font | Weight | Size | Line Height | Letter Spacing |
|---|---|---|---|---|---|
| Display Large | | | | | |
| Display Medium | | | | | |
| Display Small | | | | | |
| Headline Large | | | | | |
| Headline Medium | | | | | |
| Headline Small | | | | | |
| Title Large | | | | | |
| Title Medium | | | | | |
| Title Small | | | | | |
| Body Large | | | | | |
| Body Medium | | | | | |
| Body Small | | | | | |
| Label Large | | | | | |
| Label Medium | | | | | |
| Label Small | | | | | |

### Spacing Tokens
Base grid: 8px. Provide named spacing values:
- `spacing-1` through `spacing-12` (8px increments)
- `spacing-half` (4px)
- Component-specific: form field height, card padding, stepper height, table row height, chip height, button height

### Shape (Border Radius) Tokens
Provide corner radius per Material shape scale:
- `shape-none`, `shape-extra-small`, `shape-small`, `shape-medium`, `shape-large`, `shape-extra-large`, `shape-full`
- And which shape applies to: cards, buttons, chips, input fields, dialogs, side panels

### Elevation Tokens
Shadow definitions for Material elevation levels 0–5 (used for cards, dialogs, menus, app bar).

### Component-Specific Tokens
For each component used in Forge Launchpad, specify the key tokens:

- **MatCard**: background, border-radius, padding, elevation
- **MatFormField (outline)**: border-colour, focus-colour, label-colour, input-height
- **MatButton (filled)**: background, text-colour, border-radius, height
- **MatButton (outlined)**: border-colour, text-colour, border-radius, height
- **MatChip**: background, text-colour, border-radius, height — for each variant (provider, status, environment)
- **MatStepper**: connector-colour, active-colour, completed-colour, label-font
- **MatTable**: header-background, row-hover, divider-colour, row-height
- **MatSlideToggle**: track-colour-on, track-colour-off, thumb-colour

### Output Format
Provide tokens in this structure so engineering can paste directly into SCSS variables and Angular Material theme configuration:

```json
{
  "color": { ... },
  "typography": { ... },
  "spacing": { ... },
  "shape": { ... },
  "elevation": { ... },
  "components": { ... }
}
```

---

---

## Prompt B — Screen Designs (Run Second, after tokens confirmed)

Design a web admin tool called "Pervaxis Forge Launchpad" using Google Material Design 3 (Material You) principles. This is an internal desktop-only platform tool — no mobile layout needed. Use a clean, professional dark-on-light theme with Clarivex brand purple as the primary accent colour.

Design the following 7 screens:

---

### SCREEN 1 — Vertical Dashboard (Landing Page)
The home screen after login. Shows enrolled business verticals as cards in a grid.

Each card shows:
- Vertical slug as a badge (e.g. "clarivolt")
- Display name as card title
- Cloud provider chip (e.g. "AWS" in orange)
- Source control chip (e.g. "GitHub" in dark)
- Service count (e.g. "12 services generated")
- Last generated timestamp
- "Open" primary button

Top-right: "+ Enroll New Vertical" filled button  
Empty state (no verticals yet): centred illustration + "Enroll Your First Vertical" CTA button

---

### SCREEN 2 — Enrollment Wizard: Step 1 — Vertical Identity
5-step linear Material Stepper at the top showing: Identity → Cloud Provider → Source Control → Tech Defaults → Review & Enroll

Step 1 fields (2-column grid layout):
- Slug (left) — text input, kebab-case hint below, live preview chip showing e.g. "clarivolt / intake-service"
- Display Name (right) — text input
- Description (full width) — textarea
- Owner Team (left) — text input
- Owner Email (right) — email input

Back / Next buttons bottom right

---

### SCREEN 3 — Enrollment Wizard: Step 2 — Cloud Provider
Three large provider selection cards side by side:
- AWS — active, selectable, shows AWS logo area
- Azure — greyed out, "Coming Soon" chip overlay
- GCP — greyed out, "Coming Soon" chip overlay

When AWS is selected, an additional panel appears below with fields:
- AWS Account ID (left)
- Default Region — dropdown (right)
- IAM Role ARN (full width) — with info tooltip: "Forge uses this role to provision resources. No access keys stored."

---

### SCREEN 4 — Enrollment Wizard: Step 3 — Source Control
Three platform cards: GitHub (active), GitLab (Coming Soon), Azure DevOps (Coming Soon)

When GitHub selected, panel below shows:
- GitHub Organisation (left)
- Default Visibility — radio: Private / Public (right)
- Access Token (full width) — password field with show/hide toggle
- Default Branch Protection — slide toggle, default on
- "Verify Access" outlined button — shows green success badge or red error chip after click

---

### SCREEN 5 — Enrollment Wizard: Step 4 — Tech Defaults
- Environments — Material chip list with add/remove, default chips: test, accp, prod
- Default Environment — dropdown sourced from chip list
- IaC checkboxes: "Generate Terraform" (checked), "Generate CDK" (checked)
- Default DB Engine — dropdown: None / PostgreSQL

---

### SCREEN 6 — Enrollment Wizard: Step 5 — Review & Enroll
Read-only summary in card sections:
- Identity section: slug, display name, owner
- Cloud section: provider chip, account ID, region, IAM ARN masked as "arn:aws:iam::***"
- Source Control section: GitHub org, visibility, branch protection
- Tech Defaults: environments chips, IaC selections

Each section has an "Edit" text link to go back to that step  
Bottom: "Enroll Vertical" filled primary button + loading spinner state

---

### SCREEN 7 — Vertical Workspace
- Header: vertical display name, AWS chip, GitHub org, settings icon button
- Stats row: total services generated, last generation date
- "Generate New Services" large primary button — prominent
- Recent generations table: date, service names (as chips), operator, status chips (Success / Failed)
- Side panel (slide-out): vertical settings — edit display name, rotate GitHub token, danger zone "Unenroll Vertical"

---

## Design Constraints

| Constraint | Value |
|---|---|
| Component library | Angular Material 3 — MatCard, MatStepper, MatFormField (outline), MatChip, MatButton, MatTable, MatSelect, MatSlideToggle |
| Typography | Google Sans or Roboto |
| Spacing | 8px base grid |
| Primary colour | Purple (Material purple 600 or similar) |
| Input appearance | Outline variant throughout |
| Icons | Material Icons only — no custom illustrations |
| Viewport | Desktop — 1280px wide minimum |
| Platform | No mobile layout needed |
