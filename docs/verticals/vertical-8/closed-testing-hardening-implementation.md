# Vertical 8 — Closed Testing Hardening Implementation

## Goal

Vertical 8 turns the completed vertical-slice prototype into a closed-testing candidate by adding a hard gate around build settings, Android/player settings, privacy/store compliance, scene wiring, missing scripts, tests, docs, and SDK readiness.

This vertical does not add more gameplay. It exists to prevent the project from entering Google Play closed testing with known avoidable blockers.

## Scope

Included:

- final build-scene validation
- Android package/version validation
- privacy/terms/data-deletion URL validation
- required config validation
- scene wiring validation
- missing-script scan across active project areas
- vertical test/doc presence validation
- SDK status aggregation
- closed-testing smoke-test plan
- Play Console gate checklist

Excluded:

- final store art production
- actual hosted privacy policy writing
- real ad unit configuration
- production IAP product creation
- Play Console account operations
- device-lab execution

## Tools

Run:

```text
Tools/Core Racer/Vertical 8/Apply Closed Testing Hardening
Tools/Core Racer/Vertical 8/Validate Closed Testing Hardening
```

The apply command is intentionally conservative. It only restores the agreed build-scene truth:

```text
Assets/CoreRacer/Scenes/CoreRacer_Main.unity
```

The validate command checks whether the project is safe to attempt an Android closed-test build.

## Blocking readiness gates

The project should not enter closed testing unless these are true:

```text
Build Settings contains exactly one enabled scene: CoreRacer_Main
Android build target is selected
Android package ID is production-style and not a default placeholder
Android version code is at least 2
privacy policy URL is HTTPS and not placeholder/example
terms URL is HTTPS and not placeholder/example
data deletion URL is HTTPS and not placeholder/example
required generated configs exist
CoreRacer_Main contains GameBootstrapper, RunController, RunSceneReferences, and MainMenuShell
RunSceneReferences has no missing required references
no active asset/prefab/scene file contains m_Script: {fileID: 0}
vertical EditMode tests exist
BDD/menu/vertical docs exist
```

## Known likely failures after applying

The validator is expected to fail until these are set manually:

```text
real hosted privacy/terms/data-deletion URLs
Android build target
production bundle identifier
Android version code
SDK symbols/adapters depending on chosen ad/IAP/analytics setup
```

Those failures are useful. They prevent a weak upload attempt.

## Definition of done

Vertical 8 is done when:

```text
all Vertical 8 validation blockers are fixed
all EditMode tests pass
one Android development build installs on a device
one Android release/AAB build is produced
the smoke-test plan passes on at least one low/mid Android device
Play Console closed-testing checklist has no unresolved blockers
```
