# Architecture

`Bootstrap` creates one persistent `GameApp` owning input, save data, localization, UI, audio and platform adapter. Async scene loading produces `MainMenu`, five separate areas, then `Ending`. There is one runtime assembly and separate test assemblies; editor generation lives outside runtime.

`GameState` has a phase and independent pause flags. Manual pause, platform pause, ad, orientation, focus, respawn and transition must all clear before gameplay resumes. Time/audio/markup follow this decision; local clock powers never change global time.

`InputRouter` owns physical Input System actions and counts independent touch pointers. `GhostMotor` uses Rigidbody2D and capsule casts, acceleration, coyote time, buffering and variable jump gravity. `GhostAnimator` deforms only the visual child. `PossessionController` chooses nearby visible objects and routes controls to the selected `Possessable`. Exit candidates must clear solid geometry and hazards.

`LevelRoot` manages a continuous sequence of authored rooms. Rooms cache their resettable contents. Lamps and music boxes activate `RevealablePlatform`; car/heavy/crate bodies latch `PressurePlate`; `PuzzleGate` controls passage; clocks affect only local `MovingPlatform`/`WorldHazard` components. Room completion and memory IDs are persistent; transient physical positions reset deterministically.

`SaveSystem` validates JSON, migrates schema 0 to 1, retains a local backup and chooses conflicts by revision, timestamp and progress tie-breaker. New game increments the revision before resetting progress. Future schemas are preserved instead of silently overwritten. Cloud writes are debounced for 3 seconds and local saves happen immediately.

`PlatformService` isolates gameplay from Yandex. The installed YG2 API provides ready/start/stop. `YandexPlatformService` uses a `.jslib` bridge only for optional APIs absent from the installed plugin modules. The custom template initializes Yandex once, supplies plugin-compatible globals and handles visibility/pause events. Reward is granted only after the official reward callback, and ad closure releases only the ad pause reason.

Pixel art and procedural sound generation run at edit time. Build assets use point filtering, a shared grayscale palette, 32 PPU and Pixel Perfect Camera. Scene/prefab creation uses Unity APIs, not hand-written YAML.
