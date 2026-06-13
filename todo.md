# Spacer Development TODO - Status Review

## ✅ Abgeschlossene Aufgaben

### Conception & Pregnancy
- ✅ Conception rules added to `PregnancyRules` (ConceptionChancePercent, ConceptionWithPartnerOnly, ConceptionInfertilityThreshold)
- ✅ `ConceptionResult` value object created
- ✅ `ConceptionService` implemented with `TryConceive()` and `CalculateModifiedConceptionChance()`
- ✅ Conception integrated into `CharacterLifecycleTurnService`
- ✅ `StartPregnancyActionHandler` and `CheckConceptionActionHandler` for event-driven conception
- ✅ Conception events enqueued on success

### Fleet Combat
- ✅ `FleetCombatantInputs` structure defined with all fields
- ✅ `FleetCombatantFactory` implements attack/defense calculations
- ✅ `FleetCombatResolver` resolves combat with all modifiers (guard, confusion, damage multipliers)
- ✅ FleetPostureProvider implemented for loyalty drift

### Faction Politics
- ✅ `FactionPoliticsService` with loyalty drift, succession, defection logic
- ✅ `FactionPoliticsTurnService` integrated into turn loop
- ✅ Loyalty drift with public opinion, population, war/peace factors
- ✅ Succession selection with age, merits, loyalty, diplomacy, rank criteria

### Economy & Research
- ✅ `PlanetEconomyService` with salary, production, population, loyalty calculations
- ✅ `PlanetResearchService` with research progression and system tech levels
- ✅ `EconomyTurnService` orchestrates planet economy processing
- ✅ Production decay and research efficiency bonuses implemented

### Character Lifecycle
- ✅ Pregnancy progression and birth mechanics
- ✅ Old age death rules with noble/common, male/female profiles
- ✅ Stat inheritance from parents to newborn
- ✅ Unborn slot reuse for efficiency

### Event System
- ✅ `EventEngine` with condition/action handler pattern
- ✅ `EventConditionEvaluator` with All/Any/Not composition
- ✅ `EventRunner` and `EventActionExecutor` for event execution
- ✅ `SetFlagActionHandler`, `SetVarActionHandler`, `AddVarActionHandler`
- ✅ `VarEqualsConditionHandler`, `VarGteConditionHandler`, `VarExistsConditionHandler`

---

## 🔴 Offene Aufgaben (Priorisiert)

### P0 - Critical Path

#### 1. Save/Load System
**Issue: spacer-c2o** - Implement save/load for event flags, variables, and game state

**Current State:**
- `InMemoryEventStateStore` exists but data is lost on exit
- `JsonGameTimeStore` persists game clock only
- No persistence for event state (flags, variables)

**Required:**
- [ ] Persist `IEventStateStore` data to JSON (flags, cooldowns, variables)
- [ ] Add load functionality to restore event state
- [ ] Integrate with game save/load workflow
- [ ] Test save/load cycle for data integrity

#### 2. Turn Loop Orchestration
**Current State:**
- Turn services exist but no unified turn orchestrator
- Services are: `EconomyTurnService`, `FactionPoliticsTurnService`, `CharacterLifecycleTurnService`

**Required:**
- [ ] Create `TurnOrchestrator` or `GameTurnService`
- [ ] Define turn phase order (economy → politics → lifecycle → combat → events)
- [ ] Hook up turn processing to game loop
- [ ] Add turn order tests

#### 3. Event System - Core Actions
**Current State:**
- Basic flag/variable actions exist
- No character/planet mutation actions

**Required:**
- [ ] Implement `SetBirth` action (set character birth)
- [ ] Implement `UpdSts` action (update character status)
- [ ] Implement `SetHoryo` action (set heir/prisoner)
- [ ] Implement character lookup by context IDs

---

### P1 - High Priority

#### 4. Fleet Combat - Input Mapping
**Current State:**
- `FleetCombatantFactory` exists but needs fleet data mapping

**Required:**
- [ ] Document source fields for `FleetCombatantInputs` from fleet data
- [ ] Implement fleet-to-inputs mapper service
- [ ] Map `GuardBonus`, `DefenseUpgrade`, `IsConfused` from fleet records
- [ ] Map `DamageMultiplier2/3` from fleet special flags

