# Core Racer Gameplay Audio Feedback Phase

## Completed

- Added a bootstrap-owned two-source audio host for music and SFX.
- Connected the existing audio catalog to menu music, run music, run start/end, pickups, obstacle passes, player hit/death, shield activation/break, and speed tiers.
- Reused the checked-in MVP clips with per-event volume and pitch tuning; no new external audio dependency was added.
- Added an idempotent Tools > Core Racer > Playability > Rebuild Core Audio Feedback catalog command.
- This patch does not include or regenerate CoreRacer_Main or the one-piece obstacle prefab.

## Validation

- EditMode: 37 passed, 0 failed.
- PlayMode: 9 passed, 0 failed.
- Live Unity check: menu music played from MenuTrack; starting a run switched to the playing Zone1Music track.
- Integration coverage proves menu, run, obstacle-pass, damage, shield-on, shield-off, and return-to-menu routing.
- Existing Unity IAP initialization warning remains unrelated.

## Manual Review

1. Confirm menu music begins without duplicate playback.
2. Start a run and confirm music transitions cleanly.
3. Collect several coins and one powerup; confirm the powerup cue is heavier/lower than the coin cue.
4. Pass multiple obstacles; confirm the subtle pass cue does not become irritating.
5. Trigger shield activation and expiry/break.
6. Take damage and die; confirm hit and death cues remain readable during crash slow motion.
7. Reach both speed escalation thresholds.
8. Return Home and confirm menu music resumes.
9. Adjust Music and SFX sliders and confirm both channels respect their saved values.

## Deletions

Nothing must be deleted.

## Known Issue

- The one-piece wall collision still depends on the user's manual prefab collider adjustment and is not claimed as resolved by this phase.

## Next Slice

Focused portrait gameplay clarity: HUD hierarchy, score/distance legibility, active powerup readability, pause affordance, and safe-area/device review.
