# Core Racer — Vertical Roadmap

## Product target

Core Racer is a mobile-friendly arcade runner where the player spins a low-poly mag craft around the inside of a glowing hex tunnel, dodges bold obstacles, collects hex coins and powers, chases a bright orange core, upgrades, customises, and runs again.

## Delivery philosophy

The current codebase has many systems scaffolded. The risk is not lack of architecture; the risk is that the game becomes broad but shallow.

The roadmap therefore prioritises:

1. playable feel,
2. clear obstacle readability,
3. rewards and progression,
4. final menus,
5. commercial/release services.

## Final vertical order

### Vertical 0 — Project Truth Stabilisation Gate

Purpose: prove the project opens, compiles, runs, and points at the correct truth.

This vertical exists because previous states had scene/config/doc drift.

Outputs:

- `CoreRacer_Main` is the only active build scene.
- no missing script references in active scene/config assets.
- package manifest is restored.
- BDD/menu/vertical docs are in project.
- first EditMode smoke tests exist.
- validation tools run without obvious project-truth failures.

This is mostly covered by Step 1, but remains the entry gate for future work.

---

### Vertical 1 — Core Run MVP

Purpose: make the game loop playable in its simplest form.

Player journey:

```text
Splash/Main Hub -> Play -> Run HUD -> dodge walls -> collect coins -> crash -> Game Over -> rewards -> retry/hub
```

Keep it deliberately small. This is the first "is the game fun?" check.

Must include:

- entering a run from the final menu flow,
- orbital movement around the tunnel,
- readable tunnel speed,
- wall obstacle patterns,
- coin collection,
- basic scoring/distance,
- player health/crash,
- run-end reward summary,
- retry and return-to-hub.

Do not include fans, lasers, shop, IAP, deep upgrades, achievements, or daily rewards here.

---

### Vertical 2 — Obstacle Identity

Purpose: establish the four first-release obstacle families as distinct readable gameplay.

Obstacle roster:

1. Walls
2. Fans
3. Lasers
4. Closing doors

Must include:

- each obstacle has a clear visual language,
- each obstacle has at least one safe solution,
- obstacle patterns escalate without becoming random noise,
- collisions are fair,
- debug spawn controls can force each obstacle family,
- early accessibility/readability check for colour, contrast, silhouette, and telegraphing.

This vertical should create the "Core Racer" identity.

---

### Vertical 3 — Pickups and Powerups

Purpose: add positive decisions and short-term excitement.

First-release pickup/powerup roster:

1. Hex coins
2. Magnet
3. Shield
4. Score multiplier
5. Coin multiplier
6. Pilot Assist / Rescue

Optional, only if they improve feel:

- Speed Boost
- Slow Motion
- Coin Bonanza

Must include:

- pickup lanes/patterns are readable,
- powerup timers/status appear on HUD,
- powerup effects have VFX/SFX feedback,
- upgrades can modify powerup value later,
- powerups do not trivialise obstacle skill.

---

### Vertical 4 — Run Feel, Art, Audio, and VFX

Purpose: make the core run look and feel like the intended game.

Visual target:

- glowing hex tunnel,
- soft ambient lighting,
- low-poly mag craft,
- bold red/orange/purple obstacle colours,
- hex coins,
- bright orange core at the end of the tunnel,
- speed lines/tunnel motion,
- readable depth and silhouettes.

Audio target:

- run music loop,
- coin pickup,
- powerup pickup,
- shield hit,
- crash,
- obstacle warning cues,
- UI confirm/back,
- reward reveal.

VFX target:

- coin sparkle,
- magnet stream,
- shield bubble,
- laser glow,
- door warning pulse,
- fan wind,
- crash burst,
- reward sparkle,
- ship trail.

This vertical should not expand menu scope. It should polish the actual run.

---

### Vertical 5 — Final Menus and Meta Loop

Purpose: wire the locked final menu set into a coherent player journey.

Final menu set:

- Splash / Bootstrap
- Main Hub
- Play
- Hangar
- Lab
- Shop
- Progression
- Settings
- Privacy / Consent
- Run HUD
- Pause
- Crash / Continue Offer
- Game Over

Bottom nav:

```text
Play | Hangar | Lab | Shop | Progression
```

Settings remains a gear/profile action, not a bottom-nav item.

Must include:

- consistent navigation,
- back behaviour,
- modal stacking rules,
- disabled/locked states,
- clear currencies,
- no fake "recommended" panels,
- no dead buttons.

---

### Vertical 6 — Progression, Economy, and Retention

Purpose: make repeated runs meaningful.

Must include:

- run rewards,
- wallet persistence,
- XP/level track,
- upgrade costs,
- upgrade effects,
- cosmetic unlocks,
- daily reward inside Progression,
- tasks inside Progression,
- achievements inside Progression if they genuinely support retention,
- economy tuning pass.

This vertical should answer: "Why play another run?"

---

### Vertical 7 — Commercial Services and Compliance

Purpose: integrate monetisation and platform services safely.

Must include:

- rewarded continue,
- rewarded double rewards,
- remove ads,
- restore purchases,
- purchase result UI,
- privacy/consent entry points,
- analytics event contracts,
- crash/error reporting hook,
- no forced ad dependency for core play.

This vertical should not be started until the core run and reward loop are fun.

---

### Vertical 8 — Closed Testing Hardening

Purpose: prepare for Google Play closed testing and real device QA.

Must include:

- Android build validation,
- device performance checks,
- screen-size checks,
- FTUE completion,
- save/load resilience,
- offline behaviour,
- privacy/compliance smoke checks,
- tester feedback checklist,
- release candidate checklist.

This vertical produces the first serious external test build.
