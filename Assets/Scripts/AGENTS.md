# AGENTS.md

## C# Naming Conventions

- Classes, interfaces, enums, structs, and public members: PascalCase.
- Methods: PascalCase verbs or verb phrases (for example, `InitializeSession`, `ApplyDamage`).
- Private fields: _camelCase.
- Serialized private fields: `[SerializeField] private Type _camelCase;`.
- Local variables and parameters: camelCase.
- Constants:
  - UPPER_SNAKE_CASE for `const` values with project-wide or cross-file significance.
  - PascalCase for narrowly scoped constants to match standard C# conventions.
- Readonly fields: _camelCase for private readonly members.

---

## Module Structure (Project Direction)

Primary folders under Assets/Scripts/:

- Gameplay/
- UI/
- Services/
- Core/ (minimal shared utilities only)

When modifying legacy folders that do not match this structure,
relocate them into the appropriate directory as part of the same change,
provided the move is scoped and safe.

Do not perform broad repository-wide restructuring.
Migrate opportunistically when touching the module.

---

## Module Boundary Rules

- Do not access another module’s internal implementation.
- Cross-module communication must occur via:
  - Public APIs
  - Interfaces
  - Events
- Avoid circular dependencies between Gameplay, UI, Services, and Core.
- Do not allow Core to accumulate gameplay-specific logic.

Gameplay must not depend on UI.
UI must not directly control gameplay internals.
Services must expose clear APIs and avoid hidden global state.

---

## MonoBehaviour Change Guidance

- Keep `Awake` and `OnEnable` lightweight and predictable.
- Do not introduce hidden side effects in lifecycle methods
  (for example, implicit service registration, scene-wide state mutation,
  or expensive runtime lookups) unless clearly documented.
- Prefer explicit setup/initialization methods for orchestration logic.
- Preserve existing inspector wiring assumptions:
  - Do not rename/remove serialized fields without migration handling.
  - Prefer extending behavior over changing serialized contract shape.
  - If initialization order matters, make dependencies explicit and guard for missing references.
- Always pair event subscription with proper unsubscription.

---

## Change Safety Checklist

Before finalizing C# changes:

- Null-guard serialized references before use where missing references can occur.
- Avoid breaking existing scene/prefab bindings by changing serialized field names, types, or visibility without a safe migration path.
- Prefer additive, non-breaking changes over in-place contract changes.
- Keep public API changes minimal and intentional.
- Update all call sites in the same change when altering APIs.
- Ensure no new circular dependencies were introduced.
- Confirm moved files maintain namespace and assembly definition consistency.

---

## Static Validation When Execution Is Unavailable

If you cannot run Play Mode/tests, perform compile-likely inspection checks:

- Verify renamed symbols/types/namespaces are updated across usages (`rg`/IDE references).
- Inspect inheritors and interface implementations for signature drift.
- Confirm serialized field/type changes still match expected component usage patterns.
- Check event/API call paths for nullability and obvious runtime exceptions.
- Confirm no cross-module boundary violations were introduced.