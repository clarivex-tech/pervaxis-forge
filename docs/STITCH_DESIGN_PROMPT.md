# Pervaxis Forge Launchpad — Stitch Design Prompt

**Purpose:** Use this prompt with Google Stitch to generate screen designs for the Forge Launchpad UI.  
**Target:** 7 screens covering the full Vertical Enrollment flow and core workspace views.  
**Design System:** Material Design 3 + Angular Material 3 components.

---

## Prompt

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
