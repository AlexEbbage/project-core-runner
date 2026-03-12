# Codex Context

## Purpose

This file explains how Codex should operate in `project-core-racer`.

## Project Rules

- Architecture mode: `hybrid`
- Treat the docs set as the durable source of truth for product and implementation state
- Do not invent features, dependencies, or completion status beyond what the repo and approved roadmap support
- Do not move to another feature without user approval
- Keep `feature-registry.md`, `task-registry.md`, `script-registry.md`, and `decision-registry.md` current when implementation work happens
- Prefer the thinnest useful slice first

## Standard Workflow

1. Read `product-requirements.md`
2. Read `feature-registry.md` and `task-registry.md`
3. Read `architecture.md`, `style-guide.md`, and `decision-registry.md`
4. Confirm the feature chosen for implementation
5. Implement only the scripts and assets needed for that feature
6. Update the registries and decisions before finishing

## Validation Expectations

- Prefer the cheapest test layer that proves the behavior
- Call out required inspector or asset wiring changes
- Document manual verification when automated tests are weak
- Do not document a feature as complete if it is only partially implemented in code or scene wiring
