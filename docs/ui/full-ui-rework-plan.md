# Complete Core Racer UI Rework

## Status

Implemented and live-validated in the Editor. Android/device acceptance is the next release gate.

## Problem Statement

The current menu setup is visually unacceptable and most buttons are reported non-functional. Existing static validation and isolated route tests did not prove a usable menu. The current uGUI implementation remains only as a behavioural/content reference while non-UI work continues.

## Final Architecture

- One primary persistent UI Toolkit root with only the layers Core Racer needs for HUD, screens, overlays, popups, effects, toasts, and loading.
- `UIDocument` plus UXML structure, shared USS tokens/components/utilities, and screen-specific USS.
- Explicit C# views and presenters/controllers; optional view models only where they reduce complexity.
- Central screen routing and lifecycle instead of arbitrary cross-screen activation.
- Source-driven event subscriptions and stable named element contracts; no critical Inspector listener wiring.
- LitMotion as the semantic meaningful-animation system; USS transitions only for small visual state changes.
- Event/state-driven refresh with cached element queries and no unnecessary per-frame UI rebuilding.
- Modal input blocking that prevents gameplay input passing through menus or dialogs.
- Responsive portrait-first layouts using flex/min/max sizing and appropriate safe-area behaviour.
- A development-only component gallery covering real reusable components, states, errors, loading, dialogs, notifications, and standard motion.

## Migration Rule

This is a complete replacement, not a parallel prototype or permanent compatibility layer. Preserve stable gameplay, application, persistence, economy, progression, ad, analytics, audio, and run-lifecycle behaviour. Extract any such logic currently trapped in UI controllers, migrate every important presentation surface, verify it, then remove superseded uGUI screens, Canvas hierarchies, UI prefabs/controllers, UI-only Animator/DOTween wiring, Inspector events, dead serialized references, and temporary adapters. Retain useful art, fonts, icons, textures, audio, and other content assets.

## Required Coverage

- Main hub and all navigation destinations.
- Play/level selection and pre-run choices.
- Gameplay HUD, pause, tutorial, rewarded prompts, Continue, Game Over, Retry, and Home.
- Shop, Hangar, Lab, progression/tasks, achievements, daily login, settings, privacy/support, and other existing relevant surfaces.
- Popups, confirmations, errors, loading, notifications, badges, currency/status displays, and dynamic lists.

## Definition of Done

- The project compiles and relevant EditMode/PlayMode tests pass.
- Every important existing UI behaviour is accessible through the new UI.
- UI Toolkit is the obvious primary runtime UI architecture.
- UXML, USS, C# presentation, routing/layers, reusable components, LitMotion, modal input blocking, and responsive layouts follow documented conventions.
- No important buttons or screens remain non-functional.
- Screen routing, lifecycle, required-element contracts, presenters, modal behaviour, and important state transformations have useful automated coverage.
- A development component gallery exists.
- Superseded UI implementation and temporary compatibility code are removed after verification; useful content assets are retained.
- Architecture, conventions, extension guidance, animation rules, popup/input behaviour, and testing strategy are documented.
- Full physical-device menu and core-run lifecycle acceptance passes after migration as the following release-validation phase.

## Before Implementation

Inspect the live Unity project and repository thoroughly: screens, Canvas roots, HUD, navigation, overlays, dialogs, services, prefabs, animation, input, bootstrap, tests, docs, assets, and UI classes that contain application logic. Use existing functionality as the behavioural specification and do not invent a different game feature set.
