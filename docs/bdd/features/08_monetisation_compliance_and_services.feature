@monetisation @services @vertical-release
Feature: Monetisation, compliance, and live services
  Ads, purchases, analytics, privacy, and offline states should support the game without breaking trust.

  Background:
    Given the player profile has loaded

  @p0 @manual
  Scenario: Rewarded continue grants continue only after successful ad completion
    Given the player has crashed with a rewarded continue available
    When the player accepts the continue offer
    And the rewarded ad completes successfully
    Then the run continues
    And the continue reward is recorded

  @p0 @manual
  Scenario: Failed rewarded continue does not grant the continue
    Given the player has crashed with a rewarded continue available
    When the rewarded ad fails, is unavailable, or is cancelled
    Then the run does not consume a successful continue reward
    And the player can return to the continue offer or proceed to Game Over according to configured rules
    And the UI explains what happened without blaming the player

  @p1 @manual
  Scenario: Reward doubling grants only after successful ad completion
    Given the Game Over screen shows a reward doubling option
    When the player watches the rewarded ad successfully
    Then the eligible run rewards are doubled once
    And the profile is saved with the doubled reward
    And the doubling action cannot be repeated for the same run

  @p1 @manual
  Scenario: Interstitials never interrupt the first tutorial run
    Given the player is in their first session
    When the player completes or fails the tutorial run
    Then no interstitial ad interrupts the tutorial flow

  @p1 @manual
  Scenario: Remove ads suppresses interstitial placements
    Given the player owns remove ads
    When the game reaches a normal interstitial placement
    Then no interstitial ad is shown
    And rewarded ads remain available for opt-in rewards unless disabled separately

  @p0 @manual
  Scenario: Restore purchases refreshes premium entitlement
    Given the player opens the Shop or Settings purchase area
    When the player taps Restore Purchases
    Then owned non-consumable purchases are restored when the platform confirms them
    And the UI shows the result clearly

  @p0 @manual
  Scenario: Privacy links and consent controls are available
    Given the player opens Settings
    When the player chooses Privacy or Data Controls
    Then privacy policy, terms, and tracking consent controls are accessible
    And analytics/ads services respect the saved consent state

  @p1 @manual @automatable
  Scenario: Key analytics events are captured
    Given analytics are enabled by consent and configuration
    When the player boots, starts a run, crashes, continues, ends a run, claims rewards, navigates pages, or purchases
    Then the game records events with stable names and non-sensitive payloads

  @p0 @manual
  Scenario: Offline play keeps the core run available
    Given the device is offline
    When the player opens the game with a valid local profile
    Then the player can reach the hub and play a run
    And unavailable online services degrade gracefully
    And no purchase, ad, or live-service failure blocks the core loop
