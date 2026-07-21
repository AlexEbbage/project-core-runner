# Complete UI Rework Acceptance

## Status

Planned and deferred to the final roadmap phase. Do not run this as an acceptance checklist until F22 implementation begins.

## Goal

Prove that the UI Toolkit/LitMotion replacement exposes every important existing behaviour through a coherent, responsive, source-driven interface and leaves no required route or action non-functional.

## Checklist

- [ ] Verify the persistent UI root, screen router, overlays, popups, loading, toasts, and modal input blocking across repeated transitions.
- [ ] Exercise every visible action on the hub and all navigation destinations; confirm no important button is inert.
- [ ] Verify Play, gameplay HUD, pause, tutorial, rewarded prompts, Continue, Game Over, Retry, and Home through a complete run lifecycle.
- [ ] Verify Shop, Hangar, Lab, progression/tasks, achievements, daily login, settings, privacy/support, and every other retained UI surface.
- [ ] Verify keyboard/mouse, touch, controller where supported, safe areas, portrait resolutions, disabled/loading/error states, and interruption-safe animation.
- [ ] Inspect the component gallery and confirm reusable components and their meaningful states match the shipped UI.
- [ ] Run the F22 EditMode and PlayMode suites, then repeat the full physical-device acceptance checklist.
- [ ] Confirm superseded uGUI, Canvas, Inspector listener, UI-only Animator/DOTween code, and temporary compatibility adapters have been removed without deleting useful content assets.

## Evidence to Capture

- Test results for routing, lifecycle, required-element contracts, presenters, modal behaviour, and important state transformations.
- Portrait screenshots or video of every important screen and the complete run lifecycle.
- Device, safe-area, input-mode, and performance notes.
- A final changed/added/deleted asset inventory and confirmation that no legacy UI implementation remains active.
