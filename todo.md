
Add constructors/factory methods to enforce invariants.
Add PlanetGrudge and research/production sub‑structures.
Add unit tests for duel resolution to lock behavior.


Wire faction politics into the turn loop (add roster provider, run defection/succession each turn).
Expand loyalty change rules (events + economy posture + per‑turn drift).

add precise per‑label breakdown for the *label_2166 subcalls and map each to a candidate service/use‑case in your codebase.
--------

Note: All parameters/variables prefixed `ana` (e.g., `anaWeaponReleaseChanges`) come from a config file.

----------

# Evidence From Files

* Startup/menu flow is in ``*label_1876 (new game, load, auto‑load, full screen, manual). See rai7.hsp.txt around lines ~64133.
* Main loop is `*label_2166` (turn processing). See rai7.hsp.txt around lines ~81735.
* rai7_turn_loop.md already summarizes `*label_2166` accurately and aligns with the code structure.
* External libs show HSP UI + audio + CSV tooling: hspda.dll (csv/sort), hmm.dll (audio), WINMM.DLL (time), USER32.DLL (window class). See rai7_analysis.md.
* _Hints.txt identifies key roles, labels, and variable meanings (player, trial mode, politics, assignments, pregnancy, etc.).
* rai7_var_mapping_confirmed.md and rai7_var_name_suggestions.md provide concrete variable semantics (e.g., `var_44 name`, `var_732` loyalty, `var_72` atk, `var_73` def, `var_893` sex).
* todo.md enumerates missing domain rules: economy, research, AI, politics, events, combat details.

----------

* Lock the turn loop contract. Mirror the ordering in *label_2166 and stub each phase with tests.
* Model core entities and IDs. Use array sizes as a guide for entity counts and ID ranges.
* Implement data loaders. Load scenario, map, ships, and items first.
* Rebuild event system. Decide if you’ll translate HSP events or define a new DSL. This is the biggest risk.
* Incremental system restores. Economy → Research → Politics → Combat → Events.

---------

**Detailed TODO (Open/Partial)**
* Fleet combat: finish input mapping from fleet data to `FleetCombatantInputs` (FleetCount, GuardBonus, DefenseUpgrade, IsConfused, DamageMultiplier2/3) and document the source fields and ranges.
* Fleet combat: implement attack power builders for `get_ttlatk_kan`, `get_fatkval`, `get_fdefval` and place them in domain/application services with tests.
* Fleet combat: add per‑fleet modifiers (equipment, commander stats, status effects) so `FleetCombatantFactory` reflects rai7.
* Planet economy: define production growth/decay rules and hook them into `PlanetEconomyService` and `EconomyTurnService`.
* Planet economy: define research progression, caps, and tech unlock pacing; integrate with `PlanetResearchService`.
* Planet economy: model influence of population, public opinion, and posture on production/research, not just loyalty drift.
* Character lifecycle: map pregnancy outcomes to events (birth, miscarriage, special cases) and align with rai7 conditions.
* Character lifecycle: implement detailed death rules (disease, special flags, event‑driven death) and verify old‑age thresholds.
* Character lifecycle: define rules for newborn stat inheritance and initial status from parents.
* Faction politics: implement loyalty drift and defection/Joining triggers that match `rep_king` behavior.
* Faction politics: implement rank/diplomacy influence on loyalty, promotions, and succession selection.
* Items & equipment: map item categories to concrete gameplay effects (combat multipliers, events, status changes).
* Items & equipment: implement inventory/ownership persistence and apply effects per turn/encounter.
* Event system: implement a minimal `load_eve` interpreter subset (set_birth, upd_sts, set_horyo, txtload, set_eveflg) so scripted events can mutate game state.
* Event system: define a consistent variable contract for conditions (`year`, `month`, `playerMerits`, `states`, etc.) and document it in code.
* Event system: add player choice handling flow (UI hook, choice result routing, event continuation).
* Turn loop: align RunTurn phases with `*label_2166` (AI, battles, diplomacy, events) and add order‑of‑operations tests.
* Turn loop: implement scheduling/timing (`kikan`, day/month progression) beyond the current month‑tick.
* AI: add fleet movement, target selection, diplomacy and war logic to match rai7 behavior.
* Save/Load: persist additional state beyond GameClock (events state flags, queue, AI state, diplomacy state).

