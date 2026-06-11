# rai7 Variable Analysis Summary

Source: `rai7-235.hsp` (Heat Signature-like game reverse engineering)

---

## 1. Confirmed Mappings (114 variables)

### Debug & Logging
| Variable | Name | Reason |
|----------|------|--------|
| `var_32` | `dbg_error_message` | Set by error code in `dbg_errproc` |
| `var_34` | `text_log_sec_buffer` | Buffer saved via bsave; path: `csv\\log_sec.txt` |
| `var_127` | `dbg_screen_id` | gsel var_127; debug overlay buffer |
| `var_128` | `dbg_window_x` | Debug overlay origin x |
| `var_129` | `dbg_window_y` | Debug overlay origin y |

### Age Distribution Buckets (dbg_anapsn)
| Variable | Name |
|----------|------|
| `var_126` | `age_count_0_to_10` |
| `var_124` | `age_count_10_to_20` |
| `var_130` | `age_count_20_to_30` |
| `var_131` | `age_count_30_to_40` |
| `var_118` | `age_count_40_to_50` |
| `var_132` | `age_count_50_to_60` |
| `var_133` | `age_count_60_to_70` |
| `var_134` | `age_count_gt_70` |
| `var_135` | `age_count_lt_minus40` |
| `var_136` | `age_count_minus40_to_minus30` |
| `var_137` | `age_count_minus30_to_minus20` |
| `var_138` | `age_count_minus20_to_minus10` |
| `var_139` | `age_count_minus10_to_0` |

### Audio (BGM/Voice)
| Variable | Name | Reason |
|----------|------|--------|
| `var_140` | `bgm_file_paths[]` | sdim 30x130; filled with `\bgm\*.wav/mp3` |
| `var_141` | `base_path` | dirinfo(0); root for bgm/voice paths |
| `var_154` | `voice_dpm_path` | DPM:vc*.dpm:<id>.wav in bgm_voice |

### Image/Path Handling
| Variable | Name | Reason |
|----------|------|--------|
| `var_185` | `img_senback_path` | Used as path in picload; example: senback.jpg |
| `var_192` | `page_number_or_pac_id` | Used in pagination and pac id composition |
| `var_193` | `page_button_style` | Button color/style flag for Page buttons |
| `var_196` | `image_variant_code` | a-e (plus prefixes) for image selection |
| `var_197` | `image_dpm_path` | DPM:pac<id>.dpm:<name><variant>.jpg |
| `var_919` | `img_m0811_path` | Used as path in picload; example: m0811.jpg |

### Map/Hex Features
| Variable | Name | Reason |
|----------|------|--------|
| `var_708` | `hex_mine_count` | Minefield strength/count; decreased by `dec_kirai` |
| `var_709` | `hex_feature_type` | Map/battle tile type; icons for syowa/bh/sun/stop (1..4), minefield (5), planet (6) |
| `var_710` | `hex_feature_owner_id` | Owner id for feature (notably minefield) |

### Fort Construction
| Variable | Name | Reason |
|----------|------|--------|
| `var_786` | `fort_under_construction` | Set in `new_fort`, cleared on completion |
| `var_806` | `fort_ship_cost` | Set from `shipCost(..., 8)` |
| `var_808` | `fort_weapon_release_stage` | Derived from weapon research progress |
| `var_809` | `fort_weapon_id` | Set to `wbid(..., 8)` |

### Fleet Combat (Starfighters)
| Variable | Name | Reason |
|----------|------|--------|
| `var_794` | `fleet_starfighter_count` | Compared to `flCarrierLoading`; drives fighter totals |
| `var_818` | `fleet_starfighter_attack_total` | `shOptimizedBattleSpecs(..., 6) * var_794` |
| `var_819` | `fleet_starfighter_defense_total` | `shOptimizedBattleSpecs(..., 7) * var_794` |

### Fleet Combat (Ground Units)
| Variable | Name | Reason |
|----------|------|--------|
| `var_795` | `fleet_ground_unit_count` | Compared to `flLandingPodLoading` |
| `var_815` | `fleet_ground_attack_total` | `shOptimizedBattleSpecs(..., 8) * var_795` |
| `var_816` | `fleet_ground_defense_total` | `shOptimizedBattleSpecs(..., 9) * var_795` |

### Economy & Research
| Variable | Name | Reason |
|----------|------|--------|
| `var_848` | `max_ground_troop_cap` | Cap for `plLandBattleNumber` |
| `var_858` | `owned_planet_count_snapshot` | Set each turn; detects planet-count changes |
| `var_859` | `weapon_release_research_baseline` | Stored sum of `currentResearchLevel/1000` |
| `var_862` | `espionage_target_planet_id` | Set in intel UI |

