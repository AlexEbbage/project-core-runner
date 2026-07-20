# Portrait Gameplay Clarity

## Setup

1. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` in a 9:16 Game view.
2. Start a normal run from the visible Play route.
3. Repeat on at least one portrait device with a display cutout or non-zero safe area.

## Checklist

- Score is the primary top metric and remains readable over both tunnel shades.
- Distance is distinct from score and increases in metres.
- Coins and hull are labelled and readable without obscuring the tunnel opening.
- The Pause button remains inside the device safe area and has a comfortable touch target.
- Collect each available powerup and confirm its name plus remaining seconds appears below the metrics.
- Activate two different powerups and confirm both statuses remain readable.
- Pause while a powerup is active, wait two seconds, and confirm its remaining duration does not decrease.
- Confirm the pause overlay dims the complete playfield and clearly presents `RUN PAUSED`, `RESUME`, and `HOME`.
- Resume and confirm gameplay plus powerup countdowns continue normally.
- Choose Home and confirm time scale returns to normal and both HUD and pause overlay close.
- Rotate or simulate a cutout-safe-area change and confirm the HUD remains inside the safe region.

## Capture

- One running screenshot with score, distance, coins, hull, and an active powerup.
- One paused screenshot showing the full-screen modal hierarchy.
- Device model, resolution, safe-area/cutout type, and any overlap or readability failure.

## Known Follow-up

- The one-piece obstacle collider remains a separate manual prefab follow-up.
- First-run tutorial and touch-control comprehension are the next implementation slice.
