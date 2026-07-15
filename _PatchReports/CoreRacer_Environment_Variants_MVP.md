# Core Racer Environment Variants MVP

## Outcome

The roadmap now represents one stable six-sided tunnel type with five profile-gated environments: Fire, Lightning, Radiation, Ice, and Firestorm. Environment selection changes authored palette, fog, ambient colour, and wall tint data; it does not change tunnel geometry.

## Changed files

- `Assets/CoreRacer/Runtime/Meta/Levels/LevelRoadmapConfigV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/RunZoneCatalog.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/RunZoneManagerV2.cs`
- `Assets/CoreRacer/Runtime/Gameplay/Environment/TunnelWallGeneratorV2.cs`
- `Assets/CoreRacer/Runtime/UI/MainMenu/LevelSelectPageController.cs`
- `Assets/CoreRacer/Editor/Builders/CoreRacerDefaultConfigBuilder.cs`
- `Assets/CoreRacer/Generated/Configs/LevelRoadmap.asset`
- `Assets/CoreRacer/Generated/Configs/RunZoneCatalog.asset`
- `Assets/CoreRacer/Tests/PlayMode/CoreRunPlayModeSmokeTests.cs`
- tracking docs under `docs/`

## Validation target

- All authored environments retain `TunnelSides: 6`.
- The existing late-environment unlock test now expects Firestorm and asserts the applied zone id.
- Run the full EditMode and PlayMode suites in Unity before player review.

## Deletions

Nothing must be deleted. See `DELETE_NOTHING.txt`.

## Next phase

Add environment-specific obstacle/VFX presentation using the same data contract, then perform a portrait-device readability pass. Geometry changes remain out of MVP scope.
