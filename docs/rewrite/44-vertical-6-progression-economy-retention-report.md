# Rewrite Report 44 — Vertical 6 Progression, Economy, and Retention

## Summary

Vertical 6 connects the run loop to persistent profile rewards and makes Progression/Top Bar refresh from profile truth.

## Runtime changes

- Added profile commit helpers for external meta-service mutations.
- Added reward grant batching.
- Updated daily rewards, achievements, and progression tasks to notify after claim-state mutations.
- Added progression snapshot read model.
- Added economy reward validation rules.
- Updated Progression page/hub refresh behaviour.
- Registered `ProgressionSnapshotService` in the bootstrapper.

## Tooling

Added editor menu actions:

```text
Tools/Core Racer/Vertical 6/Apply Progression Economy Retention
Tools/Core Racer/Vertical 6/Validate Progression Economy Retention
```

## Tests

Added `Vertical6ProgressionEconomyTests` covering:

- run reward grants
- achievement claim-once behaviour
- daily reward streak/profile notification
- progression snapshot task readiness

## Known limitations

Unity compilation was not run in this environment. After applying the patch, run the new EditMode tests and manually verify the progression loop in `CoreRacer_Main`.
