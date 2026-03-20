# Decision Registry

## Status Values

- `Proposed`
- `Accepted`
- `Superseded`

## Decisions

### DR-001: Use hybrid modular Unity architecture

Status: Accepted

Context:
The repository already mixes scene-driven MonoBehaviours, service adapters, ScriptableObject config, and local persistence. The project needs a stable architectural posture that reflects repo truth and the repo-level AGENTS guidance.

Decision:
Use `hybrid` architecture. Keep MonoBehaviours focused on lifecycle, inspector wiring, input bridging, and orchestration. Keep gameplay logic and service integrations explicit, modular, and safe for future extraction where practical.

Implications:

- Architecture docs must describe the real module map, not an idealized clean-room folder structure.
- Future refactors should improve boundaries without forcing large disruptive migrations.

### DR-002: Target Android only for the current production scope

Status: Accepted

Context:
The product brief and current service choices are aligned around Android delivery.

Decision:
Treat Android as the only supported platform in the current product scope.

Implications:

- Platform decisions, UX assumptions, and monetisation flows should optimize for Android first.
- Additional platform support requires a later explicit decision.

### DR-003: Keep the current single-scene deployment model for now

Status: Accepted

Context:
The current build settings and runtime flow operate from one active gameplay scene containing gameplay systems, hub UI, and overlays.

Decision:
Document and preserve the single-scene current model until a later approved architectural change replaces it.

Implications:

- Scene references and inspector wiring remain part of the core runtime contract.
- Future additive-scene or bootstrap-scene work must be introduced deliberately, not implicitly.

### DR-004: Use UGUI and TextMeshPro as the current UI baseline

Status: Accepted

Context:
The current project uses UGUI and TMP throughout the runtime and menu stack.

Decision:
Treat UGUI plus TextMeshPro as the production UI baseline for current work.

Implications:

- New UI work should integrate with the current stack unless another decision supersedes it.
- Documentation should not describe non-installed UI frameworks as present dependencies.

### DR-005: Document the UI stack as current plus target

Status: Accepted

Context:
The project has an established current UI stack, but the desired visual bar likely benefits from future animation and layout tooling.

Decision:
Document both the current installed UI stack and the intended target additions, while clearly separating approved future tooling from present dependencies.

Implications:

- DOTween and other potential tools can be referenced in docs without being treated as already adopted.
- Future package adoption requires its own implementation slice and validation.

### DR-006: Use rewarded ads plus premium/remove-ads monetisation as the current monetisation spine

Status: Accepted

Context:
The repo already contains rewarded continue flow, reward doubling hooks, side prompt hooks, premium currency, and a remove-ads purchase path.

Decision:
Treat rewarded ads and premium/remove-ads monetisation as the current monetisation backbone, with broader bundles and currency packs documented as future catalog expansion.

Implications:

- Monetisation docs should distinguish current implemented flows from future store expansion.
- Analytics and UX decisions should continue to track monetisation source and player-facing clarity.

### DR-007: Use a shared analytics event-name contract with stable parameter keys

Status: Accepted

Context:
The project now spans run, hub, shop, hangar, progression, and ad flows that all need comparable telemetry without a second reporting schema.

Decision:
Keep analytics event names and common payload keys centralized in `AnalyticsEventNames`. Prefer player-facing action or outcome events, and reuse shared parameter keys across callers instead of inventing per-script payload shapes.

Implications:

- Future telemetry work should extend the shared contract first.
- Controllers should log through the existing analytics service boundary rather than building local event vocabularies.

## Template

```text
### DR-00X: Title

Status: Proposed

Context:
Why this is needed now.

Decision:
What is being chosen.

Implications:
- impact 1
- impact 2
```
