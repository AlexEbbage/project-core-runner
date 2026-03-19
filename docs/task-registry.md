# Task Registry

## Working Rule

Break approved features into tasks labeled `F#-T#`. Update status as the work changes. This file is the immediate backlog, not a speculative dump.

## Task Tracking Table

| Task | Feature | Description | Owner | Status | Notes |
| --- | --- | --- | --- | --- | --- |
| F11-T1 | F11 | Consolidate current progression task UI, config, reward states, and task terminology into a production-ready feature spec | Codex/User | Not started | Align daily/weekly/monthly tasks with upgrade-point economy |
| F11-T2 | F11 | Define reward-granting rules, persistence ownership, and acceptance criteria for task completion and claiming | Codex/User | Not started | Needs product decisions and repo implementation slice |
| F12-T1 | F12 | Define level/environment unlock model, success thresholds, and how selection UI maps to unlocked content | Codex/User | Not started | Current level picker is present but not fully productized |
| F12-T2 | F12 | Specify environment data, unlock persistence, and scene/runtime integration for future environments | Codex/User | Not started | Should preserve single-scene current model unless explicitly changed |
| F13-T1 | F13 | Finalize rewarded ad placement policy across continue flow, reward doubling, and side prompts | Codex/User | Completed | Mid-run rewarded offers are now wired into the run loop with interval timing and weighted rewards |
| F13-T2 | F13 | Validate weighted offer rewards, interval pacing, and pause/resume behavior against gameplay states | Codex/User | Implemented (awaiting review) | Covers popout timing, modal flow, ad outcomes, and reward grant correctness |
| F6-T1 | F6 | Centralize game-over presentation data and bind the existing game-over screen to resolved run stats and rewards | Codex/User | In progress | Covers score, distance, coins, combo modifier, and base reward presentation |
| F4-T1 | F4 | Gate continue behind a short game-over timer and keep revive flow separate from double rewards | Codex/User | In progress | Continue should unlock after the timer, remain ad-driven, and resume with no temporary powerups restored |
| F13-T3 | F13 | Make the game-over `x2 Rewards` path one-shot and prevent duplicate bonus grants across ad callbacks | Codex/User | In progress | Covers bonus-delta grant, button retirement, and coexistence with continue |
| F14-T1 | F14 | Define first production monetisation catalog beyond remove-ads, including currency packs and premium bundles | Codex/User | Not started | Needs explicit product approval before implementation |
| F15-T1 | F15 | Expand analytics event contract for run outcomes, ad sources, progression, economy, and shop actions | Codex/User | Not started | Start from current analytics service and event names |
| F16-T1 | F16 | Decide whether to add DOTween and any additional UI tooling, plus integration boundaries | Codex/User | Not started | Docs should drive package adoption, not the reverse |
| T1 | Docs Refresh | Complete docs scaffold population and set docs as authoritative project context | Codex | Completed | Done in this pass |
| T2 | Docs Refresh | Validate docs against repo truth and separate implemented vs partial vs planned status correctly | Codex | Completed | Done in this pass |
| T3 | Meta Roadmap | Converge progression, economy, and reward systems into a first approved implementation slice | Codex/User | Not started | Recommended next production feature area |
| T4 | Level Roadmap | Complete environment unlock and level-select product definition | Codex/User | Not started | Recommended after progression alignment |
| T5 | UI Roadmap | Lock the UI tooling/polish direction and transition strategy | Codex/User | Not started | Recommended before major UI polish work |

## Notes

- Task status values: `Not started`, `In progress`, `Blocked`, `Implemented (awaiting review)`, `Completed`
- Use `F#-T#` when a task belongs to a specific tracked feature.
- Keep future work grouped under the feature that owns it whenever possible.
