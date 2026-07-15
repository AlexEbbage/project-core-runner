# Known manual configuration and art wiring

The super-patch installer handles code-side and scene-side wiring that can be inferred safely. These release-specific items still need real project values/assets:

- Production rewarded-ad provider component
- Production interstitial-ad provider component
- Unity IAP installation, store configuration and `CORE_RACER_UNITY_IAP` define when ready
- Store product mapping for `premium_user`; currency packs remain disabled until receipt-backed products are implemented
- Privacy policy, terms and data-deletion HTTPS URLs
- Android application identifier, version and version code
- Real ship, skin, trail and core-FX prefabs/art. The generated starter definitions are safe metadata placeholders and do not invent visual assets
- Final balancing of ship stat multipliers, level speed curves and upgrade costs
- Auto-pilot obstacle layer mask so it scans only gameplay obstacles rather than all colliders
- Device testing for touch safe areas, drag sensitivity, ads, IAP restore and app resume/interruption cases

`ShieldRecharge` currently maps to increased shield capacity because the project does not yet contain a shield recharge loop.
