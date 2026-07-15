# Core Racer — Implementation Order

## Non-negotiable sequence

The build sequence is:

```text
Truth -> Core Run -> Obstacles -> Powerups -> Feel -> Menus -> Progression -> Commercial -> Closed Testing
```

This is intentionally strict. The current codebase already has many scaffolded systems; the project needs playable integration more than more abstractions.

## Per-vertical implementation pattern

For every vertical:

1. Confirm BDD IDs and final menu contract.
2. Identify existing code/assets that already support it.
3. Delete/archive obsolete duplicate paths only when safe.
4. Implement the smallest working path in `CoreRacer_Main`.
5. Add tests for deterministic logic.
6. Add debug controls where useful.
7. Do a manual player-flow pass.
8. Update the relevant docs.

## Vertical 1 recommended build order

1. Hub Play button to run entry.
2. Runtime state machine: Hub, Running, Paused, Crashed, GameOver.
3. Player orbit movement.
4. Hex tunnel motion.
5. Wall obstacle pattern spawn.
6. Coin spawn and collection.
7. HUD binding.
8. Crash and GameOver.
9. Retry/Hub flow.
10. First PlayMode smoke.

## Vertical 2 recommended build order

1. Final obstacle enum/data contract.
2. Wall pass: verify existing behaviour.
3. Fan prefab/logic/telegraph.
4. Laser prefab/logic/telegraph.
5. Closing door prefab/logic/telegraph.
6. Obstacle pattern weighting.
7. Difficulty ramp.
8. Debug force-spawn menu.
9. Collision fairness pass.

## Vertical 3 recommended build order

1. Final pickup/powerup enum/data contract.
2. Hex coin collection polish.
3. Magnet effect.
4. Shield effect.
5. Score multiplier.
6. Coin multiplier.
7. Pilot Assist / Rescue.
8. HUD timers.
9. Powerup spawn weighting.
10. Powerup upgrade hooks.

## Vertical 4 recommended build order

1. Hex tunnel material/lighting direction.
2. Orange core focal point.
3. Mag craft final placeholder model.
4. Obstacle material pass.
5. Coin/powerup visual pass.
6. Ship trail and speed lines.
7. VFX pass.
8. Audio mixer and clips.
9. Camera shake/feedback.
10. Performance/readability pass.

## Vertical 5 recommended build order

1. Single menu router.
2. Splash/bootstrap.
3. Main Hub.
4. Bottom nav shell.
5. Play screen.
6. Hangar.
7. Lab.
8. Shop.
9. Progression.
10. Settings.
11. Pause.
12. Crash/Continue offer.
13. Game Over.
14. No-dead-buttons pass.

## Vertical 6 recommended build order

1. Reward formula.
2. Wallet/profile persistence.
3. Upgrade formula.
4. Upgrade purchase/effect loop.
5. Unlock/cosmetic progression.
6. Daily reward.
7. Tasks.
8. Achievements/milestones.
9. Economy simulation.
10. Balance pass.

## Vertical 7 recommended build order

1. Fake ads/IAP service mode.
2. Rewarded continue.
3. Rewarded double rewards.
4. Remove Ads.
5. Restore purchases.
6. Privacy/consent flow.
7. Analytics event contract.
8. Error/offline handling.
9. Store readiness checklist.

## Vertical 8 recommended build order

1. Build settings validation.
2. Android build.
3. Device smoke matrix.
4. FTUE pass.
5. Save/load resilience.
6. Pause/resume.
7. Performance profile.
8. UI safe area/screen sizes.
9. Tester feedback sheet.
10. Release candidate checklist.
