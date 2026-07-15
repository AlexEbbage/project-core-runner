# Rewrite Report 38 — Step 4 Vertical Slice Plan

## Summary

Step 4 converts the Step 2 BDD product truth and Step 3 final menu set into an ordered vertical delivery plan.

The plan deliberately prioritises playable integration over more isolated system scaffolding.

## New documentation added

```text
docs/verticals/README.md
docs/verticals/vertical-roadmap.md
docs/verticals/bdd-to-vertical-map.md
docs/verticals/vertical-slice-contracts.md
docs/verticals/implementation-order.md
docs/verticals/qa-and-test-strategy.md
docs/verticals/asset-production-plan.md
docs/verticals/definition-of-done.md
README_STEP4_VERTICAL_PLAN_PATCH.md
```

## Locked vertical order

```text
Vertical 0 — Project Truth Stabilisation Gate
Vertical 1 — Core Run MVP
Vertical 2 — Obstacle Identity
Vertical 3 — Pickups and Powerups
Vertical 4 — Run Feel, Art, Audio, and VFX
Vertical 5 — Final Menus and Meta Loop
Vertical 6 — Progression, Economy, and Retention
Vertical 7 — Commercial Services and Compliance
Vertical 8 — Closed Testing Hardening
```

## Key decision

The next implementation step should be Vertical 1, not menu polish or monetisation.

Vertical 1 must prove:

```text
Hub -> Play -> Run -> Walls/Coins -> Crash -> Game Over -> Retry/Hub
```

## Rationale

The project already contains many broad systems. The highest risk is that the codebase continues to grow without the central 60-second gameplay loop becoming fun and testable.

The vertical plan therefore keeps scope narrow until the core run proves itself.
