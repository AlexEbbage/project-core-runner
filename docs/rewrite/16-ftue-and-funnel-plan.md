# FTUE and Funnel Plan

## FTUE steps

1. Welcome / objective: start the first route from the hub.
2. Input: hold left/right or drag to rotate around the tunnel.
3. Dodge: avoid the first obstacle pattern.
4. Collect: collect the deterministic tutorial coin.
5. Powerup: collect the deterministic tutorial powerup.
6. Crash/continue: explain respawn offers without forcing a crash.
7. Upgrade: open the Lab upgrade prompt.
8. Tasks: open the daily reward/task prompt from Progression.

## Implementation status

- Implemented in `CoreRacer_Main` through the existing save-backed `TutorialService`.
- Tutorial reset is exposed from the support/debug panel.
- Debug analytics emits tutorial start, step completion, completion, and reset events through `IAnalyticsService`.

## Funnel events

- tutorial_started
- tutorial_step_completed
- tutorial_completed
- first_run_started
- first_run_finished
- first_crash
- first_continue_offer_seen
- first_upgrade_purchased
- first_shop_opened
- first_task_claimed
- first_daily_reward_claimed
- first_ad_watched

Use these events to find first-session drop-off before spending time on more content.
