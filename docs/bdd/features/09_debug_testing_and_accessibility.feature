@qa @debug @accessibility @vertical-zero @vertical-release
Feature: Debug, testing, and accessibility support
  The project should be easy to validate repeatedly and safe to play across common mobile conditions.

  @p0 @manual
  Scenario: Debug build can reset profile and tutorial state
    Given the game is running in a development or debug build
    When the tester opens the support/debug panel
    Then the tester can reset profile progress
    And the tester can reset tutorial progress
    And the actions require confirmation

  @p0 @manual
  Scenario: Debug build can force obstacle and pickup scenarios
    Given the game is running in a development or debug build
    When the tester selects a force-spawn action
    Then the tester can spawn walls, fans, lasers, closing doors, coins, and each first-release powerup
    And the result is repeatable enough for manual acceptance testing

  @p0 @manual @automatable
  Scenario: Deterministic seed produces repeatable run patterns
    Given a deterministic test seed is selected
    When the tester starts a run
    Then the first section of obstacle and pickup patterns is repeatable
    And the same seed can be used for regression testing

  @p0 @manual
  Scenario: Small-screen HUD remains usable
    Given the game is running on a small supported phone aspect ratio
    When the player starts a run
    Then HUD values are readable
    And touch controls do not overlap critical buttons
    And pause remains reachable without accidental taps

  @p0 @manual
  Scenario: Safe area is respected
    Given the device has notches, rounded corners, or gesture areas
    When menus and HUD are shown
    Then important controls and text remain inside safe areas

  @p1 @manual
  Scenario: Haptics can be disabled
    Given haptics are enabled
    When the player disables haptics in Settings
    Then collision, pickup, and button haptics stop firing
    And the setting persists after restarting the game

  @p1 @manual
  Scenario: Reduced motion can be enabled
    Given intense effects are enabled by default
    When the player enables reduced motion or comfort mode
    Then camera shake, flashes, and extreme motion feedback are reduced
    And gameplay remains readable and testable

  @p1 @manual
  Scenario: Testers can capture a useful support snapshot
    Given the game is running in debug or support mode
    When the tester opens the support panel
    Then the panel shows profile summary, run state, build/version, active services, and recent error breadcrumbs
    And the data avoids exposing unnecessary personal information
