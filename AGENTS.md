# AGENTS.md

## Unity 2022 Codex Guide

Use this guidance for designing, implementing, refactoring, and testing Unity 2022 projects in Codex.

## Working Model

- Prefer native Codex mechanisms first: `AGENTS.md`, project-local `AGENTS.md`, native skills, and MCP tools.
- Keep setup lean. Do not add new MCP servers, rules, or skills unless they remove repeated work across repositories.
- Use MCP for structured project context, metrics, or automation. Do not use MCP to duplicate instructions that belong in `AGENTS.md` or a skill.

## Unity 2022 Defaults

- Target Unity 2022 LTS conventions unless the repository clearly uses a different version or package baseline.
- Preserve serialized data compatibility. Do not rename serialized fields without an explicit migration path such as `[FormerlySerializedAs]`.
- Treat scenes, prefabs, ScriptableObjects, animator controllers, and input assets as part of the code contract. Code changes that rely on inspector wiring must call out those dependencies.
- Prefer small, composable runtime classes over large god objects.
- Keep frame-loop work predictable. Avoid unnecessary allocations, reflection-heavy patterns, and hidden per-frame work.

## Design Expectations

- Start from player-facing goals: core loop, win or fail states, control feel, feedback, progression, and scope.
- Reduce ambiguity before implementation. State assumptions, constraints, and edge cases when proposing systems.
- Prefer designs that can be tested in isolation and tuned with data rather than hard-coded branching.
- When a feature is risky, propose the thinnest playable version first.

## Code Expectations

- Keep `MonoBehaviour` classes focused on Unity lifecycle, inspector references, input bridging, and scene orchestration.
- Move reusable game logic into plain C# services, domain objects, or testable support classes.
- Use dependency direction intentionally. UI should observe or request through stable interfaces rather than mutate gameplay state directly.
- Preserve public APIs unless the task explicitly includes a breaking change.
- Minimize hidden coupling across systems. Prefer explicit references, constructor parameters, or clear service boundaries.

## UI Expectations

- Default to `hand-authored UI` with `reference-bound controllers` for player-facing screens.
- Treat the Unity editor hierarchy, layout groups, anchors, prefabs, and serialized references as the canonical UI source once a screen is authored.
- Prefer controller scripts that bind data, state, and events to existing serialized references rather than creating layout in play mode.
- `Editor scaffolding` is allowed for initial setup, repeated shells, or one-shot regeneration passes, but the generated result should become normal authored UI afterward.
- When using editor scaffolding, keep the regeneration boundary clear and avoid workflows that silently overwrite intentional hand edits.
- Use prefabs, shared theme assets, and reusable view components to reduce repetition instead of rebuilding widget trees in runtime code.

## UI Anti-Patterns To Avoid

- Avoid `runtime bootstrap` for player-facing UI by default. Do not create or rebuild full screen hierarchies in `Awake`, `OnEnable`, or `Start` unless the user explicitly asks for that pattern.
- Avoid fallback code that destroys and recreates authored UI because references are missing. Fix the wiring instead.
- Avoid mixing layout construction and behavior logic in the same large view script when a hand-authored hierarchy plus serialized references would be clearer.
- Avoid mutating authored layout structure during play just to "help" missing scene setup, except for dev-only debug tools or truly dynamic content containers.
- Avoid hiding inspector or scene wiring problems with `FindObjectOfType` and emergency UI reconstruction when a stable serialized reference should exist.

## Preferred UI Terminology

- `Runtime bootstrap`: play mode code creates or rebuilds UI hierarchy from code.
- `Editor scaffolding`: editor tools generate UI hierarchy ahead of play.
- `Hand-authored UI`: hierarchy is built manually in the Unity editor.
- `Reference-bound controller`: script binds behavior to serialized UI references without constructing layout.

## Testing Expectations

- Prefer the smallest test layer that can prove the behavior: plain C# unit tests first, then Edit Mode tests, then Play Mode tests when Unity runtime behavior matters.
- Test behavior, invariants, and regressions rather than mirroring implementation line by line.
- For gameplay work, include manual verification notes when automated coverage is weak or impossible.
- For refactors, identify serialization, prefab wiring, scene references, and animation or event hookups as regression risks.

## Repository Rules

This repository uses a hybrid Unity architecture. Apply these rules to all code changes here.

- Keep `MonoBehaviour` classes thin. They should own Unity lifecycle, scene references, inspector wiring, and input bridging.
- Put gameplay logic in services or domain classes, not inside Unity callbacks unless the logic is inherently engine-facing.
- Prefer plain C# services for logic that can run without scene state.
- Systems communicate through services or stable interfaces.
- UI must not modify gameplay state directly.
- Avoid direct cross-system references when an intermediate service or event path is clearer.
- Never rename serialized fields without a migration path.
- Use `[FormerlySerializedAs]` when renaming serialized fields.
- Treat prefab, scene, and ScriptableObject references as part of the change surface.
- Check prefab and scene wiring when changing component fields or required references.
- Maintain public API contracts unless the task explicitly includes a breaking change.
- Call out any required inspector or asset updates in the final response.

## Communication

- Be explicit about assumptions, inspector setup, and required editor actions.
- When proposing architecture, explain why the added structure is worth the complexity.
- When no dedicated skill is needed, stay concise and solve the task directly.

---

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

- localization/ -> move under Core/ or Services/ (depending on responsibility).
- monetisation/ -> move under Services/.
- scenemanagement/ -> move under Core/ or Gameplay/ (based on orchestration role).
- camera/ -> move under Gameplay/ if gameplay-driven.
- settings/ -> move under Core/ or UI/ depending on usage.

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

- Do not access another module's internal implementation.
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

## Documentation

- [Skills catalog](./SKILLS.md)

## Tracking Docs Maintenance

- Keep the tracking docs current whenever feature scope, implementation status, or testability changes.
- Update `docs/feature-registry.md` when a feature is added, approved, started, partially implemented, completed, or re-scoped.
- Update `docs/task-registry.md` when feature work is broken into actionable tasks or when task status changes.
- Update `docs/script-registry.md` when ownership of a major runtime script changes materially.
- Update `docs/implementation-plan.md` when delivery order, validation expectations, or the active slice changes.
- Update `docs/decision-registry.md` when a product or technical decision is made that affects future implementation.
- Update `docs/user-testing/` when a feature becomes newly testable, when the expected player flow changes, or when new blockers/validation steps should be captured.
- Treat `docs/README.md` as the index for the docs set and keep it aligned when new tracking sections or folders are added.
- Do not leave tracking docs stale after meaningful feature work. If code changes but tracking docs do not need a change, say that explicitly in the final response.

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
