---
name: clear-framework
description: C.L.E.A.R. Framework for systematic AI code review and quality assessment
metadata:
  type: feedback
---

# C.L.E.A.R. Framework für KI-Code-Review

Ein systematisches Review-Verfahren für KI-generierten Code und Artefakte.

## Die 5 Dimensionen

### C - Correctness (Korrektheit)
**Prüffragen:**
- Kompiliert/interpretiert der Code ohne Fehler?
- Erfüllt der Code die funktionalen Anforderungen?
- Sind Edge Cases berücksichtigt?
- Stimmt die Logik? (Off-by-One-Errors, Null-Checks)

**Typische KI-Probleme:**
- Semantische Fehler bei korrekter Syntax
- Fehlende Fehlerbehandlung
- Unvollständige Implementierungen ("Happy Path only")

---

### L - Libraries & Dependencies (Bibliotheken)
**Prüffragen:**
- Existieren alle importierten Packages tatsächlich?
- Sind die verwendeten API-Methoden in der aktuellen Version verfügbar?
- Passen die Versionen zu unserem Tech-Stack?
- Gibt es bekannte Sicherheitslücken in den vorgeschlagenen Libraries?

**Typische KI-Probleme:**
- Halluzinierte Package-Namen (Sicherheitsrisiko!)
- Veraltete API-Calls
- Inkompatible Versionen

---

### E - Efficiency & Performance (Effizienz)
**Prüffragen:**
- Ist die algorithmische Komplexität angemessen?
- Werden unnötige Operationen durchgeführt?
- Gibt es offensichtliche Performance-Probleme?
- Werden Ressourcen korrekt freigegeben?

**Typische KI-Probleme:**
- Ineffiziente Algorithmen (O(n²) statt O(n log n))
- Redundante Datenbankabfragen
- Memory Leaks durch nicht geschlossene Ressourcen

---

### A - Architecture Fit (Architekturpassung)
**Prüffragen:**
- Passt der Code in unsere bestehende Architektur?
- Werden unsere Design Patterns eingehalten?
- Entspricht der Code unseren Team-Standards?
- Ist der Code wartbar und erweiterbar?

**Typische KI-Probleme:**
- Ignoriert bestehende Patterns
- Umgeht Dependency Injection
- Verletzt Clean Architecture Grenzen

---

### R - Risks & Security (Risiken und Sicherheit)
**Prüffragen:**
- Gibt es potenzielle Sicherheitslücken (SQL Injection, XSS, etc.)?
- Werden Credentials oder Secrets hartcodiert?
- Ist Input-Validierung vorhanden?
- Werden Daten korrekt verschlüsselt/gehasht?

**Typische KI-Probleme:**
- Fehlende Input-Sanitization
- Unsichere Default-Konfigurationen
- Veraltete Kryptographie-Praktiken

---

## Erweiterung auf andere Artefakte

| Dimension | Requirements | Testfälle | UX/UI-Entwürfe |
|-----------|--------------|-----------|----------------|
| **C** | Stimmen Anforderungen mit Stakeholdern überein? | Prüft der Test das gewünschte Verhalten? | Entspricht Entwurf den Anforderungen? |
| **L** | Existieren referenzierte Standards? | Sind Testdaten/Schnittstellen verfügbar? | Existieren Komponenten im Design-System? |
| **E** | Sind Anforderungen präzise oder redundant? | Gibt es redundante Testfälle? | Ist Entwurf umsetzbar oder zu komplex? |
| **A** | Passen Anforderungen in Systemlandschaft? | Folgen Tests der Teststrategie? | Passt zum Design-System/Brand? |
| **R** | Regulatorische/Datenschutz-Risiken? | Sicherheits- und Negativtests abgedeckt? | Barrierefreiheit/Datenschutz berücksichtigt? |

---

## Anwendung

**Merke:** Das C.L.E.A.R. Framework ist keine Checkliste zum Abhaken, sondern eine Denkstruktur. Jeder Buchstabe steht für eine Dimension, in der KI-Outputs typischerweise versagen.

**Anzuwenden bei:**
- Code Reviews
- Test-Code-Generierung
- Architektur-Entwürfen
- Requirements-Erstellung
- UX/UI-Vorschlägen

---

## .NET 10.0 / Spacer Projekt-spezifische Checks

### C - Correctness (Projektspezifisch)
- [ ] `dotnet build Spacer.sln` erfolgreich
- [ ] Relevanten Tests: `dotnet test --filter "FullyQualifiedName~<TestKlasse>"`
- [ ] Entity-Framework Abfragen korrekt (AsNoTracking für Lesezugriffe)
- [ ] Async/Await korrekt verwendet (kein `.Result` oder `.Wait()`)
- [ ] ValueObjects immutable und equality korrekt implementiert

### L - Libraries & Dependencies (.NET 10)
- [ ] NuGet Packages aus `.csproj` Dateien referenziert
- [ ] Keine veralteten APIs (.NET 10 Breaking Changes geprüft)
- [ ] Minimal APIs vs Controller-Pattern konsistent
- [ ] System.Text.Json statt Newtonsoft (wo möglich)

### E - Efficiency (Game-Performance)
- [ ] Turn-Loop-Performance optimiert (kein Blockieren des Main-Threads)
- [ ] CSV-Parsing effizient (CsvRowReader Pattern)
- [ ] Event-Queue ohne Memory-Leaks
- [ ] Combat-Resolver O(n) statt O(n²) bei vielen Einheiten

### A - Architecture (Clean Architecture)
- [ ] **Domain**: Keine externen Dependencies, reine Business-Logik
- [ ] **Application**: UseCases, Ports (Interfaces), DTOs
- [ ] **Infrastructure**: CSV/JSON Repositories, externe Services
- [ ] **Presentation**: UI, Input, Rendering — keine Business-Logik
- [ ] Dependency Inversion eingehalten (Application → Infrastructure)

### R - Risks (Game-Security)
- [ ] Save/Load Daten validiert (kein Corrupt durch manipulierte JSON/CSV)
- [ ] Keine Hardcoded Paths (relative Paths verwenden)
- [ ] Exception-Handling bei Netzwerk/IO-Operationen
- [ ] Seed-Data für Multiplayer-Synchronisation

---

## Beads-Integration

C.L.E.A.R. Review ist **obligatorisch vor `bd close <id>`**.

Ausführung:
```bash
# Manuelles Starten
bd clear-review

# Vor Session-Ende (automatisch via Hook)
bd preflight
```

Siehe auch:
- `.beads/profiles/clear-review.md` — Detaillierte Checkliste
- `.claude/memory/session-completion.md` — Session Completion Protocol
