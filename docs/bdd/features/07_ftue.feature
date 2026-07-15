@ftue @tutorial @vertical-core-run
Feature: First-time user experience
  The first session should teach the core loop through deterministic, playable moments.

  Background:
    Given the player is using a new profile
    And the tutorial has not been completed

  @p0 @manual
  Scenario: First session welcomes the player without delaying Play too long
    Given the player reaches the Main Hub for the first time
    Then the tutorial introduces the goal briefly
    And the player can start the first run quickly
    And the tutorial does not open every menu before the player has played

  @p0 @manual @automatable
  Scenario: Tutorial teaches movement with a safe prompt
    Given the first tutorial run has started
    When the movement teaching step begins
    Then the player is prompted to move clockwise and anti-clockwise
    And early hazards are delayed or simplified until movement is demonstrated
    And progress is saved after the movement step completes

  @p0 @manual
  Scenario: Tutorial teaches dodging with a deterministic wall
    Given the movement step is complete
    When the dodge teaching step begins
    Then a simple wall obstacle appears with one obvious safe opening
    And the player can pass it using the taught movement
    And failing the step allows recovery or retry without ending the tutorial unfairly

  @p0 @manual
  Scenario: Tutorial teaches coin collection with a safe coin trail
    Given the dodge step is complete
    When the coin teaching step begins
    Then a short hex coin trail appears in a safe path
    And collecting the coins updates the HUD
    And the tutorial acknowledges the reward

  @p0 @manual
  Scenario: Tutorial teaches one powerup
    Given the coin step is complete
    When the powerup teaching step begins
    Then a readable powerup pickup appears in a fair position
    And collecting it activates the corresponding HUD state
    And the player sees the effect clearly before normal difficulty resumes

  @p0 @manual
  Scenario: Tutorial explains crash and continue without forcing payment
    Given the tutorial run reaches the crash/continue explanation
    When the player crashes or reaches a scripted explanation point
    Then the game explains continue and game-over flow clearly
    And the player is not forced to watch an ad to complete tutorial progression

  @p1 @manual
  Scenario: Tutorial points to the first useful upgrade
    Given the tutorial run has ended and rewards were granted
    When the player returns to the hub
    Then the tutorial highlights one useful Lab upgrade or next action
    And the player can continue to Play without being trapped in a menu tour

  @p0 @manual @automatable
  Scenario: Completed tutorial is not repeated automatically
    Given the player has completed the tutorial
    When the game is restarted
    Then the tutorial does not automatically restart
    And a debug or support action can reset it only in allowed contexts
