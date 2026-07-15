@menus @navigation @vertical-progression
Feature: Menus and navigation
  Menus should support the run-upgrade-repeat loop without burying Play.

  Background:
    Given the game has booted successfully
    And the player profile has loaded

  @p0 @manual @automatable
  Scenario: Main Hub has clear primary navigation
    Given the player is on the Main Hub
    Then the Play action is the most prominent action
    And the bottom navigation contains Play, Hangar, Lab, Shop, and Progression
    And Settings is available outside the bottom navigation
    And the top bar shows profile level, XP, soft currency, and premium currency if premium currency exists

  @p0 @manual @automatable
  Scenario: Bottom navigation switches pages without losing profile state
    Given the player is on the Main Hub
    When the player switches between Play, Hangar, Lab, Shop, and Progression
    Then the selected page changes visibly
    And the top bar remains visible or returns consistently according to the UI layout
    And profile values remain consistent across pages

  @p0 @manual
  Scenario: Play page focuses on starting the next run
    Given the player opens the Play page
    Then the page shows a Start Run action
    And it shows selected ship or route context
    And it shows best score or last score
    And it does not require the player to manage unrelated progression before starting

  @p0 @manual @automatable
  Scenario: Hangar owns ship identity and cosmetics
    Given the player opens the Hangar
    When the player previews an owned ship or cosmetic
    Then the preview updates
    And the player can equip owned items
    And locked items show their unlock or purchase requirement
    And power upgrades are not managed from the Hangar

  @p0 @manual @automatable
  Scenario: Lab owns gameplay upgrades
    Given the player opens the Lab
    Then upgrade rows show current level, next effect, and cost
    And upgrade purchase results update the player's profile and top bar
    And cosmetic equipping is not managed from the Lab

  @p0 @manual
  Scenario: Shop owns commercial offers
    Given the player opens the Shop
    Then remove ads and restore purchases are available
    And any item or currency offer has a clear details/confirmation flow
    And shop offers do not interrupt starting a run

  @p1 @manual
  Scenario: Progression owns daily rewards, tasks, and milestones
    Given the player opens Progression
    Then daily rewards, tasks, and milestones are discoverable
    And claimable rewards are clearly marked
    And unclaimable rewards explain their requirement

  @p0 @manual @automatable
  Scenario: Settings control player comfort and privacy
    Given the player opens Settings
    Then the player can change music, SFX, haptics, graphics or comfort settings
    And the player can access privacy links and consent controls
    And changes persist after returning to the hub

  @p0 @manual
  Scenario: Destructive actions require confirmation
    Given the player chooses a destructive action such as resetting progress or quitting a run
    When the action is selected
    Then a confirmation is shown
    And cancelling the confirmation leaves the current state unchanged
