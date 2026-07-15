# Boosters User Test

## Goal

Verify pre-run boosters can be selected, affect the run, and clear correctly afterward.

## Setup

- Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Use the `Level Select` hub page and scroll to the booster loadout section.

## Verification Result

- EditMode: 32/32 passed on 2026-07-15.
- PlayMode: 4/4 passed, including persisted loadout, effect application, and run-only reset coverage.
- Unity MCP live result: three options rendered; Start Shield, Coin Boost, and Score Boost applied to the Firestorm environment; Home restored score and coin multipliers to x1 with `Time.timeScale` at 1.
- Screenshot: `_PatchReports/Screenshots/CoreRacer_Level_Select_Booster_Run.png`.
- Closeout status: implementation and runtime behavior are proven; human portrait-device clarity remains the signoff gate.

## Checklist

- [x] Confirm the booster selection UI can be opened from the pre-run hub flow.
- [ ] Confirm the booster family labels are clear on a portrait device.
- [x] Equip one booster in each family and confirm the selection persists after leaving and returning to the hub.
- [x] Start a run and confirm the selected boosters apply once at run start.
- [x] Confirm Start Shield, x2 coins, and x2 score reflect the equipped boosters.
- [x] End the run and confirm the booster effect does not remain active unexpectedly.
- [x] Reopen the booster flow and confirm equipped state is still correct.
- [ ] Confirm the booster UI does not break hub navigation or portrait readability.
- [ ] Confirm missing booster references fail safely instead of breaking the UI.

## Expected Result

Boosters should feel like a clear pre-run choice with visible payoff and clean per-run scoping.

## Test Notes

- Blocker if not testable:
- Pass/Fail:
- Notes:
