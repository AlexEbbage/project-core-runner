# Core Racer UI Toolkit Compile/Import Hotfix 1

## Reported failures

- `SerializableIntById` missing in Lab and Hangar presenters.
- `ProgressBar` missing in Hangar presenter.
- `ChangeEvent<T>` missing in Settings presenter.
- Empty asset-name exception while resolving nested UXML templates during UIDocument live reload and installer assignment.

## Root causes

1. `SerializableIntById` is declared in `CoreRacer.Meta.Progression`, but the two presenters imported only the profile namespace.
2. `ProgressBar` and `ChangeEvent<T>` are UI Toolkit types from `UnityEngine.UIElements`.
3. The generated UXML used `project://database/Assets/...` dependency addresses. The target project is Unity 2022.3.62f3; the hotfix uses the documented `/Assets/...` absolute `src` form for both `Template` and `Style` references.

## Changed scope

Three C# presenter files and all eleven modular UXML files.

## Deletions

None.

## Verification completed outside Unity

- All UXML files parse as XML.
- No empty `src`, `path`, or `template` attributes remain.
- No `project://database/Assets/` references remain.
- Required namespaces are present in each reported C# file.

Unity compilation and UXML import must be rerun in the editor.
