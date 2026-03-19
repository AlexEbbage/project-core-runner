# Daily Login Reward User Test

## Goal

Verify the daily login claim flow, streak handling, and optional `Claim x2` behavior.

## Setup

- Open the daily login flow on app/menu start or through its manual entry point.
- If day-skip tools exist, have them ready.

## Checklist

- [ ] Confirm the daily login UI can be opened.
- [ ] Confirm the current day/streak reward is clearly highlighted.
- [ ] Use `Claim` and confirm the reward is granted once.
- [ ] If `Claim x2` exists, use it and confirm bonus grant behavior is correct.
- [ ] Confirm the claim state updates immediately after success.
- [ ] Reopen the screen and confirm the same day cannot be claimed twice.
- [ ] If streak simulation exists, move forward a day and confirm the next reward is shown correctly.

## Expected Result

The flow should be quick, satisfying, and clear about current-day reward versus streak progression.

## Test Notes

- Claim mode tested:
- Pass/Fail:
- Notes:
