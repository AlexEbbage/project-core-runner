# Hub (Main Menu) User Test

## Goal

Verify the hub lands on `Level Select`, routes clearly to the five target destinations, and exposes side-entry reward/progression actions with sensible badge state.

## Setup

- Open `GameScene`.
- Stay in menu state.

## Checklist

- [ ] Confirm the main menu loads without hidden gameplay panels overlapping it.
- [ ] Confirm the default visible hub page is `Level Select`.
- [ ] Confirm the top bar shows level, XP progress, soft currency, premium currency, and settings access.
- [ ] Confirm the play button from `Level Select` starts a run.
- [ ] Confirm the bottom navigation exposes `Shop`, `Ship`, `Lab`, `Level Select`, and `Achievements`.
- [ ] Confirm the `Lab` destination opens the combo + powerup upgrade surface.
- [ ] Confirm the `Ship` destination routes to the hangar/customisation shell.
- [ ] Confirm the `Achievements` destination routes to the current challenges shell.
- [ ] Confirm currency taps in the top bar still route into the shop currency view.
- [ ] Confirm side entries exist or are wired for `Daily Login`, `Special Offers`, `Tasks`, and `Notifications`.
- [ ] Confirm daily login opens from the hub and clears its badge after claiming if a claim was available.
- [ ] Confirm tasks open from the hub and any claimable-task badge only appears when task reward state is claimable.
- [ ] Confirm special offers route to a monetisation surface and the badge disappears if offers are no longer actionable.
- [ ] Confirm notifications route to the highest-priority available action without breaking hub state.
- [ ] Confirm settings can be opened and closed.
- [ ] Confirm the level selection area updates the preview text correctly, shows lock-aware cards, and keeps the selected route after returning to the menu.
- [ ] Confirm the menu returns cleanly after ending a run and going back.
- [ ] Confirm any notification or badge state that exists is readable and not misleading.

## Expected Result

The player should know where to play next and where to spend or claim progression without guessing. The hub should feel like a single connected shell rather than a set of unrelated old pages.

## Test Notes

- Pass/Fail:
- Notes:
