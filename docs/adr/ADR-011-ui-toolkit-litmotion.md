# ADR-011: Adopt UI Toolkit and LitMotion as Core Racer's Primary UI Solution

## Status

Accepted. This patch implements the final source architecture; Unity import, scene installation, automated tests, and device acceptance remain verification gates.

## Date

2026-08-06

## Context

The earlier runtime UI had already moved away from uGUI, but it concentrated almost every screen, popup, data transformation, navigation action, and dynamic list in one large controller and one monolithic UXML tree. That made the implementation difficult to review, modify safely, test in isolation, and continue through AI coding agents. It also encouraged dense, nested card layouts that did not match the approved polished-casual visual direction.

Core Racer needs one obvious runtime UI architecture that keeps structure, styling, behaviour, and game state separate. Important actions must remain discoverable in source rather than hidden in Inspector events, deep scene hierarchies, or Animator Controllers.

## Decision

Use Unity UI Toolkit as the primary runtime UI solution:

- UXML owns screen and component structure.
- USS owns the dark theme, typography, spacing, responsive layout, and simple visual states.
- C# Views cache and validate named UI elements.
- C# Presenters translate game/application services into UI state and handle user intent.
- `CoreRacerScreenRouter` owns exclusive hub-screen navigation and lifecycle.
- One persistent `UIDocument` owns ordered HUD, screen, overlay, popup, effects, toast, and loading layers.
- LitMotion is the standard for meaningful, interruptible motion through `IUiAnimationService` and `UiAnimationSettings`.
- USS transitions remain appropriate for local hover, pressed, selected, colour, and opacity state feedback.
- Gameplay and domain state remain outside the UI layer and are accessed through the existing Core Racer composition/services.
- Required C# to UXML contracts use stable names and fail immediately through `Require<T>`.
- Superseded uGUI Canvas hierarchies and Inspector-driven navigation are removed by the installer rather than retained as a permanent compatibility layer.

## Visual decision

Use the approved **light-layout structure with dark-mode colours**:

- spacious portrait-first composition;
- deep navy page background and restrained raised surfaces;
- off-white primary text and grey-blue supporting text;
- orange/red primary actions;
- cyan/blue secondary progression and upgrade states;
- gold credits, blue shards, and purple rare-core rewards;
- large readable controls;
- minimal nested cards, borders, bevels, and glow;
- a low-density gameplay HUD that leaves the 3D low-poly tunnel visible;
- bottom sheets for Continue and Game Over rather than full-screen dashboard panels.

## Why UI Toolkit

UI Toolkit keeps the important implementation in readable `.uxml`, `.uss`, and `.cs` files, supports responsive flex layout, provides deterministic event routing and modal capture, and avoids editing large `RectTransform` hierarchies or scene YAML for ordinary UI work.

## Why LitMotion

USS transitions are well suited to small local state changes but not coordinated, interruptible screen, popup, reward, toast, punch, or bottom-sheet sequences. LitMotion provides explicit handles and cancellation while avoiding Animator Controllers as the default UI behaviour mechanism.

## Consequences

- The final implementation is split by screen and responsibility rather than by one global manager.
- Screens are easier to test and change independently.
- UI contracts fail clearly when UXML and C# drift.
- Hidden screens do not poll or perform per-frame refreshes.
- Modal layers explicitly block covered UI/gameplay interaction.
- Existing game and progression services remain the behavioural source of truth.
- The editor installer is still required to wire the final `UIDocument` and remove superseded Canvas roots.
- Device-specific safe-area, performance, touch, controller, and aspect-ratio acceptance must be run in Unity after import.

## Genuine exceptions

The 3D ship, tunnel, obstacles, pickups, world effects, and gameplay camera remain normal Unity scene/gameplay content. UI Toolkit owns the interface layered over them. A non-UI-Toolkit technique should only remain where a documented technical requirement cannot be met reasonably by UI Toolkit.

## Validation

- EditMode: required-element contracts and router lifecycle/state.
- PlayMode: one `UIDocument`, no competing Canvas, Play-to-run, HUD, Continue, Game Over, Retry, Home, navigation, modal blocking, and level carousel.
- Editor installer: modular UXML contract, scene wiring, and one-root validation.
- Manual/device: portrait safe area, narrow/tall phones, tablet portrait, touch/controller, rapid navigation, interrupted animation, and idle performance.

## Links

- `docs/ui/ui-toolkit-architecture.md`
- `docs/ui/ui-visual-system.md`
- `docs/ui/full-ui-rework-plan.md`
- `docs/decision-registry.md`
