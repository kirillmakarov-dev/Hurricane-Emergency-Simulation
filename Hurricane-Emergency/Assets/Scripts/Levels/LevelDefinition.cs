using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class RuleDefinition
{
    [SerializeField] private string ruleId;
    [SerializeField] private string displayName;
    [SerializeField] private string description;
    [SerializeField] private string animationCommand;
    [SerializeField] private Events runtimeEvent;
    [SerializeField] private bool distractor;

    public string RuleId => ruleId;
    public string DisplayName => displayName;
    public string Description => description;
    public string AnimationCommand => animationCommand;
    public Events RuntimeEvent => runtimeEvent;
    public bool IsDistractor => distractor;

    public RuleDefinition(
        string ruleId,
        string displayName,
        string description,
        string animationCommand,
        Events runtimeEvent,
        bool distractor = false)
    {
        this.ruleId = ruleId;
        this.displayName = displayName;
        this.description = description;
        this.animationCommand = animationCommand;
        this.runtimeEvent = runtimeEvent;
        this.distractor = distractor;
    }
}

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Hurricane/Level Definition")]
public sealed class LevelDefinition : ScriptableObject
{
    [SerializeField] private string levelId;
    [SerializeField] private string title;
    [TextArea(2, 5)]
    [SerializeField] private string briefing;
    [TextArea(2, 5)]
    [SerializeField] private string objective;
    [SerializeField] private ModeName mode;
    [SerializeField] private List<RuleDefinition> availableRules = new();
    [SerializeField] private List<string> expectedRuleIds = new();
    [SerializeField] private List<Events> requiredRuntimeEvents = new();

    public string LevelId => levelId;
    public string Title => title;
    public string Briefing => briefing;
    public string Objective => objective;
    public ModeName Mode => mode;
    public IReadOnlyList<RuleDefinition> AvailableRules => availableRules;
    public IReadOnlyList<string> ExpectedRuleIds => expectedRuleIds;
    public IReadOnlyList<Events> RequiredRuntimeEvents => requiredRuntimeEvents;

    public void ConfigureRuntime(
        string id,
        string displayTitle,
        string levelBriefing,
        string levelObjective,
        ModeName targetMode,
        IEnumerable<RuleDefinition> rules,
        IEnumerable<string> expectedRules,
        IEnumerable<Events> runtimeEvents)
    {
        levelId = id;
        title = displayTitle;
        briefing = levelBriefing;
        objective = levelObjective;
        mode = targetMode;
        availableRules = new List<RuleDefinition>(rules);
        expectedRuleIds = new List<string>(expectedRules);
        requiredRuntimeEvents = new List<Events>(runtimeEvents);
    }
}

public static class GoBagPrototypeLevelFactory
{
    public static LevelDefinition Create()
    {
        LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
        level.name = "GoBagPrototypeLevel";
        level.hideFlags = HideFlags.DontSave;

        RuleDefinition water = new(
            "pack-water",
            "Pack water",
            "Add drinking water to the emergency bag.",
            GoBagLesson.GaBagAnimations.KelenTakeWater.ToString(),
            Events.PackWater);

        RuleDefinition flashlight = new(
            "pack-flashlight",
            "Pack a flashlight",
            "Add a flashlight in case the power goes out.",
            GoBagLesson.GaBagAnimations.KelenTakeFlashlight.ToString(),
            Events.PackFlashlight);

        RuleDefinition books = new(
            "pack-books",
            "Pack books",
            "Add a book for a longer stay in the shelter.",
            GoBagLesson.GaBagAnimations.KelenTakeBooks.ToString(),
            Events.PackBook);

        RuleDefinition candles = new(
            "pack-candles",
            "Pack candles",
            "An open flame is unsafe during an emergency.",
            GoBagLesson.GaBagAnimations.KelenTakeCandels.ToString(),
            Events.Empty,
            true);

        RuleDefinition scissors = new(
            "pack-scissors",
            "Pack scissors",
            "This is not required for this lesson's emergency bag.",
            GoBagLesson.GaBagAnimations.KelenTakeScissors.ToString(),
            Events.Empty,
            true);

        RuleDefinition ball = new(
            "pack-ball",
            "Pack a ball",
            "The ball takes space needed by essential supplies.",
            GoBagLesson.GaBagAnimations.KelanTakeBall.ToString(),
            Events.Empty,
            true);

        level.ConfigureRuntime(
            "go-bag-prototype",
            "Build a Go Bag",
            "Kelan's family is preparing for a hurricane. Choose the essential items and arrange the actions before the simulation begins.",
            "After the parents give a reminder, pack water, a flashlight, and books in that order.",
            ModeName.GoBagLesson,
            new[] { water, flashlight, books, candles, scissors, ball },
            new[] { water.RuleId, flashlight.RuleId, books.RuleId },
            new[] { Events.GobagReminder, Events.PackWater, Events.PackFlashlight, Events.PackBook });

        return level;
    }
}
