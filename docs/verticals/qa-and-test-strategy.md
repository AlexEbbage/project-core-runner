# Core Racer — QA and Test Strategy by Vertical

## Test pyramid target

Core Racer should use:

- EditMode tests for pure services and deterministic logic.
- PlayMode tests for Unity scene/player flows.
- Manual QA for feel, readability, and fun.
- Device QA for performance, safe area, lifecycle, and Android release behaviour.

## Vertical 0 tests

Required:

- compile check,
- no missing scripts in active scene/config assets,
- build scene points at `CoreRacer_Main`,
- smoke EditMode tests.

## Vertical 1 tests

EditMode:

- score calculation,
- coin reward calculation,
- run state transitions where pure.

PlayMode:

- Hub -> Run,
- Run -> Crash,
- Crash -> GameOver,
- GameOver -> Retry,
- GameOver -> Hub.

Manual:

- movement responsiveness,
- obstacle fairness,
- coin readability.

## Vertical 2 tests

EditMode:

- obstacle pattern deterministic generation,
- difficulty ramp thresholds,
- collision classification if pure.

PlayMode:

- force-spawn wall,
- force-spawn fan,
- force-spawn laser,
- force-spawn door.

Manual:

- can the player see the hazard early enough?
- is the safe route visible?
- are hitboxes fair?

## Vertical 3 tests

EditMode:

- powerup timer lifecycle,
- shield consumption,
- magnet radius/effect,
- multiplier calculations.

PlayMode:

- collect each powerup,
- verify HUD timer,
- verify expiry,
- verify effect removed.

Manual:

- does it feel powerful?
- does it ruin obstacle skill?
- does it create visual clutter?

## Vertical 4 tests

Manual and device-heavy:

- screenshot readability,
- 10-second capture readability,
- small screen test,
- audio mix test,
- performance with busy VFX,
- reduced motion option if needed.

## Vertical 5 tests

EditMode:

- menu route table validity,
- modal stacking rules if pure,
- disabled state rules.

PlayMode/manual:

- every button works or is intentionally disabled,
- back behaviour,
- pause/resume,
- game over transitions,
- no old/unused menus reachable.

## Vertical 6 tests

EditMode:

- wallet persistence,
- upgrade purchase,
- insufficient funds,
- upgrade effect application,
- daily reward timing,
- task progress,
- achievement progress.

Manual:

- economy feels fair,
- repeated run loop feels meaningful.

## Vertical 7 tests

EditMode/fake services:

- rewarded ad success,
- rewarded ad failure,
- ad unavailable,
- purchase success,
- purchase failure,
- restore success/failure,
- remove ads persistence.

Manual/device:

- no-network behaviour,
- store sandbox where available,
- privacy/consent visibility.

## Vertical 8 tests

Release-focused:

- Android build installs,
- first launch,
- FTUE completion,
- app pause/resume,
- low memory-ish restart,
- save continuity,
- screen safe area,
- performance sampling,
- tester feedback capture.
