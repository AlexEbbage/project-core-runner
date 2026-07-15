# Google Play Closed Testing Gate — Core Racer

## Upload gate

Do not upload to Google Play closed testing until all blockers in this list are complete.

## Unity project gates

```text
CoreRacer_Main is the only enabled build scene
Android build target selected
package ID is production-ready
bundle version is intentional
Android version code incremented
no missing script references in active project files
EditMode tests pass
one Android build installs on a physical device
```

## Store/compliance gates

```text
real privacy policy URL
real terms URL
real data deletion URL
content rating questionnaire prepared
ads declaration prepared
IAP declaration prepared if Remove Ads is live
Data safety form prepared
target audience/children declaration prepared
app access instructions prepared if required
```

## Monetisation gates

```text
rewarded ads do not grant on cancel/not-ready/failure
premium/remove-ads entitlement persists
restore purchases can complete safely
unknown IAP product IDs do not unlock premium
ad frequency does not block first-run experience
```

## Testing gates

```text
closed-testing smoke plan completed
first launch passes
first run loop passes
retry and hub return pass
all obstacle families are readable
all first-release powerups work
progression saves after relaunch
settings/privacy links work
```

## Asset/polish gates

```text
app icon present
feature graphic ready or scheduled
screenshots ready or scheduled
store description draft ready
support email selected
final app name selected
```

## Recommended closed-test notes

Use a small tester group first. Ask testers to focus on:

```text
Can they understand the controls in 30 seconds?
Can they tell what each obstacle does?
Does the run feel fair when they crash?
Do coins and upgrades make them want another run?
Any device-specific crashes or UI scaling issues?
```
