# Complete UI Rework Acceptance

## Status

Technical Editor acceptance passed on 2026-07-21. Player visual signoff on the reference-driven hub refinement and physical Android acceptance remain.

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
- [x] Verify the Play hub exposes one playable MVP Core Run, one locked `NEXT ZONE` preview, and no alternate environment route list.
- [x] Verify first/last carousel arrow visibility and that the locked preview disables Start and displays `Coming soon...`.

## Editor Evidence

- EditMode: 43 passed, 0 failed (`7acc16e370ab46709f81976f301ba047`).
- PlayMode: 8 passed, 0 failed (`1f0ec703ac09467e87be650aee397dd3`).
- Live scene validation: one `UIDocument`, zero legacy Canvases, and `RunSceneReferences.RunUiBehaviour` wired to `CoreRacerUiController`.
- Live `NavigationSubmitEvent` on `PlayButton` entered `Running` with `Time.timeScale == 1`; crash showed Game Over and Continue returned to `Running` with normal time.
- Reference-driven Play hub capture: `Assets/Screenshots/ui-play-hub-reference.png`.
- The hub now presents large profile/XP chrome, centred icon-led currencies, a cog settings action, high score/stars/reward previews, three horizontal run boosters, a large Start action, and icon-led bottom navigation.
- Live panel picking at the Start button centre resolves to a `PlayButton` descendant; passive Overlay, Popup, and Toast layer roots ignore pointer hits so mouse input reaches the menu.
- PlayMode tests isolate and restore the persisted profile. A genuinely new profile renders level 1 with zero XP, currencies, best score, stars, runs, and claimed rewards, and test runs no longer contaminate player state.
- The level card remains geometrically centred by retaining an invisible opposite-arrow layout slot, and all three booster cards fit above the fixed bottom navigation in the initial portrait viewport.
- Star rewards are milestone previews only in this slice; claim-state persistence was not invented without an approved reward-claim contract.

## Evidence to Capture

- Test results for routing, lifecycle, required-element contracts, presenters, modal behaviour, and important state transformations.
- Portrait screenshots or video of every important screen and the complete run lifecycle.
- Device, safe-area, input-mode, and performance notes.
- A final changed/added/deleted asset inventory and confirmation that no legacy UI implementation remains active.
