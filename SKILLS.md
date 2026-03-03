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

## Monetization + Analytics Service Change Standard

### Intent
Standardize how contributors modify monetization and analytics services so ad, purchase, and analytics integrations remain stable, testable, and module-boundary compliant.

### Scope boundaries
In scope:
- Changes to rewarded ads, remove-ads purchase handling, and analytics dispatch paths.
- Integration validation and regression checks for those three service areas.
- Service-facing notes about SDK configuration locations.

Out of scope:
- Broad gameplay/UI refactors unrelated to service integration.
- Replacing ad/analytics/IAP providers end-to-end.
- Unrelated migration of legacy folders not touched by the service change.

### Preconditions
- Identify impacted service paths and call sites before editing:
  - `Assets/Scripts/Services/Ads/LevelPlayRewardedAdsService.cs`
  - `Assets/Scripts/Monetisation/RemoveAdsIAPManager.cs`
  - `Assets/Scripts/Services/Analytics/FirebaseAnalyticsService.cs`
- Confirm SDK settings assets are available and mapped to the target environment (for example `Assets/LevelPlay/Resources/LevelPlayMediationSettings.asset`).
- Confirm the consumer path uses only service public APIs/events (no direct access to private fields or SDK wrappers).

### Steps
1. Build an integration map before making changes:
   - Document entry points, initialization order, public API surface, and event/callback outputs for each service file.
   - Identify all UI callers and ensure they consume only public service APIs/events.
2. Implement service changes with lifecycle safety:
   - Validate initialization path for each service.
   - Verify callback registration/unregistration lifecycle symmetry.
   - Add or maintain null/ready-state guards for SDK objects and async responses.
   - Preserve platform guards (`UNITY_ANDROID`) and defined editor fallback behavior.
3. Run module-scoped validation for each service:
   - Rewarded ads: initialization, load/show callbacks, reward callback integrity, and unsubscribe behavior on disable/destroy.
   - Remove-ads IAP: purchase initialization, purchase success/failure callback handling, and restoration/no-ads state propagation.
   - Firebase analytics: service initialization, event dispatch path, and no-op/fallback behavior when SDK is unavailable.
4. Execute regression checklist and document evidence:
   - Rewarded flow should still grant rewards exactly once on successful completion.
   - Remove-ads purchase flow should still disable ads after purchase/restore and persist expected state.
   - Analytics events should still emit expected names/parameters without throwing in editor or unsupported platforms.
5. Record configuration notes and migration impact:
   - Note touched SDK settings locations (for example `Assets/LevelPlay/Resources/LevelPlayMediationSettings.asset`) and any required environment value changes.
   - If changes touched a legacy monetisation path, include scoped migration notes for moving that touched module under `Assets/Scripts/Services/` when safe.

### Validation checklist
- [ ] **Integration map complete:** all touched entry points and callbacks documented for ads, IAP, and analytics services.
- [ ] **Initialization verified:** each service has a deterministic initialization path with safe re-entry behavior.
- [ ] **Lifecycle safe:** callback subscriptions are paired with unsubscriptions in teardown paths.
- [ ] **Ready-state safe:** null checks and SDK-ready guards prevent invalid calls.
- [ ] **Platform behavior verified:** `UNITY_ANDROID` guarded paths and editor behavior are explicitly validated.
- [ ] **UI decoupling maintained:** UI only uses service public APIs/events and does not touch service internals.
- [ ] **Rewarded regression pass:** reward granted once, failure path does not grant reward, and ad availability state recovers.
- [ ] **Remove-ads regression pass:** purchase/restore flow updates ad-disable state and no lingering ad requests remain.
- [ ] **Analytics regression pass:** required events dispatch with expected payload and graceful fallback when provider is unavailable.

### Rollback/safety notes
- Revert touched service scripts and settings assets together if lifecycle or initialization regressions appear to avoid mixed SDK state.
- Keep provider keys and mediation settings in versioned assets; avoid temporary local-only config edits.
- If runtime validation is blocked, ship only with static verification plus a documented follow-up runtime checklist.

### Example invocation
"Use the Monetization + Analytics Service Change Standard skill to update rewarded ad completion handling and ensure remove-ads + analytics flows keep lifecycle-safe callbacks and platform guards."

**Skill owner:** Services Team
**Last-reviewed date:** 2026-03-03
**Review cadence:** Monthly

#### UI decoupling checklist (service boundaries)
- [ ] UI triggers rewarded ads only through `LevelPlayRewardedAdsService` public methods/events.
- [ ] UI reads remove-ads state only through `RemoveAdsIAPManager` public API/event contract.
- [ ] UI emits analytics only through `FirebaseAnalyticsService` (or its interface) public methods.
- [ ] UI does not reference SDK-specific classes, enums, or callback delegates directly.
- [ ] Cross-module communication remains event/API-driven with no reflection/singleton internals access.

#### Regression checklist (focused flows)
- [ ] Rewarded ad flow: load → show → completion callback grants reward once.
- [ ] Rewarded ad cancellation/failure: no reward granted; availability state/event updates correctly.
- [ ] Remove-ads purchase flow: successful purchase disables ad requests and persists state.
- [ ] Remove-ads restore flow: restored entitlement disables ads consistently across app relaunch.
- [ ] Analytics dispatch: key monetization events (reward started/completed/failed, remove-ads purchased/restored) are emitted with expected parameters.
- [ ] Editor behavior: analytics/ads paths fail gracefully or no-op without hard exceptions.

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
