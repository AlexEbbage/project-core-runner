# Core Racer — Vertical Slice Contracts

Each vertical has:

- goal,
- player-facing acceptance criteria,
- implementation boundaries,
- test focus,
- assets required,
- exit gate.

## Vertical 0 — Project Truth Stabilisation Gate

### Goal

Make the project safe to work on.

### Acceptance criteria

- Unity opens the project without package restore errors.
- `CoreRacer_Main` is the only build scene.
- No active scene/config assets contain missing script references.
- Project docs describe `Assets/CoreRacer`, not the old `Assets/Scripts` runtime.
- First smoke tests exist and run.
- Validation tooling can be run from Unity.

### Implementation boundary

Do not build gameplay here unless needed to fix compile/runtime blockers.

### Exit gate

The project can be opened, entered into Play Mode, and the main scene can be loaded.

---

## Vertical 1 — Core Run MVP

### Goal

Produce the smallest fun run loop.

### Player-facing acceptance criteria

- From the hub, tapping Play starts a run.
- The camera enters a hex tunnel.
- The player controls a mag craft orbiting around the tunnel.
- Wall obstacles spawn ahead and can be avoided.
- Hex coins spawn in readable paths and can be collected.
- HUD shows distance, score, coins, health/shield placeholder, and pause.
- Hitting a wall damages or crashes the craft.
- Game Over shows distance, coins, score, and buttons for Retry and Hub.
- Retry starts a fresh run without reloading the whole app.

### Implementation boundary

Allowed:

- movement,
- tunnel runtime,
- wall patterns,
- coins,
- HUD,
- crash/game-over.

Not allowed:

- fan/laser/door work beyond placeholders,
- IAP,
- daily reward,
- tasks,
- achievements,
- cosmetic deep UI.

### Test focus

- EditMode tests for score/reward calculations.
- PlayMode smoke test for Hub -> Run -> Crash -> GameOver.
- Manual test for movement feel.

### Exit gate

A tester can play 10 one-minute runs and understand the game without explanation.

---

## Vertical 2 — Obstacle Identity

### Goal

Implement the four first-release obstacle families.

### Player-facing acceptance criteria

- Walls block part of the tunnel and have obvious safe gaps.
- Fans create a visible rotating/spinning hazard or force zone.
- Lasers are readable before they become dangerous.
- Closing doors telegraph before closing.
- Every obstacle can be avoided using skill, not guesswork.
- Difficulty ramps over time through pattern density, timing, and combinations.
- Collision feels fair at normal game speed.
- Debug controls can force-spawn each obstacle type for QA.

### Implementation boundary

Allowed:

- obstacle enum/data model changes,
- pattern generation,
- telegraphs,
- collision volumes,
- debug spawn menu,
- obstacle VFX/SFX hooks.

Not allowed:

- shop monetisation,
- long-term economy expansion.

### Test focus

- deterministic obstacle pattern tests,
- collision fairness checks,
- manual readability pass.

### Exit gate

A player can name each obstacle type after one run and understands how to respond to each.

---

## Vertical 3 — Pickups and Powerups

### Goal

Make collection and temporary advantages satisfying.

### Player-facing acceptance criteria

- Coins are easy to recognise and satisfying to collect.
- Magnet pulls nearby coins visibly.
- Shield protects from one or more impacts depending on tuning.
- Score multiplier increases score while active.
- Coin multiplier increases coin reward while active.
- Pilot Assist / Rescue helps recover from a mistake without playing the whole game for the user.
- HUD shows active powerup icons/timers.
- Powerups expire clearly.
- Powerups are weighted so runs do not become chaotic.

### Implementation boundary

Allowed:

- powerup runtime,
- pickup spawning,
- HUD timers,
- VFX/SFX feedback,
- initial upgrade hooks.

Not allowed:

- full monetisation,
- complex challenge/task systems.

### Test focus

- pure service tests for timers/effects,
- collection collision tests,
- manual balancing pass.

### Exit gate

Powerups make the run more exciting without making dodging irrelevant.

---

## Vertical 4 — Run Feel, Art, Audio, and VFX

### Goal

Make the core run match the intended identity.

### Player-facing acceptance criteria

- The tunnel is recognisably hexagonal.
- The end of the tunnel has a bright orange core/goal focal point.
- The mag craft is low-poly and readable.
- Obstacles use bold danger colours and silhouettes.
- Coins and powerups are visually distinct.
- Movement feels responsive and not floaty.
- Crash feedback is satisfying.
- Run audio supports speed, collection, danger, and reward.
- Visual effects support readability instead of hiding gameplay.

### Implementation boundary

Allowed:

- materials,
- shaders,
- lighting,
- VFX,
- particle systems,
- audio clips/mixers,
- camera shake,
- post-processing if performant.

Not allowed:

- adding new gameplay systems without BDD decision.

### Test focus

- performance on target mobile/desktop profile,
- readability at small screen size,
- colour-blind/contrast sanity pass,
- audio mix pass.

### Exit gate

A screenshot or 10-second capture clearly communicates the game.

---

## Vertical 5 — Final Menus and Meta Loop

### Goal

Make the final menu set functional and coherent.

### Player-facing acceptance criteria

- Splash/Bootstrap leads to Main Hub.
- Bottom nav includes Play, Hangar, Lab, Shop, Progression.
- Settings is available from a top-right gear/profile action.
- Play starts the run.
- Hangar previews/equips ships, skins, trails, and core FX.
- Lab shows gameplay upgrades and clearly communicates costs/effects.
- Shop shows commercial offers and restore purchases.
- Progression contains daily rewards, tasks, achievements/milestones, and level rewards where retained.
- Pause lets the player resume, restart, change settings, or exit to hub.
- Game Over connects back into rewards, upgrades, retry, and hub.
- No first-release menu has dead placeholder buttons.

### Implementation boundary

Allowed:

- screen routing,
- modal manager,
- menu controllers,
- menu data binding,
- disabled/locked states,
- UI polish for existing menu scope.

Not allowed:

- adding extra first-release menu destinations.

### Test focus

- navigation matrix tests,
- modal stacking tests,
- save/load across menu changes,
- manual "no dead buttons" check.

### Exit gate

Every visible button either works or is intentionally disabled with clear copy.

---

## Vertical 6 — Progression, Economy, and Retention

### Goal

Give players a reason to keep running.

### Player-facing acceptance criteria

- Run results grant coins/XP correctly.
- Wallet persists between sessions.
- Lab upgrades have meaningful effects and escalating costs.
- Unlocks are visible before and after unlocking.
- Daily rewards are claimable once per valid period.
- Tasks give short-term goals.
- Achievements/milestones reward longer-term progress if retained.
- Economy feels fair without requiring ads/IAP.

### Implementation boundary

Allowed:

- reward formulas,
- upgrade formulas,
- profile persistence,
- unlock data,
- retention surfaces.

Not allowed:

- making monetisation mandatory for normal progress.

### Test focus

- deterministic economy tests,
- profile migration tests,
- clock/time edge cases,
- save corruption fallback.

### Exit gate

A tester can play several runs, buy upgrades, and feel a measurable difference.

---

## Vertical 7 — Commercial Services and Compliance

### Goal

Add monetisation safely after the game loop works.

### Player-facing acceptance criteria

- Rewarded continue is optional and only appears at appropriate crash/game-over moments.
- Rewarded double rewards is optional and accurately doubles eligible rewards.
- Remove Ads purchase is clear and persistent.
- Restore Purchases is available in Shop/Settings.
- Privacy/consent is accessible.
- Failed purchases/ads produce clear non-blocking messages.
- Core play remains available when ads/IAP are unavailable.

### Implementation boundary

Allowed:

- ads service adapters,
- IAP service adapters,
- analytics events,
- consent/privacy screens,
- store-facing product IDs.

Not allowed:

- blocking the run behind ads or purchase.

### Test focus

- fake service tests,
- error-state tests,
- no-network tests,
- purchase restoration manual checks.

### Exit gate

Monetisation can fail gracefully without breaking the game.

---

## Vertical 8 — Closed Testing Hardening

### Goal

Prepare a real build for testers.

### Player-facing acceptance criteria

- App installs and launches.
- FTUE teaches movement, coins, obstacles, powerups, rewards, and upgrades.
- Main flow survives app pause/resume.
- Saves survive restart.
- UI fits target screens.
- Performance remains acceptable during busy runs.
- Crash/error reports are available.
- Tester feedback has a clear checklist.

### Implementation boundary

Allowed:

- polish,
- bug fixes,
- build settings,
- QA tooling,
- test coverage,
- performance optimisation.

Not allowed:

- new major first-release features.

### Exit gate

A build can be submitted to closed testing with known limitations documented.
