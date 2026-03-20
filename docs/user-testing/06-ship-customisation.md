# Ship Customisation User Test

## Goal

Verify the player can preview, equip, and persist ship visual customization.

## Setup

- Open the `Ship` or hangar/customisation surface from the main menu.
- Ensure the starter shop slice has granted or sold at least one alternate item to the profile.
- If needed, buy one cosmetic first from the shop and return to customisation without restarting the scene.
- Verify the customisation view shows the runtime tab row for `Ships`, `Skins`, `Trails`, `Core FX`, and `Upgrades`.

## Checklist

- [ ] Confirm the customization surface opens.
- [ ] Confirm the current-loadout preview card is visible and reflects the currently equipped ship/cosmetics.
- [ ] Confirm the current ship preview is visible.
- [ ] Switch between `Ships`, `Skins`, `Trails`, `Core FX`, and `Upgrades` and confirm the selected tab is obvious.
- [ ] Select a different cosmetic option and confirm the preview updates immediately.
- [ ] Select a different unlocked ship and confirm the stat rows and loadout preview update immediately.
- [ ] Confirm the equipped state is obvious.
- [ ] Close and reopen the screen and confirm the selected cosmetic persists.
- [ ] Start a run and confirm the equipped cosmetic appears in gameplay.
- [ ] Confirm unavailable items remain clearly locked.
- [ ] Confirm newly purchased items from the shop appear here without requiring a scene reload.

## Expected Result

The player should be able to tell what is equipped, see the result immediately in preview and in-run, and move naturally from shop unlock to ship equip flow.

## Test Notes

- Cosmetic types tested:
- Pass/Fail:
- Notes:
