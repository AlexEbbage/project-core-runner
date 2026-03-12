# Feature Registry

## Working Rule

Do not implement a feature unless it appears in this table and the user has chosen it. Use the status values in this file to separate repo truth from roadmap intent.

## Feature Tracking Table

| # | Group | Feature | Description | Status | Notes |
| --- | --- | --- | --- | --- | --- |
| F1 | Core Gameplay | Endless Run Foundation | Core endless run loop, player movement, crash state, restart/menu flow | Implemented | Backed by `GameManager`, player systems, and gameplay UI |
| F2 | Core Gameplay | Obstacle and Pickup Generation | Procedural tunnel obstacle rings, hazard patterns, pickup placement, difficulty ramp | Implemented | Hazard roster includes walls, fans, doors, lasers |
| F3 | Core Gameplay | Score, Combo, and Speed Loop | Distance and pickup scoring, combo growth/decay, speed escalation | Implemented | Central to current run feel and reward output |
| F4 | Core Gameplay | Continue and Rewarded Continue Flow | Crash recovery with capped continues and rewarded ad usage | Implemented | Current cap and ad flow exist in runtime |
| F5 | Core Gameplay | Powerup Gameplay Loop | Magnet, shield, autopilot, multiplier and speed-oriented powerups with upgrade hooks | Implemented | Some roster/tuning remains open for production |
| F6 | UI | In-Run HUD and Feedback | HUD, countdown, reward prompt UI, pause, crash and game-over overlays | Implemented | Rewarded side prompt exists but is gated/partial in rollout |
| F7 | UI | Main Hub Navigation | Top bar, bottom nav, play/shop/hangar/challenges/progression page flow | Implemented | Hub structure exists in current scene |
| F8 | Meta | Currency and Player Profile | Soft/premium currencies, XP/level, unlocks, selected cosmetics, local persistence | Implemented | Backed mainly by `PlayerProfile` |
| F9 | Meta | Shop and Hangar Basics | Item browsing, purchase flow, equip flow, upgrade surfaces, cosmetic previews | Implemented | Catalog depth is still limited |
| F10 | Meta | Daily Login Rewards | Daily cadence rewards with local claim logic and preview UI | Implemented | Local-only, no backend dependency |
| F11 | Meta | Progression Tasks and Rewards | Daily/weekly/monthly task surfaces and reward-node presentation | Partial | UI/config shell exists; broader production rules still need completion |
| F12 | Meta | Environment Unlocks and Level Select | Unlockable environments and goals-based level selection | Partial | Level selection UI exists; unlock rules/content are not fully realized |
| F13 | Services | Ads and Reward Monetisation | Rewarded ads, interstitial hooks, run reward doubling, side prompts | Partial | Core rewarded flow exists; production policy and placement need finalization |
| F14 | Services | Premium Commerce | Remove-ads purchase plus future bundles and currency packs | Partial | Remove-ads exists; broader catalog is planned |
| F15 | Services | Analytics Coverage | Run, monetisation, progression, and economy telemetry | Partial | Some event names and calls exist; coverage is incomplete |
| F16 | UI | UI Polish and Motion Stack | High-fidelity motion, transitions, effects, responsive layout tooling | Planned | DOTween and richer tooling are target additions, not current dependencies |
| F17 | Meta | Expanded Content Catalog | More ships, trails, VFX, packs, bundles, progression reward content | Planned | Documented roadmap, not yet complete in repo |
| F18 | Meta | Long-Term Progression Economy | Stronger upgrade loops around combo progression, powerups, tasks, and unlocks | Planned | Product direction approved; tuning and implementation remain open |

## Feature Groups

- Core Gameplay
- UI
- Meta
- Services
- Tools

## Review Notes

- Keep exactly one feature `In progress` unless the user explicitly wants parallel implementation.
- Treat `Implemented` as "present in repo and usable now", not "fully production complete".
- Treat `Partial` as "some implementation exists but the documented product goal is not complete".
