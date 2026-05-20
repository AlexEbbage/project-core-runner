# Economy Simulation and Protection

## Added tools

- Economy ledger
- Economy anomaly detector
- Economy simulation settings
- Economy simulation report generator
- Product catalogue validator

Run:

```txt
Tools/Core Racer/Run Default Economy Simulation
Tools/Core Racer/Validate Product Catalogues
```

## What to tune

- Time to first upgrade: target 2–5 runs
- Time to second ship: target depends on session length, but should not feel unreachable
- Rewarded ad boost: should feel helpful, not mandatory
- Daily/weekly/monthly task rewards: should support retention without breaking progression
- Premium: should remove friction but not remove all rewarded choices

## Economy protection

Use ledger entries for every currency grant/spend:

- source
- amount
- balance before
- balance after
- timestamp
- reward/purchase/run id

This makes support issues and economy bugs much easier to investigate.
