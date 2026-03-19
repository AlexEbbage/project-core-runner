# Feature Registry

## Working Rule

Do not implement a feature unless it appears in this file and the user has chosen it. Use the status values here to separate repo truth from roadmap intent.

## Product Goal

- Build and polish a complete run-based arcade experience with strong meta-progression.
- Prioritize the main loop in this order: `Run -> Rewards -> Progression -> Repeat`.
- Treat each experience feature as a combination of `Core System`, `UI`, `UX / Feel`, and optional `Debug Tools`.

## Experience Tracking Table

This table is the product-facing view of the target experience. The `Linked Repo Features` column maps each experience slice back to the stable `F#` identifiers used by the task and script registries.

| Priority | Loop Stage | Experience Feature | Linked Repo Features | Core System | UI | UX / Feel | Debug Tools | Status | Notes |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Run | Powerups | F5, F18 | Five target powerups: `x2 Score`, `x2 Coin Spawn`, `Magnet`, `Autopilot`, `Shield`; time-based effects; shield can use hit-based durability; lab upgrades for duration, strength, and charges | HUD indicators for active powerups plus duration/progress bars | Clear pickup, active, and expiry feedback with immediate gameplay impact | Force-spawn powerups, grant upgrade levels, inspect active timers | Partial | Runtime powerups and HUD support exist, and the active roster has now been narrowed toward the target set. Upgrade content, authored visuals, and final lab presentation still need completion. |
| 2 | Run -> Rewards | Rewarded Offers (Mid-Run) | F6, F13 | Random timed offers during gameplay; limited response window; accept can pause gameplay and open an offer modal; ignore/timeout dismisses cleanly | Small popout with countdown timer and a modal showing reward, rewarded-ad CTA, and ignore action | Should feel visible but non-intrusive, with clean expiry behavior | Force-trigger offer, choose reward type, simulate ad-ready state | Partial | The interval trigger, weighted reward pool, runtime popout, and tapped modal flow are now wired in code. A scene-authored polish pass and user-testing verification are still pending. |
| 3 | Rewards | Game Over Screen | F4, F6, F13 | End-of-run state, run stats, rewards earned, optional `x2 Rewards` ad, bonus grant after ad completion, continue gating | Game-over panel with stats, rewards, combo modifier display, continue CTA, double-reward CTA, restart/menu actions | Ad decision should feel deliberate without being frustrating; buttons should retire cleanly after use | Force end-run state, simulate ad outcomes, inspect reward grant logs | In progress | Base reward settlement is now centralized, the screen receives a resolved run summary payload, continue is timer-gated, and `x2 Rewards` is one-shot. Final inspector wiring and runtime verification still need a pass. |
| 4 | Repeat | Hub (Main Menu) | F7, F8 | Central navigation across progression systems and run entry points | Top bar for profile/currencies/settings, bottom navigation, side-entry icons, notification indicators | Clear entry points to rewards, upgrades, and play-again flow | Toggle page locks, badge states, notification visibility | Partial | The hub exists, but current repo pages are `Play`, `Shop`, `Hangar`, `Challenges`, and `Progression` rather than the target `Shop`, `Ship`, `Lab`, `Level Select`, and `Achievements` information architecture. |
| 5 | Repeat -> Progression | Shop | F9, F14, F17 | Premium bundles, cosmetics, boosters, and currency packs with purchase, unlock, persistence, and entitlement handling | Scrollable sections, headered grids, rarity styling, item detail modal | Browse-to-buy flow should feel quick, clear, and monetisation-safe | Mock catalog loader, fake purchase mode, entitlement reset tools | Partial | Shop controllers and modal views exist, but no authored `ShopDatabase.asset` was found in the checked-in repo. |
| 6 | Progression | Ship Customisation | F9, F17 | Equipable ship parts with persisted loadout and unlock ownership | Ship preview, equip slots, tabbed grids for item categories | Selection should update the preview immediately and support quick comparison | Unlock/equip cheats, preview swapper, missing-art warnings | Partial | Current hangar support is centered on skins, trails, and core FX. The target part taxonomy of `Core`, `Wings`, `Thruster`, and `Colour` is broader than the current authored content. |
| 7 | Progression | Lab | F5, F18 | Upgrade powerups and combo modifier with scaling costs, persistent levels, and clear gameplay impact | Upgrade cards/list showing level, next effect, cost, and lock state | Upgrade paths should feel understandable and worth chasing | Currency grant shortcuts, upgrade-level injector, delta readout | Partial | Upgrade systems exist in code and save data, but the repo currently surfaces them through hangar-style upgrades and an empty `PowerupUpgradeConfig.asset`. |
| 8 | Progression | Achievements | F19 | Multi-tier milestone challenges with escalating rewards and optional combo-modifier boosts | Achievement list with progress indicators, tier rewards, and claim states | Long-tail goals should feel motivating and readable | Seed progress, reset achievements, skip to tier tools | Planned | The repo has a `Challenges` page slot, but no dedicated achievement controller or authored milestone reward flow was found. |
| 9 | Progression | Tasks (Daily / Weekly / Monthly) | F11 | Time-based reset, randomized task selection, task-point economy, claim and claim-all flows, milestone reward track | Task list with rewards, cadence tabs, timers, claim actions, horizontal reward track | Reward availability should be obvious from the hub and inside the page | Time fast-forward, reroll tasks, force completion, claim-state reset | Partial | Task UI and config shell exist, but the data is currently static/mock and runtime reset/claim logic is not yet complete. |
| 10 | Rewards -> Repeat | Daily Login Reward | F10 | Once-per-day claim, streak progression, milestone rewards, optional `Claim x2` rewarded-ad variant | Login modal plus manual access from the hub | Claim flow should be fast and satisfying, with obvious streak value | Clock/day skip tools, reward preview, claim simulator | Partial | Daily login manager and preview UI exist, but no checked-in reward config asset was found and the `x2` ad variant is not yet grounded as complete. |
| 11 | Repeat -> Run | Boosters | F20 | Pre-run booster selection with one active booster per type and per-run effects on XP, rewards, or score | Booster loadout UI before play and active-state indication during runs | The benefit should be obvious without creating confusion or excessive friction | Grant booster inventory, log per-run effect deltas, disable-cost mode | Planned | No dedicated booster system or UI was found in the current repo. |
| 12 | Progression | XP & Levels | F8 | XP gained per run, level progression, feature unlock gates, upgrade-slot/system unlocks | Top-bar level/XP display, lock states, level-up popup/feedback | Growth should feel steady and meaningful, with clear unlock anticipation | Add XP, force level, inspect unlock matrix | Partial | Profile level/XP and some page locks exist, but the broader unlock map and level-up feedback are not fully realized yet. |
| 13 | Repeat -> Run | Level Select | F12 | Level or zone selection, three challenges per level, score thresholds to unlock the next zone, claimable level rewards | Level cards/details showing rewards, completion, locks, and play CTA | New zones should feel aspirational and readable, not buried in the UI | Unlock-all toggle, force challenge completion, reward claim tester | Partial | Level-selection UI shell exists in the play page, but environment unlock rules, authored zone content, and challenge reward logic are not complete. |

