# ADR-011: Adopt UI Toolkit and LitMotion as the Final UI Architecture

## Status

Accepted and implemented. Android/device acceptance is the next release gate.

## Date

2026-07-21

## Context

Core Racer's current uGUI menu is visually poor and most buttons are reported non-functional. Its scene hierarchy, Inspector wiring, and mixed UI/application responsibilities are difficult to inspect and maintain reliably. Continuing incremental repairs would invest further in a presentation architecture that is not intended to ship.

## Decision

Replace all relevant runtime UI in one complete end-of-roadmap migration with Unity UI Toolkit. Use UXML for structure, USS for styling and simple states, explicit C# views/presenters for behaviour, central routing/layer ownership, and LitMotion for meaningful coordinated animation. Preserve stable underlying game/application systems. After full migration and verification, remove superseded uGUI, Canvas, Inspector-event, UI Animator, and DOTween presentation code rather than retaining a permanent old/new compatibility layer.

## Consequences

- The eventual change is intentionally large and must be planned as a complete product migration.
- Current UI work should be limited to changes strictly required to unblock non-UI development; broad repair or polish is deferred.
- UI contracts become primarily source-readable and AI-friendly in `.cs`, `.uxml`, and `.uss`.
- The migration must cover every important existing UI surface, not only infrastructure or sample screens.
- Modal input capture, interruption-safe animation, responsive layout, reusable components, automated routing/contract tests, a component gallery, and full device acceptance are part of completion.
- Useful art/content assets remain; only superseded implementation is removed.

## Alternatives Considered

- Continue repairing uGUI/DOTween: rejected because the current menu is broken and the architecture is not the desired final state.
- Add UI Toolkit alongside uGUI incrementally: rejected because it would create competing architectures and indefinite compatibility code.
- Use USS transitions or Animator Controllers for all animation: rejected because coordinated, interruptible animation needs a dedicated semantic LitMotion layer.

## Validation

Implemented in `CoreRacer_Main` with one `UIDocument`, source-driven routing, layered modal ownership, LitMotion, and no active legacy Canvas. EditMode and PlayMode coverage plus live Editor interaction prove the replacement architecture and core run lifecycle. Physical Android safe-area, input, and performance acceptance remains the next release gate.

## Links

- `docs/ui/full-ui-rework-plan.md`
- `docs/decision-registry.md` (DR-011)
- `docs/feature-registry.md` (F22)
