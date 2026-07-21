# Implementation Plan

## Delivery Strategy

Use the docs set as the durable operating system for the project. Foundation documentation is completed first, then future work proceeds one approved feature at a time. Core gameplay and a validated Android Development APK exist, but the current menu/UI is not acceptance-ready: the layout is poor and most menu actions are reported non-functional. Physical-device release signoff is therefore deferred. The final roadmap phase will completely replace the current UI implementation with UI Toolkit, UXML, USS, source-driven C# presentation, and LitMotion before end-to-end acceptance is repeated.

## Phases

| Phase | Goal | Exit Criteria | Status |
| --- | --- | --- | --- |
| 1 | Foundation | Core docs, architecture, style guide, registries, and decisions reflect repo truth and approved direction | Completed |
| 2 | First approved feature slice | Exactly one chosen feature, or one explicitly approved milestone bundle, is documented, implemented, validated, and reflected in all registries | In progress |
| 3 | Expansion | Additional approved features are implemented in priority order with docs kept current | Not started |
| 4 | Polish and validation | UI polish, tuning, telemetry coverage, testing, and documentation cleanup are complete for the chosen release target | In progress; existing asset wiring pass implemented for the clean scene/configs |

## Implementation Order

1. Read `product-requirements.md`, `architecture.md`, `style-guide.md`, and the registries.
2. Choose one feature from `feature-registry.md`.
3. Move that feature to `In progress` and add or update its tasks in `task-registry.md`.
4. Implement only that feature slice, or the explicitly approved milestone bundle.
5. Update `script-registry.md`, `decision-registry.md`, and validation notes before closing the slice.

## Current Recommended Order

1. Continue only explicitly approved non-UI gameplay, content, service, or progression work; do not spend more time repairing the temporary menu architecture
2. Preserve the current UI only as a behavioural/content reference while underlying systems are completed
3. At the end of the roadmap, execute the complete F22 UI rework in one migration: UI Toolkit/UXML/USS/C# presentation, LitMotion, central routing/layers, reusable components, modal input blocking, component gallery, tests, documentation, and removal of superseded uGUI/DOTween UI
4. After the replacement UI is complete, repeat full Android device acceptance for every important screen/button plus the complete Play/Continue/Retry/Home lifecycle
5. Keep production ad SDK verification separate from editor/dev fallback validation

## Current Asset Wiring Status

- **Acceptance warning:** the current uGUI menu hierarchy is temporary and reported broken. Existing scene/UI validation proves references and some isolated routes, not a usable or visually acceptable menu. Do not describe it as finished or acceptance-ready.
- `CoreRacer_Main.unity` now uses generated wrappers around safe existing player, obstacle, pickup, tunnel, audio, and VFX assets.
- `CoreRacer_Main.unity` now also contains an authored clean hub flow for `Play`, `Shop`, `Hangar`, `Lab`, `Progression`, and `Settings`, with nested level select, daily login, rotating tasks, achievements, comfort, privacy, and support/debug surfaces.
- `CoreRacer_Main.unity` now has the Phase 6 FTUE tutorial overlay, scene director, deterministic first coin/powerup assistance, support reset action, and save-backed tutorial progress wiring. The gameplay-focused v4 sequence waits for a real crash and successful Continue before completion.
- Default touch steering now reacts immediately to either screen half with a centre dead zone; the optional Drag Controls setting retains continuous analog steering. A sustained traversal smoke test proves bounded obstacle/pickup pools and Continue recovery grace.
- `Tools > Core Racer > Build > Android Development APK` creates a profiler-connectable debug-signed APK without exposing release signing credentials. It forces APK output only for the build and restores the custom-keystore and App Bundle preferences afterward.
- `CoreRacer_Main.unity` now has Phase 7 safe SDK adapter wiring for verified Unity IAP, Firebase Analytics, and Mobile Notifications APIs; LevelPlay, Crashlytics, and Addressables remain disabled/manual setup blockers.
- `CoreRacer_Main.unity` now routes the visible bottom Play button directly through validated level selection into run startup. The development editor command is `Tools > Core Racer > Playability > Start Core Run`.
- `CoreRacer_Main.unity` now owns a `RuntimeTunnel` generator configured by the selected route. The gameplay camera follows the player's orbital position and roll so the craft stays upright in frame while the tunnel appears to rotate.
- `CoreRacer_Main.unity` retains five persisted route cards over one stable six-sided MVP tunnel type, but all routes intentionally use one neutral two-tone tunnel presentation for MVP; one booster per family applies only for the active run.
- In Editor and Development Builds, `GameBootstrapper` adds a `DummyRewardedAdService` only when no rewarded provider is assigned, allowing Continue and Double Rewards to be proven without changing production provider wiring.
- Core runs now start on the bottom rail at 270 degrees. The camera-local Y offset is `1.25`, placing the player at portrait viewport Y `0.411`; steering is initially tuned to `140` degrees/second.
- `RuntimeTunnel` uses two renderer-local white/grey shades on alternating longitudinal sections so the shared `WallMaterial` remains unchanged, and player trails are cleared after the run-start teleport.
- Obstacle generation now uses clean copies of the authored Blender wedge, fan, and door prefabs. Pattern groups snap to the six tunnel sides and unlock by elapsed difficulty plus the selected route multiplier.
- Coin pickups now use centre pivots rotated to six wall-centre angles with an offset `PickupBody` child holding both mesh and trigger, keeping lane placement readable in the hierarchy.
- Phase 8 final validation and handoff is captured in `docs/rewrite/final-handoff.md`; closed testing remains blocked by placeholder privacy links. Live inspection on 2026-07-15 confirmed `Assets/CoreRacer/Scenes/CoreRacer_Main.unity` is the only enabled Build Settings scene.
- Manual art follow-up is tracked in `docs/rewrite/manual-art-wiring-needed.md`.

