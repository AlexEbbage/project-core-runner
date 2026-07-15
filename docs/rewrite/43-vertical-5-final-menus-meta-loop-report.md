# Rewrite Report 43 — Vertical 5 Final Menus and Meta Loop

## Summary

Vertical 5 adds a concrete first-release menu contract and stabilises the route from meta screens back into the run loop.

## Final bottom navigation

```text
Play
Hangar
Lab
Shop
Progression
```

Settings remains a top-bar action.

## Files changed

```text
Assets/CoreRacer/Runtime/UI/MainMenu/FinalMenuSetRules.cs
Assets/CoreRacer/Runtime/UI/MainMenu/MainMenuPageRouter.cs
Assets/CoreRacer/Runtime/UI/MainMenu/BottomNavBarController.cs
Assets/CoreRacer/Runtime/UI/MainMenu/TopBarController.cs
Assets/CoreRacer/Runtime/UI/MainMenu/MainMenuShell.cs
Assets/CoreRacer/Runtime/UI/MainMenu/PlayPageController.cs
Assets/CoreRacer/Runtime/UI/GameOver/GameOverController.cs
Assets/CoreRacer/Editor/Verticals/FinalMenusMetaLoopVerticalInstaller.cs
Assets/CoreRacer/Tests/EditMode/Vertical5FinalMenuSetTests.cs
```

## Validation

Run:

```text
Tools/Core Racer/Vertical 5/Apply Final Menus Meta Loop
Tools/Core Racer/Vertical 5/Validate Final Menus Meta Loop
```

Then run the EditMode test suite.

## Known limitations

This patch cannot verify Unity compilation inside this environment. It also does not create new menu prefabs; it assumes the clean scene already contains the menu shell and page views from earlier work.
