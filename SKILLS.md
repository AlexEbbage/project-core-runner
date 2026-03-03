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

## Skill maintenance policy

Every skill catalog entry must include and maintain:

- **Skill owner**: named team or person accountable for correctness and updates.
- **Last-reviewed date**: date of the most recent explicit review.
- **Review cadence**: expected review frequency (for example, every sprint or monthly).

### Staleness policy

- A skill is considered **stale** when it is not reviewed within its declared cadence.
- Stale skills must be marked with a deprecation note and replacement guidance (if available).
- Deprecated skills should include:
  - Date deprecated.
  - Reason for deprecation.
  - Replacement skill/reference, or explicit archival status when no replacement exists.
- Remove or archive obsolete skills only after replacement guidance has been documented to avoid workflow gaps.

### Architecture/process change requirement

- Any PR that changes project architecture, module boundaries, or contribution process **must update affected skills in the same PR**.
- Reviews should block architecture/process PRs when related skill entries are missing required updates.

## Skill catalog (grouped by module)

### Gameplay

| Name | Trigger (when to use) | Inputs required | Output artifact | Owner |
|---|---|---|---|---|
| Gameplay Feature Change | Adding or modifying runtime gameplay behavior (player, obstacles, pickups, gameflow, gameplay VFX). | Target gameplay module, user story/acceptance criteria, impacted scenes/prefabs. | Updated gameplay scripts/assets in `Assets/Scripts/Gameplay/` (+ notes on validations). | Gameplay Team |

**Required checks**
- Lightweight static checks (required):
  - Compile-check touched gameplay scripts.
  - Verify serialized field null-guards in changed components.
  - Validate changed references stay within gameplay/public APIs (no UI/Services internals).
- Optional runtime checks (when scenes/prefabs are affected):
  - Play Mode smoke test for touched gameplay loop/path.
  - Quick scene/prefab sanity run to confirm bindings and event hookups.

**PR evidence to include**
- Files touched under `Assets/Scripts/Gameplay/` (and any related assets/prefabs).
- Boundary checks performed (how UI/Services/Core interaction stays via events/interfaces/APIs).
- Migration notes if any touched legacy gameplay path was relocated.

**Failure handling**
- If local Unity/runtime dependencies are unavailable, still complete static checks and document skipped runtime checks with reason.
- Record exact missing dependency/tooling and provide the minimal follow-up command for maintainers to run in a full Unity environment.

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

**Required checks**
- Lightweight static checks (required):
  - Compile-check touched UI scripts.
  - Confirm UI only consumes public gameplay/service APIs or events.
  - Validate serialized references and inspector fields are null-guarded where needed.
- Optional runtime checks (when visual flows are impacted):
  - Play Mode smoke test for the updated screen/HUD flow.
  - Interaction sanity pass for the specific UI path changed.

**PR evidence to include**
- Files touched under `Assets/Scripts/UI/` and related UI assets/prefabs.
- Boundary checks proving no gameplay logic moved into UI and no module-internal reach-through.
- Migration notes if any touched legacy `ui/` path was relocated.

**Failure handling**
- If runtime/Play Mode cannot run in the current environment, include static-check output and explicitly mark runtime checks as pending.
- Document missing dependencies (Unity version/editor package/tool) and handoff steps.

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

**Required checks**
- Lightweight static checks (required):
  - Compile-check touched service scripts.
  - Verify API/interface surface remains explicit and call sites use public contracts.
  - Confirm no tight service-to-service coupling added in changed code.
- Optional runtime checks (when integration endpoints are testable):
  - Run module-scoped integration smoke path with sandbox/test credentials.
  - Validate fallback behavior for unavailable provider responses.

**PR evidence to include**
- Files touched in `Assets/Scripts/Services/` plus relevant config changes.
- Boundary checks (consumer access path, interface usage, coupling review).
- Migration notes if legacy `services/` or `monetisation/` code was relocated.

**Failure handling**
- If credentials, SDKs, or network access are unavailable, run static checks only and document blocked runtime checks.
- Capture what dependency is missing and provide reproducible commands/steps for maintainers.

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

**Required checks**
- Lightweight static checks (required):
  - Compile-check touched core utility scripts.
  - Confirm utility remains module-agnostic and not gameplay-specific.
  - Verify at least one real consumer still resolves through explicit API/event usage.
- Optional runtime checks (when behavior is observable):
  - Module-scoped smoke test in a consumer flow (e.g., localization/scene/config usage path).
  - Quick regression pass for the affected shared utility entry point.

**PR evidence to include**
- Files touched under `Assets/Scripts/Core/` and immediate consumer call sites.
- Boundary checks showing no module-internal leakage and no circular dependency introduction.
- Migration notes if legacy `localization/`, `scenemanagement/`, `config/`, or `settings/` paths were relocated.

**Failure handling**
- If consumer runtime validation cannot execute, provide static-check evidence and list unverified runtime paths.
- Note missing environment pieces and provide follow-up validation steps for maintainers.

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

