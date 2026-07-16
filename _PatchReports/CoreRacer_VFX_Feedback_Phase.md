# Core Racer VFX Feedback Phase

## Outcome

The MVP remains permanently hexagonal while the run now has explicit feedback hooks for pickup collection, damage/collision impact, shield activation and expiry, continue warp, crash dissolve, and speed-scaled particles. Existing pooled VFX definitions are resolved through `VfxManager`; a lightweight runtime speed-particle emitter is created only when the authored scene has no speed-particle component. The crash presentation now stops camera follow, uses 0.2 time scale slow motion, hides/dissolves the player, and restores normal time/camera/player state on Continue or Home. Missing VFX material references were replaced with the valid CoreMaterial asset.

## Changed files

- `Assets/CoreRacer/Runtime/Gameplay/Vfx/VfxManager.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Vfx/VfxPooledInstance.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Vfx/SpeedParticlesControllerV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Pickups/PickupWorldController.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerHealth.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerVisual.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Player/PlayerCameraFollow.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Run/RunController.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/RunZoneManagerV2.cs`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- `Assets/Prefabs/VFX/*.prefab` (valid material reference repair)
- `_PatchReports/CoreRacer_VFX_Feedback_Phase.md`

## Feedback contract

| Gameplay event | Feedback |
| --- | --- |
| Coin pickup | `PickupBurst` |
| Powerup pickup | `PowerupPulse` |
| Damage/collision | `CrashSparks` |
| Shield activation | `ShieldShell` |
| Shield expiry/break | `ShieldBreak` |
| Continue | `ContinueRespawnWarp` |
| Final crash | `CrashSparks` + player dissolve |
| Rising forward speed | speed particle emission and speed |

Crash presentation uses a 0.2 slow-motion scale, no camera follow, larger pooled impact effects, and player dissolve. Continue restores `Time.timeScale = 1`, camera follow, and player visibility.

## Validation

The new PlayMode smoke test verifies VFX manager presence, damage impact, shield activation/expiry routing, and speed-particle intensity. Run the full Unity EditMode and PlayMode suites before accepting the phase.

## Deletions

Nothing must be deleted. See `DELETE_NOTHING.txt`.

## Next phase

Replace the runtime speed-particle fallback with authored, environment-tuned particle prefabs and add environment-specific obstacle/VFX styling after visual review.
