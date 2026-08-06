# Core Racer UI Visual System

## Locked direction

The approved direction combines the spacious hierarchy of the light concepts with the dark colour treatment of the later concepts. It should look like a polished casual mobile game, not a dense spaceship control panel.

## Principles

- Portrait-first and readable at a glance.
- Use whitespace and alignment before adding a container.
- Keep one clear primary action per screen.
- Use large type and touch targets.
- Keep the low-poly game visible during runs.
- Use subtle depth and border contrast; reserve glow for selected, claimable, success, and rare states.
- Avoid cards inside cards, thick sci-fi frames, ornamental corner cuts, tiny uppercase copy, and orange outlines on every surface.

## Palette roles

- Background: deep blue-black.
- Surface: quiet navy, with one raised level for interactive groups.
- Primary action: orange/red.
- Secondary/progression: blue/cyan.
- Credits: gold.
- Shards: bright cyan/blue.
- Rare core/reward: purple.
- Success: green.
- Error/destructive: red.
- Primary text: warm off-white.
- Supporting text: grey-blue.

## Screen structure

### Shared shell

- Profile summary at top-left with avatar, title, level and XP.
- Larger centred currency pills with icons.
- Cog settings shortcut.
- Large bottom navigation with icon + label.
- Locked states use reduced contrast, padlock, and tooltip.

### Play / Level Select

- `LEVEL SELECT` title.
- One playable MVP Core Run and one disabled next-zone preview only.
- Central emblem, high score, star rating and three reward states.
- Rewards: claimed with green confirmation, next reward highlighted, later reward locked.
- Three boosters with quantity badges and equipped/equip/buy states.
- Large `START` action.
- Carousel arrows hidden at their boundaries.

### Gameplay HUD

- Distance left, score centre, run currency and pause right.
- Thin zone/progress indicator.
- HUD floats without a giant frame.
- Powerups sit near the lower sides and disappear when inactive.
- Continue and Game Over use a clear bottom sheet that does not obscure the entire tunnel.

### Shop

- Featured offer and category tabs.
- Product grid/rows with restrained surfaces.
- Clear pending, owned, unavailable and purchase-failed states.

### Hangar

- Large 3D ship remains the hero.
- Compact carousel/thumbnail selection.
- Readable stat bars and two clear actions: equip and upgrade.

### Lab

- Broad section rows for booster, passive and core research upgrades.
- Progress and cost are aligned and scannable.
- Locked requirements are explicit.

### Progress

- Strong level/XP summary.
- Milestones, tasks and achievements are readable lists rather than a dashboard of unrelated cards.
- Claimable rewards are obvious without constant animation.

## Responsive targets

Optimise first for 9:16 and 9:19.5 portrait phones. Verify narrow phones, tall phones, tablet portrait, safe areas/cut-outs, and landscape gameplay where supported. Use deliberate wrap/reflow rules rather than uniformly scaling one fixed screenshot.

## Motion

Motion should explain hierarchy and reward interaction, not decorate every idle state. Screen transitions are short, bottom sheets feel direct, success/invalid feedback is restrained, and reduced-motion mode remains fully usable.
