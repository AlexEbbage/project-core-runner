# Final Menu Implementation Checklist

Use this checklist when converting the final menu set into implementation verticals.

## Menu source of truth

- [ ] Bottom nav contains exactly Play, Hangar, Lab, Shop, Progression.
- [ ] Settings is reachable through top-right gear/profile action.
- [ ] Run HUD never shows bottom navigation.
- [ ] Pause, Crash, and Game Over are run states, not hub tabs.
- [ ] Daily, Tasks, Achievements, Milestones, and Level Rewards live inside Progression.
- [ ] Cosmetic selection lives inside Hangar.
- [ ] Upgrade purchasing lives inside Lab.
- [ ] Remove Ads and Restore Purchases live inside Shop.

## Shared shell

- [ ] Main menu screens share a top bar.
- [ ] Main menu screens share bottom nav.
- [ ] Currency/level display updates after claims, rewards, purchases, and upgrades.
- [ ] Safe-area padding is applied.
- [ ] All interactable menu items show pointer/pressed feedback.
- [ ] Disabled buttons explain why they are disabled.
- [ ] Async buttons prevent duplicate taps.

## Play and run entry

- [ ] Play tab has Start Run as the most prominent action.
- [ ] Selected ship is visible before starting.
- [ ] Best score and/or last score is visible.
- [ ] Route selection is hidden unless multiple routes are polished.
- [ ] Starting a run hides bottom nav and enters countdown.

## Hangar

- [ ] Owned craft/cosmetics can be previewed.
- [ ] Owned craft/cosmetics can be equipped.
- [ ] Locked craft/cosmetics show unlock requirement.
- [ ] Cosmetic purchase/equip actions refresh state immediately.
- [ ] Hangar does not contain powerup upgrade purchasing.

## Lab

- [ ] At least one useful coin upgrade exists.
- [ ] Upgrade cards show current level, next effect, and cost.
- [ ] Purchase updates profile/currency immediately.
- [ ] Insufficient currency has a readable state.
- [ ] Max-level upgrades cannot be purchased again.
- [ ] Lab does not equip cosmetics.

## Shop

- [ ] Remove Ads item reflects entitlement state.
- [ ] Restore Purchases action is visible.
- [ ] Offers have success/cancel/fail states.
- [ ] Shop hides unimplemented products.
- [ ] Shop does not contain progression claim lists.

## Progression

- [ ] Daily reward state is visible if implemented.
- [ ] Claimable rewards show badges.
- [ ] Task/milestone progress is readable.
- [ ] Claimed rewards cannot be claimed twice.
- [ ] Progression can route player back to Play or Lab when appropriate.

## Settings

- [ ] Music setting works.
- [ ] SFX setting works.
- [ ] Haptics setting works or is hidden on unsupported platforms.
- [ ] Comfort/reduced motion option is present if visual intensity is high.
- [ ] Privacy/terms/consent controls are present for release builds.
- [ ] Reset save requires destructive confirmation.
- [ ] Debug options are hidden outside dev builds.

## Run states

- [ ] Pause resumes immediately.
- [ ] Restart requires confirmation.
- [ ] Quit requires confirmation.
- [ ] Crash clearly explains failure.
- [ ] Continue offer only appears if eligible.
- [ ] Game Over commits rewards once.
- [ ] Replay starts a new run cleanly.
- [ ] Hub return lands on Play tab or Main Hub default.

## Modal behaviour

- [ ] Only one modal can be open at once.
- [ ] Modal buttons cannot be double-submitted.
- [ ] Modal cancellation is only available when safe.
- [ ] Service failure leaves player in a recoverable state.
- [ ] Reward/purchase callbacks cannot duplicate rewards.

## QA sign-off

- [ ] Every final screen can be reached in editor.
- [ ] Every final screen can be reached on device.
- [ ] Every primary button has a working action or is hidden.
- [ ] Every destructive action has confirmation.
- [ ] Every purchase/ad flow has success/fail/cancel handling.
- [ ] Screens work on small Android aspect ratios.
- [ ] Screens work with no internet except service-specific actions.
