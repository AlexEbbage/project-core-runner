# Addressables and Asset Loading Plan

The package includes `IAssetProvider`, `ResourcesAssetProvider` and `AddressablesAssetProvider`.

## MVP mode

Use `ResourcesAssetProvider` or direct serialized prefab references. This keeps development simple.

## Scale-up mode

When content grows, define `CORE_RACER_ADDRESSABLES`, install Addressables, and wire `AddressablesAssetProvider` to:

- Load ships
- Load skins/trails/core FX
- Load zone themes
- Load VFX and audio banks
- Load seasonal/event content
- Preload next zone assets before transition

## Rules

- Gameplay code should request assets through `IAssetProvider`.
- UI should not hard-reference every seasonal asset.
- Keep local fallback assets for offline play.
