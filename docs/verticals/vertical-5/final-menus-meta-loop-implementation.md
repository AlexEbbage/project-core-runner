# Vertical 5 — Final Menus and Meta Loop Implementation

## Goal

Vertical 5 locks the first-release menu/meta-loop around the gameplay verticals already implemented:

```text
Hub -> Play -> Run HUD -> Continue/Game Over -> Retry or Hub
```

It also locks the final bottom navigation contract:

```text
Play | Hangar | Lab | Shop | Progression
```

Settings is intentionally excluded from bottom navigation and should be opened from a top-right gear/profile action.

## Runtime changes

- `FinalMenuSetRules` is the runtime source of truth for first-release pages.
- `MainMenuPageRouter` now tracks the current page, exposes `PageChanged`, validates final menu pages, and can report missing required bottom-nav pages.
- `BottomNavBarController` only exposes first-release bottom-nav destinations as normal actions.
- `TopBarController` can open Settings and Progression from top-bar actions.
- `MainMenuShell` refreshes the top bar when pages change and can expose direct page helpers.
- `PlayPageController` refreshes embedded level selection when the Play page opens.
- `GameOverController` now has explicit button-action methods for retry, hub return, continue, decline continue, and double rewards.

## Editor tooling

Run:

```text
Tools/Core Racer/Vertical 5/Apply Final Menus Meta Loop
Tools/Core Racer/Vertical 5/Validate Final Menus Meta Loop
```

The apply tool wires obvious scene references where it can. It does not invent a new UI layout; it stabilises the existing clean menu scene.

## Manual acceptance checks

1. Open `Assets/CoreRacer/Scenes/CoreRacer_Main.unity`.
2. Run the Vertical 5 apply tool.
3. Run the Vertical 5 validator.
4. Enter Play Mode.
5. Confirm the player lands on the Main Hub / Play page.
6. Navigate bottom tabs in this order: Play, Hangar, Lab, Shop, Progression.
7. Confirm Settings is opened from the top bar, not the bottom nav.
8. Start a run from Play.
9. Crash or press the debug crash key.
10. Confirm Continue Offer appears before final Game Over when available.
11. Confirm Retry starts a new run.
12. Confirm Hub returns to the main menu.

## Scope deliberately deferred

Vertical 5 does not finalise:

- economy balance,
- daily reward timing,
- achievement tuning,
- IAP SDK behaviour,
- ad production behaviour,
- final UI art/layout polish.

Those belong to Verticals 6 and 7.
