# Game flow visual refresh

The lesson flow uses a light neutral canvas, white panels, dark ink text and teal primary actions. Its screens, layouts, fonts, rounded nine-slice sprite and view references are serialized in `Assets/prefabs/GameFlow`.

- Removed the decorative navigation from the visible main menu.
- Separated card text from artwork; repaired six thumbnail references, including the incorrect supermarket picture on Cleaning Garden.
- Kept text sizes explicit, prevented oversized navigation buttons and added scrolling to both rule lists.
- Added a viewport-aware two-column lesson grid. Runtime code adjusts its cell width only; UI objects remain prefab-owned.
- Kept button base images white so Unity's selectable tint is applied once. Hover, focus, pressed and disabled states use the same palette.
- Result captions are neutral because a completed run can contain mistakes. The controller still reports the actual outcome.

`Tools > Hurricane > Apply UI Visual Refresh` explicitly reapplies the visual migration in the Editor. The existing rebuild command also calls this migration. It preserves scene wiring, rule data and animation logic; thumbnail repair changes presentation references only.

## Validation (2026-09-13)

Unity 6000.3.3f1 compiled and rendered the prefab UI in an isolated project copy. All five screens were rendered at 1440x810 and 1024x768, with live lesson assets and populated rule/lesson prefabs. Final run exited with code 0, no compiler errors, exceptions or text-height overflow reports. The ten preview PNGs and Unity validation log are in the task's visualization folder.

Normal-state text contrast: ink on white 11.96:1; secondary text on white 6.39:1; white on primary teal 4.88:1. Primary hover/focus colors darken to retain contrast.

This verifies compilation and rendered UI layout. A full simulation playthrough and interactive keyboard/mouse checks were not performed. HUD previews show the interface on a neutral canvas, without the animated scene.
