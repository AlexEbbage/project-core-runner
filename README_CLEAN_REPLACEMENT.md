# Core Racer clean replacement package

This package is a clean replacement baseline for the Core Racer Unity project.

It keeps your non-code assets from the original upload, removes the legacy `Assets/Scripts` and `Assets/Editor` game code, and adds the modular replacement under `Assets/CoreRacer`.

## What changed

- Legacy large-controller architecture removed from the active code tree.
- New modular runtime added under `Assets/CoreRacer/Runtime`.
- New editor tools added under `Assets/CoreRacer/Editor`.
- Generated Google Play tangle preserved under the new IAP folder.
- Default package manifest added for uGUI, TextMeshPro, IAP, notifications and Input System.
- Docs added under `docs/rewrite`.

## First Unity steps

1. Open this package as a Unity project.
2. Let Unity import and compile.
3. Run `Tools/Core Racer/Generate Default Config Assets`.
4. Run `Tools/Core Racer/Create Clean Replacement Scene`.
5. Run `Tools/Core Racer/Validate Project`.
6. Wire production prefabs, materials, sprites, audio clips and SDK adapters.

## Important note

The original scenes and prefabs may contain missing-script references because the legacy scripts were intentionally removed from the active code tree. Use the scene builder to create clean scenes, then migrate art/audio/config references into the new architecture.

## Premium/ad rule implemented

Premium users bypass:

- continue/respawn ads,
- run double-reward ads,
- interstitial ads.

Premium users still watch rewarded ads for:

- daily login double rewards,
- mid-run rewarded offers,
- optional shop/reward bonuses.

The product ID is centralised as `premium_user` in `IapProductIds`.
