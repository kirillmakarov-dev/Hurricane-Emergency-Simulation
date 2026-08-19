# MB Technical Implementation Plan

_Purpose: a step-by-step implementation guide for moving the current WebGL-driven simulation into a native Unity flow while preserving the existing animations, scene logic, and gameplay behavior._

## Implementation Status

The Unity-native lesson pattern is implemented for `GoBagLesson`, `KitchenLesson`, the player-facing Children Room lesson backed by `ChildrenRoomMode`, the direct-launch `Shelter` and `AfterTheHurricane` scenes, plus the garden rule sequence in `GardenViewMode`:

- `MainMenu.unity` is build scene 0 and owns lesson selection, briefing, and rule assembly.
- `SampleScene.unity` is build scene 1 and owns the existing simulation content.
- Both scenes contain a serialized `GameFlowUI.prefab` instance; runtime code does not construct the interface.
- Lesson cards, available-rule cards, and selected-rule rows are instantiated only from dedicated prefab templates.
- `LevelCatalog.asset` references one editable `LevelDefinition` ScriptableObject per lesson.
- Each lesson selects its ordered required items and distractors through `LessonRuleItem` enum lists in the Inspector.
- `LessonLaunchContext` transfers the selected rule ids between scenes locally in Unity.
- `Check` launches the selected rules for every lesson; correctness is reported during play rather than blocking scene start.
- `SimulationEventChannel` and `LevelSessionController` validate runtime events without replacing existing animations.
- Result, replay, and return-to-menu paths are connected.
- WebGL callbacks remain available as compatibility output; they are not the source of truth for this flow.

Current lesson contracts:

| Lesson | Existing mode | Accepted rule order | Runtime event order |
| --- | --- | --- | --- |
| Go Bag | `GoBagLesson` | water, flashlight, books | `GobagReminder`, `PackWater`, `PackFlashlight`, `PackBook` |
| Kitchen | `KitchenLesson` | canned food, crackers, water | `GobagReminder`, `PackCannedFood`, `PackCrackers`, `PackWater` |
| Children Room | `ChildrenRoomMode` / `ModeName.ChildrenRoom` | clothes, water, flashlight, toy | `HurricaneWatch`, `PackClothes`, `PackWater`, `PackFlashlight`, `PackToys` |
| Garden View | `GardenViewMode` / `ModeName.GardenView` | toys, ball, bicycle | `HurricaneWarning`, `GetToys`, `GetBall`, `GetBicycle` |
| Shelter | `ShelterMod` / `ModeName.Shelter` | colors a book, plays with toy | `ColorBook`, `PlayToy` |
| After the Hurricane | `AfterTheHurricane` / `ModeName.AfterTheHurricane` | branches, bottles, cleanup | `PickBranches`, `PickBottles`, `CutBranches` |

Each adapter starts the already existing animation queue and functions for its mode. It does not replace Animator controllers, clips, triggers, object swaps, movement, or timing.

The remaining phases apply this pattern to additional lessons and then extract repeated queue, mapping, and UI coordination code into helpers only after behavior is accepted.

## Goal

Build a Unity-first flow where:

1. The player opens a main menu and chooses a scene or lesson.
2. Before the level starts, the player sees the rules and the required steps.
3. The player assembles or reviews the rules for that level.
4. The player presses `Check`.
5. Pressing `Check` starts the lesson with whatever rules the player selected; runtime event checks determine success or failure during the run.
6. During gameplay, the system validates correct and incorrect events.
7. The level ends with feedback and progression to the next level or back to menu.
8. Existing animations, triggers, timers, and scene mechanics continue to work as they do now.

## Non-Negotiables

- Do not break the current animation flow.
- Do not replace working scene logic just to change architecture.
- Do not create Canvas, panels, text, buttons, layout components, or other UI hierarchy from runtime code.
- Keep every permanent UI element in a scene or prefab; create repeated elements only by instantiating an authored prefab.
- Keep visual configuration editable in the Unity Inspector rather than encoded in `GameFlowController`.
- Keep existing mode-specific behavior intact.
- Add the new flow on top of the current system first, then refactor later.
- Keep event names and level actions traceable and debuggable.

