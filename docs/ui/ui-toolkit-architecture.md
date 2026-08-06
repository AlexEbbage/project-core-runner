# Core Racer Final UI Toolkit Architecture

## Overview

Core Racer uses one persistent `GameUiRoot` containing one `UIDocument` and one `CoreRacerUiController` composition root. The runtime interface is built from modular UXML templates, shared and screen-specific USS, explicit C# Views/Presenters, a lifecycle-aware router, semantic LitMotion animation, and the existing application/game services.

The UI displays and gathers intent. It does not own profile persistence, run rules, purchases, rewards, progression, or gameplay state.

## Runtime layers

```text
GameUiRoot
└── SafeArea
    ├── HudLayer
    ├── ScreenLayer
    │   └── MainMenuScreen
    ├── OverlayLayer
    ├── PopupLayer
    ├── EffectsLayer
    ├── ToastLayer
    ├── LoadingLayer
    └── ComponentGallery (development only)
```

The single root provides deterministic draw order. Full screens route inside `ScreenLayer`; gameplay HUD is independent; Pause/Tutorial use overlays; generic dialogs and run-result sheets use popup ownership; toasts never become a second navigation authority.

## Directory structure

```text
Assets/CoreRacer/Runtime/UI/Toolkit/
├── CoreRacerUiController.cs
├── CoreRacerUiRoot.uxml
├── CoreRacerScreenId.cs
├── CoreRacerScreenRouter.cs
├── IUiAnimationService.cs
├── LitMotionUiAnimationService.cs
├── UiElementQuery.cs
├── Animation/
│   └── UiAnimationSettings.cs
├── Bootstrap/
│   └── CoreRacerUiContext.cs
├── Components/
│   └── UiDynamicElements.cs
├── Infrastructure/
│   ├── UiClassNames.cs
│   ├── UiModalInputBlocker.cs
│   ├── UiSafeAreaController.cs
│   └── UiVisibility.cs
├── Navigation/
│   └── IUiScreenPresenter.cs
├── Shell/
│   ├── MainMenuShell.uxml
│   ├── MenuShellView.cs
│   └── MenuShellPresenter.cs
├── Screens/<Feature>/
│   ├── <Feature>Screen.uxml
│   ├── <Feature>Screen.uss
│   ├── <Feature>ScreenView.cs
│   └── <Feature>ScreenPresenter.cs
├── Hud/
├── Overlays/
├── Gallery/
└── Theme/
    ├── Tokens.uss
    ├── Typography.uss
    ├── Components.uss
    ├── Utilities.uss
    └── Layout.uss
```

## Composition and service access

`CoreRacerUiController` is the composition root, not a global behaviour manager. It:

1. validates the root UXML contract;
2. constructs `CoreRacerUiContext` from serialized content and `GameServices`;
3. constructs animation, toast, modal, shell, HUD, overlay, View, and Presenter objects;
4. registers screens with `CoreRacerScreenRouter`;
5. implements `IRunUiPresenter` by delegating to the HUD and run-overlay presenters;
6. rebuilds presentation when the service registry is replaced;
7. disposes subscriptions and motion handles deterministically.

Do not add screen-specific behaviour back into the composition root.

## View and Presenter responsibilities

A View:

- receives a screen/template root;
- caches all required named elements once;
- exposes typed element references and small visual-state operations;
- contains no persistence, purchasing, reward, or gameplay rules.

A Presenter:

- subscribes/unsubscribes source-driven UI actions;
- reads application/game services;
- converts model state into UI-ready values;
- invokes existing services for user intent;
- refreshes on relevant events rather than every frame;
- requests semantic animation intent.

ViewModels are optional. Add one only where state transformation becomes substantial enough to benefit from a stable immutable model.

## Screen lifecycle and routing

Every screen implements `IUiScreenPresenter` through `UiScreenPresenterBase`:

```text
Initialize → Show → Refresh → Hide → Dispose
```

`CoreRacerScreenRouter`:

- owns one current hub screen;
- hides all non-selected screens;
- calls each Presenter lifecycle explicitly;
- updates bottom-navigation `is-selected` state;
- fails if an unregistered screen is requested.

Ordinary menu navigation does not change scenes. Presenters do not activate arbitrary peer screens.

## Adding a screen

