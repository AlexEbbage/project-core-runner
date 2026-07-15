# Core Racer — BDD to Vertical Map

This file maps the Step 2 BDD backlog to delivery verticals.

The BDD IDs below are the product truth. If implementation currently differs, update implementation unless the product spec is intentionally changed.

## Vertical 0 — Project Truth Stabilisation Gate

| BDD / Contract | Included because |
|---|---|
| Project truth cleanup | Prevents scene/config/doc drift |
| Menu contracts | Ensures final UI targets are known |
| Validation tools | Prevents regressions before adding features |

## Vertical 1 — Core Run MVP

| BDD ID | Behaviour |
|---|---|
| BDD-001 | Starting a run from the hub |
| BDD-002 | Player movement around the tunnel |
| BDD-003 | Returning to a stable hub state |
| BDD-004 | Showing the run HUD |
| BDD-005 | Avoiding wall obstacles |
| BDD-009 | Collecting hex coins |
| BDD-012 | Crashing into an obstacle |
| BDD-013 | Ending the run and showing results |
| BDD-015 | Restarting or returning to hub |

## Vertical 2 — Obstacle Identity

| BDD ID | Behaviour |
|---|---|
| BDD-005 | Walls |
| BDD-006 | Fans |
| BDD-007 | Lasers |
| BDD-008 | Closing doors |
| BDD-014 | Difficulty escalation |
| BDD-025 | Debug force-spawn / test controls |
| BDD-026 | Accessibility/readability for hazards |

## Vertical 3 — Pickups and Powerups

| BDD ID | Behaviour |
|---|---|
| BDD-009 | Hex coins |
| BDD-010 | Collecting a powerup |
| BDD-011 | Active powerup HUD feedback |
| BDD-021 | Upgrading powerups |
| BDD-022 | Powerup balancing and weighting |

## Vertical 4 — Run Feel, Art, Audio, and VFX

| BDD ID | Behaviour |
|---|---|
| BDD-002 | Movement feel |
| BDD-004 | HUD readability |
| BDD-005 to BDD-008 | Obstacle visual identity |
| BDD-009 to BDD-011 | Pickup/powerup feedback |
| BDD-012 | Crash feedback |
| BDD-026 | Accessibility/readability |

## Vertical 5 — Final Menus and Meta Loop

| BDD ID | Behaviour |
|---|---|
| BDD-016 | Main menu navigation |
| BDD-017 | Hangar |
| BDD-018 | Lab |
| BDD-019 | Shop |
| BDD-020 | Progression |
| BDD-023 | Settings |
| BDD-024 | Pause menu |
| Final Menu Feature | Final bottom navigation and modal ownership |

## Vertical 6 — Progression, Economy, and Retention

| BDD ID | Behaviour |
|---|---|
| BDD-013 | Reward summary |
| BDD-018 | Lab upgrades |
| BDD-020 | Progression |
| BDD-021 | Upgrade effects |
| BDD-027 | Daily reward |
| BDD-028 | Tasks |
| BDD-029 | Achievements / milestones |
| BDD-030 | Economy balancing |

## Vertical 7 — Commercial Services and Compliance

| BDD ID | Behaviour |
|---|---|
| BDD-019 | Shop |
| BDD-031 | Rewarded continue |
| BDD-032 | Rewarded double rewards |
| BDD-033 | Remove ads |
| BDD-034 | Restore purchases |
| BDD-035 | Privacy / consent |
| BDD-036 | Analytics event contract |

## Vertical 8 — Closed Testing Hardening

| BDD ID | Behaviour |
|---|---|
| BDD-037 | Save/load resilience |
| BDD-038 | Offline behaviour |
| BDD-039 | Android build smoke test |
| BDD-040 | Device performance |
| BDD-041 | FTUE completion |
| BDD-042 | Closed testing readiness |