## Step-by-Step Plan

### Phase 1: Inventory the current system

- List all current scenes, lesson modes, and gameplay events.
- Map each scene to the functions and triggers it already uses.
- Identify which actions come from WebGL, which actions are local Unity behavior, and which actions are shared.
- Mark the current “source of truth” for each lesson so we can reuse it instead of rewriting it.

### Phase 2: Define the Unity rule model

- Create a data model for one level’s rules.
- Each level should define:
  - scene or lesson id
  - allowed actions
  - required actions
  - order constraints if needed
  - completion condition
  - failure or mismatch feedback
- Keep this model reusable so all levels follow the same pattern.
- Do not hardcode rule logic inside UI buttons.

### Phase 3: Add the main menu / level selector

- Create a start scene that becomes the first entry point in Unity.
- Add buttons or cards for each lesson / level.
- On selection, load the chosen level’s configuration.
- Show the level title, short description, and the key objective before entering gameplay.
- If needed, allow returning from a level back to the menu.

### Phase 4: Add the rule builder / review screen

- Show the rules before the level begins.
- Let the player assemble or confirm the rule sequence for that scene.
- Keep the UI simple and readable:
  - selected rules
  - available rule options
  - short explanation of the expected flow
  - `Check` button
- The `Check` button should launch the lesson with the assembled rules.
- The player should see feedback during the run, not only before starting.

### Phase 5: Add runtime event validation

- Introduce a gameplay session controller that listens to events already emitted by the scene.
- Track:
  - correct event
  - incorrect event
  - missing step
  - completed step
  - current level progress
- Emit feedback to UI for each state change.
- Keep the existing animation and scene triggers as the thing that actually drives visual behavior.
- Validation should observe the flow, not replace it.

### Phase 6: Add completion and progression

- Detect when all required steps for the level are completed.
- Show success feedback.
- Move to the next level automatically if that is the chosen flow.
- Otherwise allow returning to the menu.
- Preserve level-by-level isolation so one level does not leak state into another.

### Phase 7: Preserve current mechanics exactly

- Keep animation controllers, animation events, timers, and scene object behavior intact.
- Reuse the functions and mode scripts that already exist for each scene.
- Do not rewrite the animation pipeline before the new flow is working.
- If a scene already has a working action, connect the new rule system to that action instead of duplicating it.

### Phase 8: Refactor after the working flow is stable

- Once the menu, rule validation, runtime checks, feedback, and progression work reliably:
  - extract repeated code into helper classes
  - separate shared UI logic from scene logic
  - separate rule evaluation from scene execution
  - reduce duplicated event handling across lessons
- The goal of this phase is architecture clarity, not feature expansion.

## Recommended Runtime Flow

1. Open main menu.
2. Choose a scene / lesson.
3. Read the level objective and required rule flow.
4. Build or confirm the rule set.
5. Press `Check`.
6. Start the level after validation.
7. Run gameplay while the system checks each step.
8. Show immediate feedback for correct / incorrect actions.
9. Finish the level.
10. Continue to the next level or return to the menu.

## Implementation Notes

- Prefer event-driven validation instead of polling where possible.
- Keep the level UI separate from the simulation runtime.
- Maintain a clear contract between:
  - scene actions
  - rule definitions
  - validation results
  - user feedback
- If a rule is already represented by an existing function or event, reuse it.
- If a new rule does not exist yet, add the smallest possible hook that fits the current architecture.

## Current Project Baseline

The first implementation must be built around the architecture that already exists:

- `SimulationManager` remains the central coordinator and the only normal entry point for changing `ModeName`.
- `GameModeFactory` remains responsible for resolving an `ISimulationMode` implementation.
- Each existing mode remains responsible for its own scene objects, animation queues, timers, audio, and visual transitions.
- `GameModeUIController` continues to activate the content belonging to the selected mode.
- Existing `Events` values and calls to `WebGLBridge.SendEvent(...)` are treated as the first event catalog for level validation.
- `ObjectsHolder.SetScineIndex(int)` remains supported for the current WebGL integration while the Unity-native menu uses `ModeName` directly.
- `EntryScreenMod` becomes the first Unity-native menu state. This avoids moving the current Inspector-wired simulation into multiple Unity scenes during the first implementation.

