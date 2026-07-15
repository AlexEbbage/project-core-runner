# Level Select User Test

## Goal

Verify the player can read available levels/zones, understand locks, persist the selected route, and enter the intended run from selection.

## Setup

- Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
- Open the level select surface from the menu.

## Verification Result

- EditMode: 32/32 passed on 2026-07-15.
- PlayMode: 4/4 passed, including route persistence and selected-run configuration.
- Unity MCP live result: all five configured routes were available; selecting `deca_sector_05` persisted index 4 and started a ten-sided tunnel run.
- Closeout status: implementation and runtime behavior are proven; human portrait-device clarity remains the signoff gate.

## Checklist

- [x] Confirm the level select surface can be opened.
- [x] Confirm all five polygon routes exist in authored order: `HEXAGON`, `HEPTAGON`, `OCTAGON`, `NONAGON`, `DECAGON`.
- [ ] Confirm available levels or zones are readable and selectable.
- [ ] Confirm locked levels clearly show the required level or lock reason.
- [x] Change selection and confirm the selected route updates correctly.
- [x] Confirm each route config carries three display-only challenge goals with no claim buttons or fake completion state.
- [x] Confirm selecting a level persists after leaving and returning to the menu.
- [x] Start a run from the selected level and confirm the world matches the selection.
- [x] Confirm the selected route changes the tunnel side count correctly when the run starts.
- [ ] If lock states exist, confirm locked content is clearly marked.
- [ ] If reward or challenge info exists per level, confirm it is understandable.
- [x] Return to the menu and confirm the last selected state behaves as intended.

## Expected Result

Level selection should make it obvious what can be played now, what is locked, and what the player is choosing.

## Test Notes

- Levels tested:
- Pass/Fail:
- Notes:
