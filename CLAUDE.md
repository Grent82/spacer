# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

<!-- BEGIN BEADS INTEGRATION v:1 profile:minimal hash:ca08a54f -->
## Beads Issue Tracker

This project uses **bd (beads)** for issue tracking. Run `bd prime` to see full workflow context and commands.

### Quick Reference

```bash
bd ready              # Find available work
bd show <id>          # View issue details
bd update <id> --claim  # Claim work
bd close <id>         # Complete work
```

### Rules

- Use `bd` for ALL task tracking — do NOT use TodoWrite, TaskCreate, or markdown TODO lists
- Run `bd prime` for detailed command reference and session close protocol
- Use `bd remember` for persistent knowledge — do NOT use MEMORY.md files

## Session Completion

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd dolt push
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds

**C.L.E.A.R. Quality Gate** — *Before `bd close <id>`*:
Run a C.L.E.A.R. review on all changed code. Document findings in the ticket comment:
```
## C.L.E.A.R. Review
- **C**orrectness: ✓ / ⚠ / ✗ — [brief note]
- **L**ibraries: ✓ / ⚠ / ✗ — [brief note]
- **E**fficiency: ✓ / ⚠ / ✗ — [brief note]
- **A**rchitecture: ✓ / ⚠ / ✗ — [brief note]
- **R**isks: ✓ / ⚠ / ✗ — [brief note]
```
<!-- END BEADS INTEGRATION -->

---

## Build & Test Commands

```bash
# Build entire solution
dotnet build Spacer.sln

# Run a specific test file
dotnet test --filter "FullyQualifiedName~TestClassName"

# Run all tests
dotnet test

# Build a specific project
dotnet build src/Spacer.Domain/Spacer.Domain.csproj

# Run the main application
dotnet run --project src/Spacer.Presentation/Spacer.Presentation.csproj

# Run the tools application
dotnet run --project src/Spacer.Tools/Spacer.Tools.csproj
```

---

## Architecture Overview

**Spacer** is a .NET 10.0 game project (reverse-engineering a Heat Signature-like game) following Clean Architecture principles.

### Layer Structure

| Layer | Purpose | Key Components |
|-------|---------|----------------|
| **Domain** (`Spacer.Domain`) | Core business logic, entities, value objects | Entities (Character, Faction, Fleet, Planet), Rules, Services (combat resolution, marriage, economy), Enums, ValueObjects |
| **Application** (`Spacer.Application`) | Use cases, orchestration, DTOs | UseCases, Services (turn processing, lifecycle), Ports (interfaces for dependencies), Events, DTOs, GameState |
| **Infrastructure** (`Spacer.Infrastructure`) | External systems, persistence, implementations | Persistence (CSV/JSON repositories), Services (GameClock, EventQueue), Scripting, Config, Assets |
| **Presentation** (`Spacer.Presentation`) | UI, entry point | Program.cs, Screens, ViewModels, Input, Rendering |
| **Tools** (`Spacer.Tools`) | Utility applications | Commands, Export/Import, Diagnostics |

### Key Architectural Patterns

- **Dependency Injection**: Application layer defines ports (interfaces) in `Ports/`, Infrastructure provides implementations
- **Turn-based Game Loop**: Turn services in `Application/Services/` handle phased turn processing (economy, politics, lifecycle, combat)
- **Event-driven**: Event queue system (`IEventQueue`, `IEventStateStore`) with catalog (`IEventCatalog`)
- **CSV-driven data**: Character, faction, planet, fleet specs loaded from CSV files
- **In-memory state**: Game state held in memory with persistence hooks

### Core Domain Concepts

- **Entities**: Character, Faction, Fleet, Planet, ShipSpec, WeaponSpec, ItemSpec
- **Turn Services**: `EconomyTurnService`, `FactionPoliticsTurnService`, `CharacterLifecycleTurnService`
- **Combat**: `FleetCombatResolver`, `CharacterDuelResolver`, `CombatResolver`
- **Economy/Research**: `PlanetEconomyService`, `PlanetResearchService`, `WeaponResearchProgressionService`

