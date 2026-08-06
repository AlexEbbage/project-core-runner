# Conventions
- Behaviour-first, minimal-change delivery; implement one focused vertical slice and validate the player-facing behaviour.
- Runtime structure converges opportunistically toward `Assets/Scripts/Gameplay`, `UI`, `Services`, and minimal `Core`; do not perform unrelated migrations.
- Keep MonoBehaviours thin; plain C# owns reusable gameplay logic. UI requests through stable APIs/events and does not mutate gameplay directly.
- Authored scene/prefab UI plus serialized reference-bound controllers is canonical. No destructive/runtime UI regeneration or `FindObjectOfType` fallback hiding bad wiring.
- Preserve serialized field names; if rename is unavoidable, use `FormerlySerializedAs` and verify scene/prefab references.
- Null-guard required references and log explicit failures. Preserve public APIs unless breaking change is in scope.