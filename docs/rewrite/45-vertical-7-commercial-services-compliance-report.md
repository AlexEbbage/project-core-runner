# Rewrite Report 45 — Vertical 7 Commercial Services and Compliance

## Summary

Vertical 7 wires the commercial/compliance layer to the first-release product contract.

## Runtime changes

- Added commercial readiness models and rules.
- Hardened rewarded ad outcome handling.
- Added ad/IAP analytics hooks to ad controllers and IAP facade.
- Added premium-aware shop purchase handling.
- Added purchase/restore completion events.
- Extended privacy settings to include data deletion and consent toggles.
- Registered `CommercialReadinessService` in `GameBootstrapper`.

## Editor tooling

Added:

```text
Tools/Core Racer/Vertical 7/Apply Commercial Services Compliance
Tools/Core Racer/Vertical 7/Validate Commercial Services Compliance
```

## Test coverage

Added EditMode tests covering:

- premium rewarded bypass grant rule
- missing rewarded provider no-grant rule
- premium product entitlement grant
- unknown product protection
- placeholder policy URL rejection
- commercial readiness snapshot

## Known blocker

`PrivacyLinks.asset` still uses placeholder `example.com` URLs in the uploaded project. This is intentionally flagged by the validator and must be replaced before closed testing.
