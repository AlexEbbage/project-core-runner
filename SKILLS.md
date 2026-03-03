# SKILLS

This document defines reusable **skills** for working in `project-core-runner`.

## Purpose

Skills are lightweight, repeatable workflows that help contributors make consistent changes without violating module boundaries.

- **For developers**: use skills as implementation playbooks during feature work, bug fixes, and refactors.
- **For agents**: use skills as execution triggers to choose the right workflow, validations, and output format automatically.

## Skill entry template (required)

Every new or updated skill entry **must** use the template below with all headings present.

```md
## <Skill name>

### Intent
<What this skill is designed to achieve.>

### Scope boundaries
<What is in scope and explicitly out of scope.>

### Preconditions
<Required context, inputs, tools, or repository state before starting.>

### Steps
1. <Ordered action>
2. <Ordered action>
3. <Ordered action>

### Validation checklist
- [ ] <Check proving the change works as intended>
- [ ] <Check proving architecture/module boundaries are respected>
- [ ] <Check proving no regressions were introduced>

### Rollback/safety notes
- <How to back out safely if validation fails>
- <Any guardrails, feature flags, or migration cautions>

### Example invocation
<Concrete example showing how someone should trigger/use this skill>
```

### Review gate for skill entries

- Reject any skill PR/review item that omits one or more mandatory headings above.
- Reject entries that leave mandatory sections blank or with placeholder-only content.
- Reject entries without an actionable validation checklist.
- Require rollback/safety notes for any skill that changes assets, scripts, or module structure.

## Definition of done for skills

A skill is production-ready only when all items below are true:

- [ ] Uses the required template with all mandatory headings.
- [ ] Intent and scope boundaries are specific and non-overlapping with existing skills.
- [ ] Preconditions list concrete inputs/dependencies.
- [ ] Steps are deterministic, ordered, and implementable.
- [ ] Validation checklist is measurable and aligned to architectural rules.
- [ ] Rollback/safety notes explain a safe recovery path.
- [ ] Example invocation is realistic and maps to the documented trigger.
- [ ] Ownership is clear in the skill catalog entry.

## Skill catalog

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Gameplay Feature Change | Adding or modifying runtime gameplay behavior (player, obstacles, pickups, gameflow, gameplay VFX). | Target gameplay module, user story/acceptance criteria, impacted scenes/prefabs. | Updated gameplay scripts/assets in `Assets/Scripts/Gameplay/` (+ notes on validations). | Gameplay Team |
| UI Tweak / Screen Update | Updating HUD, menus, popups, or other presentation behavior. | Target UI screen/component, UX requirement, relevant gameplay/service events. | Updated UI scripts/assets in `Assets/Scripts/UI/` with reactive event wiring. | UI Team |
| Service Integration | Adding/changing external-facing integrations (ads, analytics, audio, notifications, monetisation). | Service API contract, environment/config keys, feature flag or call sites. | Updated service adapters/APIs in `Assets/Scripts/Services/` and integration notes. | Services Team |
| Refactor / Migration | Touching legacy module paths that should move toward `Gameplay/`, `UI/`, `Services/`, or `Core/`. | Legacy path being changed, target module destination, namespace/reference impact. | Scoped relocation + reference updates + migration summary. | Architecture Owners |
| Cross-cutting Core Utility | Introducing or adjusting shared minimal utilities (localization, scene management, config, helpers). | Shared use case, consumer modules, non-goal boundaries. | Focused utility updates in `Assets/Scripts/Core/` with dependency rationale. | Core Maintainers |

## Quick start

### 1) Gameplay feature change
1. Confirm the work belongs in `Assets/Scripts/Gameplay/`.
2. Define interfaces/events for cross-module communication.
3. Implement additive changes and preserve prefab/scene bindings.
4. Validate impacted gameplay flow and null-guard serialized fields.

### 2) UI tweak
1. Place UI logic under `Assets/Scripts/UI/`.
2. Keep UI reactive to state/events; avoid embedding gameplay logic.
3. Update bindings and verify no direct gameplay internals are accessed.
4. Validate target screens and event-driven updates.

### 3) Service integration
1. Implement/extend clear public APIs in `Assets/Scripts/Services/`.
2. Avoid tight coupling between services.
3. Wire consumers through interfaces or explicit APIs.
4. Validate service behavior with local configuration and guarded fallbacks.

### 4) Refactor / migration
1. If modifying a legacy module, migrate that module in the same scoped change.
2. Move files to the appropriate target (`Gameplay/`, `UI/`, `Services/`, `Core/`).
3. Update namespaces/references and verify no circular dependencies are introduced.
4. Document migration steps in the PR summary.

### 5) Small shared utility
1. Confirm the change is truly cross-cutting and minimal.
2. Implement under `Assets/Scripts/Core/` only when module ownership is ambiguous.
3. Keep APIs narrow and avoid gameplay-specific assumptions.
4. Validate at least one consuming module path.
