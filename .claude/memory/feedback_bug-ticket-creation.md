---
name: feedback_bug_ticket_creation
description: Bei Bug-Tickets immer zuerst Template erstellen, dann mit bd create einfache Titel senden und description separat update
metadata:
  type: feedback
---

**Was:** Bug-Tickets mit komplexen Inhalten (Markdown, Code-Blöcke) nicht in einem `bd create --description='...'` Befehl erstellen.

**Warum:** Shell interpretiert Sonderzeichen (`$`, `{`, `}`, `'`) und bricht den Befehl ab. Inhalt geht verloren oder wird beschädigt.

**Wie anwenden:**
1. Zuerst Bug-Fix Template in `.beads/formulas/bugfix.md` erstellen (wenn nicht vorhanden)
2. Ticket nur mit Titel erstellen: `bd create --title="..." --type=bug --priority=2`
3. Description separat mit `bd update <id> --description='...'` nachschieben
4. Bei sehr komplexen Inhalten: Beschreibung in temporäre .md Datei schreiben, dann kopieren

**Beispiel:**
```bash
# NICHT:
bd create --title="Fix bug" --description='## Code\n```csharp\nvar x = $"{y}"\n```'

# SONDERN:
bd create --title="Fix bug" --type=bug --priority=2
bd update <id> --description='## Beschreibung hier'
```
