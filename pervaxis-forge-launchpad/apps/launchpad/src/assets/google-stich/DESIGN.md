---
name: Pervaxis Forge Launchpad Design System
colors:
  surface: '#FEF7FF'
  surface-dim: '#ded8e0'
  surface-bright: '#fdf7ff'
  surface-container-lowest: '#FFFFFF'
  surface-container-low: '#F7F2FA'
  surface-container: '#F3EDF7'
  surface-container-high: '#ECE6F0'
  surface-container-highest: '#E6E0E9'
  on-surface: '#1D1B20'
  on-surface-variant: '#49454F'
  inverse-surface: '#322f35'
  inverse-on-surface: '#f5eff7'
  outline: '#79747E'
  outline-variant: '#CAC4D0'
  surface-tint: '#6750a4'
  primary: '#6750A4'
  on-primary: '#FFFFFF'
  primary-container: '#EADDFF'
  on-primary-container: '#21005D'
  inverse-primary: '#cfbcff'
  secondary: '#625B71'
  on-secondary: '#FFFFFF'
  secondary-container: '#E8DEF8'
  on-secondary-container: '#1D192B'
  tertiary: '#7D5260'
  on-tertiary: '#FFFFFF'
  tertiary-container: '#FFD8E4'
  on-tertiary-container: '#31111D'
  error: '#B3261E'
  on-error: '#FFFFFF'
  error-container: '#F9DEDC'
  on-error-container: '#410E0B'
  primary-fixed: '#e9ddff'
  primary-fixed-dim: '#cfbcff'
  on-primary-fixed: '#22005d'
  on-primary-fixed-variant: '#4f378a'
  secondary-fixed: '#e8def9'
  secondary-fixed-dim: '#ccc2dc'
  on-secondary-fixed: '#1e192b'
  on-secondary-fixed-variant: '#4a4358'
  tertiary-fixed: '#ffd9e3'
  tertiary-fixed-dim: '#eeb8c8'
  on-tertiary-fixed: '#31111d'
  on-tertiary-fixed-variant: '#633b48'
  background: '#FEF7FF'
  on-background: '#1D1B20'
  surface-variant: '#E7E0EB'
  aws-orange: '#FF9900'
  github-dark: '#24292F'
  status-success: '#2E7D32'
  status-failed: '#D32F2F'
  coming-soon: '#9E9E9E'
typography:
  display-lg:
    fontFamily: Work Sans
    fontSize: 57px
    fontWeight: '400'
    lineHeight: 64px
    letterSpacing: -0.25px
  display-md:
    fontFamily: Work Sans
    fontSize: 45px
    fontWeight: '400'
    lineHeight: 52px
    letterSpacing: 0px
  display-sm:
    fontFamily: Work Sans
    fontSize: 36px
    fontWeight: '400'
    lineHeight: 44px
    letterSpacing: 0px
  headline-lg:
    fontFamily: Work Sans
    fontSize: 32px
    fontWeight: '400'
    lineHeight: 40px
    letterSpacing: 0px
  headline-md:
    fontFamily: Work Sans
    fontSize: 28px
    fontWeight: '400'
    lineHeight: 36px
    letterSpacing: 0px
  headline-sm:
    fontFamily: Work Sans
    fontSize: 24px
    fontWeight: '400'
    lineHeight: 32px
    letterSpacing: 0px
  title-lg:
    fontFamily: Work Sans
    fontSize: 22px
    fontWeight: '500'
    lineHeight: 28px
    letterSpacing: 0px
  title-md:
    fontFamily: Work Sans
    fontSize: 16px
    fontWeight: '500'
    lineHeight: 24px
    letterSpacing: 0.15px
  title-sm:
    fontFamily: Work Sans
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.1px
  body-lg:
    fontFamily: Work Sans
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0.5px
  body-md:
    fontFamily: Work Sans
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0.25px
  body-sm:
    fontFamily: Work Sans
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0.4px
  label-lg:
    fontFamily: Work Sans
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.1px
  label-md:
    fontFamily: Work Sans
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.5px
  label-sm:
    fontFamily: Work Sans
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.5px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  spacing-half: 4px
  spacing-1: 8px
  spacing-2: 16px
  spacing-3: 24px
  spacing-4: 32px
  spacing-5: 40px
  spacing-6: 48px
  spacing-7: 56px
  spacing-8: 64px
  spacing-9: 72px
  spacing-10: 80px
  spacing-11: 88px
  spacing-12: 96px
  form-field-height: 56px
  card-padding: 24px
  stepper-height: 72px
  table-row-height: 52px
  chip-height: 32px
  button-height: 40px
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
