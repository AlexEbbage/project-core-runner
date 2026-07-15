# Core Racer Screen Contracts

Each screen contract defines what the screen owns, what it must show, what actions it must support, and what it must not absorb from other screens.

## Shared menu shell

### Required elements

- Top bar with currencies and settings gear.
- Screen title or visually obvious current destination.
- Bottom navigation on main menu destinations only: Play, Hangar, Lab, Shop, Progression.
- Selected bottom-nav state.
- Safe-area support for mobile notches and rounded screens.
- Loading, empty, locked, success, and error states where relevant.

### Shared button behaviour

- Enabled buttons must respond immediately with visual/audio feedback.
- Disabled buttons must explain why they are disabled.
- Async buttons must prevent duplicate submissions while pending.
- Destructive buttons must open a confirmation modal.

## Splash / Bootstrap

### Owns

- Initial service boot.
- Profile/save load.
- Entitlement load.
- Consent/privacy gate routing.
- First-session routing to FTUE or hub.

### Must show

- Core Racer branding.
- Loading progress or simple loading message.
- Fallback retry state if critical loading fails.

### Actions

- Continue automatically when boot succeeds.
- Retry if boot fails.
- Open privacy/consent gate when required.

### Does not own

- Store browsing.
- Upgrade decisions.
- Gameplay settings beyond minimum consent/privacy routing.

## Main Hub

### Owns

- Primary Play CTA.
- Summary of current player state.
- One next useful action.
- Top-level navigation.

### Must show

- Selected ship/craft.
- Coins and player level.
- Best score or current route best.
- Primary Play button.
- One and only one highlighted next action, such as claim daily reward or upgrade shield.

### Actions

- Start/navigate to Play.
- Navigate to Hangar, Lab, Shop, Progression, Settings.
- Open highlighted next action.

### Does not own

- Full upgrade catalogue.
- Full shop catalogue.
- Long task/achievement lists.

## Play

### Owns

- Final pre-run confirmation.
- Route/tunnel summary.
- Selected ship summary.
- Optional booster selection if boosters are retained.

### Must show

- Start Run button.
- Selected route/tunnel name or default route.
- Best score and last score.
- Selected ship.
- Any active booster/loadout state.

### Actions

- Start run.
- Change route only when multiple routes are implemented and polished.
- Change booster only if boosters are implemented and clear.

### Does not own

- Cosmetic equipping.
- Upgrade purchasing.
- Progression claiming.

## Hangar

### Owns

- Ship selection.
- Cosmetic preview and equip.
- Locked/owned states for ships, trails, skins, and core FX.

### Must show

- Large craft preview.
- Owned/equipped/locked state.
- Unlock requirement or price.
- Equip/buy button where applicable.

### Actions

- Preview item.
- Equip owned item.
- Buy/unlock cosmetic if supported.
- Route to Shop only for commercial cosmetic packs, not for basic locked-state confusion.

### Does not own

- Powerup upgrades.
- Stat upgrades unless they are read-only summaries.

## Lab

### Owns

- Gameplay upgrades.
- Powerup improvements.
- Coin spending for player power.

### Must show

- Upgrade rows/cards with current level, next effect, and cost.
- Upgrade categories for core ship stats and retained powerups.
- Currency top bar.
- Clear max-level state.

### Actions

- Buy upgrade.
- Show purchase confirmation for expensive/important upgrades if needed.
- Block insufficient currency with readable reason.

### Does not own

- Cosmetic preview/equip.
- IAP store catalogue except optional route to Shop.

## Shop

### Owns

- Remove ads.
- Restore purchases.
- Simple IAP/currency/commercial offers.
- Purchase result states.

### Must show

- Remove Ads entitlement state.
- Restore Purchases action.
- Offers that are actually implemented.
- Clear prices through platform purchasing UI when available.

### Actions

- Start purchase.
- Restore purchases.
- Show success, pending, cancelled, failed, and unavailable states.

### Does not own

- Daily task claims.
- Upgrade purchase flow with soft currency.
- Cosmetic equipping after purchase beyond a simple success route.

## Progression

### Owns

- Player level progress.
- Daily reward.
- Daily/weekly tasks if implemented.
- Milestones/achievements.
- Claimable reward states.

### Must show

- Level/XP progress.
- Claimable daily reward state.
- Task/milestone progress.
- Claimed, claimable, locked, and completed states.

### Actions

- Claim daily reward.
- Claim task reward.
- Claim milestone/achievement reward.
- Navigate to Play/Lab when a goal suggests it.

### Does not own

- Store purchases.
- Ship equipping.
- Run controls.

## Settings

### Owns

- Music volume/toggle.
- SFX volume/toggle.
- Haptics toggle.
- Input sensitivity or control comfort if implemented.
- Reduced motion / comfort.
- Graphics quality.
- Privacy/consent links.
- Support/contact links.
- Reset options behind confirmation.

### Must show

- Current preference states.
- Privacy policy and terms links.
- Restore purchases shortcut may be duplicated here if useful, but Shop remains the owner.

### Actions

- Save preference changes immediately.
- Open privacy/consent screen.
- Reset save only after destructive confirmation.

### Does not own

- Upgrade balancing.
- Progression claims.

## Run HUD

### Owns

- During-run information and controls.

### Must show

- Score.
- Coins collected this run.
- Distance/progress.
- Health/shield state.
- Active powerup timers.
- Pause button.

### Actions

- Pause.
- Surface powerup state, not upgrade it.

### Does not own

- Permanent shop/upgrades.
- Profile management.

## Pause

### Owns

- Temporary interruption of a run.

### Must show

- Resume.
- Restart.
- Quit to hub.
- Quick audio/haptics toggles.

### Actions

- Resume immediately.
- Restart only after confirmation.
- Quit only after confirmation.

### Does not own

- Reward claims.
- Purchases.

## Crash / Continue Offer

### Owns

- Explaining the crash state.
- Offering an eligible continue.

### Must show

- Why the run ended.
- Continue offer only if eligible and ad/service state allows it.
- Skip/continue to Game Over.

### Actions

- Watch rewarded ad to continue.
- Decline and continue to Game Over.

### Does not own

- Reward doubling.
- Upgrade purchases.

## Game Over

### Owns

- Run summary and post-run actions.

### Must show

- Score.
- Best score/new best state.
- Distance.
- Coins earned.
- XP earned.
- Any task/milestone progress highlights.
- Replay and Hub buttons.
- Double rewards offer only if eligible.

### Actions

- Replay.
- Return to hub.
- Claim/double eligible rewards.

### Does not own

- Full progression management.
- Full shop catalogue.
