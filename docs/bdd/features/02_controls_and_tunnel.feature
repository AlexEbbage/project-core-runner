@controls @tunnel @vertical-core-run @vertical-feel
Feature: Ship control and tunnel readability
  The player should feel locked into a fast tunnel run with responsive orbital movement and readable safe routes.

  Background:
    Given a run has started
    And the countdown has completed

  @p0 @manual @automatable
  Scenario: Player steers clockwise around the tunnel
    Given the ship is travelling through the tunnel
    When the player drags or presses clockwise input
    Then the ship rotates clockwise around the inside of the tunnel
    And the ship remains attached to the playable orbital path
    And the camera keeps the ship and upcoming hazards readable

  @p0 @manual @automatable
  Scenario: Player steers anti-clockwise around the tunnel
    Given the ship is travelling through the tunnel
    When the player drags or presses anti-clockwise input
    Then the ship rotates anti-clockwise around the inside of the tunnel
    And the ship remains attached to the playable orbital path

  @p0 @manual
  Scenario: Movement feels responsive without becoming twitchy
    Given the player alternates clockwise and anti-clockwise input quickly
    When the ship changes direction
    Then the ship responds immediately enough to dodge hazards
    And the motion still has enough smoothing to feel premium rather than jittery

  @p0 @manual @automatable
  Scenario: Input is ignored when the run is not active
    Given the run is in countdown, paused, crashed, continue-offer, or game-over state
    When the player provides movement input
    Then the ship does not move because of that input
    And the run state remains unchanged

  @p0 @manual
  Scenario: The tunnel communicates safe and dangerous space
    Given an obstacle pattern is visible ahead
    When the player looks down the tunnel
    Then dangerous segments are visibly distinct from safe segments
    And the safe route can be understood before the ship reaches the obstacle
    And pickups do not obscure the hazard silhouette

  @p0 @manual
  Scenario: The orange core gives a forward goal
    Given the player is running through the tunnel
    Then a bright orange core or energy focus is visible deeper in the tunnel
    And it reinforces the sense of speed and direction
    And it does not hide hazards or pickups

  @p1 @manual
  Scenario: Camera and VFX scale with speed
    Given the run speed increases over time
    When the player reaches a faster difficulty band
    Then speed lines, particles, field-of-view, tunnel lighting, or audio intensity increase
    And readability remains more important than spectacle

  @p1 @manual
  Scenario: Comfort settings reduce intense motion
    Given reduced motion or comfort mode is enabled
    When the player starts a run
    Then camera shake, flashes, and intense motion effects are reduced
    And hazard readability is preserved
