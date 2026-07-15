# Core Racer Final Menu Set

This document locks the first-release menu set for Core Racer. It is the product truth for menus and navigation. Implementation should adapt to this, rather than preserving every current UI page just because it exists.

## Menu design goal

The menus should support one clear loop:

```text
Open game -> Play run -> Earn coins and XP -> Upgrade/equip -> Play again
```

Anything that does not support that loop directly should be merged, hidden, delayed, or moved behind debug/dev tooling.

## Final player-facing screens

| Screen | Release status | Purpose |
| --- | --- | --- |
| Splash / Bootstrap | P0 | Load services, profile, save data, consent state, and route the player to the correct first screen. |
| Main Hub | P0 | Primary home screen with Play CTA, currencies, selected ship, and one useful next action. |
| Play | P0 | Start the next run with route/ship summary and minimal optional booster info. |
| Hangar | P0 | Select, preview, and equip ships/cosmetics. |
| Lab | P0 | Upgrade ship/powerup capabilities using coins/currency. |
| Shop | P0/P1 | Remove ads, restore purchases, and simple offers. No cluttered catalogue in first release. |
| Progression | P0/P1 | Daily reward, tasks, milestones, achievements, player level, and next unlocks. |
| Settings | P0 | Audio, haptics, graphics/comfort, privacy, support, and reset options. |
| Privacy / Consent | P0 release gate | Consent, privacy policy, terms, data controls, and tracking options. |
| Run HUD | P0 | During-run score, distance, coins, health/shield, combo, active powerups, and pause. |
| Pause | P0 | Resume, restart/quit confirmation, and quick settings. |
| Crash / Continue Offer | P0/P1 | Explain fatal hit and offer one fair continue if eligible. |
| Game Over | P0 | Score, rewards, XP, new best, replay, hub, and optional double reward offer. |

## Final modal set

| Modal | Release status | Purpose |
| --- | --- | --- |
| Rewarded Continue Modal | P0/P1 | Player chooses whether to watch an ad to continue after a crash. |
| Rewarded Double Rewards Modal | P1 | Player chooses whether to watch an ad to double eligible run rewards. |
| Daily Reward Modal | P1 | Claim daily reward without turning Progression into a cluttered page. |
| Purchase Confirmation Modal | P0/P1 | Confirm IAP/currency purchases and show results. |
| Insufficient Currency Modal | P0 | Explain missing currency and route to play/shop only when appropriate. |
| Confirm Destructive Action Modal | P0 | Used for quit run, restart run, reset save, delete/debug reset. |
| Error / Offline Modal | P0 | Recover gracefully when a purchase, ad, save, or service action fails. |

## Bottom navigation

Bottom navigation is locked for first release:

1. Play
2. Hangar
3. Lab
4. Shop
5. Progression

Settings is not a bottom-nav item. It should be accessible from a top-right gear/profile button.

## Top bar

Most menus should share a consistent top bar:

- Player level or profile icon.
- Coins.
- Premium currency only if it exists in first release.
- Settings gear.
- Optional notification badge for claimable Progression rewards.

The top bar must update immediately after purchases, upgrades, claims, and run rewards.

## Screens explicitly not included as separate first-release destinations

These should not exist as independent top-level destinations in the first-release menu map:

| Removed standalone destination | Where it goes instead |
| --- | --- |
| Achievements | Inside Progression. |
| Tasks | Inside Progression. |
| Daily Rewards | Inside Progression, surfaced by modal when claimable. |
| Inbox | Remove unless there is a real live-ops/customer-support requirement. |
| Calendar | Remove from Core Racer. Not relevant to this game loop. |
| Notes | Remove from Core Racer. Not relevant to this game loop. |
| Workflow | Remove from Core Racer. Not relevant to this game loop. |
| Ideas | Remove from Core Racer. Not relevant to this game loop. |
| Projects | Remove from Core Racer. Not relevant to this game loop. |
| Prospects | Remove from Core Racer. Not relevant to this game loop. |
| Deep route select | Delay until multiple polished routes exist. |
| Separate inventory | Delay unless consumable boosters/items become meaningful. |

## Menu principles

1. Play is always the clearest action.
2. Hangar owns identity and cosmetics.
3. Lab owns power and upgrades.
4. Progression owns claims, goals, level progress, and achievements.
5. Shop owns commercial actions only.
6. Settings owns preferences and compliance controls.
7. Run HUD owns the run; it should not expose permanent progression management.
8. Every destructive action needs confirmation.
9. Every failed service action needs a readable recovery state.
10. Every button should either navigate, perform a confirmed action, or be hidden/disabled with a clear reason.

## First-release UX target

The player should be able to understand the whole menu system in under one minute:

- Tap Play to run.
- Use coins in Lab.
- Change ship in Hangar.
- Claim goals in Progression.
- Use Shop only for purchases/restore/remove ads.
- Use Settings for preferences/privacy.
