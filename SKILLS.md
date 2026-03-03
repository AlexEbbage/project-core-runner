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

## Skill catalog (grouped by module)

### Gameplay

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Gameplay Feature Change | Adding or modifying runtime gameplay behavior (player, obstacles, pickups, gameflow, gameplay VFX). | Target gameplay module, user story/acceptance criteria, impacted scenes/prefabs. | Updated gameplay scripts/assets in `Assets/Scripts/Gameplay/` (+ notes on validations). | Gameplay Team |

**Do**
- Keep runtime gameplay orchestration inside Gameplay modules.
- Communicate with UI/Services/Core via events, interfaces, or explicit public APIs.
- Preserve prefab/scene bindings and null-guard serialized fields.

**Don't**
- Access UI/Services internal implementations directly.
- Introduce circular dependencies with sibling modules.
- Move gameplay logic into UI components.

### UI

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| UI Tweak / Screen Update | Updating HUD, menus, popups, or other presentation behavior. | Target UI screen/component, UX requirement, relevant gameplay/service events. | Updated UI scripts/assets in `Assets/Scripts/UI/` with reactive event wiring. | UI Team |

**Do**
- Keep UI logic in `Assets/Scripts/UI/` and make it reactive to state/events.
- Consume gameplay/services through interfaces, events, or explicit APIs.
- Validate bindings so screens update without breaking inspector links.

**Don't**
- Put gameplay rules or gameflow decisions in UI code.
- Reach into Gameplay module internals.
- Couple UI directly to service implementation details.

### Services

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Service Integration | Adding/changing external-facing integrations (ads, analytics, audio, notifications, monetisation). | Service API contract, environment/config keys, feature flag or call sites. | Updated service adapters/APIs in `Assets/Scripts/Services/` and integration notes. | Services Team |

**Do**
- Expose clear public APIs for service consumers.
- Keep integrations isolated to Services with explicit contracts.
- Use interface-driven access when reasonable.

**Don't**
- Create hidden global state unless justified and documented.
- Create tight service-to-service coupling.
- Access Gameplay/UI internals from services.

### Core

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Cross-cutting Core Utility | Introducing or adjusting shared minimal utilities (localization, scene management, config, helpers). | Shared use case, consumer modules, non-goal boundaries. | Focused utility updates in `Assets/Scripts/Core/` with dependency rationale. | Core Maintainers |

**Do**
- Keep Core utilities minimal, shared, and module-agnostic.
- Prefer narrow APIs consumed through explicit interfaces/events.
- Confirm at least one real cross-module consumer.

**Don't**
- Turn Core into a dumping ground for unrelated logic.
- Add gameplay-specific logic to Core.
- Bypass module boundaries by exposing internals.

### Cross-cutting (refactor/migration)

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Refactor / Migration | Touching legacy module paths that should move toward `Gameplay/`, `UI/`, `Services/`, or `Core/`. | Legacy path being changed, target module destination, namespace/reference impact. | Scoped relocation + reference updates + migration summary. | Architecture Owners |

**Migration note for legacy folders**
- Legacy folders may include: `camera/`, `config/`, `debug/`, `feedback/`, `fx/`, `gameplay/`, `localization/`, `meta/`, `monetisation/`, `scenemanagement/`, `services/`, `settings/`, `ui/`.
- When touching one of these modules, relocate **that touched module only** in the same change when safe.
- Target destinations: gameplay-driven systems → `Gameplay/`; presentation → `UI/`; platform/external integrations → `Services/`; minimal shared helpers/orchestration → `Core/`.
- Keep migration scoped: move files, update namespaces/references, preserve scene/prefab integrity, and verify no circular dependencies.
- Document what moved, why it moved, and any required follow-up in the PR notes.

## Quick start

1. Pick the relevant module group (Gameplay, UI, Services, Core, Cross-cutting).
2. Follow the group's **Do/Don't** boundaries before implementation.
3. Use events/interfaces/public APIs for all cross-module communication.
4. If touching a legacy folder, apply the scoped migration note in the same change when safe.
5. Validate only impacted modules and confirm no boundary/circular-dependency violations.