## Canonical Repo Feature Map

These `F#` identifiers remain the stable feature IDs used by `task-registry.md` and `script-registry.md`.

| ID | Group | Canonical Feature | Status | Notes |
| --- | --- | --- | --- | --- |
| F1 | Core Gameplay | Endless Run Foundation | Implemented | Core endless run loop, player movement, crash state, restart/menu flow |
| F2 | Core Gameplay | Obstacle and Pickup Generation | Implemented | Procedural tunnel obstacle rings, hazard patterns, pickup placement, difficulty ramp |
| F3 | Core Gameplay | Score, Combo, and Speed Loop | Implemented | Distance and pickup scoring, combo growth/decay, speed escalation |
| F4 | Core Gameplay | Continue and Rewarded Continue Flow | Implemented | Crash recovery with capped continues and rewarded ad usage |
| F5 | Core Gameplay | Powerup Gameplay Loop | Partial | Runtime powerups are present and the current slice has converged the roster, HUD support, and upgrade fallback behavior, but authored content polish is still pending |
| F6 | UI | In-Run HUD and Reward Presentation | Partial | HUD, reward prompts, pause, crash, and game-over UI exist, but some reward UX is still incomplete |
| F7 | UI | Main Hub Navigation | Implemented | Hub structure exists in the current scene |
| F8 | Meta | Currency, Profile, XP, and Account Progression | Partial | Currencies and profile persistence exist; broader unlock and level-progression mapping is incomplete |
| F9 | Meta | Shop, Hangar, and Customisation Basics | Partial | UI and purchase/equip scaffolding exist, but authored content is thin or missing |
| F10 | Meta | Daily Login Rewards | Partial | Structural system exists, but checked-in reward content is missing |
| F11 | Meta | Tasks and Milestone Rewards | Partial | UI/config shell exists; randomized live task logic and claim flow are not complete |
| F12 | Meta | Environment Unlocks and Level Select | Partial | Level selection UI exists; unlock rules/content are not fully realized |
| F13 | Services | Ads and Reward Monetisation | In progress | Continue ads, double rewards, and mid-run rewarded offers are wired; the current active slice is converging the game-over reward presentation and gating |
| F14 | Services | Premium Commerce | Partial | Remove-ads exists; broader premium catalog is planned |
| F15 | Services | Analytics Coverage | Partial | Event names and some calls exist; coverage is incomplete |
| F16 | UI | UI Polish and Motion Stack | Planned | High-fidelity motion, transitions, and tooling remain future work |
| F17 | Meta | Expanded Content Catalog | Planned | More ships, cosmetics, VFX, packs, bundles, and authored shop/customisation content |
| F18 | Meta | Long-Term Progression Economy and Lab Upgrades | Planned | Stronger upgrade loops around combo progression, powerups, tasks, and unlocks |
| F19 | Meta | Achievements and Challenge Tiers | Planned | Multi-tier achievement system is a tracked target but not yet implemented |
| F20 | Meta | Pre-Run Boosters | Planned | Booster selection and per-run effect system is a tracked target but not yet implemented |

## Review Notes

- Keep exactly one canonical feature `In progress` unless the user explicitly wants parallel implementation.
- Treat `Implemented` as "present in repo and usable now", not "fully production complete".
- Treat `Partial` as "some implementation exists but the documented product goal is not complete".
- Experience rows may map to multiple canonical `F#` features when the player-facing slice spans gameplay, UI, and monetisation.
