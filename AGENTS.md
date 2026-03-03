# AGENTS.md

## Purpose and Architectural Direction

This repository hosts the project-core-runner Unity project.

The project follows a **Clean, small-project modular architecture** optimized for clarity and maintainability.

Primary top-level structure target:

Assets/Scripts/
- Gameplay/
- UI/
- Services/
- Core/ (optional, minimal shared utilities)

This structure keeps the project simple while maintaining strong modular boundaries.

---

## Transitional Architecture Policy

The current repository may contain legacy top-level folders such as:

- camera/
- config/
- debug/
- feedback/
- fx/
- gameplay/
- localization/
- meta/
- monetisation/
- scenemanagement/
- services/
- settings/
- ui/

These will be **gradually refactored** into the new structure.

### Migration Rule

If a change is made to a module that does not align with:

- Gameplay/
- UI/
- Services/
- Core/

Then that module should be relocated as part of the same change,
provided the relocation is scoped and safe.

Examples:

- localization/ → move under Core/ or Services/ (depending on responsibility).
- monetisation/ → move under Services/.
- scenemanagement/ → move under Core/ or Gameplay/ (based on orchestration role).
- camera/ → move under Gameplay/ if gameplay-driven.
- settings/ → move under Core/ or UI/ depending on usage.

Do not perform broad, unrelated migration work.
Only migrate the module being actively modified.

This ensures gradual convergence without disruptive large-scale refactors.

---

## Architectural Rules

### 1. Gameplay/

Contains all runtime gameplay systems:

- Environment
- Gameflow
- Obstacles
- Pickups
- Player
- Powerups
- Gameplay VFX

Rules:
- Gameflow acts as gameplay orchestrator.
- Submodules must not reach into sibling internals.
- Communication via events or defined interfaces.
- No gameplay logic inside UI.

---

### 2. UI/

Contains all UI logic:

- HUD
- Screens
- Menus
- Popups

Rules:
- UI reacts to gameplay state.
- UI must not directly control gameplay systems.
- UI communicates via events or service APIs.

---

### 3. Services/

Contains external-facing and platform services:

- Ads
- Analytics
- Audio
- Notifications
- Monetisation

Rules:
- Expose clear public APIs.
- Avoid service-to-service tight coupling.
- No hidden global state unless justified.
- Prefer interface-driven access when reasonable.

---

### 4. Core/ (Optional)

Contains small, shared cross-cutting utilities:

- Localization
- Scene management
- Shared configuration
- Lightweight helpers

Rules:
- Keep minimal.
- Avoid turning Core into a dumping ground.
- No gameplay-specific logic here.

---

## Module Boundary Enforcement

- Do not access another module’s internal implementation.
- Communicate across modules via:
  - Interfaces
  - Events
  - Explicit public APIs
- Avoid circular dependencies.
- Prefer additive, non-breaking changes.

---

## Refactor Philosophy

- No large, sweeping refactors unless explicitly requested.
- Refactor opportunistically when touching a module.
- Keep migrations scoped to the area being modified.
- Maintain scene/prefab integrity during moves.

---

## Validation Expectations

- Validate only the module being changed.
- Null-guard serialized fields.
- Avoid breaking inspector bindings.
- Verify moved namespaces and references update correctly.
- Ensure no circular dependencies introduced.

---

## Commit Standards

Commit format:

<type>: <short summary>

Types:
- feat
- fix
- refactor
- docs
- test
- chore

PR must include:
- What changed
- Why
- Validation performed
- Any migration steps required