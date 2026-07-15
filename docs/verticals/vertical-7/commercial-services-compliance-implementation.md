# Vertical 7 — Commercial Services and Compliance Implementation

## Goal

Complete the first production-facing commercial layer without changing the core run, menus, or economy loop.

The target loop is:

```text
Shop -> Remove Ads / Restore Purchases
Run crash -> Continue Offer
Game Over -> Double Reward
Settings -> Privacy / Consent / Data controls
```

## Implemented behaviours

### Rewarded ad policy

Rewarded ad results now have a clear grant rule:

```text
Rewarded             -> grant reward
BypassedByPremium    -> grant reward where policy allows premium bypass
NotReady             -> no reward
FailedToShow         -> no reward
ClosedBeforeReward   -> no reward
```

This prevents accidental reward grants when a provider is missing or not ready.

### Premium policy

Premium remains policy-led:

- Continue Run: bypassed by premium and grants continue.
- Double Run Rewards: bypassed by premium and grants doubled reward.
- Interstitial: bypassed by premium.
- Daily Login Double Reward: not bypassed by premium.
- Mid Run Rewarded Offer: not bypassed by premium.

### IAP facade

`IapPurchaseService` now owns:

- purchase request event
- restore request event
- purchase completion event
- restore completion event
- premium grant for known premium product only
- unknown product protection
- failed/cancelled/not-initialised outcomes

Unity IAP remains behind `UnityPurchasingAdapter` so the product implementation can swap SDKs later if needed.

### Shop UI

The shop refreshes when:

- premium changes
- purchase completes
- restore completes

Premium items become unavailable once premium is owned.

### Privacy and consent

`PrivacySettingsController` now supports:

- Privacy Policy link
- Terms link
- Data Deletion link
- delete local progress
- grant/deny analytics consent
- grant/deny personalised ads consent
- visible consent status text

### Commercial readiness

`CommercialReadinessService` and `CommercialComplianceRules` provide a simple readiness snapshot for validation and future debug/support UI.

The validation intentionally rejects `example.com` and `localhost` URLs.

## Acceptance coverage

| BDD area | Status |
|---|---|
| Remove Ads / premium entitlement | Implemented |
| Restore purchases | Implemented facade + UI state hooks |
| Rewarded continue policy | Implemented |
| Rewarded double-reward policy | Implemented |
| Interstitial premium bypass | Implemented |
| Privacy policy / terms / data deletion links | Implemented UI hooks |
| Consent state controls | Implemented |
| Closed-testing readiness validation | Implemented editor validator |

## Follow-up before closed testing

Replace `Assets/CoreRacer/Generated/Configs/PrivacyLinks.asset` URLs with production-hosted pages:

```text
PrivacyPolicyUrl
TermsUrl
DataDeletionUrl
```

Then rerun:

```text
Tools/Core Racer/Vertical 7/Validate Commercial Services Compliance
```
