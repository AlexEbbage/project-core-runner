Feature: Final menu set
  The game should have a small, clear menu system that supports the run-upgrade-repeat loop.

  Background:
    Given the player profile has loaded successfully
    And the player has completed any required privacy or consent gate

  Scenario: Main bottom navigation contains the final first-release destinations
    Given the player is on a main menu screen
    Then the bottom navigation should contain Play, Hangar, Lab, Shop, and Progression
    And the bottom navigation should not contain Settings
    And the bottom navigation should not contain Daily Rewards, Tasks, Achievements, Inbox, Calendar, Notes, Ideas, Workflows, Projects, or Prospects

  Scenario: Settings is opened from the top bar
    Given the player is on a main menu screen
    When the player taps the settings gear
    Then the Settings screen should open
    And the previous main menu destination should be remembered
    When the player closes Settings
    Then the player should return to the previous main menu destination

  Scenario: Play is the default primary action
    Given the player enters the main hub after boot
    Then the primary visible action should be Play or Start Run
    And the player should be able to start a run without visiting another menu

  Scenario: Hangar owns cosmetic selection
    Given the player opens Hangar
    Then the player should be able to preview ships or cosmetics
    And the player should be able to equip owned ships or cosmetics
    And Hangar should not sell powerup upgrades

  Scenario: Lab owns gameplay upgrades
    Given the player opens Lab
    Then the player should see gameplay upgrades with current level, next effect, and cost
    And the player should be able to buy an affordable upgrade
    And Lab should not equip cosmetic items

  Scenario: Progression owns goals and claims
    Given the player opens Progression
    Then the player should be able to view level progress
    And the player should be able to view or claim daily rewards, tasks, milestones, or achievements when implemented
    And Progression should not show commercial purchase offers as its main purpose

  Scenario: Shop owns purchase and restore flows
    Given the player opens Shop
    Then the player should be able to view Remove Ads if supported
    And the player should be able to restore purchases
    And unimplemented products should be hidden

  Scenario: Run screens hide menu navigation
    Given the player starts a run
    When the countdown begins
    Then the bottom navigation should be hidden
    And the Run HUD should show only run-relevant information and controls

  Scenario: Pause protects destructive run actions
    Given the player is in an active run
    When the player pauses
    Then the player should be able to resume immediately
    And restarting the run should require confirmation
    And quitting the run should require confirmation

  Scenario: Game Over gives clear post-run choices
    Given the player has crashed and no continue is being used
    When Game Over opens
    Then the player should see score, rewards, XP, and best-score state
    And the player should be able to replay
    And the player should be able to return to the hub