#### 5. Fleet Combat - Attack Power Builders
**Current State:**
- Basic attack calculation in `FleetCombatantFactory.ComputeTotalAttackPower`

**Required:**
- [ ] Implement `get_ttlatk_kan` (total attack power)
- [ ] Implement `get_fatkval` (fleet attack value)
- [ ] Implement `get_fdefval` (fleet defense value)
- [ ] Add per-fleet modifier tests

#### 6. Faction Politics - Rank/Diplomacy Influence
**Current State:**
- Basic succession with rank thresholds
- No rank-based loyalty promotions

**Required:**
- [ ] Implement rank-based loyalty adjustments
- [ ] Add diplomacy influence on succession
- [ ] Track promotion history

#### 7. Character Lifecycle - Death Events
**Current State:**
- Death events are stubbed (commented out)

**Required:**
- [ ] Implement `EnqueueDeathEvent` with full event data
- [ ] Add disease death rules
- [ ] Add special flag death conditions

#### 8. Character Lifecycle - Birth Events
**Current State:**
- Birth events are stubbed

**Required:**
- [ ] Implement birth notification events
- [ ] Add special birth conditions (twins, special heirs)

---

### P2 - Medium Priority

#### 9. Event System - Player Choice Flow
**Current State:**
- `EventExecutionResult` supports choices
- No UI hook or choice result routing

**Required:**
- [ ] Define UI callback interface for choices
- [ ] Implement choice result routing to event continuation
- [ ] Add choice persistence for save/load

#### 10. Turn Loop - Scheduling/Timing
**Current State:**
- `GameClock` tracks year/month
- No `kikan` (period/days) tracking

**Required:**
- [ ] Add day counter within months
- [ ] Implement `kikan` scheduling system
- [ ] Add turn timing events

#### 11. AI - Basic Fleet Behavior
**Current State:**
- No AI implementation

**Required:**
- [ ] Fleet target selection (enemy planets, fleets)
- [ ] Diplomacy-based aggression levels
- [ ] War/peace decision logic

#### 12. Items & Equipment
**Current State:**
- `ItemCatalog` exists but no effects

**Required:**
- [ ] Map item categories to effects
- [ ] Implement inventory system
- [ ] Add per-turn equipment effects

---

### P3 - Low Priority / Future

#### 13. Planet Economy - Grudge System
**Required:**
- [ ] Add `PlanetGrudge` value object
- [ ] Implement grudge accumulation rules
- [ ] Add grudge effects on loyalty/production

#### 14. Combat Resolver - Full Integration
**Required:**
- [ ] Integrate `CombatResolver` with turn loop
- [ ] Add fleet vs fleet combat
- [ ] Add character duel integration

#### 15. Event System - Full load_eve Interpreter
**Required:**
- [ ] Translate HSP event scripts to event definitions
- [ ] Add text loading (`txtload`) action
- [ ] Implement event script compiler/transpiler

---

## 📋 Technical Debt

### Build Hygiene
- ⚠️ CS8602 warning in `FactionPoliticsService.cs:146` (nullable dereference)
- ⚠️ CS8601 warning in `InMemoryPlanetFleetSpecStore.cs:20` (nullable assignment)

### Architecture
- [ ] Add `ConceptionService` to DI container in `InfrastructureBootstrap`
- [ ] Register `StartPregnancyActionHandler` and `CheckConceptionActionHandler`
- [ ] Move `ConceptionService` to Infrastructure (depends on ICharacterStore)

### Testing
- [ ] Add unit tests for `ConceptionService`
- [ ] Add integration tests for turn loop
- [ ] Add tests for event action handlers

---

## 📊 Progress Summary

| Category | Complete | In Progress | Pending |
|----------|----------|-------------|---------|
| Core Domain Services | ✅ 80% | - | 20% |
| Turn Processing | ⚠️ 60% | - | 40% |
| Event System | ⚠️ 50% | - | 50% |
| Fleet Combat | ✅ 75% | - | 25% |
| Save/Load | ❌ 10% | - | 90% |
| AI | ❌ 0% | - | 100% |
| UI/Presentation | ❌ 0% | - | 100% |

**Overall Progress: ~45%**
