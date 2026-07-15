# Vertical 6 — Progression, Economy, and Retention Implementation

## Goal

Vertical 6 makes the game loop persistent and rewarding without turning the project into a monetisation task yet.

The supported loop is:

```text
Run -> earn score/coins/XP -> crash -> rewards applied -> top bar/progression refresh -> spend coins in Lab -> claim daily/tasks/achievements -> run again
```

## Included systems

### Run rewards

`RunRewardService` remains the authority for converting run stats into a `RunResult`. It grants:

- soft currency from collected hex coins
- premium currency trickle based on coin thresholds
- XP from score
- run totals, best score, best distance, and powerup totals

`ProgressionEconomyRules` provides shared validation rules for non-negative run rewards.

### Profile change propagation

`PlayerProfileService` now exposes `CommitExternalMutation()` and `Mutate(...)` so meta services that intentionally mutate profile state can save and notify UI listeners.

Updated services:

- `DailyLoginService`
- `DailyRewardCalendarService`
- `AchievementService`
- `ProgressionTaskService`

This fixes the previous problem where claim states and streaks could save silently without refreshing top-bar/progression UI.

### Progression snapshots

`ProgressionSnapshotService` creates a lightweight read model for menus. It summarises:

- level and current XP progress
- currencies
- total runs
- best score/distance
- total coins and powerups
- task readiness summary

### Progression page refresh

`ProgressionPageController` now refreshes its active child panel when shown and when the profile changes. `ProgressionHubController` remembers whether Daily Login, Tasks, or Achievements is currently visible and refreshes that panel.

### Editor validation

`ProgressionEconomyRetentionVerticalInstaller` adds:

```text
Tools/Core Racer/Vertical 6/Apply Progression Economy Retention
Tools/Core Racer/Vertical 6/Validate Progression Economy Retention
```

The apply action assigns generated daily reward, rotating task, and achievement assets to `GameBootstrapper`.

## Acceptance criteria covered

- Run rewards persist to the profile.
- Top-bar currency updates after rewards/claims/upgrades.
- Lab upgrades spend coins and save levels.
- Daily rewards advance streaks and notify UI.
- Achievements grant rewards once.
- Progression has a single source of truth for tasks/achievements/daily reward state.

## Deferred to Vertical 7

- real IAP SKU validation
- real rewarded ad SDK integration hardening
- store compliance flows
- remove-ads production verification
- final economy balancing based on telemetry
