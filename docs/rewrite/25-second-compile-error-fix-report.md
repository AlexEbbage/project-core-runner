# Second Compile Error Fix Report

This package fixes the second batch of compile errors reported after importing the launch-ready package.

## Fixed

### GooglePlayTangle / Obfuscator

`GooglePlayTangle.cs` referenced `Obfuscator.DeObfuscate`, but the matching Unity IAP security helper is not guaranteed to exist until Unity IAP is fully installed/configured. The generated tangle now returns its embedded byte array directly so the project can compile without the Unity IAP security helper present.

When Unity IAP is fully configured, regenerate the tangle from Unity IAP if you want the generated obfuscation helper back in the exact Unity-generated form.

### UnityEngine.Time namespace shadowing

Files under `CoreRacer.Services.*` conflicted with the project namespace `CoreRacer.Services.Time`, causing references such as `Time.unscaledDeltaTime`, `Time.frameCount`, and `Time.unscaledTime` to be resolved as a namespace instead of `UnityEngine.Time`.

All runtime `Time.*` references are now explicitly qualified as `UnityEngine.Time.*` where needed.

### Shop / IAP API compatibility

`ShopService` expected `IapPurchaseService.BuyPremium()` and `IapPurchaseService.RestorePurchases()`. These compatibility methods now exist on the SDK-agnostic IAP facade.

The facade now exposes request events that the Unity Purchasing adapter can subscribe to when `CORE_RACER_UNITY_IAP` is enabled.

### RewardGrant.Currency helper

`ShopService` expected `RewardGrant.Currency(CurrencyAmount)`. This helper now exists and maps soft/premium currency grants into the existing reward grant model.

### CurrencyAmount.Type compatibility

UI views expected `CurrencyAmount.Type`. `CurrencyAmount` now exposes `Type` as a compatibility alias for `Currency`.

## Still manual / Unity-side

- Install and configure Unity IAP before release.
- Enable `CORE_RACER_UNITY_IAP` after Unity IAP is installed and the adapter compiles against the package version in your project.
- Regenerate Google Play tangle/receipt validation files through Unity IAP if using full receipt validation.
- Wire real product IDs in Google Play Console and Unity IAP.
- Run Unity compile, scene validation, and production validators after import.
