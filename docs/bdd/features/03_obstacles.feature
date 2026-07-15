@obstacles @vertical-obstacles
Feature: First-release obstacle roster
  Obstacles should be readable, fair, varied, and visually bold.

  Background:
    Given a run has started
    And the player is in a running state

  @p0 @manual @automatable
  Scenario: Wall obstacle leaves a readable safe route
    Given a wall obstacle pattern is spawned
    When the pattern enters the player's preview distance
    Then at least one safe opening is visible
    And the blocked tunnel segments are clearly marked as hazardous
    And the player can avoid the wall using normal movement at the current speed

  @p0 @manual
  Scenario: Fan obstacle communicates its danger before contact
    Given a fan obstacle pattern is spawned
    When the fan becomes visible
    Then the blades, wind, rotation, or danger area are readable before collision range
    And the player can identify where to move to avoid damage
    And fan motion does not create an unavoidable instant hit

  @p0 @manual
  Scenario: Laser obstacle telegraphs before becoming fatal
    Given a laser obstacle pattern is spawned
    When the laser enters preview distance
    Then the laser shows a warning, charge-up, or beam path before it can damage the player
    And the active beam is visually distinct from the warning state
    And the player has enough time to avoid it using normal movement

  @p0 @manual
  Scenario: Closing door gives fair warning and a timed opening
    Given a closing door obstacle pattern is spawned
    When the door enters preview distance
    Then the open gap and closing motion are visible
    And the player can tell whether to pass through or move around it
    And the door does not close instantly without warning

  @p0 @manual @automatable
  Scenario: Consecutive obstacle patterns remain fair
    Given the obstacle generator is producing patterns for the current difficulty band
    When several patterns are spawned in sequence
    Then the combined path never requires impossible movement
    And the player is not forced from one side of the tunnel to the opposite side faster than allowed by movement tuning
    And each pattern has a readable safe solution

  @p0 @manual
  Scenario: Difficulty ramps by speed and pattern pressure
    Given the run continues for increasing distance or time
    When the player reaches a higher difficulty band
    Then obstacles become faster, denser, more varied, or more demanding
    And the early run remains approachable for a first-time player

  @p1 @manual
  Scenario: Mixed obstacle patterns create variety without noise
    Given the player has survived long enough for mixed patterns
    When two obstacle families are combined
    Then their visual languages remain distinguishable
    And there is still one intended safe solution
    And coins or powerups are not placed in guaranteed-death positions

  @p0 @manual
  Scenario: Debug tools can force each obstacle family
    Given the game is running in a development or debug build
    When the tester chooses an obstacle family from the debug tools
    Then that obstacle can be spawned on demand
    And the tester can repeat it without waiting for random generation
