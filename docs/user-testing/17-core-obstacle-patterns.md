# Core Obstacle Pattern Review

## Goal

Confirm that the restored obstacle loop is readable, varied, fair, and increasingly difficult while the tunnel remains six-sided.

## Setup

- Open `CoreRacer_Main` in portrait Game view.
- Start the default route through the visible Play button or `Tools > Core Racer > Playability > Start Core Run`.
- Use a fresh run and continue for at least 75 seconds.

## Checks

1. The tunnel alternates two white/grey longitudinal shades and does not switch to elemental colors.
2. Early obstacles use red wedge geometry aligned to the six tunnel faces, with their outer corners touching the tunnel wall rather than floating inside it.
3. Wedges repeat in short readable groups but rotate to different tunnel-side positions between groups.
4. Rotating fans begin appearing after the opening difficulty window.
5. Sliding doors and narrower wedge gates appear later in the run.
6. Every pattern leaves a readable route through the obstacle.
7. Colliding with any visible red obstacle damages or ends the run and produces orange crash/dissolve feedback.
   - Deliberately hit both a single wedge wall and a three-piece wall; both must register on the first pass, including at increased run speed.
8. Retry starts with the easy wedge group again and does not retain late-run difficulty.

## Capture

- One screenshot of the early wedge group.
- One screenshot of a fan or door later in the run.
- Console errors, if any, with the active pattern name and approximate run duration.
