---
name: project_bugfix_template
description: Bug-Fix Template existiert bei .beads/formulas/bugfix.md mit schrittweisen Anweisungen
metadata:
  type: project
---

**Was:** Bug-Fix Template für dieses Projekt existiert unter `.beads/formulas/bugfix.md`

**Warum:** Ermöglicht konsistente, AI-freundliche Bug-Tickets mit schrittweisen Fix-Anweisungen

**Wie anwenden:**
- Bei jedem neuen Bug-Ticket Template-Struktur verwenden
- Template enthält: Bug Summary, Root Cause, Symptoms, Fix Instructions (5 Steps), Testing, References
- Besonders wichtig für Code-Warnungen (CS8602, CS8601) und Null-Reference-Risiken

**Beispiel-Struktur:**
```
## Bug Summary (What, Where, Severity)
## Root Cause
## Symptoms
## Fix Instructions (STEP BY STEP)
  Step 1: Open file
  Step 2: Locate code
  Step 3: Apply fix
  Step 4: Build
  Step 5: Verify
## Testing
## References
```
