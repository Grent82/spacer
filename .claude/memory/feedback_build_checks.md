---
name: feedback_build_checks
description: Häufige Build-Fehler durch API-Verwechslungen und Property-Namen
metadata:
  type: feedback
---

## Häufige Build-Fehler in diesem Projekt

### JSON Element API
- `JsonElement` hat **keine** `TryGetBoolean()` Methode — verwende `ValueKind == JsonValueKind.True/False`
- `TryGetString()` existiert — gibt `string?` zurück (null wenn nicht string)
- `TryGetInt32()` existiert mit `ref` Parameter
- Für Konvertierung: `GetBoolean()`, `GetInt32()`, `GetString()` direkt aufrufen nach ValueKind-Check

### Property-Namen konsistent halten
- Bei `*Rules` record structs: Property-Namen genau prüfen vor Referenzierung
- IDE-Intellisense nutzen statt aus dem Kopf schreiben

### Build nach jedem größeren Edit
- Nicht mehrere Dateien hintereinander ändern ohne Build-Check
- Nach jeder Rules-Struktur-Änderung sofort bauen

**Warum:** Vermeidet kumulative Fehler und erleichtert das Finden der Ursache.

**How to apply:**
1. Nach jedem Edit: `dotnet build Spacer.sln`
2. Bei JSON-Element-Zugriff: ValueKind zuerst prüfen
3. Property-Namen aus Code kopieren, nicht tippen