**Required checks**
- Lightweight static checks (required):
  - Compile-check moved/updated scripts and namespace/reference updates.
  - Verify relocation scope is limited to touched module(s) only.
  - Confirm no circular dependencies or module-boundary violations were introduced.
- Optional runtime checks (when scenes/prefabs are affected):
  - Open/run impacted scene path to verify prefab/script bindings survived the move.
  - Smoke test the migrated feature entry point only.

**PR evidence to include**
- Exact moved/touched paths (from legacy source to new module destination).
- Boundary checks and dependency notes after relocation.
- Migration notes: what moved, why it moved now, and any required follow-up.

**Failure handling**
- If full Unity runtime verification is unavailable, provide static relocation evidence (file moves + reference updates) and list pending runtime checks.
- Document missing tooling/dependencies and assign clear follow-up validation ownership.

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

## Android-only Runtime Guardrails

### Intent
Implement Android-specific runtime behavior safely by isolating platform API access behind compile-time guards, runtime checks, and deterministic fallbacks so Editor and non-Android builds never crash.

### Scope boundaries
In scope:
- Runtime code paths that call Android platform APIs, JNI bridges, or Android SDK adapters.
- Service adapters that use external platform SDKs from `Assets/Scripts/Services/`.
- Feedback adapter behavior in `Assets/Scripts/Feedback/VibrationController.cs`.

Out of scope:
- iOS/native Objective-C integrations.
- Broad architectural migrations unrelated to touched Android-facing modules.
- UI-only behavior that does not invoke platform APIs.

### Preconditions
- Identify each Android-only call site in touched files.
- Confirm a safe no-op or alternate behavior for Editor and non-Android runtime.
- Confirm minimum Android API level assumptions for each feature.
- Have a logging category/tag ready for platform bridge failures.

### Steps
1. Wrap Android API access with preprocessor directives and runtime guards.
   - Compile gate all Android bridge code with `#if UNITY_ANDROID && !UNITY_EDITOR`.
   - Inside the gated block, guard runtime assumptions (`Application.platform == RuntimePlatform.Android`, non-null activity/context, non-empty class/method names).
   - Keep Android-specific code in a small adapter method so consumers call a platform-agnostic public API.
2. Define fallback behavior for Editor and non-Android platforms.
   - Return success/failure values explicitly (avoid implicit exceptions or null propagation).
   - Implement no-op behavior for unsupported platforms when side effects are optional (for example vibration/haptics).
   - Provide deterministic fallback outputs for mandatory flows (for example cached/default value, disabled state, or feature flag off path).
3. Standardize JNI/Java bridge error handling and logging.
   - Catch `AndroidJavaException` at the adapter boundary and prevent propagation into gameplay/UI callers.
   - Log one concise error with action context, class/method, and failure reason.
   - Log only once per action path unless state changes; avoid per-frame spam.
   - Include actionable remediation hints when context/activity/class lookup fails.
4. Apply target-location conventions and examples.
   - For `Assets/Scripts/Feedback/VibrationController.cs`, expose one public vibration API and route Android calls through a private guarded method.
   - For service adapters (ads, analytics, notifications, monetisation, or other SDK wrappers), keep platform calls in adapter internals and expose interface/public API to callers.
   - Do not let Gameplay/UI modules call JNI/`AndroidJavaObject` directly.
5. Run the Android runtime safety checklist before completion.
   - Validate activity/context acquisition path (null-safe and guarded).
   - Validate API-level support checks before method invocation.
   - Validate behavior when app is backgrounded/resumed if the bridge depends on current activity.
   - Validate fallback behavior in Editor and non-Android play paths.

### Validation checklist
- [ ] Every Android API/JNI call is inside `#if UNITY_ANDROID && !UNITY_EDITOR`.
- [ ] Runtime checks prevent using missing activity/context/class/method references.
- [ ] Android bridge failures are caught, logged once per action, and do not crash callers.
- [ ] Editor and non-Android flows are deterministic (no-op or explicit fallback).
- [ ] Minimum API-level checks exist for Android features that are not universally supported.
- [ ] Module boundaries are preserved (Gameplay/UI consume only public APIs from Feedback/Services adapters).

### Rollback/safety notes
- If runtime regressions appear, disable Android call sites behind a feature flag and fall back to no-op while preserving public API contracts.
- Revert only the touched adapter/module path; avoid broad SDK refactors during incident mitigation.
- Keep logs at warning/error level only for failure paths to reduce runtime noise.
- If activity/context instability is observed, short-circuit on null and retry only on next explicit user action.

### Example invocation
"Use the Android-only Runtime Guardrails skill to update `Assets/Scripts/Feedback/VibrationController.cs` so vibration works on Android via JNI, gracefully no-ops in Editor/iOS, and logs actionable errors when activity/context or Java calls fail. Apply the same adapter pattern to touched service SDK wrappers under `Assets/Scripts/Services/`."

### Skill owner
Services Team (with Feedback module maintainers)

### Last-reviewed date
2026-03-03

### Review cadence
Every sprint
