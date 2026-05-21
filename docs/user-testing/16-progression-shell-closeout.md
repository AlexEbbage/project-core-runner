# Progression Shell Review Closeout User Test

## Goal

Verify the authored hub and progression surfaces are ready for review closeout without breaking navigation, readability, or input flow.

## Setup

- Open `CoreRacer_Main`.
- Start from the main menu hub and move through the visible progression surfaces.

## Verification Result

- Static validation: `dotnet build Assembly-CSharp.csproj -nologo` passed.
- Static closeout fix: daily login, tasks, and achievements configs now resolve through `Resources` instead of relying only on editor-time asset discovery.
- Runtime Unity scene verification: not performed in this environment, so the checklist remains the live signoff gate.
- Closeout status: the clean `CoreRacer_Main` scene now has the authored hub/progression shell, runtime config loading is hardened, and the remaining work is the player-facing review pass for layout and content polish.

## Checklist

- [ ] Confirm hub page switches land on the correct authored page and do not leave overlapping pages visible after the transition.
- [ ] Confirm `Shop`, `Ship`, `Lab`, `Level Select`, and `Achievements` can each be opened cleanly from the hub.
- [ ] Confirm top-bar profile, currency, and XP feedback refresh after progression actions without noisy motion.
- [ ] Open and close the shop item details modal, lab panel, and any other progression overlay to confirm input remains usable after close.
- [ ] Confirm daily login, tasks, achievements, and level-select flows remain readable and do not break the hub shell.
- [ ] Confirm portrait layout remains readable and CTA access stays clear.
- [ ] Confirm missing optional references fail safely instead of breaking the UI.

## Expected Result

The progression shell should feel coherent and stable. Navigation, overlays, and feedback should reinforce the current state without obscuring the underlying hub flow.

## Test Notes

- Pass/Fail:
- Notes:
