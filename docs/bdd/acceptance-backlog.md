# BDD Acceptance Backlog

This backlog turns the BDD feature files into implementation-ready acceptance targets.

## P0 — closed-testing core

| ID | Feature | Scenario focus | Vertical |
| --- | --- | --- | --- |
| BDD-001 | Core Run | Start a run from hub and enter countdown | Core Run |
| BDD-002 | Core Run | Auto-forward tunnel run begins | Core Run |
| BDD-003 | Controls | Player steers around tunnel with responsive orbital movement | Core Run |
| BDD-004 | Controls | Input is ignored during countdown, pause, crash, and game over | Core Run |
| BDD-005 | Obstacles | Wall obstacle always leaves a readable safe route | Obstacles |
| BDD-006 | Obstacles | Fan obstacle communicates rotation/wind hazard before contact | Obstacles |
| BDD-007 | Obstacles | Laser obstacle telegraphs before becoming fatal | Obstacles |
| BDD-008 | Obstacles | Closing door creates a timed safe opening and fair warning | Obstacles |
| BDD-009 | Pickups | Hex coins can be collected once and update run HUD immediately | Core Run |
| BDD-010 | Powerups | Shield prevents one fatal hit and gives readable feedback | Powerups |
| BDD-011 | Powerups | Magnet attracts nearby coins without collecting obstacles | Powerups |
| BDD-012 | Scoring | Score, coins, combo, and best score update predictably | Core Run |
| BDD-013 | Crash | A fatal collision pauses run progression and explains failure | Core Run |
| BDD-014 | Continue | Eligible player can watch rewarded ad to continue once/capped | Release |
| BDD-015 | Game Over | Run rewards, XP, and replay/menu actions are shown clearly | Progression |
| BDD-016 | Menus | Bottom navigation has Play, Hangar, Lab, Shop, Progression | Menus |
| BDD-017 | Lab | Player can spend coins on one useful upgrade | Progression |
| BDD-018 | FTUE | First session teaches movement, dodge, coin, powerup, crash loop | FTUE |
| BDD-019 | Settings | Player can control audio, haptics, comfort, and privacy basics | Release |
| BDD-020 | Debug | Dev build can force core scenarios for repeatable testing | Vertical Zero |

## P1 — retention and polish

| ID | Feature | Scenario focus | Vertical |
| --- | --- | --- | --- |
| BDD-021 | Powerups | Score multiplier and coin multiplier apply only while active | Powerups |
| BDD-022 | Powerups | Pilot Assist/Rescue saves or corrects one mistake clearly | Powerups |
| BDD-023 | Feel | Camera shake, pickup burst, impact, speed, and tunnel lighting feedback | Feel |
| BDD-024 | Rewards | Reward doubling applies only after successful rewarded ad callback | Release |
| BDD-025 | Hangar | Player can preview/equip owned ship cosmetic | Progression |
| BDD-026 | Daily | Daily reward can be claimed once per day with readable streak state | Progression |
| BDD-027 | Tasks | Daily tasks progress from real run outcomes and can be claimed | Progression |
| BDD-028 | Achievements | Milestones show progress and claimable rewards | Progression |
| BDD-029 | Shop | Remove ads purchase suppresses interstitial placements | Release |
| BDD-030 | Analytics | Key run, reward, ad, purchase, and navigation events are recorded | Release |

## P2 — expansion

| ID | Feature | Scenario focus | Vertical |
| --- | --- | --- | --- |
| BDD-031 | Routes | Multiple tunnel zones unlock through progression | Progression |
| BDD-032 | Live Ops | Events or temporary offers appear without breaking offline play | Release |
| BDD-033 | Catalog | Broader ship/cosmetic catalogue with clear unlock rules | Progression |
| BDD-034 | Advanced Obstacles | Composite obstacle patterns mix families without becoming unfair | Obstacles |
| BDD-035 | Accessibility | Alternative input/visual modes are validated on small screens | Release |

## Scenario completion states

| State | Meaning |
| --- | --- |
| Proposed | Behaviour is wanted but not started. |
| Implemented | Code/assets exist for the behaviour. |
| Wired | Behaviour is present in `CoreRacer_Main`. |
| Verified | Manually tested in editor/device. |
| Automated | Covered by EditMode or PlayMode tests. |
| Signed Off | Accepted as player-ready for the current release target. |

## Recommended first vertical sequence

1. BDD-001 to BDD-004: run entry and movement.
2. BDD-005, BDD-009, BDD-012, BDD-013, BDD-015: one complete wall/coin/crash/reward loop.
3. BDD-006 to BDD-008: final first-release obstacle roster.
4. BDD-010, BDD-011, BDD-021, BDD-022: powerup roster.
5. BDD-016 to BDD-018: menu/upgrades/FTUE loop.
6. BDD-014, BDD-019, BDD-024, BDD-029, BDD-030: release services and monetisation.
