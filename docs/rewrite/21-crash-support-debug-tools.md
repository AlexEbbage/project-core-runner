# Crash Reporting and Support Tools

## Included seams

- `ICrashReportingService`
- `DebugCrashReportingService`
- `FirebaseCrashlyticsAdapter`
- `SupportBundleExporter`
- `SupportDebugPanel`

## Support bundle should include

- App version
- Device model
- OS version
- Session id
- Player id
- Save presence/version
- Consent state
- Recent breadcrumbs
- Recent economy ledger entries
- Ad/IAP availability state
- Remote config/balance version

## Manual SDK work

Install Firebase Crashlytics, define `CORE_RACER_FIREBASE_CRASHLYTICS`, and replace guarded adapter sections with the exact package API calls.