The project currently runs primarily from `SampleScene.unity`. For the first working version, "scene" in the player-facing UI means a lesson or simulation mode, not necessarily a separate Unity `.unity` scene.

## Proposed Runtime Components

Create the following components only as they become necessary. Their first versions should stay small and should not absorb existing animation code.

### Data and configuration

#### `LevelDefinition`

A `ScriptableObject` containing authored data for one lesson. This is implemented for Go Bag, Kitchen, Children Room, Garden View, Shelter, and After the Hurricane:

- stable `levelId`
- display title and description
- learning objective and pre-level instructions
- target `ModeName`
- ordered `Required Items` selected from `LessonRuleItem` enum
- separate `Distractors` selected from the same enum
- opening runtime events such as `GobagReminder` or `HurricaneWatch`
- next level id
- completion behavior: next level or menu

At runtime, `LevelDefinition` derives available rule cards, stable expected rule ids, animation commands, and required completion events from the enum selections. These derived values are not duplicated in the asset.

#### `LessonRuleItem`

A shared Inspector enum for authored lesson choices. Each value is mapped once by `LessonRuleItemCatalog` to:

- the existing mode that owns it
- stable rule id and player-facing label
- the existing animation enum command
- the existing completion `Events` value

The current enum contains every selectable item for Go Bag, Kitchen, Children Room, Garden View, Shelter, and After the Hurricane. Add the items for a new lesson before creating that lesson's `LevelDefinition` asset.

#### `RuleDefinition`

A serializable rule entry containing:

- stable `ruleId`
- card label and optional icon
- condition, actor, and action identifiers
- matching runtime `Events` value or stable event id
- required/optional flag
- order index or group

Do not use visible UI text as an identifier. Text can change during localization; `levelId`, `ruleId`, and event ids must remain stable.

#### `LevelCatalog`

A `ScriptableObject` containing the ordered list of available `LevelDefinition` assets. `Assets/Data/Lessons/LevelCatalog.asset` is serialized into `GameFlowController` in both runtime scenes and is the source for the main menu and gameplay lookup.

### Flow and state

#### `GameFlowController`

Owns only the high-level state machine:

```text
MainMenu -> Briefing -> RuleBuilder -> ValidatingRules
         -> Playing -> LevelResult -> NextLevel or MainMenu
```

Responsibilities:

- remember the selected level
- open the correct UI screen
- request rule validation
- start the selected `ModeName` after a successful `Check`
- create and finish a gameplay session
- decide whether to continue or return to the menu

It must not directly play animations or manipulate lesson objects.

#### `LevelSessionController`

Owns the runtime state for one attempt:

- current level
- current expected step
- completed steps
- mistakes
- session state
- start/end timestamps if analytics are needed later

It subscribes to gameplay events when play starts and unsubscribes on completion, restart, menu return, and destruction. A fresh session is created for every attempt so progress cannot leak between levels.

#### `RuleSelectionController`

Owns the rules assembled in the pre-start UI. It supports adding, removing, replacing, and reordering cards without knowing anything about scene animations.

#### `RuleValidator`

A plain C# service that can compare the player's selected rules with the expected rules from `LevelDefinition` when needed. The current runtime flow no longer uses it as a gate before starting the lesson, so it should stay independent from `MonoBehaviour`, scene objects, or UI.

Suggested output:

```csharp
public sealed class RuleValidationResult
{
    public bool IsValid;
    public IReadOnlyList<string> MissingRuleIds;
    public IReadOnlyList<string> UnexpectedRuleIds;
    public IReadOnlyList<string> MisorderedRuleIds;
}
```

### Runtime event layer

#### `SimulationEventChannel`

Introduces one Unity-side event stream for completed gameplay actions. The initial API can remain simple:

```csharp
public event Action<SimulationEventData> EventRaised;
public void Raise(Events eventType, string source = null);
```

#### `SimulationEventData`

Carries the information needed by validation and feedback:

- event type
- active `ModeName`
- optional source/object id
- event time

#### `SimulationEventReporter`

Provides the compatibility boundary for existing code. When an existing action completes, it should:

1. raise the Unity-side event for `LevelSessionController`;
2. keep sending the same callback to WebGL;
3. avoid changing animation timing or completion conditions.

During the first integration, existing calls should be migrated one by one through this reporter. Do not replace all event calls in one large edit.

#### `RuntimeStepEvaluator`

A plain C# service that receives one `SimulationEventData` and compares it with the current expected runtime step. It returns one of these results:

- `Correct`
- `Incorrect`
- `OutOfOrder`
- `Ignored`
- `LevelCompleted`

The evaluator observes completed actions. It does not invoke mode functions and does not control animation playback.

### UI

#### UI authoring rule

`GameFlowUI.prefab` is the editable source of truth for permanent screens. It is instantiated and serialized in `MainMenu.unity` and `SampleScene.unity`. `LessonButton.prefab`, `RuleOptionButton.prefab`, and `SelectedRuleRow.prefab` are the only dynamic UI templates in the current flow. Runtime scripts may bind data and callbacks or instantiate these templates, but they may not assemble UI components or layout hierarchies in code.

This keeps the flow visually editable without changing gameplay code and preserves a clear boundary between presentation and orchestration.

The Editor menu command `Tools/Hurricane/Rebuild Game Flow UI Assets` exists only to scaffold or recover the baseline assets. It does not run in the player. Because it replaces the generated prefabs, normal visual work must be done directly in prefab mode and the command should be rerun only intentionally.

#### `MainMenuView`

Builds level buttons/cards from `LevelCatalog` and reports the selected level to `GameFlowController`.

#### `LevelBriefingView`

Shows the level title, learning objective, instructions, and the action required to pass before the player starts assembling rules.

#### `RuleBuilderView`

Displays available rule cards, selected rule slots, ordering controls, validation errors, and the `Check` button.

#### `GameplayHUDView`

Displays current progress and short correct/incorrect feedback without blocking the existing scene animation.

#### `LevelResultView`

Displays success/failure summary and offers `Next Level`, `Retry`, and `Main Menu` according to the current `LevelDefinition`.

Views render state and forward button actions. They should not evaluate rules or call lesson animation functions directly.

## Event Contract

The same completed gameplay action must be visible to both Unity validation and the existing WebGL host:

```text
Existing lesson function or animation event
                  |
                  v
       SimulationEventReporter
          |                 |
          v                 v
SimulationEventChannel   WebGLBridge
          |
          v
 LevelSessionController
          |
          v
 RuntimeStepEvaluator -> Feedback UI / Level completion
```

Rules for this contract:

- Raise an event at the same moment the existing code currently reports completion to WebGL.
- Never mark a step complete when its animation merely starts unless the current gameplay already defines that moment as completion.
- Each physical action should produce one logical completion event. Guard against duplicate animation-event callbacks.
- Events from a mode other than the active level are ignored and logged in development builds.
- Unknown events are logged but do not crash or silently complete a step.
- WebGL callbacks remain active, allowing the browser version and Unity-native validation to coexist.

## Rule Validation Versus Runtime Validation

These are two separate checks and must not be mixed:

| Check | When | Purpose | Failure behavior |
| --- | --- | --- | --- |
| Rule validation | Player presses `Check` before play | Verify the chosen rule cards and their order | Stay in rule builder and explain what is missing or misplaced |
| Runtime validation | Existing scene actions complete | Verify that gameplay follows the accepted rules | Show feedback, record the mistake, and continue or fail according to level configuration |

A successful pre-start `Check` creates an immutable snapshot of the accepted rule selection for the attempt. Editing rules is disabled while the simulation is running.

## Integration With Existing Modes

For every `ModeName`, create a short integration table before changing code:

| Field | Required answer |
| --- | --- |
| Existing start entry point | Which current public method or `OnSimulationStart()` begins the sequence? |
| Existing callable actions | Which methods are currently triggered from WebGL/editor? |
| Completion events | Which `Events` values are already sent, and at what exact animation point? |
| Missing events | Which completed actions currently have no callback? |
| Completion condition | Which event or state means the lesson is complete? |
| Reset requirements | Which queues, flags, objects, timers, and subscriptions must return to their initial state? |

Initial catalog to inventory:

- `House`
- `ClearingGarden`
- `SuperMarket`
- `ChildrenRoom`
- `GardenView`
- `Shelter`
- `AfterTheHurricane`
- `GoBagLesson`
- `KitchenLesson`
- `BathRoomLesson`

Implement one representative lesson end to end first. `GoBagLesson`, `KitchenLesson`, or `BathRoomLesson` is a good candidate because these modes already expose focused action queues and event callbacks. After the vertical slice passes Play Mode testing, apply the same integration pattern to the remaining modes.

## Recommended Folder Layout

```text
Assets/Scripts/
  Flow/
    GameFlowController.cs
    GameFlowState.cs
    LevelSessionController.cs
  Levels/
    LevelCatalog.cs
    LevelDefinition.cs
    RuleDefinition.cs
  Rules/
    RuleValidator.cs
    RuleValidationResult.cs
    RuntimeStepEvaluator.cs
  Events/
    SimulationEventChannel.cs
    SimulationEventData.cs
    SimulationEventReporter.cs
  UI/
    MainMenuView.cs
    LevelBriefingView.cs
    RuleBuilderView.cs
    GameplayHUDView.cs
    LevelResultView.cs
```

Existing mode classes stay in `Assets/Scripts/Modes/`. Do not move them during the working-system phase because Unity script moves and serialized references add avoidable risk.

## Delivery Sprints

### Sprint 0: Verified inventory

- Record every level, action function, outbound event, animation completion point, and reset path.
- Confirm all serialized mode components and UI roots in `SampleScene.unity`.
- Create a manual smoke-test list for current behavior before edits.
- Result: a baseline against which animation behavior can be compared.

### Sprint 1: Flow shell and menu

- Implement the flow state machine.
- Turn `EntryScreenMod` into the menu state.
- Create `LevelCatalog` and one test `LevelDefinition`.
- Select a lesson and open its briefing without starting its mode.
- Result: menu -> briefing -> rule builder navigation works inside the current scene.

### Sprint 2: Rule builder and pre-start `Check`

- Implement rule cards, selection state, ordering, and removal.
- Implement or keep the pure C# `RuleValidator` as an optional helper, not as a pre-start gate.
- Show precise missing/unexpected/order feedback.
- Start the selected mode only after a valid result.
- Result: one lesson can be configured and launched through Unity UI.

### Sprint 3: One complete runtime vertical slice

- Add the event channel and compatibility reporter.
- Route one lesson's existing completion callbacks through it while preserving WebGL calls.
- Implement session tracking, runtime evaluation, feedback, completion, retry, and exit to menu.
- Result: one full level works from menu to final result.

### Sprint 4: Remaining levels

- Author a `LevelDefinition` for every supported mode.
- Integrate existing functions and completion events mode by mode.
- Add only the missing completion hooks required for validation.
- Verify reset/retry and switching from every mode.
- Result: the complete lesson catalog follows one consistent flow.

### Sprint 5: Progression and persistence

- Add next-level navigation.
- Persist unlocked/completed levels with a versioned save model.
- Keep a direct return-to-menu path available.
- Result: players can continue a course without losing valid progress.

### Sprint 6: Stabilization

- Add defensive handling for duplicate, late, and unknown events.
- Check pause/resume behavior and input locking during transitions.
- Verify browser callbacks in a WebGL build.
- Result: the system is ready for content iteration.

### Sprint 7: Refactor only after acceptance

- Extract repeated animation queue code where behavior is genuinely identical.
- Extract shared event mapping and UI helpers.
- Normalize naming and interfaces in small, separately testable changes.
- Keep adapters for legacy WebGL names while migration is in progress.
- Result: clearer architecture with no intentional gameplay change.

## Unity Editor Setup Checklist

- `MainMenu.unity` and `SampleScene.unity` each contain one configured `GameFlowController` with a serialized `GameFlowView` reference.
- Both scenes contain a `GameFlowUI.prefab` instance rather than runtime-generated screens.
- `LessonButton.prefab`, `RuleOptionButton.prefab`, and `SelectedRuleRow.prefab` have all labels and buttons assigned in their view components.
- Runtime Flow scripts contain no `new GameObject` or `AddComponent` UI construction.
- Gameplay UI roots are separate from mode-specific scene roots.
- `LevelCatalog` contains unique level ids in intended progression order.
- Every `LevelDefinition` references an existing `ModeName` and unique rule ids.
- Required icons, localized labels, and descriptions are assigned.
- Event channel/reporter references are assigned once and are not duplicated per mode.
- Buttons have one intended listener and do not retain obsolete Inspector callbacks.
- Existing animator controllers, animation clips, object references, and timers remain assigned.
- Build Settings still include every Unity scene actually used by the selected implementation.

## Play Mode Verification Checklist

Run this checklist first for the vertical-slice lesson and then for every level:

1. Start in the main menu with no gameplay mode active behind it.
2. Select a level and verify the correct briefing text and available rule cards.
3. Submit empty, incomplete, incorrect, and misordered rules; gameplay must not start.
4. Submit valid rules; the correct mode starts exactly once.
5. Verify every existing animation, audio cue, timer, object swap, and transition against the baseline.
6. Perform a correct action and confirm one progress event and one positive feedback message.
7. Perform an incorrect or out-of-order action and confirm the configured feedback without corrupting progress.
8. Trigger a duplicate animation callback and confirm it does not complete two steps.
9. Complete the lesson and confirm that the result screen appears once.
10. Retry and verify that queues, flags, timers, held objects, and progress are reset.
11. Return to the menu and start another level; no callbacks or state from the previous level may remain.
12. Test pause/resume and confirm that validation remains synchronized with the animation state.
13. Build WebGL and confirm that existing browser event names and callbacks still fire.

## Automated Test Targets

Use Edit Mode tests for logic that does not need a Unity scene:

- correct rule set passes
- missing/extra/misordered rules fail with the correct details
- runtime correct/out-of-order/ignored results
- duplicate event protection
- next-level lookup and final-level behavior

Use Play Mode tests where practical for:

- flow state transitions
- subscription cleanup between sessions
- correct `ModeName` activation
- menu/retry/next-level transitions

Automated tests supplement but do not replace manual animation and Inspector verification.

## Logging and Diagnostics

In development builds, log structured entries with:

- level id
- active mode
- session state
- expected event
- received event
- validation result

Keep player-facing feedback short and friendly. Keep technical detail in logs so incorrect level data can be diagnosed without exposing internal ids in the UI.

## Definition of Done for Each Level

A level is complete only when:

- its briefing and rule cards are authored;
- runtime mistakes are reported with useful feedback during play;
- valid rules start the correct existing mode;
- all required existing functions are reachable through the accepted flow;
- correct, incorrect, out-of-order, and duplicate runtime events behave as configured;
- existing animations and timing match the baseline;
- success, retry, next-level, and menu paths reset state correctly;
- Unity Play Mode is manually verified;
- WebGL compatibility is verified in a browser build if that level uses browser callbacks.

## Success Criteria

- The player can choose a level from a menu.
- The player can see what needs to be done before starting.
- `Check` validates the selected rules or setup.
- Gameplay starts only after validation passes.
- Correct and incorrect actions are tracked during the level.
- Completion advances the player to the next level or back to menu.
- Current animations and existing scene logic still work.

## Later Refactor Targets

- Shared level flow controller
- Shared rule validation service
- Shared event mapping helpers
- Shared UI state helpers
- Shared level metadata model
