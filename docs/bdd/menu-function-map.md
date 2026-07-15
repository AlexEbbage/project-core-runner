# Final Menu and Function Map

This file is now a short source-of-truth index. The detailed menu contracts live in `docs/menus/`.

## Locked first-release menu set

### Main player destinations

- Splash / Bootstrap
- Main Hub
- Play
- Hangar
- Lab
- Shop
- Progression
- Settings
- Privacy / Consent

### Run states

- Run HUD
- Pause
- Crash / Continue Offer
- Game Over

### Modal set

- Rewarded Continue Modal
- Rewarded Double Rewards Modal
- Daily Reward Modal
- Purchase Confirmation Modal
- Insufficient Currency Modal
- Confirm Destructive Action Modal
- Error / Offline Modal

### Development-only

- Dev / Debug Menu

## Bottom navigation

The first-release bottom nav is locked to:

1. Play
2. Hangar
3. Lab
4. Shop
5. Progression

Settings is opened through the top-right gear/profile action. It is not a bottom-nav destination.

## Function ownership rules

| Function | Owner |
| --- | --- |
| Start run | Play |
| Active run score/health/powerups/pause | Run HUD |
| Ship/cosmetic preview and equip | Hangar |
| Gameplay and powerup upgrades | Lab |
| Remove ads, restore purchases, IAP/commercial offers | Shop |
| Daily reward, tasks, achievements, milestones, level progress | Progression |
| Audio, haptics, comfort, privacy, support, reset | Settings |
| Consent and privacy gate | Privacy / Consent |
| Continue after crash | Crash / Continue Offer |
| Post-run summary/replay/hub/double rewards | Game Over |

## Detailed contracts

Use these files for implementation planning:

- `docs/menus/final-menu-set.md`
- `docs/menus/screen-contracts.md`
- `docs/menus/navigation-and-modal-flow.md`
- `docs/menus/menu-content-pruning.md`
- `docs/menus/menu-implementation-checklist.md`
- `docs/bdd/features/10_final_menu_set.feature`

## First-release cuts

The following are not separate first-release destinations:

- Daily Rewards
- Tasks
- Achievements
- Inbox
- Calendar
- Notes
- Ideas
- Workflows
- Projects
- Prospects
- Deep route select
- Separate inventory

Daily Rewards, Tasks, Achievements, Milestones, and Level Rewards are merged into Progression.
