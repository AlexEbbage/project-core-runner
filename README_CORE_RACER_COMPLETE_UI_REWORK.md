# Core Racer Complete Modular UI Toolkit Rework

This changed-files-only patch replaces the existing monolithic UI Toolkit implementation with Core Racer's final modular runtime UI architecture.

## Included

- Unity UI Toolkit as the primary runtime UI
- UXML structure with stable named contracts
- shared USS tokens, typography, components, utilities and responsive layout
- screen-specific Views and Presenters
- central lifecycle-aware screen routing
- persistent HUD, screen, overlay, popup, effects, toast and loading layers
- semantic LitMotion animation service with interruption cleanup
- source-driven navigation and button subscriptions
- event-driven gameplay HUD
- pause, tutorial, continue, Game Over, modal and toast presentation
- reusable dynamic elements
- development component gallery
- architecture tests, documentation and ADR updates

## Visual direction

- spacious light-style composition rendered with a dark palette
- polished casual mobile-game presentation
- portrait-first responsive layout
- deep navy/blue-black backgrounds
- off-white text and restrained grey-blue support text
- orange/red primary actions, cyan/blue progression accents
- gold credits, blue shards and purple rare rewards
- minimal nested cards, frames and glow
- open gameplay HUD so the low-poly tunnel remains dominant

## Installation

1. Commit or back up the Unity project.
2. Close Unity.
3. Extract this zip into the **Unity project root**, preserving paths and overwriting matches.
4. Reopen Unity and allow package import and compilation to finish.
5. Open the intended main scene, normally:
   `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`
6. Run:
   `Tools > Core Racer > UI Toolkit > Install Final UI`
7. Run:
   `Tools > Core Racer > UI Toolkit > Validate Final UI`
8. Run all EditMode and PlayMode tests.
9. Enter Play Mode and verify the complete menu/run loop.

## Important scene warning

The installer validates the new UXML contract, creates or reuses `GameUiRoot`, wires existing services/content, assigns the run presenter, removes superseded `Canvas` GameObjects from the **currently open scene**, and saves it. Open and back up the correct scene before running it.

No project files require manual deletion.

## Verification checklist

### Shared shell
- Profile, level, XP and currencies refresh.
- The cog opens Settings.
- Bottom navigation routes to Play, Shop, Hangar, Lab and Progress.
- unavailable actions provide visible feedback.

### Play
- only the MVP Core Run is playable;
- the next zone can be previewed but not started;
- high score, stars and reward states refresh;
- booster inventory/equip/buy states display;
- Start hides the menu and begins one run.

### HUD and overlays
- distance, score, credits, health/progress and powerups update from events;
- non-interactive HUD areas do not block gameplay touch input;
- Pause blocks gameplay input and resumes;
- tutorial, continue, Game Over, retry, home and double rewards remain connected;
- repeated/interrupted overlays reset cleanly.

### Shop, Hangar, Lab and Progress
- shop tabs/catalog/modal/purchase status work;
- hangar selection/equip/stats/upgrades use profile data;
- lab upgrade and experiment states refresh;
- daily rewards, tasks and achievements display and claim.

### Responsive layouts
Check 9:16, 9:19.5, a narrow phone, tablet portrait, safe-area simulation and supported gameplay landscape.

## Static validation completed

- 11 UXML documents parsed
- 163 stable named UXML elements found
- 147 literal required-element contracts checked with zero missing
- 1,009 Unity metadata GUIDs checked with zero duplicates
- zero `m_Script: {fileID: 0}` references under `Assets/CoreRacer`
- C# and USS brace-balance checks passed
- `git diff --check` passed
- no unresolved TODO/FIXME/NotImplemented markers in the new UI implementation

A Unity Editor was not available in the patch environment. Unity compilation, package API compatibility, scene installation, tests and target-device rendering remain to be verified locally.

## On failure

Send the first complete compiler/import/test error, Unity version, output from `Validate Final UI`, and screenshots of the Game view plus UI Toolkit Debugger hierarchy.