## Dependencies and Blockers

- End-to-end MVP and device acceptance are blocked by the current broken menu implementation. The approved resolution is the deferred full F22 UI rework, not incremental repair or another parallel UI layer.
- Monetisation expansion depends on an approved catalog and entitlement model.
- UI tooling changes depend on an explicit package adoption decision.
- The progression bundle now depends on a player-facing review pass in `CoreRacer_Main` to confirm navigation clarity, layout polish, content readability, and FTUE pacing across the authored hub shell.
- Level Select and boosters are runtime-proven but still require a human portrait-device clarity review. Shop, ship customisation, lab, tasks, daily login, achievements, and UI polish retain their broader UX review gates.
- DOTween remains part of the temporary uGUI implementation only. The final F22 UI architecture standardizes meaningful UI motion on LitMotion and removes superseded DOTween UI wiring after migration.
- SDK release blockers remain for LevelPlay C# API installation/verification, Firebase Crashlytics installation, Addressables installation if remote content is required, and replacement of placeholder privacy links.
- Build settings currently target the clean `CoreRacer_Main.unity`; keep this checked during release preparation rather than relying on the stale legacy-scene assumption.
- The runtime tunnel now generates 48 longitudinal mesh sections for the fixed six-sided MVP type and recenters in 40-unit steps while keeping 20 units behind the player. The preserved FBX tunnel is hidden only during Play Mode.
- The Game Over Retry, Double Rewards, and Menu actions now use distinct authored RectTransform positions. Live pointer execution and PlayMode coverage confirm Retry, Menu, and subsequent Play transitions without regenerating the UI.

## Validation Notes

- Cheapest test layer to use:
  - Static repo checks and compile-likely inspection for documentation-only changes
  - Edit Mode or plain C# tests first for logic-heavy future feature slices
  - Play Mode/manual scene verification when inspector wiring or runtime scene behavior is involved
- Manual checks:
  - Confirm documented dependencies and scene flow against current repo state
  - Confirm implemented vs partial vs planned status labels remain accurate
- Asset or inspector dependencies:
  - Treat scene references, prefabs, ScriptableObjects, and UI wiring as part of the feature change surface for all future implementation work
  - Core-run PlayMode smoke test: `CoreRunPlayModeSmokeTests.VisiblePlay_StartsCoreGameplay`
- Latest automated result: 47/47 EditMode tests and 13/13 PlayMode tests passed after the Android build on 2026-07-21. `CoreRacer-1.1.2-dev.apk` is a valid 128.2 MB ARMv7/ARM64 APK targeting API 36, signed with the Android debug certificate; SHA-256 is `B396DD79DFCB38FD917AFD5A37BB939CB7D17B33B46390A337AD3261860510D3`. Device profiling remains pending because no Android device was connected.
