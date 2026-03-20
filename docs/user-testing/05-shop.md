# Shop User Test

## Goal

Verify the first production shop slice works end to end for the authored catalog and the existing `Remove Ads` premium path.

## Setup

- Open the visible `SHOP` or `PREMIUM` entry from the main menu.
- Start from a fresh profile or clear PlayerPrefs if old state hides the default catalog state.
- Give the profile enough currency if needed for the soft-currency items.

## Checklist

- [ ] Confirm the shop opens from the live main-menu entry point in `GameScene`.
- [ ] Confirm `Skins`, `Ships`, `Trails`, and `Currency` all show authored content instead of empty tabs.
- [ ] Select an item and confirm a details modal appears.
- [ ] Confirm title, description, action label, and price/state text are understandable for each item type.
- [ ] Buy a soft-currency item and confirm the purchase persists.
- [ ] Confirm an unaffordable item clearly shows that more currency is needed and does not silently fail.
- [ ] Confirm the `Currency` tab routes `Remove Ads` and `Restore Purchases` through the existing premium flow without creating unlock-item state.
- [ ] Return to menu and reopen the shop to confirm purchased state is retained.
- [ ] Confirm locked, owned, premium-action, and purchasable states are visually distinct.
- [ ] If the purchased item is a skin or trail, open the hangar/customisation surface and confirm the newly owned item is available there.

## Expected Result

Browsing, purchasing, and premium routing should feel clear, and owned cosmetics should persist correctly.

## Test Notes

- Catalog tested:
- Pass/Fail:
- Notes:
