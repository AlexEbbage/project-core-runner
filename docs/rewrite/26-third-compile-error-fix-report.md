# Third Compile Error Fix Report

This patch fixes the third compile batch reported after importing `core-racer-clean-replacement-launch-ready-final-compile-fix-2.zip`.

## Fixes included

### Unity Test Framework / NUnit

Errors such as:

```text
The type or namespace name 'NUnit' could not be found
The type or namespace name 'TestAttribute' could not be found
```

were caused by the generated EditMode tests being present while the Unity Test Framework package was not explicitly declared.

Changes:

- Added `com.unity.test-framework` to `Packages/manifest.json`.
- Updated `Assets/CoreRacer/Tests/CoreRacer.Tests.asmdef` to reference:
  - `CoreRacer.Runtime`
  - `UnityEngine.TestRunner`
  - `UnityEditor.TestRunner`
- Kept `optionalUnityReferences: ["TestAssemblies"]`.

### ObstaclePatternDefinition.DisplayName

Error:

```text
ObstaclePatternDefinition does not contain a definition for DisplayName
```

was caused by the default config builder assigning a display name to obstacle pattern assets while `ObstaclePatternDefinition` only had `Id`.

Change:

- Added `public string DisplayName;` to `ObstaclePatternDefinition`.

## Import notes

After replacing the package, let Unity refresh packages. If the NUnit errors persist, open Package Manager and confirm **Test Framework** is installed, or delete the `Library/PackageCache` / `Library` folder and reopen the project so Unity resolves `Packages/manifest.json` again.
