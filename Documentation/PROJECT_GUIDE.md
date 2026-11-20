# Шорох / Shorok

An original monochrome 2D possession puzzle platformer for Unity **6000.3.24f1** (6.3 LTS), Web and Yandex Games. Open this folder in that Unity version. Open `Assets/Game/Scenes/Bootstrap.unity`, then Play. The source scene and installed Your Games plugin remain preserved.

## Controls
- A/D or arrows: move. Space/W/up: jump; release for a shorter jump, hold while falling for a brief float.
- E: possess a nearby object, or leave it. F: use its ability. Q: reverse a fan.
- Escape: pause. Hold R: restore the current room.
- Touch: simultaneous movement and jump buttons, contextual possession/action buttons. Portrait pauses the game.

## Content and builds
- `Tools > Shorok > Build Game Content` regenerates the authored scenes, prefabs, art, sounds and configurations. Generated scenes are intentionally owned by the generator: edit room definitions and construction code before regenerating.
- `Tools > Shorok > Process Ghost Sprite` removes only border-connected background pixels and creates copies. Source: `Assets/Game/Art/Ghost/Source/ghost_source.png`. Gameplay sprite: `Assets/Game/Art/Ghost/Generated/ghost_clean.png`.
- `Tools > Shorok > Validate Project` checks scenes, references, sprite imports, localization and unique collectibles/checkpoints.
- `Tools > Shorok > Build > Yandex Release` builds production Web, checks sizes/names, creates ZIP and verifies its extraction.
- Web output: `D:/UnityGames/Builds/Yandex/shorok`.
- ZIP: `D:/UnityGames/Release/yandex/shorok-yandex.zip`.
- Serve the Web folder over HTTP, not `file://`.
- `Tools > Shorok > Clear Local Save` clears Editor progress; `Open Save Folder` shows the persistent data directory. Web local progress uses PlayerPrefs/IndexedDB.

## Extend
Derive a new ability from `Possessable`, implement `HandleInput`, and supply its sprite/prefab in `ShorokContentBuilder`. The ghost input controller does not need a new branch. Connect revealable platforms, pressure plates, gates and local mechanisms in the editor builder. Add rooms through `RoomDefinition`, areas through `LevelDefinition` and the builder's area arrays. Gameplay state belongs to scene instances, never ScriptableObject configuration assets.

## Integration and licensing
YG2 core handles SDK lifecycle and gameplay/ready calls. `YandexPlatformService` and the small Web bridge cover missing cloud, language and ad modules using the same SDK instance. Guest gameplay and local saves work when the platform is unavailable. No forced login or external ads.

The five detailed monochrome backgrounds are original image-generation assets. The user-approved attic PNG is preserved verbatim, and the other four use it as the style reference. Sources and art direction are recorded in `Documentation/ART_SOURCES.md`. Foreground sprites and audio are generated in the Editor. The supplied ghost source is preserved verbatim. Press Start 2P by CodeMan38 is licensed under SIL OFL 1.1; the font and license are in `Assets/Game/Art/UI`.

## Checks
Test Runner: `Shorok.Tests.EditMode`, `Shorok.Tests.PlayMode`. Unity writes the reports and logs when these checks run. Final Yandex-hosted SDK, ads and cloud behavior must also be checked in a console draft; a localhost fallback cannot establish production platform behavior.

Run `node Tools/check-platform.cjs` to check the actual JavaScript bridge with deterministic SDK callbacks. Run `node Tools/serve-web.cjs` after building, then open `http://127.0.0.1:8765`. F1 opens the debug panel only in the Editor or development builds; it is excluded from release.
