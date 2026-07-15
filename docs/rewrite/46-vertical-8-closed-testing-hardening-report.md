# Rewrite Report 46 — Vertical 8 Closed Testing Hardening

## Summary

Vertical 8 adds the final closed-testing hardening layer. It does not change the core gameplay loop. It creates a practical readiness gate for Android closed testing and documents the minimum smoke plan before Play Console upload.

## Added

```text
Assets/CoreRacer/Runtime/Services/Diagnostics/ClosedTestingReadinessRules.cs
Assets/CoreRacer/Editor/Verticals/ClosedTestingHardeningVerticalInstaller.cs
Assets/CoreRacer/Tests/EditMode/Vertical8ClosedTestingRulesTests.cs
docs/verticals/vertical-8/closed-testing-hardening-implementation.md
docs/testing/closed-testing-smoke-test-plan.md
docs/store/google-play-closed-testing-gate.md
README_VERTICAL8_CLOSED_TESTING_HARDENING_PATCH.md
```

## Tooling

New Unity menu actions:

```text
Tools/Core Racer/Vertical 8/Apply Closed Testing Hardening
Tools/Core Racer/Vertical 8/Validate Closed Testing Hardening
```

## Readiness checks

The validator checks:

```text
build settings
Android/player settings
privacy links
shop/commercial config
required generated configs
CoreRacer_Main scene wiring
missing script references
vertical tests/docs
SDK status warnings
```

## Expected remaining blockers

The project is still expected to need manual production values before upload:

```text
real hosted privacy policy URL
real hosted terms URL
real hosted data deletion URL
Android build target
production package ID
Android version code
store/IAP/ad SDK configuration decisions
```

## Next step

After applying the patch:

```text
1. Run Vertical 8 apply.
2. Run Vertical 8 validate.
3. Fix blockers.
4. Run EditMode tests.
5. Make an Android development build.
6. Complete the smoke-test plan.
7. Produce an AAB for Google Play closed testing.
```
