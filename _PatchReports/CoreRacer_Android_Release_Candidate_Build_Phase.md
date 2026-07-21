# CoreRacer Android Release Candidate Build Phase

## Outcome

A reproducible Android Development APK command is available at `Tools > Core Racer > Build > Android Development APK`. It builds the enabled `CoreRacer_Main` scene through IL2CPP, enables profiler/debug development support, uses Android debug signing without exposing release credentials, forces APK output, and restores both the custom-keystore and App Bundle preferences in a `finally` block.

## Artifact

- Path: `Builds/Android/CoreRacer-1.1.2-dev.apk`
- Size: 134,419,885 bytes (128.2 MB)
- Package: `com.RidgebackGames.CoreRunner`
- Version: `1.1.2` (`versionCode 4`)
- SDK: minimum 24, target 36
- ABIs: ARMv7 and ARM64
- Signature: Android debug certificate, APK Signature Scheme v2 verified
- SHA-256: `B396DD79DFCB38FD917AFD5A37BB939CB7D17B33B46390A337AD3261860510D3`

The APK is ignored by Git and is not included in the changed-files patch zip.

## Changed Files

- `DELETE_NOTHING.txt`
- `docs/feature-registry.md`
- `docs/implementation-plan.md`
- `docs/script-registry.md`
- `docs/task-registry.md`
- `docs/user-testing/21-mvp-mobile-acceptance.md`

## New Files

- `Assets/CoreRacer/Editor/BuildTools/CoreRacerDevelopmentBuildMenu.cs` and Unity `.meta` files
- `_PatchReports/CoreRacer_Android_Release_Candidate_Build_Phase.md`

## Deleted Files

None.

## Validation

- Unity Android IL2CPP build: passed for ARMv7 and ARM64.
- `aapt dump badging`: valid package, version, SDK, and application metadata.
- `apksigner verify --verbose --print-certs`: v2 signature verified with one Android debug signer.
- APK archive: both ABI variants contain `libil2cpp.so` and `libunity.so`.
- Post-build console refresh: zero errors.
- EditMode: 47/47 passed.
- PlayMode: 13/13 passed.
- `adb devices -l`: no connected device, so installation, controls, safe area, thermal, battery, and five-minute on-device profiling remain unverified.

## Installation

1. Connect and authorize an Android test device with USB debugging enabled.
2. Run `adb install -r Builds/Android/CoreRacer-1.1.2-dev.apk`.
3. Launch `Core Runner` and attach the Unity Profiler if required.
4. Follow `docs/user-testing/21-mvp-mobile-acceptance.md`.

## Next Slice

Complete the physical Android device checklist and record screenshots plus first-minute/five-minute performance evidence. If that passes, close MVP core-loop acceptance and move to the next approved content or polish milestone.
