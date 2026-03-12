# Style Guide

## Visual Direction

- Pillars: Premium sci-fi readability, glowing tunnel speed, clean mobile UI, high-contrast hazards, restrained futuristic polish
- Mood: Fast, luminous, precise, and slightly dangerous rather than gritty or industrial
- Shape language:
  - Hexagons, angled ship silhouettes, wedge hazards, beveled cards, segmented rails
  - Clean geometric forms over noisy decorative detail
- Color approach:
  - Bright white and frosted translucency for UI surfaces
  - Orange-red glow for danger, premium CTA emphasis, and fail states
  - Cool cyan/blue accents for ship thrust, selected premium currency, and player tech identity
  - Soft violet haze and atmospheric bloom in backgrounds, but not as the primary UI brand color

## World and Gameplay Readability

- The tunnel should read as a bright hex corridor with depth and forward pull.
- Hazards should be immediately legible through silhouette and glow before the player reaches them.
- Speed should be sold with tunnel lighting, particles, motion streaks, and tight camera feel rather than camera chaos.
- Pickups and powerups should remain readable at high speed and not blend into hazard colors.

## UI Direction

- Information hierarchy:
  - Top bar: profile level, XP progress, soft currency, premium currency
  - Bottom nav: primary hub destinations
  - Main content area: one dominant panel or content stack per page
  - CTAs: strongest contrast reserved for primary action or monetisation prompt
- HUD principles:
  - Keep score, combo, and active powerups readable in motion
  - Avoid cluttering the center play lane
  - Reward prompts must feel additive, not like surprise hard stops
- Menu principles:
  - Portrait-first layouts
  - White/frosted panel cards with subtle hex texture or light noise
  - Premium buttons use warm glow borders and stronger saturation
  - Secondary buttons stay low-noise and cooler in tone
- Accessibility notes:
  - Do not rely on color alone to distinguish currencies, states, or actionable buttons
  - Maintain strong text contrast over bloom-heavy backgrounds
  - Use consistent iconography for soft currency, premium currency, ads, settings, and progression

## UI Stack

### Current Baseline

- UGUI (`Canvas`, `RectTransform`, layout groups, `ScrollRect`)
- TextMeshPro for all production text
- Unity Input System for gameplay and UI input
- Unity IAP for commerce
- Unity LevelPlay for rewarded ads

### Target Additions

- DOTween is the preferred future UI animation layer for panel transitions, button feedback, and reward emphasis.
- Additional layout/effects tooling may be adopted later if it clearly improves mobile responsiveness or visual quality.
- These target additions are not part of the current dependency contract until explicitly installed and accepted.

## Audio Direction

- Music: Clean electronic energy, brighter menu ambience, more urgent gameplay loop
- SFX: Crisp UI taps, readable pickup confirmation, distinct collision failure burst, premium-feeling purchase/reward stingers
- Voice: None required in current scope

## FX and Feedback

- Hit feedback:
  - Sharp impact burst, screen flash, and readable fail messaging
  - Collision feedback should feel severe but not visually muddy
- Reward feedback:
  - Pickup bursts, reward bars, ad reward confirmation, unlock confirmation, and level progression should feel generous and premium
- Failure feedback:
  - Game-over and crash prompts should preserve readability and emotional urgency without becoming visually noisy

## Content Language

- Naming conventions:
  - Short, high-clarity feature and screen names
  - Cosmetic item names can be more branded, but system labels should stay plain
- Tone:
  - Direct, modern, confident
  - Avoid joke-heavy or overly casual copy
- Writing style:
  - Short CTA labels
  - Clear value statements for purchases, upgrades, and rewards
  - Failure messaging should be dramatic but concise

## Reference Notes

- The provided tunnel art establishes the desired world tone: bright central glow, strong hazard silhouettes, luminous sci-fi motion.
- The provided UI references establish the desired menu language: top-bar currencies, bottom navigation, white premium cards, orange-red CTA glow, and portrait mobile layout rhythm.
