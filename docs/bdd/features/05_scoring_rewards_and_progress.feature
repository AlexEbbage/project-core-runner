@scoring @rewards @progression @vertical-progression
Feature: Scoring, rewards, and progression
  The player should understand what they earned during a run and why the next run can be better.

  Background:
    Given the player profile has loaded successfully

  @p0 @manual @automatable
  Scenario: Run score starts from zero
    Given the player starts a new run
    Then score starts at zero
    And run coins start at zero
    And combo starts at its base value
    And no previous run state affects the new run score

  @p0 @manual @automatable
  Scenario: Distance increases score during active running
    Given the player is in a running state
    When the ship survives forward movement through the tunnel
    Then score increases over time or distance
    And score does not increase while paused, crashed, in countdown, or on game over

  @p0 @manual @automatable
  Scenario: Collecting coins updates score and combo feedback
    Given the player is in a running state
    When the player collects coins in sequence
    Then run coins increase
    And score increases by the configured pickup value
    And combo feedback becomes more rewarding while the streak is maintained

  @p0 @manual @automatable
  Scenario: Combo decays or resets when the player breaks the streak
    Given the player has built a combo
    When the combo window expires or the player takes damage according to tuning
    Then the combo returns toward its base value
    And the HUD communicates the change without feeling punitive or confusing

  @p0 @manual @automatable
  Scenario: Best score updates only when a completed run beats it
    Given the player has an existing best score
    When a completed run ends with a higher score
    Then the best score is updated
    And the Game Over screen marks it as a new best

  @p0 @manual @automatable
  Scenario: Lower score does not replace best score
    Given the player has an existing best score
    When a completed run ends with a lower score
    Then the best score remains unchanged
    And the Game Over screen still shows both final score and best score

  @p0 @manual @automatable
  Scenario: Completed run grants soft currency and XP
    Given the player completes a run through crash or normal game-over flow
    When rewards are granted
    Then the player receives soft currency based on collected coins and configured bonuses
    And the player receives XP based on the configured run reward rules
    And the profile is saved before returning to the hub

  @p0 @manual @automatable
  Scenario: Player can spend coins on a useful Lab upgrade
    Given the player has enough soft currency for a Lab upgrade
    When the player purchases the upgrade
    Then the currency cost is deducted
    And the upgrade level increases
    And the next run reflects the upgraded effect
    And the top bar refreshes immediately

  @p0 @manual @automatable
  Scenario: Upgrade purchase is blocked when currency is insufficient
    Given the player does not have enough soft currency for a Lab upgrade
    When the player tries to purchase the upgrade
    Then the upgrade is not applied
    And no currency is deducted
    And the UI explains that more currency is needed

  @p1 @manual
  Scenario: Daily reward can be claimed once per day
    Given the player has an available daily reward
    When the player claims it
    Then the reward is granted once
    And the next claim is locked until the next eligible day
    And the Progression page shows the updated streak or calendar state

  @p1 @manual
  Scenario: Tasks progress from real run outcomes
    Given the player has active daily tasks
    When the player completes a run with matching actions
    Then relevant tasks gain progress
    And completed tasks become claimable
    And claimed task rewards update the profile

  @p1 @manual
  Scenario: Level-up feedback appears after earning enough XP
    Given the player is close to the next profile level
    When a completed run grants enough XP to level up
    Then the Game Over or Hub flow communicates the level-up
    And any newly unlocked menu or item state is refreshed
