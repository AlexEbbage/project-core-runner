# Open Product Decisions

This file tracks product decisions that affect implementation scope.

## Closed by Step 3 — final menu set

### D4 — Does Hangar own upgrades?

Decision: no.

- Hangar owns ships, skins, trails, and core FX.
- Lab owns gameplay upgrades, ship stat upgrades, and powerup upgrades.

### D5 — How many menu destinations should appear in bottom nav?

Decision: five.

1. Play
2. Hangar
3. Lab
4. Shop
5. Progression

Settings lives behind a top-right gear/profile action.

### D8 — Should route/level select be P0?

Decision: no.

One polished default tunnel route is the P0 target. Route/zone selection becomes P1/P2 once multiple routes are polished enough to justify the UI.

### D10 — Are Daily Rewards, Tasks, Achievements, and Milestones separate screens?

Decision: no.

They live inside Progression. Claimable daily rewards may also appear as a modal or hub next-action card.

### D11 — Are Inbox, Calendar, Notes, Workflows, Ideas, Projects, or Prospects part of Core Racer?

Decision: no.

These are not part of the Core Racer runner loop and should not be present in the first-release navigation map.

## Still open before final vertical implementation

### D1 — Is movement continuous or lane-snapped?

Preferred first-release answer: continuous orbital movement with lane-like visual readability. The player should feel smooth movement, while obstacle/pickup placement can still use angular slots internally.

### D2 — What is the first-release obstacle roster?

Preferred first-release answer:

- Walls
- Fans
- Lasers
- Closing doors

Keep spinners/gates only if they map cleanly to one of those families or provide a clearly different readable challenge.

### D3 — What is the first-release powerup roster?

Preferred first-release answer:

- Magnet
- Shield
- Score Multiplier
- Coin Multiplier
- Pilot Assist/Rescue

Do not force SpeedBoost, SlowMo, or CoinBonanza into the first closed-testing build unless they improve the core run.

### D6 — What is the minimum useful progression loop?

Preferred first-release answer: run grants coins and XP; coins buy at least one useful Lab upgrade; XP/level gates or previews later unlocks. Daily/task/achievement systems can exist, but they should not distract from the run-upgrade-repeat loop.

### D7 — What ads exist in first closed testing?

Preferred first-release answer:

- Rewarded continue.
- Rewarded double rewards.
- Interstitials only after multiple runs, never during the first tutorial run, never immediately after a rewarded ad, and disabled by remove-ads.

### D9 — What counts as a completed vertical?

Preferred answer: a vertical is complete when its BDD scenarios are wired in `CoreRacer_Main`, manually verified on device/editor, and either automated or explicitly marked manual-only.
