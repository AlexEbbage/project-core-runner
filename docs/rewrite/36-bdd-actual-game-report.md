# Step 2 — BDD the Actual Game, Not the Current Code

Date: 2026-06-03
Scene truth: `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`

## Purpose

This pass creates a behaviour-first source of truth for Core Racer. The goal is to stop current implementation details, legacy docs, broad scaffolding, or half-wired menu pages from defining the final game by accident.

The new BDD files define what the player should experience. Existing code should be mapped to these behaviours during vertical planning, not the other way around.

## Added files

```text
./docs/bdd/README.md
./docs/bdd/product-behaviour-map.md
./docs/bdd/menu-function-map.md
./docs/bdd/acceptance-backlog.md
./docs/bdd/open-decisions.md
./docs/bdd/features/01_core_run.feature
./docs/bdd/features/02_controls_and_tunnel.feature
./docs/bdd/features/03_obstacles.feature
./docs/bdd/features/04_pickups_and_powerups.feature
./docs/bdd/features/05_scoring_rewards_and_progress.feature
./docs/bdd/features/06_menus_and_navigation.feature
./docs/bdd/features/07_ftue.feature
./docs/bdd/features/08_monetisation_compliance_and_services.feature
./docs/bdd/features/09_debug_testing_and_accessibility.feature
./docs/rewrite/36-bdd-actual-game-report.md
```

## Product direction captured

Core Racer should be treated as:

> A fast mobile endless tunnel runner where the player pilots a low-poly mag craft around the inside of a glowing hex tunnel, dodges bold readable hazards, collects hex coins and powers, chases a bright orange core in the distance, upgrades, customises, and runs again.

## First-release behavioural scope

The first closed-testing game should prioritise:

1. Start run from hub.
2. Responsive orbital tunnel movement.
3. Readable hex tunnel and orange core direction cue.
4. Wall, fan, laser, and closing-door obstacle families.
5. Hex coin collection.
6. Shield and magnet as immediate core powerups.
7. Score/coin/XP reward loop.
8. Crash, continue, game over, replay, return-to-hub loop.
9. One useful Lab upgrade purchase.
10. Clear final menu ownership.
11. FTUE that teaches movement, dodge, coin, powerup, crash, and first upgrade.
12. Basic ad/IAP/privacy/service safety for closed testing.

## Important product decisions recommended by the BDD pass

- Bottom navigation should be: Play, Hangar, Lab, Shop, Progression.
- Settings should not be a bottom-nav destination.
- Hangar owns ships and cosmetics.
- Lab owns gameplay upgrades and powerups.
- One polished tunnel route is better than premature route/level breadth.
- First-release obstacle roster should be Walls, Fans, Lasers, Closing Doors.
- First-release powerup roster should be Magnet, Shield, Score Multiplier, Coin Multiplier, and Pilot Assist/Rescue.
- SpeedBoost, SlowMo, and CoinBonanza should be treated as optional until they prove they improve the run.
- Achievements, tasks, dailies, and route unlocks should support the repeat loop, not bury the Play button.

## How to use this next

During Step 3, plan vertical slices by choosing BDD IDs from `docs/bdd/acceptance-backlog.md`.

Recommended first implementation sequence:

1. BDD-001 to BDD-004: run entry and movement.
2. BDD-005, BDD-009, BDD-012, BDD-013, BDD-015: one complete wall/coin/crash/reward loop.
3. BDD-006 to BDD-008: final obstacle roster.
4. BDD-010, BDD-011, BDD-021, BDD-022: powerup roster.
5. BDD-016 to BDD-018: menu/upgrades/FTUE loop.
6. BDD-014, BDD-019, BDD-024, BDD-029, BDD-030: release services and monetisation.

## Completion rule

A feature should not be marked player-ready because a script or prefab exists. It is player-ready only when the relevant BDD scenario passes in `CoreRacer_Main`, is manually verified, and is either automated or deliberately marked as manual-only.
