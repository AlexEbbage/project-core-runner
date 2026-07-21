# Complete UI Rework Acceptance

## Status

Editor acceptance passed on 2026-07-21. Physical Android acceptance follows as the next release-validation phase.

## Goal

Prove that the UI Toolkit/LitMotion replacement exposes every important existing behaviour through a coherent, responsive, source-driven interface and leaves no required route or action non-functional.

## Checklist

- [x] Verify the persistent UI root, screen router, overlays, popups, loading, toasts, and modal input blocking across repeated transitions.
- [x] Exercise hub navigation and important actions in automated/live Editor checks; no retained primary action is inert.
- [x] Verify Play, gameplay HUD, Continue, Game Over, Retry, and Home through a complete run lifecycle.
- [x] Verify Shop, Hangar, Lab, progression/tasks, achievements, daily login, settings, privacy/support, and retained UI surfaces are routed by the replacement root.
- [ ] Verify touch/controller, device safe areas, device performance, and release-build lifecycle on Android.
- [x] Inspect the development component gallery and its reusable states.
- [x] Run the F22 EditMode and PlayMode suites.
- [x] Confirm superseded uGUI, Canvas, Inspector listener, UI-only Animator/DOTween code, and temporary compatibility adapters have been removed without deleting useful content assets.

## Editor Evidence

- EditMode: 43 passed, 0 failed (`14879ac9b4464a40b9711878c16843b0`).
- PlayMode: 7 passed, 0 failed (`884f9c42e6cf4b62a5a737df4964794c`).
- Live scene validation: one `UIDocument`, zero legacy Canvases, and `RunSceneReferences.RunUiBehaviour` wired to `CoreRacerUiController`.
- Live `NavigationSubmitEvent` on `PlayButton` entered `Running` with `Time.timeScale == 1`; crash showed Game Over and Continue returned to `Running` with normal time.
- Captures: `Assets/Screenshots/ui-toolkit-menu.png` and `Assets/Screenshots/ui-toolkit-gameplay.png`.

## Evidence to Capture

- Test results for routing, lifecycle, required-element contracts, presenters, modal behaviour, and important state transformations.
- Portrait screenshots or video of every important screen and the complete run lifecycle.
- Device, safe-area, input-mode, and performance notes.
- A final changed/added/deleted asset inventory and confirmation that no legacy UI implementation remains active.
