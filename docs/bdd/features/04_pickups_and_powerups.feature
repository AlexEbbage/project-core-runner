@pickups @powerups @vertical-powerups
Feature: Pickups and powerups
  Coins and powerups should reward risk, improve moment-to-moment play, and remain easy to understand.

  Background:
    Given a run has started
    And the player is in a running state

  @p0 @manual @automatable
  Scenario: Player collects a hex coin
    Given a hex coin is placed on the player's path
    When the ship intersects the coin pickup area
    Then the coin is collected once
    And the run coin count increases immediately
    And the pickup feedback confirms the collection

  @p0 @manual @automatable
  Scenario: Coin cannot be collected twice
    Given the player has collected a coin
    When the ship remains inside or re-enters the previous pickup area
    Then the same coin is not collected again
    And the run coin count does not duplicate

  @p0 @manual
  Scenario: Coin trails guide but do not trick the player
    Given a coin trail is visible ahead
    When the coin trail passes near an obstacle
    Then the trail suggests a useful route or risk-reward choice
    And it does not lead the player into unavoidable damage

  @p0 @manual @automatable
  Scenario: Magnet attracts nearby coins
    Given the Magnet powerup is active
    And coins are within the magnet radius
    When the player moves near those coins
    Then the coins move toward the ship
    And collected coins still count only once
    And obstacles and hazards are not affected by the magnet

  @p0 @manual @automatable
  Scenario: Shield prevents one fatal hit
    Given the Shield powerup is active
    When the ship collides with a fatal hazard
    Then the shield absorbs the hit
    And the ship does not enter crash state for that hit
    And the shield feedback makes the save obvious
    And the shield charge or active state is consumed according to tuning

  @p1 @manual @automatable
  Scenario: Score multiplier increases score while active
    Given the Score Multiplier powerup is active
    When the player earns distance score or pickup score
    Then the added score is multiplied by the active score multiplier
    And the HUD shows the remaining active duration or state
    And the multiplier stops applying when the powerup expires

  @p1 @manual @automatable
  Scenario: Coin multiplier increases banked coin value while active
    Given the Coin Multiplier powerup is active
    When the player collects a coin
    Then the run reward value for that coin is multiplied by the active coin multiplier
    And the HUD shows the remaining active duration or state
    And the multiplier stops applying when the powerup expires

  @p1 @manual
  Scenario: Pilot Assist or Rescue saves one mistake clearly
    Given the Pilot Assist or Rescue powerup is active
    When the player would otherwise collide with a fatal hazard
    Then the assist prevents or corrects the mistake according to its tuning
    And the feedback makes clear that the assist was consumed
    And the player regains control quickly

  @p0 @manual
  Scenario: Powerup pickups are rare and readable
    Given a powerup pickup appears in the tunnel
    When the player sees it ahead
    Then its pickup type is visually distinct from coins and hazards
    And the player has a fair opportunity to collect it
    And it does not hide the safe route through nearby hazards

  @p1 @manual
  Scenario: Active powerup states are visible on the HUD
    Given one or more timed powerups are active
    When the player looks at the HUD
    Then each active powerup is shown with icon and duration or charge state
    And expired powerups are removed from the active strip
