# Core Racer Complete Modular UI Toolkit Rework — Patch Report

## Outcome

The pre-existing UI Toolkit proof-of-concept has been replaced in place. The former large controller and monolithic UXML tree are now a modular runtime architecture with explicit composition, routing, lifecycle, Views, Presenters, reusable components, layered overlays and central semantic animation.

## Runtime layers

```text
GameUiRoot
└── SafeArea
    ├── HudLayer
    ├── ScreenLayer
    ├── OverlayLayer
    ├── PopupLayer
    ├── EffectsLayer
    ├── ToastLayer
    └── LoadingLayer
```

The menu shell owns the persistent profile/resources header and bottom navigation. Play, Shop, Hangar, Lab, Progression and Settings are separate UXML/USS/View/Presenter units. Gameplay presentation is split into an event-driven HUD and run overlays.

## Major additions

- semantic animation settings/service
- UI composition context
- screen lifecycle and routing contracts
- visibility, safe-area and modal-input infrastructure
- reusable booster/shop/action elements
- modular screens and shared shell
- gameplay HUD and run overlays
- modal and toast services
- development component gallery
- shared visual system and documentation

## Behaviour retained

- MVP route selection and next-zone preview
- run start, HUD and run lifecycle presentation
- pause, tutorial, continue, Game Over, retry, home and rewards
- shop and purchasing presentation
- hangar selection/equip/upgrades
- lab upgrades/experiments
- daily rewards, tasks and achievements
- settings, accessibility, support and tutorial reset

## Replacement strategy

Canonical UI files retain their Unity GUIDs but their monolithic implementations are overwritten. The installer removes the superseded Canvas scene hierarchy after validating the replacement tree. No permanent old/new runtime compatibility layer is introduced.

## LitMotion

Presenters request semantic animations through `IUiAnimationService`. The LitMotion implementation tracks handles, cancels interrupted motion and restores opacity/translation/scale before replacement animations. USS remains responsible for small hover/pressed/selected/colour states.

## Tests and documentation

Architecture tests now cover required-element failures, root-tree cloning, construction of modular Views, router lifecycle and the expanded animation contract. Existing public controller entry points and important element names were retained for PlayMode behaviour tests.

ADR-011, project registries, rework plan and architecture documentation were updated. A visual system, screen inventory and implementation report were added.

## Verification boundary

Static source/asset validation passed. Unity compilation, imports, scene installation, tests, input behaviour and device rendering were not executable in this environment.
