# Task Registry

## Working Rule

Break approved features into tasks labeled `F#-T#`. Update status as the work changes. This file is the immediate backlog, not a speculative dump.

## Task Tracking Table

| Task | Feature | Description | Owner | Status | Notes |
| --- | --- | --- | --- | --- | --- |
| F10-T1 | F10 | Author a checked-in daily login reward config and move claim cadence persistence into `PlayerProfile` | Codex/User | Implemented (awaiting review) | Covers streak day progression, single-claim enforcement, and reward preview state |
| F10-T2 | F10 | Route daily login access through the hub/tasks shell without auto-claiming on open | Codex/User | Implemented (awaiting review) | Keeps the first slice manual and preview-driven while leaving optional ad-doubling for later |
| F11-T1 | F11 | Productize the current progression task UI/config into a claimable daily/weekly/monthly task flow | Codex/User | Implemented (awaiting review) | Covers action-button states, persisted claim state, and cadence-level refresh |
| F11-T2 | F11 | Move task and milestone reward ownership into `PlayerProfile` and grant profile rewards through a shared path | Codex/User | Implemented (awaiting review) | Uses profile-backed cadence state instead of transient UI-only state |
| F11-T3 | F11 | Decide whether future task cadence selection should stay authored-per-group or expand into randomized rotation | Codex/User | In progress | Current milestone keeps authored task groups and defers broader reroll breadth |
| F13-T1 | F13 | Finalize rewarded ad placement policy across continue flow, reward doubling, and side prompts | Codex/User | Completed | Mid-run rewarded offers are now wired into the run loop with interval timing and weighted rewards |
| F13-T2 | F13 | Validate weighted offer rewards, interval pacing, and pause/resume behavior against gameplay states | Codex/User | Implemented (awaiting review) | Covers popout timing, modal flow, ad outcomes, and reward grant correctness |
| F6-T1 | F6 | Centralize game-over presentation data and bind the existing game-over screen to resolved run stats and rewards | Codex/User | Implemented (awaiting review) | Covers score, distance, coins, combo modifier, and base reward presentation |
| F4-T1 | F4 | Gate continue behind a short game-over timer and keep revive flow separate from double rewards | Codex/User | Implemented (awaiting review) | Continue should unlock after the timer, remain ad-driven, and resume with no temporary powerups restored |
| F13-T3 | F13 | Make the game-over `x2 Rewards` path one-shot and prevent duplicate bonus grants across ad callbacks | Codex/User | Implemented (awaiting review) | Covers bonus-delta grant, button retirement, and coexistence with continue |
| F7-T1 | F7 | Remap the existing main-menu pages into the target hub destinations and make `Level Select` the default landing page | Codex/User | Implemented (awaiting review) | Covers bottom-nav routing for shop, ship, lab, levels, and achievements |
| F7-T2 | F7 | Add side-entry hub actions and badge visibility for daily login, special offers, tasks, and notifications | Codex/User | Implemented (awaiting review) | Keep badge state lightweight and sourced from existing managers/controllers |
| F8-T1 | F8 | Keep the hub top bar in sync with profile level, XP, and currency state | Codex/User | Implemented (awaiting review) | Currency taps should continue routing into the shop currency surface |
| F8-T2 | F8 | Roll run XP into level progression, unlock feedback, and persisted level-select state | Codex/User | Implemented (awaiting review) | XP now advances levels, queues a level-up toast, and persists the selected level route |
| F9-T1 | F9 | Author a checked-in shop catalog and align it with ship, skin, and trail unlock ids already used by profile and hangar systems | Codex/User | Implemented (awaiting review) | First slice covers one meaningful item path per visible shop tab plus default owned content |
| F9-T2 | F9 | Harden shop browse and modal purchase flow so owned, purchasable, insufficient-funds, and premium-action states are distinct | Codex/User | Implemented (awaiting review) | Covers card labels, modal state messaging, purchase persistence, and hangar refresh after unlocks |
| F14-T1 | F14 | Keep the currency tab scoped to the existing `Remove Ads` entitlement and restore flow without adding new live IAP products | Codex/User | Implemented (awaiting review) | The first production shop slice intentionally avoids a second premium-commerce architecture |
| F17-T1 | F17 | Reframe the current hangar shell into a clearer ship customisation surface with preview-first equip flow | Codex/User | Implemented (awaiting review) | The hangar now exposes a customisation-first tab row and current-loadout preview while keeping upgrades as a secondary surface |
| F17-T2 | F17 | Make equip state, lock state, and preview refresh obvious across the visible customisation categories | Codex/User | Implemented (awaiting review) | Covers ships, skins, trails, and core FX plus shop-to-hangar refresh and gameplay cosmetic handoff |
| F12-T1 | F12 | Turn the play page into a persisted level-select surface with readable lock states and selection cards | Codex/User | Implemented (awaiting review) | Reuses the existing play page, but now surfaces unlock-aware cards and selection persistence |
| F12-T2 | F12 | Use profile level thresholds to gate selectable levels and keep the selected run target in sync with the hub | Codex/User | Implemented (awaiting review) | Locked cards show the required level and the chosen route survives returning to the hub |
| F18-T1 | F18 | Keep `Lab` as the dedicated hub destination for combo and powerup upgrades while separating it from ship loadout browsing | Codex/User | In progress | Current milestone preserves the hub route and refresh hooks but still needs a fully authored lab presentation pass |
| F18-T2 | F18 | Finalize upgrade card clarity, authored tuning data, and runtime validation for combo plus the target powerup set | Codex/User | In progress | Purchase refresh is wired, but content tuning and scene-authored validation still remain |
| F19-T1 | F19 | Convert the current challenges destination into a first achievement shell with tier progress and claim states | Codex/User | Implemented (awaiting review) | Uses `AchievementsPageController` plus authored `AchievementsConfig.asset` content |
| F19-T2 | F19 | Persist achievement tier claims and reward grants through `PlayerProfile` without adding a second save owner | Codex/User | Implemented (awaiting review) | Achievement rewards now use the same profile grant path as tasks and daily login |
| F15-T1 | F15 | Expand analytics event contract for run outcomes, ad sources, progression, economy, and shop actions | Codex/User | Not started | Start from current analytics service and event names |
| F16-T1 | F16 | Decide whether to add DOTween and any additional UI tooling, plus integration boundaries | Codex/User | Not started | Docs should drive package adoption, not the reverse |
| T1 | Docs Refresh | Complete docs scaffold population and set docs as authoritative project context | Codex | Completed | Done in this pass |
| T2 | Docs Refresh | Validate docs against repo truth and separate implemented vs partial vs planned status correctly | Codex | Completed | Done in this pass |
| T3 | Meta Roadmap | Converge progression, economy, and reward systems into a first approved implementation slice | Codex/User | In progress | Current bundle spans XP/levels and level select; the broader progression shell work remains tracked separately |
| T4 | Level Roadmap | Complete environment unlock and level-select product definition | Codex/User | Not started | Recommended after progression alignment |
| T5 | UI Roadmap | Lock the UI tooling/polish direction and transition strategy | Codex/User | Not started | Recommended before major UI polish work |

## Notes

- Task status values: `Not started`, `In progress`, `Blocked`, `Implemented (awaiting review)`, `Completed`
- Use `F#-T#` when a task belongs to a specific tracked feature.
- Keep future work grouped under the feature that owns it whenever possible.
