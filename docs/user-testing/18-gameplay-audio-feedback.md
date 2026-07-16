# Gameplay Audio Feedback Review

## Goal

Confirm that core-loop audio communicates actions and state changes without masking gameplay.

## Setup

- Use headphones or device speakers at a comfortable level.
- Start in the main menu, then play a run for at least 75 seconds.

## Checks

1. Menu music starts once and does not layer after returning Home.
2. Play triggers a short transition cue and switches to run music.
3. Coin and powerup pickups are distinct through pitch and weight.
4. Passing obstacles produces a subtle cue that remains comfortable during repeated groups.
5. Damage and death cues remain clear during crash slow motion.
6. Shield activation and shield expiry/break have distinct cues.
7. Speed escalation produces a cue at each threshold, not every frame.
8. Game Over and Home transition cleanly back to menu music.
9. Music and SFX settings control their respective sources.

## Capture

- A short recording covering menu -> run -> pickup -> collision -> Home.
- Console errors or warnings tagged CoreRacer.Audio.

## Test Notes

- Pass/fail:
- Device/output:
- Volume balance notes:
- Repetition fatigue notes:
