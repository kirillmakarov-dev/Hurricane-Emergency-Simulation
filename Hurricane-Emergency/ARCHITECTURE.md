# Hurricane Emergency Simulation: Current Architecture

_Current checkout snapshot: 2026-08-19_

## Document Role

This file is the source of truth for the architecture that exists in the Unity project now.

- It describes current code, scene wiring, runtime flow, and known constraints.
- It records the implemented Unity-native lesson slices and identifies later expansion work.
- The future implementation order and proposed classes are documented in `MB_IMPLEMENTATION_PLAN.md`.
- When code or scene wiring changes, update this file in the same change.

## Project Baseline

| Item | Current value |
| --- | --- |
| Unity version | `6000.3.3f1` |
| Runtime style | Unity menu scene plus a mode-based simulation scene |
| Build scene 0 | `Assets/Scenes/MainMenu.unity` |
| Build scene 1 | `Assets/Scenes/SampleScene.unity` |
| Render pipeline | Universal Render Pipeline `17.3.0` |
| UI | UGUI `2.0.0` and TextMesh Pro assets |
| Browser integration | `Assets/Plugins/Web.jslib` through `WebGLBridge` |
| Main coordinator | `SimulationManager` |
| Mode lifecycle | `ISimulationMode` |

Other scene assets exist, but they are not enabled in Build Settings:

- `Assets/Scenes/New Scene.unity`
- `Assets/Settings/Scenes/URP2DSceneTemplate.unity`
- recovery scenes under `Assets/_Recovery/`

Recovery scenes and the URP template are not runtime levels.

## Runtime Model

Unity now owns ten lesson flows: House, Cleaning Garden, Supermarket, Children Room, Garden View, Shelter, After the Hurricane, Go Bag, Kitchen, and Bathroom. `MainMenu.unity` contains the persistent menu entry objects (`GameFlowController`, a `GameFlowUI.prefab` instance, camera, and `EventSystem`). The prefab contains the lesson selection, briefing, rule-builder, gameplay HUD, and result screens. `GameFlowController` only changes screen state, binds events, and supplies data to those serialized views.

After `Check`, `LessonLaunchContext` stores the selected level id and rule ids in order and loads `SampleScene.unity`. The gameplay scene restores that selection, starts the matching mode through `SimulationManager`, listens to `SimulationEventChannel`, displays runtime feedback, and shows the result screen. The existing mode scripts and animation controllers remain responsible for visual behavior. House, Cleaning Garden, and Supermarket use local sequence adapters around their existing public actions; the adapters do not replace their animation logic. The player-facing Children Room lesson intentionally maps to the existing `ModeName.ChildrenRoom` mode and `Children room` scene root.

`SampleScene.unity` still contains all simulation lesson roots and switches between them by `ModeName`; it is not duplicated per lesson.

## UI Authoring Constraint

This is a non-negotiable project rule so designers can change every visual component in the Unity Inspector:

- Permanent UI hierarchy must be serialized directly in a Unity scene or in a prefab instance referenced by that scene.
- Repeated runtime elements may be instantiated only from authored prefab assets.
- Runtime gameplay code must not construct UI with `new GameObject`, `AddComponent`, or dynamically assembled Canvas/layout/text/button hierarchies.
- `GameFlowController` is a coordinator, not a UI factory. It reads serialized references from `GameFlowView`, binds button callbacks, updates text/state, and instantiates only authored repeated prefab templates.
- Visual changes belong in prefab mode or the scene Inspector rather than in C# layout code.
- `Tools/Hurricane/Rebuild Game Flow UI Assets` is an Editor-only scaffolding/recovery command. It never runs in a build, and running it intentionally overwrites the generated prefab visuals. Normal UI iteration must edit the prefab assets directly.

Current authored assets:

| Asset | Responsibility |
| --- | --- |
| `Assets/prefabs/GameFlow/GameFlowUI.prefab` | All five permanent flow screens and their serialized controls |
| `Assets/prefabs/GameFlow/LessonButton.prefab` | Repeated editable lesson-selection card |
| `Assets/prefabs/GameFlow/RuleOptionButton.prefab` | Repeated available-rule card |
| `Assets/prefabs/GameFlow/SelectedRuleRow.prefab` | Repeated selected-rule row with reorder/remove controls |
| `Assets/Scenes/MainMenu.unity` | Serialized controller, UI prefab instance, camera, and EventSystem |
| `Assets/Scenes/SampleScene.unity` | Serialized controller and UI prefab instance alongside existing simulation objects |

Current authored lesson data:

| Asset | Responsibility |
| --- | --- |
| `Assets/Data/Lessons/LevelCatalog.asset` | Ordered list used by the menu and gameplay scene |
| `Assets/Data/Lessons/HouseLesson.asset` | House copy, mode, ordered actions, distractors, thumbnail, and completion events |
| `Assets/Data/Lessons/CleaningGardenLesson.asset` | Cleaning Garden copy, mode, ordered actions, distractors, thumbnail, and completion events |
| `Assets/Data/Lessons/SupermarketLesson.asset` | Supermarket copy, mode, ordered supplies, distractors, thumbnail, and completion events |
| `Assets/Data/Lessons/GoBagLesson.asset` | Go Bag copy, mode, required enum items, distractors, thumbnail, and opening events |
| `Assets/Data/Lessons/KitchenLesson.asset` | Kitchen copy, mode, required enum items, distractors, thumbnail, and opening events |
| `Assets/Data/Lessons/BedroomLesson.asset` | Children Room copy, mode, required enum items, distractors, thumbnail, and opening events |
| `Assets/Data/Lessons/GardenViewLesson.asset` | Garden View copy, mode, required enum items, distractors, thumbnail, and opening events |
| `Assets/Data/Lessons/ShelterLesson.asset` | Shelter copy, mode, required enum items, distractors, thumbnail, and opening events |
| `Assets/Data/Lessons/AfterTheHurricaneLesson.asset` | After the Hurricane copy, mode, required enum items, distractors, thumbnail, and opening events |

`LevelDefinition` does not serialize animation command strings or duplicate runtime rule records. Designers choose the correct ordered items and distractors through `LessonRuleItem` enum lists. `LessonRuleItemCatalog` maps each enum value to the existing mode command, stable rule id, label, and completion event. A mode mismatch or duplicate item is reported as a configuration error. The catalog order follows the course flow: House, Cleaning Garden, Supermarket, Children Room, Garden View, Shelter, After the Hurricane, Go Bag, Kitchen, and Bathroom.

`Tools/Hurricane/Rebuild Lesson Data Assets` intentionally restores the lesson assets to project defaults. Normal edits are made directly in the assets. Rebuilding Game Flow UI only creates missing lesson assets and does not overwrite existing lesson configuration.

```text
MainMenu.unity rule selection or legacy WebGL command
               |
               v
ObjectsHolder / SimulationManager
               |
               v
SimulationManager.SwitchMode(ModeName)
       |                       |
       v                       v
ISimulationMode lifecycle   GameModeUIController
       |                       |
       v                       v
Mode-specific animation    Enable selected content root
queues, timers and objects
               |
               v
Animation events / completed coroutines
               |
               v
EventsManager or WebGLBridge
               |
               v
Browser host callbacks
```

All ten catalog lessons no longer depend on the browser host for orchestration or rule validation. WebGL remains a supported build target and compatibility callback bridge.

## Core Components

### `SimulationManager`

Path: `Assets/Scripts/SimulationManager.cs`

`SimulationManager` is a singleton and the central runtime coordinator.

Current responsibilities:

- create `GameModeFactory`;
- initialize every value in `ModeName`;
- store `CurrentMode` and `PreviousMode`;
- call `Cleanup()` on the previous mode;
- resolve and start the next mode with `OnSimulationStart()`;
- publish `OnModeChanged` for UI activation;
- initialize/reset the browser host through `WebGLBridge`;
- pause or resume `Time.timeScale` and audio from a WebGL string command;
- reload the active scene for a legacy simulation reset without assuming a fixed build index.

Important current behavior:

- In the Unity Editor, `Update()` compares `CurrentMode` with the public `newMode` field and switches automatically.
- `SampleScene.unity` currently serializes `newMode` as enum value `1`, which is `House`; the Unity-native flow immediately switches to the selected lesson mode after loading.
- `SwitchMode` calls `Cleanup()`, but it does not call `OnSimulationEnd()`.
- The manager and all mode components are serialized on the same scene GameObject.
- If a mode component is missing, `GameModeFactory` can add it dynamically. This is unsafe for modes that require Inspector references.

### `ISimulationMode`

Path: `Assets/Scripts/ISimulationMode.cs`

Every mode implements:

```csharp
void Initialize();
void OnSimulationStart();
void OnSimulationEnd();
void Cleanup();
```

The lifecycle is only partially implemented across modes. Several `Cleanup()` methods only log or are empty, but `GardenViewMode.Cleanup()` now clears its timers, queues, and scene state so mode transitions do not throw.

### `GameModeFactory`

Path: `Assets/Scripts/Modes/GameModeFactory.cs`

The factory maps every `ModeName` to one component type. It first looks for that component on the `SimulationManager` GameObject and adds it if missing.

All current mode components are already serialized in `SampleScene.unity`, so normal startup uses the Inspector-configured instances.

### `GameModeUIController`

Path: `Assets/Scripts/GameModeUIController.cs`

This component subscribes to `SimulationManager.OnModeChanged` and controls one configured scene root per gameplay mode.

Current scene configuration:

| Enum value | `ModeName` | Controlled root |
| ---: | --- | --- |
| `0` | `EntryScreen` | No object assigned |
| `1` | `House` | `HouseMod` |
| `2` | `ClearingGarden` | `Clearing Garden mod` |
| `3` | `SuperMarket` | `Super Market` |
| `4` | `ChildrenRoom` | `Children room` |
| `5` | `GardenView` | `GardenViewMode` |
| `6` | `Shelter` | `Shelter` |
| `7` | `AfterTheHurricane` | `Mission 8` |
| `8` | `GoBagLesson` | `GoBagLesson 9` |
| `9` | `KitchenLesson` | `Kitchen Lesson 10` |
| `10` | `BathRoomLesson` | `BathroomLesson 11` |

The future main menu needs a root assigned to `EntryScreen` or a separate flow-level UI controller that is deliberately kept outside this list.

### `ObjectsHolder`

Path: `Assets/Scripts/ObjectsHolder.cs`

`ObjectsHolder` receives browser-owned ids, translates numeric scene indexes into Unity modes, and sends timer values to the active mode.

Current WebGL scene-index mapping:

| Web scene index | `ModeName` |
| ---: | --- |
| `1` | `House` |
| `2` | `ClearingGarden` |
| `4` | `SuperMarket` |
| `5` | `ChildrenRoom` |
| `6` | `GardenView` |
| `7` | `Shelter` |
| `8` | `AfterTheHurricane` |
| `9` | `GoBagLesson` |
| `10` | `KitchenLesson` |
| `11` | `BathRoomLesson` |

There is no current mapping for index `0` or `3`, and no WebGL scene index for `EntryScreen`.

Stored browser ids:

- radio;
- June 1;
- May;
- Kay;
- hurricane watch;
- hurricane warning;
- all clear;
- Kelan's parents.

`SetCalendarTimer(float)` dispatches timer state to `House`, `SuperMarket`, `ChildrenRoom`, `GardenView`, `Shelter`, `AfterTheHurricane`, `GoBagLesson`, `KitchenLesson`, and `BathRoomLesson`.

## Mode Registry

### `EntryScreen`

Class: `EntryScreenMod`

Current status:

- registered in `ModeName` and `GameModeFactory`;
- serialized on `SimulationManager`;
- all lifecycle methods are empty;
- no UI root is assigned in `GameModeUIController`;
- not mapped by `ObjectsHolder.SetScineIndex`.

This is the natural host mode for the future Unity-native main menu, but it is currently only a placeholder.

### `House`

Class: `HouseMod`

Primary behavior:

- starts the initial house animation;
- runs the calendar timer;
- controls radio announcement and music;
- controls emergency-plan, go-bag, TV, and parent-panic sequences;
- can switch directly to `ClearingGarden`.

External/public action entry points:

- `RadioOnAnnouncement()`
- `RadioPlayingSong()`
- `June1onArrivesAnim()`
- `OnReviewPlan()`
- `OnCheckGobag()`
- `WatchTV()`
- `ParentsPanic()`
- `May1onArrivesAnim()`

Known outbound events are split between this class and `AnimationEvent`: `RadioBroadcast`, `ReviewEmergencyPlan`, `CheckGoBag`, `JuneFirst`, and `MayArrives`.

### `ClearingGarden`

Class: `ClearingGardenMod`

Primary behavior:

- maintains independent animation queues for mother and father;
- maps `Cleaning` and `Watering` to mother;
- maps `Walk` and `PlyWood` to father;
- drives leaves, plywood, character movement, and watering particles.

Action entry points:

- `OnClearYard()`
- `OnGatherPlywood()`
- `GoForWalk()`
- `OnWateringTheFlowers()`

`CleanYard` and `CollectPlywood` are reported by animation helper components rather than directly by this mode. Direct sends inside the mode are commented out.

### `SuperMarket`

Class: `SuperMarketMode`

Queue commands:

- `Tosupermarket`
- `GetCannedFood`
- `GetCrackers`
- `GetWater`
- `GetCheese`
- `GetEggs`
- `GetChicken`
- `GetFish`
- `Announcement`
- `WayToSupermarketAnimation`

Primary command entry point: `SuperQueueAnimation(string)`.

Known outbound events:

- `JuneFirst`
- `GoToSupermarket`
- `GetWater`
- `GetCannedFood`
- `GetCrackers`

Cheese, eggs, chicken, and fish use the animation queue but currently do not have dedicated values in the shared `Events` enum.

### `ChildrenRoom`

Class: `ChildrenRoomMode`

This mode uses the shared `Animations` enum for both room and later garden actions. Its own queue handles packing actions plus `HurricaneWatchAnnouncement`.

Primary command entry point: `AddAnimationFromWeb(string)`.

Known actions/events include:

- clothes, fruits, flashlight, lamp, toy, aquarium, water, scissors, and chicken pickup;
- `PackClothes`, `PackToys`, `PackWater`, and `PackFlashlight`;
- hurricane-watch playback and browser id callback;
- `HurricaneWatch` completion event.

Some outbound sends are implemented inside shared coroutines, while older direct sends are commented out.

### `GardenView`

Class: `GardenViewMode`

Primary behavior:

- separate queues for Kelan and Key;
- Kelan actions: walk, ball, toys;
- Key actions: bicycle, flowers;
- mother actions: cover window and water garden;
- hurricane warning timer and announcement.

Command entry points:

- `AddKelanAnimationFromWeb(string)`
- `AddKeyAnimationFromWeb(string)`
- `SendHurricaneWarning()`
- `HurricaneWarningAnnouncement()`
- `RadioOnAnnouncement()`
- `OnCoversWindow()`
- `OnWaterGarden()`

Known outbound events are distributed between this mode, `GardenTakesObjects`, and `AnimationEvent`: `HurricaneWarning`, `CoverWindow`, `GetBicycle`, `GetToys`, and `GetBall`.

Current state: `Cleanup()` now clears queues, timers, and scene state instead of throwing, and `OnSimulationEnd()` resets timer-related state so switching away from this mode is safe for the new menu/progression flow.

### `Shelter`

Class: `ShelterMod`

Queue commands:

- `ColoursABook`
- `PlaysWithToy`
- `PlaysOutside`
- `TalksToAStranger`

Primary command entry point: `ShelterQueueAnimation(string)`.

Known outbound events:

- `ColorBook`
- `PlayToy`
- browser-specific `OnInShelter(kayId)` timer callback.

The `GameFlowController` launches the selected lesson actions directly after the user presses `Check`; the scene is not blocked by a pre-start rule validator. The `LevelDefinition` asset still defines the mandatory actions for runtime checking, while the scene reproduces whatever was selected in the rule builder.

There are no shared `Events` values for playing outside or talking to a stranger.

### `AfterTheHurricane`

Class: `AfterTheHurricane`

Queue commands:

- `picksUpBranches`
- `picksUpBottles`
- `picksUpBrockenGlass`
- `picksUpElectricWires`
- `FatherPicksUpBrockenGlass`
- `FatherPicksUpElectricWires`
- `GoForWalk`
- `MotherCutWood`

Primary command entry point: `AfterHurricaneQueueAnimation(string)`.

Known outbound events:

- `PickBranches`
- `PickBottles`
- `PickGlass`
- `CutBranches`
- `AllClear`
- browser-specific `AllClear(allClearId)` timer callback.

The `GameFlowController` launches the selected cleanup actions directly for this mode as well, so wrong selections still reproduce in the scene. The `LevelDefinition` asset defines which cleanup actions are mandatory for runtime checking.

Not every queue command currently produces a distinct shared event.

### `GoBagLesson`

Class: `GoBagLesson`

Queue commands:

- `KelenTakeTshirt`
- `KelenTakeFlashlight`
- `KelenTakeLamp`
- `KelenTakeToy`
- `KelenTakeWater`
- `KelanTakeBall`
- `KelenTakeBooks`
- `KelenTakeCandels`
- `ColoringBook`
- `KelenTakeScissors`

Primary command entry point: `AddGoBagAnimationFromWeb(string)`.

Known outbound events:

- `GobagReminder`
- `PackClothes`
- `PackFlashlight`
- `PackToys`
- `PackWater`
- `PackBook`
- browser-specific `GivesReminder(kelanParentsId)` callback.

Several distractor actions intentionally send `Events.Empty`, including current ball, lamp, coloring-book, scissors, and candles paths. The future validator must define whether `Empty` means incorrect, ignored, or missing event coverage.

### `KitchenLesson`

Class: `KitchenLesson`

Queue commands:

- `KayTakeChees`
- `KayTakeEggs`
- `KayTakeChicken`
- `KayTakeFish`
- `KayTakeCannedFood`
- `KayTakeCrackers`
- `KayTakeWater`

Primary command entry point: `AddGoBagKitchenAnimationFromWeb(string)`.

Known outbound events:

- `GobagReminder`
- `PackCannedFood`
- `PackCrackers`
- `PackWater`
- browser-specific `GivesReminder(kelanParentsId)` callback.

Cheese, eggs, chicken, and fish currently send `Events.Empty`, so they cannot yet be distinguished by the shared event stream.

### `BathRoomLesson`

Class: `BathRoomLesson`

Queue commands:

- `PackFirstAid`
- `PackToothbrush`
- `PackWipes`
- `PackSoap`
- `PuckHairDryer`
- `PackPump`
- `PackWashingGel`
- `PackCleaningSpray`

Primary command entry point: `AddGoBagBathroomAnimationFromWeb(string)`.

Known outbound events:

- `GobagReminder`
- `PackFirstAid`
- `PackToothbrush`
- `PackWipes`
- `PackSoap`
- browser-specific `GivesReminder(kelanParentsId)` callback.

Hair dryer, pump, washing gel, and cleaning spray currently send `Events.Empty`. The serialized `roomToyOffset` field is unassigned and is not referenced by current code.

## Event and WebGL Integration

### Shared event catalog

Path: `Assets/Scripts/EventsManager.cs`

The `Events` enum currently contains:

```text
ReviewEmergencyPlan, RadioBroadcast, CheckGoBag, CleanYard,
CollectPlywood, JuneFirst, GoToSupermarket, GetCannedFood,
GetWater, GetCrackers, HurricaneWatch, HurricaneWarning,
PackClothes, PackToys, PackWater, PackFlashlight, MayArrives,
Empty, CoverWindow, GetBicycle, GetToys, GetBall, ColorBook,
PlayToy, CutBranches, PickGlass, PickBottles, PickBranches,
AllClear, GobagReminder, PackBook, PackCrackers, PackCannedFood,
PackFirstAid, PackToothbrush, PackWipes, PackSoap
```

`EventsManager` exposes animation-event-friendly methods for the older event subset. Newer modes usually call `WebGLBridge.SendEvent(...)` directly.

There is no Unity-side subscriber event today. Sending an event informs WebGL or logs in the Editor, but native Unity gameplay validation cannot listen to it without the event layer proposed in `MB_IMPLEMENTATION_PLAN.md`.

### Browser callback surface

Path: `Assets/Scripts/WebGLBridge.cs`

Current exported JavaScript calls:

- `CallINITfunction()`
- `OnResetDone()`
- `SendEvent(string)`
- `OnJuneArrives(int)`
- `OnMayArrives(int)`
- `OnInShelter(int)`
- `HurricaneWatchOnAnnounced(int)`
- `HurricaneWarningOnAnnounced(int)`
- `AllClear(int)`
- `GivesReminder(int)`

`Assets/Plugins/Web.jslib` forwards these calls to the browser's `globals` object. Outside WebGL builds, `WebGLBridge` logs instead of invoking JavaScript.

This public callback surface is a compatibility contract. The native Unity rule system should add local observation without renaming or removing these calls.

## Scene Serialization Snapshot

Static inspection of `SampleScene.unity` confirms:

- one `SimulationManager` component;
- all eleven `ISimulationMode` components serialized on the same GameObject;
- one `ObjectsHolder`;
- one `GameModeUIController`;
- one `AudioManager` and one `AudioSettingsUI`;
- all ten gameplay roots assigned in `GameModeUIController`;
- no root assigned for `EntryScreen`;
- all mode script components have their existing serialized scene references except the unused `BathRoomLesson.roomToyOffset` field;
- `MainMenu.unity` is enabled as build scene 0 and `SampleScene.unity` as build scene 1.

This is repository evidence only. It does not prove that every reference, Animator state, animation event, or callback behaves correctly in Play Mode.

## Supporting Components

