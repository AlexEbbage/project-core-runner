---
name: unity-2022-build-orchestration
description: Unity 2022 CI/CD build orchestration for deterministic batchmode builds across Android (AAB/APK) and Windows standalone with preflight validation, scene selection, output naming, and troubleshooting. Use when setting up or debugging automated Unity builds, release pipelines, or local reproducible build scripts.
---

# Unity 2022 Build Orchestration

## Overview

Use this skill to run deterministic Unity 2022 batchmode builds with a repeatable preflight gate and consistent artifact naming for Android and Windows targets.

## Required Environment Variables

Set these environment variables before running any build command:

- `UNITY_EXE`: Full path to Unity 2022 editor executable.
  - Windows example: `C:\Program Files\Unity\Hub\Editor\2022.3.XXf1\Editor\Unity.exe`
  - Linux example: `/opt/unity/Editor/Unity`
- `UNITY_PROJECT_PATH`: Absolute path to the Unity project root.
- `UNITY_LOG_FILE`: Path to the Unity log output file.
- `BUILD_OUTPUT_DIR`: Absolute directory where artifacts are written.
- `BUILD_VERSION`: Semantic app version (for naming), e.g. `1.8.0`.
- `BUILD_NUMBER`: Monotonic build number (maps to Android version code), e.g. `10800`.

Android signing and packaging variables:

- `ANDROID_KEYSTORE_PATH`: Absolute path to keystore file.
- `ANDROID_KEYSTORE_PASS`: Keystore password.
- `ANDROID_KEYALIAS_NAME`: Key alias.
- `ANDROID_KEYALIAS_PASS`: Key alias password.
- `ANDROID_BUILD_KIND`: `aab` or `apk`.

Licensing variables (CI/headless):

- `UNITY_LICENSE_FILE`: Path to a pre-activated Unity license file (`.ulf`) or license content handled by your CI secret process.
- `UNITY_USERNAME` and `UNITY_PASSWORD` (only if your license flow depends on account sign-in).

## Canonical Batchmode Command Templates

Use one command shape for all pipelines, then vary only target and output.

```bash
"$UNITY_EXE" \
  -batchmode -nographics -quit \
  -projectPath "$UNITY_PROJECT_PATH" \
  -logFile "$UNITY_LOG_FILE" \
  -executeMethod BuildAutomation.PerformBuild \
  --buildTarget "$BUILD_TARGET" \
  --buildKind "$ANDROID_BUILD_KIND" \
  --buildVersion "$BUILD_VERSION" \
  --buildNumber "$BUILD_NUMBER" \
  --outputDir "$BUILD_OUTPUT_DIR"
```

### Android AAB

```bash
BUILD_TARGET=Android
ANDROID_BUILD_KIND=aab
```

### Android APK

```bash
BUILD_TARGET=Android
ANDROID_BUILD_KIND=apk
```

### Windows Standalone (64-bit)

```bash
BUILD_TARGET=StandaloneWindows64
# Ignore ANDROID_BUILD_KIND for Windows builds.
```

Implementation expectation for `BuildAutomation.PerformBuild`:

- Read CLI args and environment variables.
- Apply Android signing credentials via `PlayerSettings.Android` before build.
- Select `BuildOptions.StrictMode` and fail fast on warnings promoted to errors as needed.
- Return non-zero exit code when build fails.

## Preflight Checklist (ProjectSettings.asset)

Before build, parse `ProjectSettings/ProjectSettings.asset` and fail the pipeline if any required field is missing/invalid.

Validate these keys:

1. `AndroidMinSdkVersion`
   - Enforce your minimum supported API level policy.
2. `AndroidTargetArchitectures`
   - Ensure required architectures are enabled for distribution policy.
3. `AndroidBundleVersionCode`
   - Must match/derive from `BUILD_NUMBER` and be monotonic.
4. `applicationIdentifier`
   - Verify package/product identifiers for all relevant targets (at minimum Android and Standalone).
   - Enforce release-safe identifiers (no debug suffix on production branches).

Recommended gate behavior:

- Print all resolved values before build.
- Exit with a single summarized error block listing all failed checks.
- Block artifact generation when any check fails.

## Scene Selection Guidance (EditorBuildSettings.asset)

Use `ProjectSettings/EditorBuildSettings.asset` as the source of truth for included scenes.

Guidance:

- Include only scenes with `enabled: 1`.
- Preserve file order; Unity build index is order-sensitive.
- Require a deterministic bootstrap scene at index 0.
- Fail preflight if zero scenes are enabled.
- Optionally enforce per-platform scene policies through your build method (e.g., exclude test scenes on release).

For deterministic behavior:

- Do not mutate scene lists in ad-hoc scripts during CI.
- Keep scene inclusion changes code-reviewed and committed.

## Deterministic Output Naming Convention

Use immutable artifact names that include app version and platform:

```text
{productName}_{version}+{buildNumber}_{platform}_{flavor}.{ext}
```

Examples:

- `Runner_1.8.0+10800_android_release.aab`
- `Runner_1.8.0+10800_android_release.apk`
- `Runner_1.8.0+10800_win64_release.zip`

Rules:

- Use lowercase for `platform` and `flavor` tokens.
- Never overwrite existing artifacts; write into a unique folder per build invocation.
- Emit a checksum file (`.sha256`) per artifact.

## Troubleshooting

### Android SDK/NDK and toolchain

- Confirm Android SDK, NDK, and JDK paths configured in Unity Preferences or via CI image provisioning.
- Ensure Unity 2022-compatible NDK is installed (mismatch can fail Gradle or IL2CPP toolchain).
- If Gradle fails with missing platform/build-tools, install exact API/build-tools versions requested in logs.
- Clear corrupted Gradle cache (`~/.gradle/caches`) in ephemeral CI runners when dependency resolution is inconsistent.

### IL2CPP failures

- Verify `Scripting Backend = IL2CPP` and architecture set matches `AndroidTargetArchitectures` policy.
- Check for native plugin ABI mismatches (e.g., `armeabi-v7a` plugin missing while architecture enabled).
- Re-run with full logs and inspect first compiler error; downstream IL2CPP errors are often cascading noise.

### Signing issues

- Validate keystore file exists and passwords are non-empty.
- Confirm alias name exactly matches keystore entry.
- Ensure CI secret injection preserves special characters and avoids newline truncation.
- Fail early by verifying signing credentials before invoking `BuildPipeline.BuildPlayer`.

### Common batchmode diagnostics

- Always archive the full Unity log and Gradle output.
- Print resolved build params (minus secrets) at build start.
- Treat any fallback/defaulted parameter as a preflight failure, not a warning.