1. Add `<Feature>Screen.uxml` with stable PascalCase `name` values for all C# references.
2. Add screen-specific USS for layout only; reuse theme/components first.
3. Add a View that calls `Require<T>` for mandatory contracts.
4. Add a Presenter with explicit source subscriptions and cleanup.
5. Add the template to `MainMenuShell.uxml`.
6. Add a `CoreRacerScreenId` only if it is routable as a primary screen.
7. Register it in `CoreRacerUiController` and add navigation if needed.
8. Add contract/router or PlayMode coverage.
9. Add component states to the gallery when reusable.

## Adding a reusable component

Use shared UXML/USS for repeated static markup, or a small `VisualElement` class for dynamic list entries that need binding and action callbacks. Reusable controls must have semantic state classes, clear bind/reset behaviour, no game-service access, and no per-frame work.

Do not abstract a one-off section merely to increase class count.

## UXML contracts

C# references elements only by stable names, never child position. `Require<T>` throws an actionable exception containing the contract root, missing name, and expected element type.

Use semantic state classes such as:

```text
is-selected  is-locked  is-disabled  is-equipped
is-claimable is-claimed is-pending  is-error
is-success   is-attention is-hidden
```

Do not set dozens of inline style properties from presenters.

## Theme and responsive layout

- `Tokens.uss`: palette and shared scale.
- `Typography.uss`: readable type hierarchy.
- `Components.uss`: buttons, navigation, resources, progress, modal, reusable rows/tiles.
- `Utilities.uss`: semantic state classes.
- `Layout.uss`: layers, shell, portrait layout and breakpoints.
- screen USS: feature-specific composition only.

The panel uses a 1080×1920 reference with match scaling, plus `UiSafeAreaController`. Use flex, min/max dimensions, wrapping, and selective absolute positioning for HUD/layers. Avoid fixed-position reproduction of one screenshot.

## LitMotion conventions

Use USS pseudo-classes/transitions for hover, press, selection, basic colour, opacity, and tiny scale feedback.

Use `IUiAnimationService` for:

- screen entrance;
- popup entrance/exit;
- bottom-sheet entrance/exit;
- toast;
- invalid-action shake;
- success/attention punch.

`LitMotionUiAnimationService` cancels existing handles and resets translate, scale, and opacity before new motion. Presenters request semantic intent; durations/eases live in `UiAnimationSettings`. Reduced-motion mode preserves state changes and removes unnecessary movement.

## Popups, modal input and gameplay blocking

A visible modal/backdrop is pickable and calls `UiModalInputBlocker` to stop pointer and navigation events from reaching covered controls. Hidden modal roots are removed from layout and picking. Gameplay state still owns pause/resume; UI only invokes the run service/controller.

Do not add a second `UIDocument`, EventSystem, Canvas, or separate popup router.

## Performance

- Cache element queries during View construction.
- Refresh on model events and screen activation.
- Dynamic lists rebuild only when their source changes or screen refreshes.
- Hidden screens do no continuous work.
- HUD changes are driven by score/distance/currency/health/powerup events.
- Stop animation when a screen or root closes.
- Prefer transform/opacity animation over layout properties.

## Component gallery

The development-only gallery exercises shared button, navigation, status, progress, modal, list, empty, error, and animation states with deterministic preview data. Add new reusable states there rather than creating a disconnected demo scene.

## Installer and verification

Run:

```text
Tools > Core Racer > UI Toolkit > Install Final UI
Tools > Core Racer > UI Toolkit > Validate Final UI
```

The installer wires the one `UIDocument`, animation settings, `IRunUiPresenter`, content assets, removes superseded Canvas roots, saves the scene, and validates the modular UXML contract.

Then run EditMode and PlayMode tests and complete portrait/device acceptance.

## AI-friendly rules

- Read the matching View, Presenter, UXML and USS before editing.
- Preserve stable element names unless every contract and test is updated.
- Put structure in UXML, styling/state in USS, presentation logic in Presenter, and rules/state in services.
- Use explicit C# subscriptions, not Inspector event lists.
- Do not edit scene/prefab YAML when the installer or source composition can own the change.
- Do not reintroduce a giant UI manager, runtime fallback builder, per-frame hierarchy query, or competing UI architecture.