| Component | Current responsibility |
| --- | --- |
| `AnimationEvent` | General animation callbacks, planks, calendar events, window cover, and emergency-plan gating |
| `LeavsAnimation` | Leaf object and particle transitions |
| `LeavsAnimationEvent` | Advances leaf cleanup and reports `CleanYard` |
| `GardenTakesObjects` | Garden scene/hand object swaps and garden pickup events |
| `RoomTakesObjects` | Children-room scene/hand object swaps |
| `SuperTakesObjects` | Supermarket scene/hand/cart object swaps |
| `ScreenFader` | Fade transitions used by house sequences |
| `AudioManager` | Mixer state, saved volume, music, SFX, pause/resume |
| `AudioSettingsUI` | Slider/toggle bindings for `AudioManager` |
| `ScineSwicher` | Activates one configured object and deactivates the current object |
| `SimulationConfig` | Singleton placeholder with reset entry point; current config id is not assigned by code |

## Current Architectural Constraints

These are facts to account for during implementation, not a request to refactor them immediately.

1. Mode scripts directly own scene references, positions, Animator triggers, timers, queues, and object swaps.
2. Several modes independently implement similar queue/dictionary/callback patterns.
3. Browser command strings, enum names, Animator names, and object names are used as runtime identifiers.
4. Outbound events are emitted from multiple layers: modes, animation event receivers, and object-swap helpers.
5. Event coverage is incomplete; several selectable actions emit `Events.Empty`.
6. Most mode cleanup methods do not fully reset queues, coroutines, timers, held objects, or Animator state.
7. `GardenViewMode.Cleanup()` now performs a safe reset during a normal mode transition.
8. `EntryScreenMod` remains unused; the Unity-native menu is owned by `MainMenu.unity` and `GameFlowController`.
9. `LevelCatalog.asset` and its referenced `LevelDefinition` assets are the source of truth for all ten lesson objectives, accepted enum items, distractors, thumbnails, and runtime event order.
10. `SimulationEventChannel` exposes completed gameplay actions to the active Unity lesson session while preserving WebGL output.
11. The Editor currently auto-selects `House` from the serialized `newMode` value before the Unity-native gameplay flow switches to the selected lesson.
12. `SimulationManager.ResetSimulatiom()` reloads the entire scene rather than resetting one level session.

## Missing Product Definitions

The code architecture is understandable enough to begin building the framework. What is still needed to author the actual levels correctly is product/content data:

1. Final lesson order and grouping. Confirm whether modes `1` through `11` are one continuous course and how the three Go Bag lessons relate to the earlier simulation.
2. A title, briefing, and learning objective for every selectable level.
3. The available rule cards for every level, including condition, actor, action, and displayed text.
4. The exact correct rule arrangement for every level.
5. The runtime event sequence that counts as successful completion.
6. Which actions are incorrect distractors, which are optional, and which should be ignored.
7. The policy for incorrect runtime actions: feedback only, retry current step, fail attempt, or allow completion with mistakes.
8. The completion destination for each level: next level, menu, or player choice.
9. Whether levels are all unlocked or unlocked progressively, and whether progress must persist between sessions.
10. Required UI languages and final copy. Stable ids can be implemented before localization, but final layout depends on the actual text.
11. Approved visual assets for menu cards, rule cards, feedback, and result screens.

The framework can be implemented with temporary data before all copy and assets are final. However, the rule definitions and completion criteria must be approved per level before that level can be considered complete.

## Safe Implementation Boundary

The first implementation should add orchestration around the current simulation:

- menu and briefing choose a `LevelDefinition`;
- rule builder lets the player assemble any selected sequence;
- `Check` starts the lesson without pre-start validation;
- existing mode methods continue to execute every animation and object transition;
- a compatibility reporter mirrors existing completion callbacks into a Unity event channel while preserving WebGL calls;
- a session controller evaluates progress and opens feedback/result UI;
- repeated queue and animation code remains untouched until the full flow is accepted.

This boundary allows the requested native Unity system to be built without changing how the current animations work.

## Verification Rules

For every architecture or gameplay change:

1. Confirm C# compilation.
2. Inspect serialized references and `GameModeUIController` entries.
3. Run the affected mode in Unity Play Mode.
4. Compare animation order, timing, audio, object swaps, and event timing with the baseline.
5. Test switching away, retrying, and returning to menu.
6. Build WebGL when browser callbacks are touched.
7. Do not describe static inspection or a C# build as Play Mode verification.
