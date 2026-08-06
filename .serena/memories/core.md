# Project Core
- Unity runner project; canonical clean scene is `Assets/Scenes/CoreRacer_Main.unity`, while build settings have historically pointed at legacy `Assets/Scenes/GameScene.unity`; verify live Editor/build scene before runtime work.
- Authoritative durable context is indexed by `docs/README.md`; implementation sequence/status lives in `docs/implementation-plan.md`.
- Repository guardrails and hybrid module boundaries are in root `AGENTS.md`; preserve serialized scenes/prefabs and do not regenerate authored UI at runtime.
- Tech/package pins: `mem:tech_stack`.
- Code/architecture conventions: `mem:conventions`.
- Completion checks: `mem:task_completion`.
- Useful Windows/Unity commands: `mem:suggested_commands`.