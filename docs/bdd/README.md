# Core Racer BDD Specs

These specs define the intended game, not merely the current code state.

## Step 2 product behaviour

The target game is:

> Spin a low-poly mag craft around the inside of a glowing hex tunnel, dodge bold hazards, collect hex coins and powers, chase the bright orange core, upgrade, customise, and run again.

## Step 3 final menu set

The first-release menu set is now locked. Use these files when planning and implementing menus:

- `docs/menus/final-menu-set.md`
- `docs/menus/screen-contracts.md`
- `docs/menus/navigation-and-modal-flow.md`
- `docs/menus/menu-content-pruning.md`
- `docs/menus/menu-implementation-checklist.md`
- `docs/bdd/features/10_final_menu_set.feature`

## Feature files

- `features/01_core_run.feature`
- `features/02_controls_and_tunnel.feature`
- `features/03_obstacles.feature`
- `features/04_pickups_and_powerups.feature`
- `features/05_scoring_rewards_and_progress.feature`
- `features/06_menus_and_navigation.feature`
- `features/07_ftue.feature`
- `features/08_monetisation_compliance_and_services.feature`
- `features/09_debug_testing_and_accessibility.feature`
- `features/10_final_menu_set.feature`

## Menu truth

Bottom navigation is locked to:

1. Play
2. Hangar
3. Lab
4. Shop
5. Progression

Settings is a top-right gear/profile action.

Daily Rewards, Tasks, Achievements, Milestones, and Level Rewards live inside Progression.
