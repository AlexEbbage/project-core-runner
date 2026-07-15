@core-run @vertical-core-run
Feature: Core tunnel run
  The player should be able to start a run, fly through the tunnel, crash, continue if eligible, see rewards, and play again.

  Background:
    Given the project launch scene is "CoreRacer_Main"
    And the player profile has loaded successfully
    And the player has a selected starter ship

  @p0 @manual @automatable
  Scenario: Start a run from the main hub
    Given the player is on the Main Hub
    When the player taps the primary Play action
    Then the game transitions to the run view
    And a short countdown is shown before hazards become active
    And the HUD shows score, coins, combo, distance or speed, health, active powerups, and pause

  @p0 @manual @automatable
  Scenario: Run begins after countdown
    Given the run countdown is visible
    When the countdown completes
    Then the ship begins moving forward automatically through the hex tunnel
    And obstacle and coin patterns spawn ahead of the player
    And the score begins increasing with distance survived

  @p0 @manual @automatable
  Scenario: Pause and resume during a run
    Given the player is in a running state
    When the player opens Pause
    Then run movement, spawning, scoring, timers, and collisions are paused
    And the player can resume the same run

  @p0 @manual @automatable
  Scenario: Quit from pause returns to the hub without granting run rewards
    Given the player is in a running state
    And the player has collected coins during the run
    When the player opens Pause
    And confirms quit to menu
    Then the run ends as abandoned
    And unbanked run rewards are not granted as a completed run
    And the player returns to the Main Hub

  @p0 @manual @automatable
  Scenario: Fatal collision enters crash state
    Given the player is in a running state
    And the ship has no active protection that can prevent the hit
    When the ship collides with a fatal hazard
    Then forward run progression stops immediately
    And the crash feedback makes the cause of failure obvious
    And the run enters either Continue Offer or Game Over based on eligibility

  @p0 @manual
  Scenario: Eligible player can continue after a crash
    Given the player has crashed
    And rewarded continue is available
    And the run has remaining continues
    When the player accepts the rewarded continue offer
    And the rewarded ad completes successfully
    Then the ship respawns safely beyond or before the crash hazard according to the configured continue rule
    And the run resumes with a short grace period
    And the continue count is consumed

  @p0 @manual @automatable
  Scenario: Player skips continue and sees game over
    Given the player has crashed
    And a continue offer is visible
    When the player chooses not to continue
    Then the Game Over screen appears
    And the final score, distance, coins, XP, best score, and run duration are shown
    And the player can choose Replay or Main Hub

  @p0 @manual @automatable
  Scenario: Restart from game over starts a fresh run
    Given the Game Over screen is visible
    When the player taps Replay
    Then a new run starts from the countdown
    And score, run coins, combo, active powerups, and damage state are reset
    And permanent profile rewards from the previous completed run remain saved

  @p0 @manual @automatable
  Scenario: Return to hub from game over refreshes profile UI
    Given the Game Over screen is visible
    And the run has granted coins and XP
    When the player taps Main Hub
    Then the Main Hub appears
    And the top bar shows the updated currencies, XP, and level state
    And any newly claimable progression entry is indicated without blocking Play
