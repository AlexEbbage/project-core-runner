# Core Racer — Vertical Slice Planning

This folder is the delivery bridge between:

- BDD feature files in `docs/bdd/features`
- the final menu contracts in `docs/menus`
- implementation work inside `Assets/CoreRacer`

The project is now planned around vertical slices, not horizontal systems.

## Vertical slice rule

A vertical is only complete when the player can move through the relevant experience in `CoreRacer_Main`.

Examples:

- "Obstacle system implemented" is not enough.
- "Player starts a run, sees walls/fans/lasers/doors, dodges them, crashes correctly, and receives rewards" is a vertical.

## Files

| File | Purpose |
|---|---|
| `vertical-roadmap.md` | Overall ordered roadmap |
| `bdd-to-vertical-map.md` | Maps BDD IDs to vertical slices |
| `vertical-slice-contracts.md` | Each vertical's scope and acceptance criteria |
| `implementation-order.md` | Practical build order and dependency rules |
| `qa-and-test-strategy.md` | How each vertical should be tested |
| `asset-production-plan.md` | Assets needed by vertical |
| `definition-of-done.md` | What "done" means before moving on |
