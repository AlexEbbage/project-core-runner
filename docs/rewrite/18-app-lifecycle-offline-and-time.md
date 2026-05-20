# App Lifecycle, Offline Handling and Time Authority

## Lifecycle handling

Use `AppLifecycleService` to respond to:

- OnApplicationPause
- OnApplicationFocus
- OnApplicationQuit

On background/quit:

- Flush local saves
- Pause audio
- Pause gameplay timers
- Record analytics session end where applicable
- Avoid granting rewards twice after ad/IAP interruption

## Offline behaviour

No internet should still allow:

- Starting and completing runs
- Local saves
- Local upgrades
- Local daily/task state with tamper checks

No internet should disable or defer:

- Ads
- IAP purchases
- Remote config refresh
- Cloud save if later added
- Analytics upload, using queued analytics instead

## Time authority

MVP uses `LocalDeviceTimeAuthority` plus `ClockTamperDetector`.

Later production can replace it with `TrustedServerTimeAuthority` backed by your backend or a trusted remote config timestamp.
