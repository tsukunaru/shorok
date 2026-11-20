# Шорох — development plan

## Audited baseline
Unity 6000.3.24f1, Universal 2D 17.3, Input System 1.20, Tilemap, uGUI and Test Framework 1.6 are installed. The project contains a template scene and Plugin Your Games 2.0 core, without optional ad/save/language modules. No existing game code or Git repository was found. Preserve the template and plugin sources.

## Implementation
1. Import the supplied ghost unchanged; editor flood-fill only border-connected checkerboard, crop/resample a separate copy, generate blink and monochrome environment art.
2. Build the Attic vertical slice: tuned Rigidbody2D motor, procedural animation, input routing, safe possession, lamp platforms, checkpoint, memory, HUD, audio and room reset. Compile and run checks before extending content.
3. Add fan, car, local clock, heavy object and music box. Author five areas with continuous room sequences, 20 memories, distinct spatial puzzles and a clock finale.
4. Persistent application owns pause flags, local/cloud versioned saves, localization, audio, menu and touch UI. Scene content references ScriptableObject data and prefabs. No gameplay dependency on the SDK.
5. Use actual YG2 ready/gameplay APIs; bridge missing features to the same SDK instance. No extra SDK initialization. Guest/local failure fallback and debounced cloud writes.
6. Run EditMode/PlayMode and content validation, production Web build, HTTP browser checks, ZIP root/name/size checks. Record evidence and remaining platform-only checks honestly.

## Working agreements
Ponytail full: reuse Unity and installed packages; no new framework; editor-generated assets and scenes, never hand-authored complex scene YAML. No subagents. Source reference remains untouched. Release outputs remain under D:/UnityGames.

## Output
- Project: D:/UnityGames/GhostGameShorok
- Web: D:/UnityGames/Builds/Yandex/shorok
- ZIP: D:/UnityGames/Release/yandex/shorok-yandex.zip
- Validation: project/Reports

Target first-play duration is 30–45 minutes; this remains a playtest target, not a measured claim until a full timed run exists.
