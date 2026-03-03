# AGENTS.md

## Purpose and Architectural Baseline

This repository hosts the **project-core-runner** Unity project.

The project follows a **Clean, modular, Unity-first architecture**.

This is NOT a layered DDD structure.
This is a **feature-oriented modular structure** designed for:

- Easy feature isolation
- Low coupling
- High replaceability
- Fast iteration
- Clear ownership per module

---

## Primary Structure

All runtime code lives under:

Assets/Scripts/

Top-level modules:

- camera/
- config/
- debug/
- feedback/
- fx/
- gameplay/
- localization/
- meta/
- monetisation/
- scenemanagement/
- services/
- settings/
- ui/

Each folder represents a **self-contained functional module**.

Modules should not tightly depend on internal implementation details of other modules.

Cross-module communication should happen via:
- Events
- Interfaces
- Clearly defined service APIs

---

## Gameplay Module Structure

Gameplay is further modularised:

Assets/Scripts/gameplay/

Submodules:

- environment/
- gameflow/
- obstacles/
- pickups/
- player/
- powerups/
- vfx/

Each submodule should:
- Own its logic
- Avoid reaching into sibling submodules directly
- Communicate via gameflow or defined contracts

Gameflow acts as the coordinator/orchestrator.

---

## Services Module Structure

Assets/Scripts/services/

Submodules:

- ads/
- analytics/
- audio/
- notifications/

Rules:
- Services expose clear public APIs.
- Other modules depend only on service interfaces, not internal implementations.
- Avoid service-to-service tight coupling.
- No hidden global singletons unless explicitly justified.

---

## Clean Unity Guidelines (Project-Specific)

- Prefer modular feature folders over horizontal layering.
- Avoid creating artificial Core/Domain layers.
- MonoBehaviours are acceptable within modules.
- Keep classes small and focused.
- Prefer events over polling.
- Prefer composition over inheritance.
- Use ScriptableObjects for configuration where useful.
- Avoid cross-module circular dependencies.
- Keep modules replaceable.

Do not introduce over-engineered abstraction.

---

## Allowed Actions

- Make minimal, focused edits.
- Modify project-owned code inside Assets/Scripts/.
- Improve modular boundaries when directly related to the task.
- Add small supporting interfaces/events within a module.
- Add documentation or clarifying comments.
- Run lightweight validation already supported in the repo.

---

## Restricted Actions

- Do not install packages or SDKs unless explicitly requested.
- Do not restructure top-level module folders without approval.
- Do not introduce large architectural shifts.
- Do not modify vendor or plugin-managed folders.
- Do not introduce secrets or machine-specific paths.

---

## Validation Expectations

- Validate only the changed module.
- Avoid full-project scans unless necessary.
- If Unity Editor validation is unavailable:
  - State what was validated.
  - State limitations clearly.

---

## Code Change Principles

- One logical concern per change.
- Avoid mixing refactor and feature work.
- Keep module boundaries clean.
- Avoid global static state unless justified.
- Guard serialized fields against null.
- Keep public APIs minimal and intentional.

---

## Commit and PR Standards

Commit format:

<type>: <short summary>

Where <type> is:

- feat
- fix
- refactor
- docs
- test
- chore

PR must include:

- What changed
- Why it changed
- Validation performed
- Limitations or follow-ups

Keep PRs single-purpose and scoped.

---

## Performance Guardrails

- Use rg for search instead of broad recursive scans.
- Avoid repo-wide find/ls unless necessary.
- Limit command output to relevant paths.
- Keep validation targeted to the changed module.