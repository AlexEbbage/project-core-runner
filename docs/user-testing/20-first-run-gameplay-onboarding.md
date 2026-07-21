# First-Run Gameplay Onboarding

## Status

Behavioural evidence retained. Final overlay, input-blocking, and interaction acceptance is deferred until F22 migrates the tutorial presentation.

## Setup

1. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` in a 9:16 Game view.
2. Use the existing support/debug tutorial reset.
3. Return to the main menu, then press the visible Play button.

## Checklist

- The welcome prompt advances only after the run starts.
- The movement prompt explains left/right or drag steering and advances after real steering input.
- The first passed obstacle advances the dodge prompt.
- A reachable coin is supplied for the coin step and sits at the centre of a tunnel wall rather than in a hex corner.
- Inspect a spawned coin and confirm its root is at tunnel centre, its root Z rotation is `30 + n * 60`, and `PickupBody` is locally offset by the tunnel radius.
- A reachable powerup is supplied for the powerup step.
- The crash explanation appears while the run continues; it does not return to the menu.
- Crashing enters slow motion and displays the Game Over Continue offer.
- The tutorial asks for Continue and does not advance from an overlay-only acknowledgement.
- Pressing the real Continue button restores the player, camera, controls, forward movement, and `Time.timeScale = 1`.
- The tutorial completes after the successful Continue and does not repeat after returning Home and starting another run.
- Running the PlayMode suite must leave tutorial progress reset rather than silently marking the local editor profile complete.
- At full health no redundant `HULL 2/2` label is shown. If the player takes non-lethal damage, `HULL 1/2` appears.
- Retry and Home still complete their normal clean lifecycle.

## Capture

- Screenshot of the movement prompt during a fresh run.
- Screenshot of the corrected coin orientation.
- Screenshot of the crash/Continue prompt with the Game Over offer visible.
- Console logs containing tutorial start/completion and the run start/Continue lifecycle.
- Device model, resolution, average frame rate, and any missed touch or collision.

## Expected External Warnings

- Unity IAP can report that Unity Gaming Services is not initialized.
- Firebase can report that its database URL is absent.
- Treat new gameplay exceptions, missing references, or tutorial stalls as failures.

## Next Slice

MVP mobile acceptance and tuning: touch-control feel, obstacle collision fairness, frame pacing, pooling stability, and a full run/Continue/Retry/Home soak on a portrait device.
