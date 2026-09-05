# User Interface Style Guide

The UI is built using Bootstrap 5 and customized with a responsive Obsidian dark-theme styling sheet.

## Design tokens (Vanilla CSS Variables)
Defined in [src/BusinessManagement.Web/wwwroot/css/site.css](file:///d:/omarWeb/src/BusinessManagement.Web/wwwroot/css/site.css):
- `--BusinessManagement-bg`: `#121212` (Base background)
- `--BusinessManagement-card-bg`: `rgba(30, 30, 30, 0.65)` (Glassmorphism backdrop)
- `--BusinessManagement-border`: `rgba(255, 255, 255, 0.08)`
- `--BusinessManagement-primary`: `#d97706` (Obsidian amber theme highlight)

## Bootstrap 5 Layout Integrations
- All data tables use the `.BusinessManagement-table` styling class providing hover states, column zebra striping, and text alignment for Arabic RTL scripts.
- Visual status indicators:
  - `<span class="badge bg-success">Confirmed</span>`
  - `<span class="badge bg-warning text-dark">Draft</span>`
  - `<span class="badge bg-danger">Cancelled</span>`
