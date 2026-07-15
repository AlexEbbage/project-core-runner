# Menu Content Pruning Rules

This file records what should be cut, merged, delayed, or hidden to keep the first-release Core Racer menu set focused.

## Keep as first-release destinations

| Destination | Reason |
| --- | --- |
| Play | Primary action and run entry. |
| Hangar | Player identity/customisation. |
| Lab | Run power/upgrades. |
| Shop | Commercial flows and restore/remove ads. |
| Progression | Claims, goals, achievements, daily reward, and level progress. |
| Settings | Preferences, support, privacy, consent. |

## Merge into Progression

| Existing/conceptual page | Final handling |
| --- | --- |
| Daily Rewards | Section or modal launched from Progression/hub next action. |
| Tasks | Section inside Progression. |
| Achievements | Section inside Progression. |
| Milestones | Section inside Progression. |
| Level rewards | Section inside Progression. |

## Merge into Lab

| Existing/conceptual page | Final handling |
| --- | --- |
| Powerup upgrades | Lab category. |
| Ship stat upgrades | Lab category if retained. |
| Booster upgrades | Lab category only if boosters exist. |
| Research/tech tree | P2 only; do not overbuild for first release. |

## Merge into Hangar

| Existing/conceptual page | Final handling |
| --- | --- |
| Ship selection | Hangar core. |
| Skins | Hangar category. |
| Trails | Hangar category. |
| Core FX | Hangar category. |
| Cosmetic collection | Hangar filter/category, not a separate destination. |

## Merge into Shop

| Existing/conceptual page | Final handling |
| --- | --- |
| Remove ads | Shop primary item plus Settings shortcut if useful. |
| Restore purchases | Shop primary utility action plus Settings shortcut if useful. |
| Premium currency | Shop only if premium currency exists. |
| Cosmetic packs | Shop offer card, then equip in Hangar. |

## Remove from Core Racer first release

These are not part of the runner/mobile game loop and should not be kept as standalone screens:

- Inbox.
- Calendar.
- Notes.
- Workflows.
- Ideas.
- Projects.
- Prospects.
- CRM/contact style screens.
- Admin planning screens.

If any of these exist in generated UI code, they should be archived or removed from the navigation map before vertical implementation.

## Delay until after the core loop is fun

| Delayed item | Why |
| --- | --- |
| Multi-route/zone select | One polished tunnel is better than several shallow routes. |
| Live events | Needs stable run/reward loop first. |
| Full inventory | Not needed unless consumables become central. |
| Complex tech tree | Risks hiding the simple run-upgrade-repeat loop. |
| Leaderboards | Requires backend/platform decisions and anti-cheat considerations. |
| Social/guild features | Not relevant to closed-testing core. |

## Simplification rules

1. If a page only displays lists of recommendations, remove it or turn one recommendation into a hub next-action card.
2. If a page has no player action, remove it.
3. If a page duplicates another page's action, move the action to the owner screen.
4. If a feature cannot be tested in a vertical slice, delay it.
5. If a menu action requires a backend/live service not available in closed testing, hide it behind dev or P2.
6. If a player cannot explain why a screen exists, merge it.

## First-release home/hub content

The hub should show:

- Game branding/scene/craft backdrop.
- Selected craft.
- Coins and level.
- Best score/last run summary.
- One primary Play button.
- One next action card.
- Bottom navigation.

The hub should not show:

- Multiple recommendation panels.
- Recent activity feeds.
- Long achievement lists.
- Deep shop offers.
- Developer/admin features.

## First-release Progression content

Progression can use tabs/sections:

1. Daily.
2. Tasks.
3. Milestones.
4. Level.

Keep the first implementation shallow but functional. Empty sections should be hidden or replaced with clear coming-soon copy only in dev builds.
