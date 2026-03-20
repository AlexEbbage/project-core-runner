# Level Select User Test

## Goal

Verify the player can read available levels/zones, understand locks, persist the selected route, and enter the intended run from selection.

## Setup

- Open the level select surface from the menu.
- If only the basic level picker is present, test that current shell and record missing unlock logic separately.

## Checklist

- [ ] Confirm the level select surface can be opened.
- [ ] Confirm available levels or zones are readable and selectable.
- [ ] Confirm locked levels clearly show the required level or lock reason.
- [ ] Change selection and confirm the preview updates correctly.
- [ ] Confirm selecting a level persists after leaving and returning to the menu.
- [ ] Start a run from the selected level and confirm the world matches the selection.
- [ ] If lock states exist, confirm locked content is clearly marked.
- [ ] If reward or challenge info exists per level, confirm it is understandable.
- [ ] Return to the menu and confirm the last selected state behaves as intended.

## Expected Result

Level selection should make it obvious what can be played now, what is locked, and what the player is choosing.

## Test Notes

- Levels tested:
- Pass/Fail:
- Notes:
