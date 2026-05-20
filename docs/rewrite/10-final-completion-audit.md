# Final Completion Audit

This final package adds the remaining integration and parity layer on top of the clean replacement.

## Added final code pieces

- Runtime diagnostics overlay
- Collision debug probe
- Music debug overlay
- Camera follow and screen shake helper
- Player visual/banking helper
- Ship hover bob
- Player respawn helper
- Tunnel/core light followers
- Tunnel atmosphere controller
- Door obstacle helper
- Obstacle ring controller/visuals
- VFX manager and pooled VFX instance
- Pickup magnet controller
- Audio duck helper
- Mobile notification adapter placeholder
- Graphics settings manager
- UI motion, click effect and interaction helpers
- Speed-up flash
- Remove-ads thank-you controller
- Shop item card/details modal
- Hangar cosmetic/stat/upgrade views
- Progression task/daily/reward views
- HUD score/health/powerup views
- Scene wiring validator
- Missing script reporter
- Manual wiring checklist editor window

## Still intentionally manual

These cannot be completed reliably outside Unity because they depend on installed packages, scene GUIDs, prefab references and platform settings:

- LevelPlay SDK method calls
- Firebase SDK method calls
- Unity IAP purchase and restore callbacks
- Unity Mobile Notifications platform calls
- Android build settings, signing and package name
- Scene/prefab serialized references
- Replacing placeholder scene objects with final art assets
- Removing missing script references from original scenes/prefabs

## Replacement confidence

The package now covers the architecture, major gameplay systems, meta systems, monetisation policy, UI structure, debug helpers, scene tools, validation tools and manual wiring documentation.

Treat remaining work as Unity integration/playtest, not architecture rewrite.
