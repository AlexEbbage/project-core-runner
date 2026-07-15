# Rewrite Report 37 — Final Menu Set

## Summary

Step 3 locks the final first-release menu set for Core Racer. The target is a focused mobile runner menu structure that supports the core loop:

```text
Play -> earn -> upgrade/equip/claim -> play again
```

## Final menu set

Player-facing screens:

- Splash / Bootstrap
- Main Hub
- Play
- Hangar
- Lab
- Shop
- Progression
- Settings
- Privacy / Consent
- Run HUD
- Pause
- Crash / Continue Offer
- Game Over

Modal set:

- Rewarded Continue
- Rewarded Double Rewards
- Daily Reward
- Purchase Confirmation
- Insufficient Currency
- Confirm Destructive Action
- Error / Offline

Development-only:

- Dev / Debug Menu

## Bottom navigation decision

Bottom navigation is locked to exactly five destinations:

1. Play
2. Hangar
3. Lab
4. Shop
5. Progression

Settings must live behind a top-right gear/profile action, not bottom navigation.

## Ownership decisions

- Hangar owns ships and cosmetics.
- Lab owns upgrades and power decisions.
- Shop owns commercial purchase/restore/remove-ads flows.
- Progression owns daily reward, tasks, milestones, achievements, level progress, and next unlocks.
- Play owns run entry.
- Run HUD owns the active run only.

## Scope cuts

The following should not exist as separate first-release destinations:

- Inbox
- Calendar
- Notes
- Workflows
- Ideas
- Projects
- Prospects
- Separate Achievements page
- Separate Daily Rewards page
- Separate Tasks page
- Deep route select before multiple polished routes exist

## Files added

- `docs/menus/final-menu-set.md`
- `docs/menus/screen-contracts.md`
- `docs/menus/navigation-and-modal-flow.md`
- `docs/menus/menu-content-pruning.md`
- `docs/menus/menu-implementation-checklist.md`
- `docs/bdd/features/10_final_menu_set.feature`

## Impact on next step

Step 4 should plan verticals using this menu set and the BDD backlog. Recommended first menu-related verticals:

1. Shared menu shell and bottom nav.
2. Play tab to run entry.
3. Game Over to replay/hub loop.
4. Lab upgrade purchase loop.
5. Hangar equip loop.
6. Progression claim loop.
7. Settings/privacy/release basics.

No runtime code was changed by this patch.
