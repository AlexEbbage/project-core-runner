# MVP Mobile Acceptance

## Status

Active next release gate after the completed F22 UI replacement. The earlier validated Android Development APK predates the replacement UI and must be rebuilt before physical-device acceptance.

## Goal

Prove the complete core run is understandable, fair, and stable on a portrait mobile device before expanding content.

## Device Checklist

1. Install a Development Build on one representative Android device and, when available, one iPhone.
2. Start from the main menu and tap Play with a fresh tutorial save.
3. Hold the left and right screen halves. Steering must respond immediately without requiring a drag.
4. Enable `Drag Controls` in Comfort settings and verify small drags produce continuous analog steering.
5. Complete a run of at least five minutes. Record minimum/average frame rate, GC spikes, memory growth, heat, and battery impact.
6. Confirm the player, HUD, tutorial, pause button, and Game Over actions remain inside the device safe area.
7. Collide with each obstacle family. Single walls, multi-walls, fan, and door must damage or end the run consistently.
8. On the first death, use Continue. Confirm slow motion ends, the player returns behind the crash, and the short grace period prevents an immediate repeat death.
9. End the continued run, press Retry, and confirm score, distance, pickups, effects, speed, and obstacle state start cleanly.
10. Press Home, confirm the menu returns at normal speed, then start another run.

## Evidence to Capture

- Device model, OS, build target, quality level, and build commit.
- Portrait screenshots of tutorial steering, active gameplay, Continue, Game Over, Retry, and Home.
- Profiler or platform frame-time capture from the first minute and after five minutes.
- Peak memory and any GC/frame spikes above the chosen device budget.
- Notes for collision misses, unreadable patterns, control confusion, thermal throttling, or unsafe-area overlap.

## Current Automated Evidence

- `TouchSteeringInterpreterTests`: eight screen-side and analog-drag cases.
- `CoreRun_LongTraversalReusesPoolsAndContinueProvidesRecoveryGrace`: sustained synthetic traversal, bounded pools, score/distance progress, Continue repositioning, time reset, and invulnerability.
- Full result on 2026-07-21: 47/47 EditMode and 13/13 PlayMode passed.
- Android artifact: `Builds/Android/CoreRacer-1.1.2-dev.apk` (128.2 MB, ARMv7 + ARM64, min SDK 24, target SDK 36, debug signed).
- APK SHA-256: `B396DD79DFCB38FD917AFD5A37BB939CB7D17B33B46390A337AD3261860510D3`.

## Android Installation

1. Enable USB debugging and connect the device so `adb devices -l` reports it as authorized.
2. Run `adb install -r Builds/Android/CoreRacer-1.1.2-dev.apk` from the project root.
3. In Unity, open `Window > Analysis > Profiler` and select the connected `Core Runner` player.
4. Complete the checklist above and record device/OS, frame timing, memory, screenshots, and failures.

To rebuild, use `Tools > Core Racer > Build > Android Development APK`. The command uses debug signing for the Development APK and restores the project release-signing and App Bundle preferences afterward.

## Signoff Boundary

Editor tests and Editor profiler samples do not close physical-device acceptance. Mark this checklist complete only after a human records a clean full lifecycle on device.
