# Core Racer UI Toolkit Architecture

## Runtime Shape

`CoreRacer_Main` owns one persistent `GameUiRoot` with one `UIDocument` and one `CoreRacerUiController`. `CoreRacerUiRoot.uxml` declares the complete visual tree. The root is divided into ordered HUD, screen, overlay, popup, toast, loading, and development-gallery layers.

The UI is source-driven. UXML owns structure, USS owns appearance and simple visual states, and C# owns routing, service subscriptions, state transformation, and actions. There are no critical Inspector `UnityEvent` listeners and no runtime hierarchy generation.

## Files and Ownership

- `CoreRacerUiRoot.uxml`: canonical runtime hierarchy and stable element names.
- `Theme/`: tokens, typography, reusable components, and responsive layout rules.
- `CoreRacerUiController.cs`: composition and presentation binding; resolves application services and implements `IRunUiPresenter`.
- `CoreRacerScreenRouter.cs`: exclusive hub-screen selection and bottom-navigation state.
- `LitMotionUiAnimationService.cs`: semantic, interruption-safe screen, popup, toast, success, and invalid-action motion.
- `UiElementQuery.cs`: fail-fast required-element queries.
- `CoreRacerUiToolkitInstaller.cs`: idempotent Editor installation and scene validation.

## Lifecycle and Routing

The controller caches required elements once, creates the router, binds actions, resolves services, subscribes in `OnEnable`, and unsubscribes in `OnDisable`. The router makes exactly one hub screen visible and refreshes only the selected screen. `RunController` communicates through `IRunUiPresenter`; gameplay does not depend on UI Toolkit types.

Menu, HUD, pause, tutorial, Continue, Game Over, Retry, and Home are state-driven through this contract. Starting or resuming a run restores `Time.timeScale`. Dynamic lists rebuild only when their screen or backing model changes, not every frame.

## UXML and USS Conventions

- Give every element queried by C# a stable PascalCase `name` describing its role, such as `PlayButton`.
- Use classes for styling and state. `is-hidden`, `is-selected`, `state--error`, and `theme--high-contrast` are semantic state classes.
- Add shared colours, spacing, radii, and typography to `Tokens.uss` and `Typography.uss`.
- Put reusable control styling in `Components.uss` and responsive/layer layout in `Layout.uss`.
- Prefer flex, min/max dimensions, and portrait-safe padding. Do not encode one device resolution.
- Do not create player-facing UI from runtime fallback builders.

## Adding a Screen or Component

For a screen, add its UXML subtree under `ScreenLayer`, add a `CoreRacerScreenId`, register the screen and navigation button, then add router and PlayMode coverage. For a reusable component, compose standard UI Toolkit elements, give it one component class, put shared styles in `Components.uss`, and add meaningful normal, disabled, loading, success, and error states to the development gallery.

Use `Require<T>` for mandatory contracts so broken UXML fails with an actionable error. Optional elements must be explicitly handled without reconstructing the hierarchy.

## Animation Rules

Use USS pseudo-classes and transitions for local hover, pressed, focus, colour, and small state changes. Use `IUiAnimationService`/LitMotion for coordinated entrances, popup lifecycle, notifications, success feedback, and invalid actions. Semantic animations cancel their previous handle before starting. Reduced-motion mode removes unnecessary movement while preserving state changes.

## Popups, Overlays, and Input

Pause and tutorial live in `OverlayLayer`; Game Over and generic dialogs live in `PopupLayer`. Modal roots stop pointer and navigation event propagation so input cannot reach gameplay or covered controls. Opening a modal makes its blocking backdrop pickable; closing it removes the layer from layout. Do not add a second `UIDocument`, Canvas, or EventSystem for a popup.

## Bootstrap and Validation

Run `Tools > Core Racer > UI Toolkit > Install Final UI` to install or repair the single scene root idempotently. Run the adjacent validation command to prove there is one `UIDocument`, no legacy Canvas, and a valid `IRunUiPresenter` reference. The saved scene remains canonical.

EditMode tests cover required-element contracts and exclusive routing. PlayMode tests submit events to the real visual tree and cover visible Play, navigation, modal blocking, run lifecycle, Continue, Retry, Home, and absence of legacy Canvas UI. Manual acceptance checks all destinations, portrait layout, input, interruptions, and the gallery. Android safe-area, performance, and physical input acceptance is the next release gate.

## AI Editing Rules

Inspect the live scene and current UXML before editing. Preserve stable element names and serialized content references. Change the smallest responsible layer: structure in UXML, shared presentation in USS, behavior in a presenter/router/service, and gameplay state outside UI. Never introduce a compatibility Canvas, Inspector listener, hidden runtime builder, per-frame query, or second routing authority to work around missing wiring.
