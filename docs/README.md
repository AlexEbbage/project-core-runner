# project-core-racer Docs

This `docs/` folder is the authoritative project context for product framing, architecture, planning, and future implementation tracking.

## Core Docs

| File | Purpose |
| --- | --- |
| `product-requirements.md` | Current game definition, goals, constraints, and open questions |
| `architecture.md` | Real repo/module structure, scene model, data ownership, and boundaries |
| `style-guide.md` | Visual direction, UI language, feedback style, and UI stack policy |
| `feature-registry.md` | Approved feature list with `Implemented`, `Partial`, and `Planned` status |
| `task-registry.md` | Immediate execution backlog and feature-level task breakdown |
| `script-registry.md` | Restore map of major runtime script ownership |
| `implementation-plan.md` | Delivery sequence and validation expectations |
| `decision-registry.md` | Accepted production and technical decisions |
| `codex-context.md` | Working rules for future Codex execution in this repo |
| `user-testing/` | Manual test checklists, one file per tracked experience feature |

## Working Rules

- Keep the product, architecture, feature, task, script, and decision docs current as work evolves.
- Keep `docs/user-testing/` current as features become playable, change flow, or gain new blockers/workarounds.
- When a feature status changes, update both `feature-registry.md` and the relevant user-testing checklist in the same pass.
- Do not start a new feature unless it is recorded in `feature-registry.md`.
- Prefer one feature `In progress` at a time unless the user explicitly requests parallel work.
- Use these docs as the restore point instead of relying on chat history.

## Feature Status Values

- `Implemented`
- `Partial`
- `Planned`
- `In progress`
- `Implemented (awaiting review)`
- `Completed`

## Task Status Values

- `Not started`
- `In progress`
- `Blocked`
- `Implemented (awaiting review)`
- `Completed`
