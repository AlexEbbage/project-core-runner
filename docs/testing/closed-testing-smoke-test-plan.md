# Core Racer — Closed Testing Smoke Test Plan

## Purpose

This is the minimum manual test pack before uploading a build to Google Play closed testing.

## Build under test

Record for each run:

```text
Date:
Unity version:
Git commit/branch:
Build type: Development / Release / AAB
Android package ID:
Bundle version:
Version code:
Device model:
Android OS version:
```

## Smoke path A — first launch and consent

```gherkin
Scenario: Fresh install reaches the main hub
  Given the app is freshly installed
  When the player launches the game
  Then the game boots without crashing
  And the splash/bootstrap completes
  And consent/privacy flow appears if required
  And the player can reach the main hub
```

Pass criteria:

```text
No crash
No blank screen
No blocked input
Privacy/consent text is readable
Settings privacy links can be opened
```

## Smoke path B — first run

```gherkin
Scenario: Player completes the first playable run loop
  Given the player is on the main hub
  When the player taps Play
  Then the run starts in the hex tunnel
  And the HUD is visible
  And coins and obstacles spawn
  When the player crashes
  Then game over appears
  And score, coins, XP, and retry/hub actions are visible
```

Pass criteria:

```text
Run starts within an acceptable time
Movement input works
Obstacles are readable
Coins can be collected
Crash reliably opens game over
Retry works
Return to hub works
```

## Smoke path C — obstacle identity

Use debug keys or natural spawning.

```text
Walls are readable and avoidable
Fans are visually distinct
Lasers are visually distinct and dangerous
Closing doors communicate timing
No obstacle pattern appears impossible at first-release speed
```

## Smoke path D — pickups and powerups

```text
Magnet attracts coins
Shield blocks one hit or safety window as designed
Score multiplier affects scoring feedback
Coin multiplier affects reward feedback
Pilot Assist provides conservative rescue/safety behaviour
Powerup HUD timers appear and expire cleanly
Powerups reset after crash/retry/hub
```

## Smoke path E — menus/meta loop

```text
Bottom nav shows Play, Hangar, Lab, Shop, Progression
Settings opens from top bar only
Lab upgrades can be bought if enough coins exist
Progression opens and refreshes
Daily reward claim does not break profile state
Shop opens without errors
Restore purchases action is visible
```

## Smoke path F — commercial/compliance

```text
Rewarded continue never grants a reward if ad result is not rewarded/bypassed
Double reward never grants if ad is cancelled/not ready
Premium/Remove Ads activates only through valid product/restore simulation
Privacy Policy opens real URL
Terms opens real URL
Data Deletion opens real URL
Delete local progress requires explicit confirmation
```

## Smoke path G — stability and performance

Run for 5 minutes on a mid-range Android device.

Record:

```text
average FPS:
worst observed FPS:
memory warning/crash? yes/no
thermal throttling signs? yes/no
input latency issues? yes/no
stutters during obstacle/pickup spawn? yes/no
```

Minimum pass:

```text
No crash
No obvious memory growth over repeated runs
No blocking UI errors
Game remains playable after 3 retries
```

## Bug severity

```text
Blocker: crash, cannot start run, cannot exit game over, privacy links missing, broken purchase/ad flow
High: impossible obstacle patterns, unreadable hazards, rewards not saved, severe FPS drops
Medium: visual polish issues, non-blocking UI layout issues, confusing copy
Low: minor animation/audio timing issues
```
