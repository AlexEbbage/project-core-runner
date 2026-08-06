# Core Racer UI Toolkit Compile/Import Hotfix 1

Apply this after `CoreRacer_Complete_UI_Toolkit_Rework_Patch`.

## Fixes

- Adds `CoreRacer.Meta.Progression` imports required for `SerializableIntById`.
- Adds `UnityEngine.UIElements` imports required for `ProgressBar` and `ChangeEvent<T>`.
- Replaces `project://database/Assets/...` UXML dependencies with Unity 2022.3-compatible `/Assets/...` paths.
- Fixes the `ArgumentException: The input asset name cannot be empty` failure raised while resolving nested UXML templates.

## Install

1. Close Unity or allow it to finish the current failed import.
2. Extract this zip into the Unity project root and overwrite matching files.
3. Return to Unity and wait for script and UXML reimport.
4. Clear the Console after compilation.
5. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
6. Run `Tools > Core Racer > UI Toolkit > Install Final UI` again.
7. Run `Tools > Core Racer > UI Toolkit > Validate Final UI`.
8. Send the first remaining compiler/import error if another appears.

No files need deleting.
