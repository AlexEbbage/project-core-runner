# Product Requirements

## Project

- Project name: `project-core-racer`
- Architecture mode: `hybrid`
- Platforms: Android only
- Monetization: Rewarded ads, premium/remove-ads purchase, premium currency, soft currency, future bundles and packs

## Summary

`project-core-racer` is a mobile endless tunnel runner where the player pilots a ship down a glowing hexagonal corridor, avoids increasingly difficult obstacle patterns, collects pickups, and sustains combo-driven scoring for as long as possible. The current product shape is broader than the original MVP brief: the live direction includes a hub-based meta layer with currencies, progression, customization, powerup upgrades, daily rewards, challenges, and monetisation hooks around rewarded ads and premium purchases.

The durable product framing for future work is "fast, readable, premium-feeling endless runner with short sessions, strong replay value, and a long-term upgrade chase."

## Core Loop

1. Enter the hub and review currencies, profile progress, available tasks, shop items, and ship setup.
2. Start an endless run from the play page.
3. Fly through the hex tunnel, steer left and right around hazards, collect pickups, trigger powerups, and build combo.
4. Survive rising speed and denser obstacle patterns to push score, distance, and run rewards higher.
5. On crash, optionally consume a rewarded continue if the run still has continues available.
6. End the run, grant currencies and progression rewards, optionally watch an ad for reward doubling, then return to the hub to spend resources or launch another run.

## Target Player Experience

- Feel: Fast, readable, reactive, and premium; the player should feel locked into a tunnel of momentum rather than driving a loose vehicle in open space.
- Difficulty: Easy to understand immediately, punishing at high speed, with mastery coming from pattern recognition and lane control.
- Session length: Short mobile sessions by default, with longer runs driven by player skill and progression.
- Replay value: High, driven by score chasing, unlock goals, upgrade surfaces, live task cadence, cosmetic collection, and future environment progression.

## Systems

- Core gameplay:
  - Endless single-run mode in a hexagonal tunnel.
  - Obstacle roster currently centered on wall segments, fans, doors, and lasers.
  - Pickup collection, combo scoring, time/difficulty speed ramp, crash/continue loop.
- Progression:
  - Profile level and XP.
  - Combo-related upgrades and powerup upgrade surfaces.
  - Daily login rewards and daily/weekly/monthly task surfaces.
  - Planned environment unlock flow tied to score or goal completion.
- Economy:
  - Soft currency and premium currency.
  - Run-end reward granting and reward doubling.
  - Shop, hangar, future currency packs, and premium bundles.
- UI:
  - Main hub with top-bar profile/currency display and bottom navigation.
  - Play, shop, hangar, challenges, and progression pages.
  - HUD, pause, crash/game-over, settings, reward prompts, and item detail modals.
- Audio:
  - Menu/gameplay music split, button feedback, reward/failure cues, and powerup SFX hooks.
- Visual feedback:
  - Bright tunnel glow, hazard readability, reward bursts, collision impact, speed feedback, and premium mobile UI polish.

## Constraints

- Tech constraints:
  - Unity 2022 LTS project.
  - UGUI/TMP/Input System-based UI stack at present.
  - Current persistence relies on ScriptableObject config plus PlayerPrefs-backed profile/state.
  - Current deployment model is a single active gameplay scene with layered runtime UI.
- Team constraints:
  - Documentation must act as the persistent project memory and restore point.
  - Changes should respect hybrid modular boundaries and thin-MonoBehaviour expectations.
- Scope constraints:
  - This docs refresh does not include runtime implementation changes.
  - Forward-looking product details should be captured as `Partial`, `Planned`, or open questions when not yet grounded in repo state.

## Goals

- Establish `docs/` as the source of truth for product, architecture, and execution tracking.
- Align the written product spec with the actual evolved game direction rather than the older MVP-only brief.
- Make future feature work decision-light by separating implemented systems from planned expansion.

## Non-Goals

- Reworking runtime architecture during this pass.
- Locking detailed economy tuning, live-ops cadence, or full monetisation catalog values that are not yet implemented.
- Documenting features as complete when they only exist as UI shell or partial flow.

## Current Product State

- Implemented in repo:
  - Endless run foundation, obstacle generation, score/combo/speed systems, rewarded continues, run rewards, multiple powerups, hub navigation, top-bar currencies, shop/hangar basics, daily login rewards, progression task UI shell, remove-ads purchase flow.
- Partially implemented:
  - Level/environment selection, task/progression depth, analytics event coverage, rewarded side prompt rollout, localization completeness, shop catalog breadth.
- Planned direction:
  - Unlockable environments, broader cosmetic catalog, curated premium bundles and currency packs, stronger long-term progression economy, deeper challenge cadence, and higher-fidelity UI tooling/polish.

## Open Questions

- What exact goal thresholds unlock each new environment or level selection entry?
- Which monetisation offers belong in the first production catalog beyond remove-ads and base currency items?
- Should premium-user bundles be separate from remove-ads ownership or packaged together?
- Which powerups remain in the long-term roster versus being pruned for clarity and balance?
- What analytics schema should become the stable production reporting contract beyond the current implemented events?

## Approval

- Requirements confirmed: Yes, for production-roadmap documentation
- Date: 2026-03-12
