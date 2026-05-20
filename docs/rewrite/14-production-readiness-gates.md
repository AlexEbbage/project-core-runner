# Production Readiness Gates

Do not ship until these gates pass.

## Build gates

- Unity project opens with zero compile errors.
- `Tools/Core Racer/Validate Project` passes.
- `Tools/Core Racer/Validate Production Readiness` has no blocking issues.
- No missing scripts in release scenes.
- Android build succeeds with release signing.
- App launches from a clean install on a physical Android device.

## Monetisation gates

- Rewarded ad success grants reward exactly once.
- Rewarded ad failure grants nothing and shows a friendly message.
- Premium users bypass continue, double-run reward and interstitial ads.
- Premium users still watch daily double and mid-run rewarded ads.
- Premium purchase grants entitlement and persists after restart.
- Restore purchases grants premium only when platform ownership is confirmed.
- Failed/cancelled purchase does not grant premium.

## Progression gates

- Daily login can be claimed once per UTC day.
- Daily login double reward works through rewarded ad.
- Daily tasks rotate at next UTC day.
- Weekly tasks rotate at the next UTC Monday boundary.
- Monthly tasks rotate on the first day of the next month UTC.
- Claimed tasks cannot be claimed again.
- Achievement rewards cannot be claimed twice.

## Save gates

- Existing old saves either migrate or safely reset with no crash.
- Corrupt profile save falls back to backup/default safely.
- Premium entitlement is restorable from store ownership.
- Settings persist across app restart.

## QA gates

Test at least:

- Low-end Android device
- Mid-range Android device
- Device with poor/no internet
- App suspend/resume mid-run
- App killed after ad reward
- Clock moved backwards/forwards
- Ad unavailable
- Purchase cancelled
- Purchase restored

