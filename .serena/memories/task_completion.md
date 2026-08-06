# Task Completion
- Inspect the live Unity Editor scene and relevant serialized references before scene-facing changes.
- After script edits: wait until compilation/domain reload finishes, then inspect Unity Console errors.
- Run the focused EditMode/PlayMode tests that prove the behaviour; use PlayMode for lifecycle, activation, physics, and scene interaction.
- Manually verify authored UI/input/feel where automation is insufficient; capture relevant logs/screenshots.
- Save all changed scenes/prefabs through Unity and re-check wiring/serialization.
- Update project tracking docs when feature scope/status/testability changes; explicitly say when no docs update is needed.
- Final report: behaviours, changed/new/deleted files, tests, exact validation commands/results, inspector steps, risks/assumptions, and next slice.