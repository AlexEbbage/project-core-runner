# Complete Modular UI Toolkit Rework Report

## Date

2026-08-06

## Objective

Replace the existing monolithic UI Toolkit implementation with the final maintainable Core Racer interface architecture while preserving working game/application services and implementing the approved spacious portrait layout with dark colours.

## Inspected starting point

The project already used one `UIDocument`, LitMotion and no active runtime Canvas, but most presentation behaviour lived in a roughly 1,100-line `CoreRacerUiController` and one large root UXML file. Screens, routing, dynamic list construction, service access, modal behaviour and run presentation were coupled together. The implementation therefore met the technology choice but not the requested final modular architecture.

## Final architecture implemented

- One persistent layered UI root.
- Root UXML composed from shell, screen, HUD, overlay and gallery templates.
- Shared theme tokens, typography, components, utilities and responsive layout.
- Screen-specific UXML, USS, View and Presenter files.
- Lifecycle-aware central router with one active hub screen.
- Explicit source subscriptions and fail-fast named-element contracts.
- `CoreRacerUiContext` as the presentation-side service/content context.
- Dedicated shell, gameplay HUD and run-overlay presenters.
- Semantic LitMotion service with central `UiAnimationSettings`.
- Safe-area handling and modal event capture.
- Development-only component gallery.
- Updated installer, validation and EditMode contracts.

## Screens migrated

- Play / Level Select
- Shop
- Hangar
- Lab
- Progress
- Settings
- Gameplay HUD
- Pause
- Tutorial
- Continue
- Game Over
- Generic modal
- Toast and loading layers

## Existing functionality retained

- Profile/currency/XP state
- MVP Core Run selection and next-zone preview
- Booster purchase/equip/loadout
- Run start, pause, continue, finish, x2, retry and home actions
- Score, distance, run-credit, health and powerup HUD events
- Shop service and inventory ownership
- Ship/cosmetic selection and ship upgrade state
- Powerup and passive upgrade purchases
- Daily rewards, tasks and achievements
- Settings, accessibility, tutorial reset and support export
- Existing game/service/bootstrap architecture

## Major systems replaced

- Monolithic screen/action logic in `CoreRacerUiController`
- Monolithic `CoreRacerUiRoot.uxml` screen hierarchy
- Screen routing based on raw `VisualElement` dictionaries
- Repeated inline dynamic-card building in the global controller
- Hardcoded animation timing spread across implementation
- Broad presentation service resolution fields in one controller

## Visual system

The rework uses the approved light-layout hierarchy with a dark palette:

- deep navy background and restrained surfaces;
- off-white text and grey-blue secondary copy;
- orange/red primary actions;
- blue/cyan secondary states;
- gold credits, blue shards and purple rare rewards;
- larger typography and touch controls;
- fewer nested cards and decorative frames;
- portrait-first flex composition;
- a deliberately light gameplay HUD and bottom-sheet run prompts.

## LitMotion integration

`LitMotionUiAnimationService` owns screen, popup, bottom-sheet, toast, invalid, success and attention motion. Active handles are cancelled before replacement, visual transforms are reset, and reduced-motion mode preserves state without unnecessary travel. Timings and eases are centralised in `UiAnimationSettings`, created/wired by the installer.

## Tests updated

- Required-element error contract retained.
- EditMode test now clones the complete root UXML and constructs every modular View.
- Router test now uses actual Presenter lifecycle contracts.
- Existing PlayMode tests retain stable element names and continue to cover Play, HUD, Continue, Game Over, Retry, Home, navigation, modal blocking, MVP route selection and absence of a competing Canvas.

## Documentation updated

- ADR-011
- final UI architecture
- visual system
- screen inventory/replacement map
- implementation plan status
- decision, feature and task registries

## Installer and verification

Use:

```text
Tools > Core Racer > UI Toolkit > Install Final UI
Tools > Core Racer > UI Toolkit > Validate Final UI
```

The installer creates/wires animation settings, assigns content/service-facing references, removes superseded Canvas roots, saves the scene, and validates the modular UXML contract.

## Assets retained

The patch does not delete ship, icon, texture, font, audio, gameplay or other content assets. Existing ScriptableObject content and service behaviour remain the source of displayed state.

## Deletions

No manual file deletion is required for this patch. Existing canonical router, animation and query file paths are overwritten in place to avoid duplicate classes and preserve Unity GUIDs.

## Validation completed outside Unity

- all UXML parses as XML;
- every `Require<T>` name resolves to a named UXML element;
- C# and USS brace checks;
- no duplicate Unity GUIDs after new metadata generation;
- no missing `.meta` files for new runtime UI assets;
- patch whitespace validation.

## Validation not completed here

A Unity Editor/runtime was not available in the patch-generation environment. The following remain genuine verification gates:

- Unity compilation and UXML/USS import diagnostics;
- installer execution and saved scene diff;
- EditMode and PlayMode test execution;
- visual review at target portrait aspect ratios;
- physical Android safe area, touch/back, performance and interruption tests;
- final art/icon/font replacement where current content does not provide approved assets.

## Next step

Import the patch, run the installer and tests, then return the first compiler/import/test error or screenshots from each screen. Use Codex with Unity MCP for live layout inspection and final spacing/art adjustments after the source architecture is proven.
