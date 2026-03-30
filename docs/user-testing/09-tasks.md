# Tasks (Daily / Weekly / Monthly) User Test

## Goal

Verify tasks show live claimability, cadence state, and milestone rewards using the profile-backed progression flow.

## Setup

- Open the hub and use the `Tasks` side-entry action.
- Have enough currency/progression state available to claim at least one task or milestone if possible.

## Checklist

- [ ] Confirm daily, weekly, and monthly views can be opened if available.
- [ ] Confirm each cadence shows exactly three active tasks from the authored pool for the current cycle.
- [ ] Confirm each task shows progress and reward information.
- [ ] Confirm completed tasks expose a clear `Claim` action instead of silently auto-claiming.
- [ ] Finish a run and confirm matching task progress updates from the real run result instead of static preview data.
- [ ] Reopen the page and confirm the same active task subset remains selected for the current cycle.
- [ ] Claim a single reward and confirm the reward is granted once.
- [ ] Confirm any milestone or points track updates after claims.
- [ ] Confirm the daily login preview remains visible in the tasks hub and reflects current claim availability.
- [ ] Confirm leaving and reopening the surface preserves claim state.
- [ ] Reopen the page and confirm claimed state persists.
- [ ] If time-reset tooling exists, simulate a cadence reset and confirm the reward states rebuild cleanly.

## Expected Result

Tasks should feel readable, rewarding, and easy to track at a glance, with claim states grounded in profile persistence instead of transient UI state.

## Test Notes

- Cadences tested:
- Pass/Fail:
- Notes:
