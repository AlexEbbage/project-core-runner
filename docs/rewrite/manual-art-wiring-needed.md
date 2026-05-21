# Phase 4 Manual Art Wiring Needed

## Completed safe wiring

- Player presentation now uses `Assets/CoreRacer/Generated/Prefabs/PlayerVisual_AssetWired.prefab`, copied from the reworked player `VisualRoot` with legacy gameplay components and missing scripts stripped.
- Obstacle and pickup generation now use clean-compatible generated wrappers:
  - `ObstacleRing_AssetWired.prefab`
  - `ObstacleSegment_AssetWired.prefab`
  - `PickupCoin_AssetWired.prefab`
  - `PickupPowerup_AssetWired.prefab`
- `CoreRacer_Main.unity` now has `TunnelV2Prefab` under `TunnelRoot`, `RunZoneManagerV2` with `RunZoneCatalog.asset`, and `VfxRoot` with `VfxManager`.
- `AudioEventLibrary.asset` and `VfxLibrary.asset` were generated from obvious existing audio and VFX assets and assigned on `CoreRacer_Bootstrapper`.

## Still needs manual art/content direction

- UI icon sprites were not assigned. The clean scene currently exposes generic button background `Image` components, not dedicated icon slots for settings, rewarded ads, or menu actions. Assigning `ADE_cog_icon.png` or `Advert Icon.png` to those backgrounds would be visually incorrect.
- Phase 5 authored the clean hub flow with functional placeholder UI surfaces, but dedicated final iconography and richer art treatment for some menu buttons, settings subsections, and progression rows are still open art tasks.
- VFX events without strong existing matches remain intentionally unmapped:
  - `SpeedStreaks`
  - `ScoreMultiplierAura`
  - `MagnetPullLines`
  - `ZoneTransitionFlash`
- Pickup powerup presentation uses the existing generic pickup mesh/material. Bespoke per-powerup art is still needed.
- Coin pickup presentation uses the existing coin pickup visual where safe, but final scale, silhouette, and animation should be reviewed in the editor.
- The generated player visual strips unsafe missing-script components from the legacy source. Any intended legacy visual behaviours should be re-authored as clean CoreRacer components before being reintroduced.
- Legacy release obstacle prefabs such as walls, doors, and fans were not used as direct generation prefabs because the clean obstacle config expects `ObstacleRingView` plus segment prefab contracts.
- Original scenes remain reference-only:
  - `Assets/Scenes/GameScene.unity`
  - `Assets/Ultimate 10 Plus Shaders/Scenes/Review.unity`

## Manual verification focus

- Confirm player visual scale and bank readability during orbital movement.
- Confirm obstacle segment mesh aligns with the six-sided clean tunnel radius and trigger collision area.
- Confirm coin and powerup pickups remain easy to read against the tunnel material.
- Confirm tunnel material and fog remain readable on Android target brightness.
