# Analytics Coverage User Test

## Goal

Verify the shared analytics contract fires for the core run loop, hub navigation, purchases, and progression claims without depending on a live backend.

This checklist is the signoff gate for `F15: Analytics Coverage`.

## Setup

- Use the debug analytics service or a Firebase-enabled test build that logs events locally.
- Start from a known profile state with at least one claimable progression action if possible.

## Checklist

- [ ] Start a run and confirm a run-start event is logged.
- [ ] Crash a run and confirm a run-end event is logged with score/time/reward context.
- [ ] Trigger a continue or rewarded-ad decision and confirm the ad lifecycle events are logged with source and result context.
- [ ] Open the hub and confirm page-selection events are logged.
- [ ] Open `Shop`, `Ship`, and `Lab` from the hub and confirm the hub-entry analytics fire.
- [ ] Buy a shop item and confirm purchase analytics fire with the item id, tab, and price.
- [ ] Equip a ship, skin, trail, or core FX and confirm hangar-equipment analytics fire.
- [ ] Purchase a lab upgrade and confirm upgrade analytics fire with the upgrade type and price.
- [ ] Claim a task, daily login reward, and achievement tier and confirm each claim event is logged.
- [ ] Select a level and press play from `Level Select` and confirm level-selection analytics fire.

## Expected Result

Telemetry should be emitted through the shared analytics service for the major player-facing actions in the approved bundle, with stable keys and no obvious ad hoc event names left behind.

## Test Notes

- Build used:
- Analytics service:
- Pass/Fail:
- Notes:
