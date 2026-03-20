# Boosters User Test

## Goal

Verify pre-run boosters can be selected, affect the run, and clear correctly afterward once the feature exists.

## Setup

- Open `GameScene`.
- Use the `Level Select` hub page and scroll to the booster loadout section.

## Verification Result

- Static validation: `dotnet build Assembly-CSharp.csproj -nologo` passed.
- Runtime Unity scene verification: not performed in this environment, so the checklist remains the live signoff gate.
- Closeout status: booster data, persistence, and run-start hooks are wired in code; the remaining work is the editor review pass against `GameScene`.

## Checklist

- [ ] Confirm the booster selection UI can be opened from the pre-run hub flow.
- [ ] Confirm the booster families are grouped or labeled clearly.
- [ ] Equip one booster in each family and confirm the selection persists after leaving and returning to the hub.
- [ ] Start a run and confirm the selected boosters apply once at run start.
- [ ] Confirm score, reward, and speed modifiers reflect the equipped boosters.
- [ ] End the run and confirm the booster effect does not remain active unexpectedly.
- [ ] Reopen the booster flow and confirm unlocked options and equipped state are still correct.
- [ ] Confirm the booster UI does not break hub navigation or portrait readability.
- [ ] Confirm missing booster references fail safely instead of breaking the UI.

## Expected Result

Boosters should feel like a clear pre-run choice with visible payoff and clean per-run scoping.

## Test Notes

- Blocker if not testable:
- Pass/Fail:
- Notes:
