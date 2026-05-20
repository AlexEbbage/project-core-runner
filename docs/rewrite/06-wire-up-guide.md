# Clean replacement wire-up guide

## 1. Generate data

Run:

`Tools/Core Racer/Generate Default Config Assets`

This creates a complete starter set of configs under:

`Assets/CoreRacer/Generated/Configs`

## 2. Generate a clean scene

Run:

`Tools/Core Racer/Create Clean Replacement Scene`

This creates:

`Assets/CoreRacer/Generated/Scenes/CoreRacer_CleanReplacement.unity`

## 3. Replace prototype visuals

Replace generated primitive objects with your real assets:

- player ship prefab,
- tunnel material,
- obstacle ring prefab,
- pickup prefab,
- HUD prefabs,
- game-over UI,
- main menu pages.

## 4. Wire monetisation

- Keep product ID as `premium_user`.
- Add the product in Google Play Console.
- Wire `IapPurchaseService` into your Unity IAP callbacks if you use package-level purchase callbacks.
- Assign `LevelPlayRewardedAdServiceAdapter` to `GameBootstrapper.rewardedAdServiceBehaviour` after SDK calls are wired.

## 5. Verify rules

Premium users should bypass continue, double rewards and interstitials. Premium users should still watch daily login double and mid-run rewarded offers.

Run the validator after every major change:

`Tools/Core Racer/Validate Project`
