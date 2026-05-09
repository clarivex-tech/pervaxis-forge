---
name: Pervaxis Forge Launchpad (Final Refined)
colors:
  primary: '#6750A4'
  on-primary: '#FFFFFF'
  primary-container: '#EADDFF'
  on-primary-container: '#21005D'
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
  background: '#FEF7FF'
  on-background: '#1D1B20'
  surface: '#FEF7FF'
  on-surface: '#1D1B20'
  surface-variant: '#E7E0EB'
  on-surface-variant: '#49454F'
  outline: '#79747E'
  outline-variant: '#CAC4D0'
  surface-container-lowest: '#FFFFFF'
  surface-container-low: '#F7F2FA'
  surface-container: '#F3EDF7'
  surface-container-high: '#ECE6F0'
  surface-container-highest: '#E6E0E9'
  aws-orange: '#FF9900'
  github-dark: '#24292F'
  success-green: '#2E7D32'
  surface-dim: '#ded8e0'
  surface-bright: '#fdf7ff'
  inverse-surface: '#322f35'
  inverse-on-surface: '#f5eff7'
  surface-tint: '#6750a4'
  inverse-primary: '#cfbcff'
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
typography:
  font-family: Google Sans, Inter, system-ui
  display-large:
    size: 57px
    weight: 400
    line-height: 64px
    spacing: -0.25px
  display-medium:
    size: 45px
    weight: 400
    line-height: 52px
    spacing: 0px
  display-small:
    size: 36px
    weight: 400
    line-height: 44px
    spacing: 0px
  headline-large:
    size: 32px
    weight: 400
    line-height: 40px
    spacing: 0px
  headline-medium:
    size: 28px
    weight: 400
    line-height: 36px
    spacing: 0px
  headline-small:
    size: 24px
    weight: 400
    line-height: 32px
    spacing: 0px
  title-large:
    size: 22px
    weight: 400
    line-height: 28px
    spacing: 0px
  title-medium:
    size: 16px
    weight: 500
    line-height: 24px
    spacing: 0.15px
  title-small:
    size: 14px
    weight: 500
    line-height: 20px
    spacing: 0.1px
  body-large:
    size: 16px
    weight: 400
    line-height: 24px
    spacing: 0.5px
  body-medium:
    size: 14px
    weight: 400
    line-height: 20px
    spacing: 0.25px
  body-small:
    size: 12px
    weight: 400
    line-height: 16px
    spacing: 0.4px
  label-large:
    size: 14px
    weight: 500
    line-height: 20px
    spacing: 0.1px
  label-medium:
    size: 12px
    weight: 500
    line-height: 16px
    spacing: 0.5px
  label-small:
    size: 11px
    weight: 500
    line-height: 16px
    spacing: 0.5px
  display-lg:
    fontFamily: Inter
    fontSize: 57px
    fontWeight: '400'
    lineHeight: 64px
    letterSpacing: -0.25px
  headline-lg:
    fontFamily: Inter
    fontSize: 32px
    fontWeight: '400'
    lineHeight: 40px
    letterSpacing: 0px
  title-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '500'
    lineHeight: 24px
    letterSpacing: 0.15px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0.5px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0.25px
  label-lg:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '500'
    lineHeight: 20px
    letterSpacing: 0.1px
elevation:
  level-0: none
  level-1: 0px 1px 3px 1px rgba(0, 0, 0, 0.15), 0px 1px 2px 0px rgba(0, 0, 0, 0.30)
  level-2: 0px 2px 6px 2px rgba(0, 0, 0, 0.15), 0px 1px 2px 0px rgba(0, 0, 0, 0.30)
  level-3: 0px 4px 8px 3px rgba(0, 0, 0, 0.15), 0px 1px 3px 0px rgba(0, 0, 0, 0.30)
  level-4: 0px 6px 10px 4px rgba(0, 0, 0, 0.15), 0px 2px 3px 0px rgba(0, 0, 0, 0.30)
  level-5: 0px 8px 12px 6px rgba(0, 0, 0, 0.15), 0px 4px 4px 0px rgba(0, 0, 0, 0.30)
shapes:
  shape-none: 0px
  shape-extra-small: 4px
  shape-small: 8px
  shape-medium: 12px
  shape-large: 16px
  shape-extra-large: 28px
  shape-full: 9999px
components:
  mat-card:
    background: surface-container-low
    border-radius: shape-medium
    elevation: level-1
  mat-form-field-outline:
    border-color: outline
    focus-color: primary
    border-radius: shape-extra-small
  mat-button-filled:
    background: primary
    text-color: on-primary
    border-radius: shape-full
    height: 40px
  mat-button-outlined:
    border-color: outline
    text-color: primary
    border-radius: shape-full
    height: 40px
  mat-chip:
    background: surface-container-high
    text-color: on-surface-variant
    border-radius: shape-small
    height: 32px
  mat-stepper:
    active-color: primary
    completed-color: primary
    connector-color: outline-variant
  mat-table:
    header-background: surface-container-low
    divider-color: outline-variant
    row-hover: surface-container-high
  mat-slide-toggle:
    track-on: primary
    track-off: surface-variant
    thumb: outline
  mat-dialog:
    background: surface-container-high
    border-radius: shape-extra-large
    elevation: level-3
  mat-sidenav:
    background: surface-container-low
    border-radius: shape-none
    elevation: level-0
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  unit: 4px
  gutter: 16px
  margin: 24px
  container-max-width: 1280px
---