### Battle Flags
| Variable | Name | Reason |
|----------|------|--------|
| `var_1643` | `fleet_is_confused` | Set by `btl_setkonran`; halves attack/defense |
| `var_1659` | `fleet_defense_stance_active` | Defense formula uses higher multipliers (20/5 vs 18/3) |
| `var_1658` | `battle_slot_action_done` | Per-battle slot flag; set after action resolution |
| `var_1665` | `carrier_strike_cooldown` | Set to 4 after `btl_kansai`; decremented each round |
| `var_2130` | `battle_initiative_score` | Computed from maneuvering + senjyu |
| `var_2246` | `fort_cannon_used` | Per-fort flag for single `btl_canon` action |
| `var_2247` | `battle_tactic_used` | Set when special tactics trigger |
| `var_2248` | `battle_attack_mode` | Selects attack routine (1=beam, 3=canon, 4=special capture) |

### Save/State
| Variable | Name | Reason |
|----------|------|--------|
| `var_2210` | `save_envdat_buffer` | Buffer saved via bsave; path: envdat.dat |
| `var_2233` | `csv_header_buffer` | CSV-like header string saved to csv |

---

## 2. Dimension Groups (Entity Prefix Mapping)

| Array Size | Entity Prefix | Count | Notes |
|------------|---------------|-------|-------|
| 3500 | `ch` (Character) | 97 vars | 57 with ch- prefix patterns |
| 200 | `pl` (Planet) | 90 vars | 6 with pl- prefix patterns |
| 450 | `fl` (Fleet), `sh` (Ship) | 56 vars | 14 fl, 5 sh |
| 1300 | Event-related | 9 vars | Event state tracking |
| 16x1300 | Event-related | 51 vars | Multi-dimensional event arrays |
| 50 | `pl` (Planet sub) | 48 vars | 23 with pl- prefix |
| 100 | `pl` (Planet sub) | 23 vars | |
| 600 | Misc | 10 vars | |
| 80 | Area/Audio | 6 vars | |
| 201 | `pl` (Planet sub) | 6 vars | |
| 30 | Generic | 26 vars | |
| 20 | Generic | 15 vars | |
| 10 | Generic | 16 vars | |

---

## 3. Missing Variables Summary (813 total)

### By Category

| Category | Count | Examples |
|----------|-------|----------|
| **Debug/Analytics** | ~50 | Age buckets, error messages, debug overlays |
| **Audio (BGM/Voice)** | ~30 | File path arrays, volume controls, track IDs |
| **UI/Display** | ~40 | Page buttons, image paths, selection lists |
| **Event System** | ~200 | 1300-dim arrays for event state, flags, indices |
| **Combat Flags** | ~30 | Battle mode selectors, cooldown flags, tactic states |
| **Faction/Politics** | ~50 | Loyalty, alliance, senjyuku flags |
| **Map/Hex** | ~40 | Feature types, mine counts, owner IDs |
| **Fort/Construction** | ~20 | Weapon stages, costs, construction flags |
| **List/Selection** | ~100 | 1300-dim list arrays, sort indices |
| **Temp/Scratch** | ~250 | Local buffers, ginfo temporaries |
| **Unidentified** | ~183 | No clear semantic context |

### High-Value Unmapped Arrays

| Variable | Size | Likely Purpose |
|----------|------|----------------|
| `var_363` | 1300 | List state tracking |
| `var_389` | 1300 | Sort/get indices |
| `var_390` | 1300 | List update flags |
| `var_121` | 200 | Item/event flags |
| `var_32` | sdim 200 | Error message buffer |

---

## 4. Methodology

### Inference Signals Used

1. **Dimension Size Heuristics**
   - Size 3500 → Character arrays (`ch*`)
   - Size 200 → Planet arrays (`pl*`)
   - Size 450 → Fleet/Ship arrays (`fl*`, `sh*`)
   - Size 1300 → Event system arrays

2. **String Context Analysis**
   - JP strings in `dbg_anapsn` → age distribution buckets
   - Path strings (`\bgm\`, `DPM:vc`) → audio file paths
   - Path strings (`DPM:pac`, `.jpg`) → image paths
   - UI labels ("Page") → pagination variables

3. **Function Association**
   - `dbg_*` functions → debug/analytics variables
   - `bgm_*`, `mm*` → audio variables
   - `btl_*` → combat-related variables
   - `list_*` → UI list management

4. **Neighbor Analysis**
   - Variables used on same line as indexed entities
   - Shared dimension declarations
   - Constant values associated with semantic meaning

5. **Usage Pattern Analysis**
   - Index usage into canonical prefixes (`ch()`, `pl()`, `fl()`)
   - Assignment from known entity arrays
   - Comparison with domain-specific constants

---

## Appendix: File History

This document consolidates the following source files (now deleted):
- `rai7_var_mapping_confirmed.md` - 114 confirmed mappings
- `rai7_var_name_suggestions.md` - Dimension groups, methodology
- `rai7_var_missing_candidates.md` - 813 missing vars
- `rai7_var_missing_jp_inferred.md` - JP string context
- `rai7_var_missing_nonjp_inferred.md` - Non-JP context
- `rai7_var_jp_context_missing.md` - Additional JP context
- `rai7_var_string_context_missing.md` - UI keyword associations
- `rai7_var_missing_focus.md` - Entity-sized array analysis