---

## Important Files & Locations

### Entry Points
- `src/Spacer.Presentation/Program.cs` - Main application entry
- `src/Spacer.Tools/Program.cs` - Tools entry point

### Data Files
- CSV files for character/faction/planet/spec data (loaded via `Csv*Repository` classes)
- JSON files for event catalog and game time store

### Key Interfaces (Ports)
Located in `src/Spacer.Application/Ports/`:
- `ICharacterRepository`, `IPlanetRepository` - Data access
- `IEventCatalog`, `IEventQueue` - Event system
- `IFactionCatalog`, `IItemCatalog` - Data catalogs
- `IGameTime`, `ICharacterRoster` - Game state providers

### Infrastructure Implementations
Located in `src/Spacer.Infrastructure/`:
- `Persistence/` - CSV/JSON data loaders
- `Services/` - `GameClock`, `InMemoryEventQueue`, `InMemoryEventStateStore`
- `Config/` - Configuration loading

---

## Project-Specific Conventions

### Naming
- Domain entities use singular nouns (`Character`, `Faction`)
- Services use descriptive names ending in `Service` or `Resolver`
- Turn services follow pattern: `[Domain]TurnService`
- Repository implementations use `Csv[Entity]Repository` naming

### Turn Processing Flow
Turn services are orchestrated in `Application/Services/` following the game's turn loop:
1. Economy calculations (`EconomyTurnService`)
2. Faction politics (`FactionPoliticsTurnService`)
3. Character lifecycle (`CharacterLifecycleTurnService`)
4. Fleet combat (`FleetCombatResolver`)

### Data Loading Pattern
- CSV data loaded via `CsvRowReader` base functionality
- Spec catalogs (weapons, ships, items) provide validation and lookup
- Map and planet data loaded via repository pattern

---

## Current Development Focus

See `todo.md` for active development tasks, including:
- Turn loop phase alignment with source game
- Core entity modeling and ID systems
- Event system implementation
- Economy/research/ politics rule expansion
- Character lifecycle (pregnancy, death, newborn) details
- Fleet combat input mapping and modifier implementation

---

## References

- **Beads integration**: Run `bd prime` for workflow details
- **Analysis notes**: See `rai7_analysis.md`, `rai7_turn_loop.md`, and related `rai7_var_*.md` files
- **TODO tracking**: `todo.md` contains detailed development tasks
- **C.L.E.A.R. Framework**: See `.claude/memory/clear-framework.md` for systematic code review methodology

---

## C.L.E.A.R. Framework Code Review

**Before accepting or committing any AI-generated code**, apply the C.L.E.A.R. Framework:

### C - Correctness
- [ ] Code compiles without errors
- [ ] Functional requirements met
- [ ] Edge cases handled (null, empty, boundaries)
- [ ] Logic is sound (no off-by-one, infinite loops)

### L - Libraries & Dependencies
- [ ] All imports/packages exist and are valid
- [ ] API methods match current .NET 10.0 version
- [ ] No hallucinated packages or methods
- [ ] Version compatibility verified

### E - Efficiency & Performance
- [ ] Algorithmic complexity appropriate
- [ ] No redundant operations or queries
- [ ] Resources properly disposed (using statements)
- [ ] No obvious bottlenecks

### A - Architecture Fit
- [ ] Follows Clean Architecture boundaries
- [ ] Uses Dependency Injection correctly
- [ ] Matches existing naming conventions
- [ ] Placed in correct layer (Domain/Application/Infrastructure/Presentation)

### R - Risks & Security
- [ ] No hardcoded secrets or credentials
- [ ] Input validation present
- [ ] Proper error handling (no swallowed exceptions)
- [ ] No SQL injection/XSS vulnerabilities

**Usage:** When reviewing code changes, explicitly walk through each dimension and report findings.
