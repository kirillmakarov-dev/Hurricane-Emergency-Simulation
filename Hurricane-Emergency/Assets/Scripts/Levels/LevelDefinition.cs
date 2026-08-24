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
    [SerializeField] private string exclusiveRuleId;

    public string RuleId => ruleId;
    public string DisplayName => displayName;
    public string Description => description;
    public string AnimationCommand => animationCommand;
    public Events RuntimeEvent => runtimeEvent;
    public bool IsDistractor => distractor;
    public string ExclusiveRuleId => exclusiveRuleId;

    public RuleDefinition(
        string ruleId,
        string displayName,
        string description,
        string animationCommand,
        Events runtimeEvent,
        bool distractor = false,
        string exclusiveRuleId = null)
    {
        this.ruleId = ruleId;
        this.displayName = displayName;
        this.description = description;
        this.animationCommand = animationCommand;
        this.runtimeEvent = runtimeEvent;
        this.distractor = distractor;
        this.exclusiveRuleId = exclusiveRuleId;
    }

    public bool IsExclusiveWith(RuleDefinition other)
    {
        return other != null &&
            (exclusiveRuleId == other.ruleId || other.exclusiveRuleId == ruleId);
    }
}

[CreateAssetMenu(fileName = "LevelDefinition", menuName = "Hurricane/Level Definition")]
public sealed class LevelDefinition : ScriptableObject
{
    [Header("Lesson")]
    [SerializeField] private string levelId;
    [InspectorName("Card Title")]
    [Tooltip("Lesson name shown on its card in the main menu.")]
    [SerializeField] private string title;
    [InspectorName("Briefing Text")]
    [TextArea(2, 5)]
    [SerializeField] private string briefing;
    [InspectorName("Card Description / Objective")]
    [Tooltip("Short description shown on the lesson card and reused as the level objective.")]
    [TextArea(2, 5)]
    [SerializeField] private string objective;
    [SerializeField] private ModeName mode;
    [InspectorName("Card Background")]
    [Tooltip("Background image assigned to this lesson card in the main menu.")]
    [SerializeField] private Sprite thumbnail;

    [Header("Rule Selection")]
    [Tooltip("Correct items in the exact order required to pass Check.")]
    [SerializeField] private List<LessonRuleItem> requiredItems = new();
    [Tooltip("Incorrect options shown together with the required items.")]
    [SerializeField] private List<LessonRuleItem> distractors = new();

    [Header("Runtime")]
    [Tooltip("Events that happen before the selected item sequence, such as a reminder or warning.")]
    [SerializeField] private List<Events> openingRuntimeEvents = new();

    [NonSerialized] private readonly List<RuleDefinition> availableRules = new();
    [NonSerialized] private readonly List<string> expectedRuleIds = new();
    [NonSerialized] private readonly List<Events> requiredRuntimeEvents = new();
    [NonSerialized] private bool runtimeDataBuilt;

    public string LevelId => levelId;
    public string Title => title;
    public string Briefing => briefing;
    public string Objective => objective;
    public ModeName Mode => mode;
    public Sprite Thumbnail => thumbnail;
    public string CardTitle => title;
    public string CardDescription => objective;
    public Sprite CardBackground => thumbnail;
    public IReadOnlyList<LessonRuleItem> RequiredItems => requiredItems;
    public IReadOnlyList<LessonRuleItem> Distractors => distractors;
    public IReadOnlyList<RuleDefinition> AvailableRules { get { EnsureRuntimeData(); return availableRules; } }
    public IReadOnlyList<string> ExpectedRuleIds { get { EnsureRuntimeData(); return expectedRuleIds; } }
    public IReadOnlyList<Events> RequiredRuntimeEvents { get { EnsureRuntimeData(); return requiredRuntimeEvents; } }

    public void RebuildRuntimeData()
    {
        runtimeDataBuilt = true;
        availableRules.Clear();
        expectedRuleIds.Clear();
        requiredRuntimeEvents.Clear();
        requiredRuntimeEvents.AddRange(openingRuntimeEvents);

        if (distractors.Count < requiredItems.Count)
        {
            Debug.LogError(
                $"Level '{levelId}' must define at least one distractor for each required rule.",
                this);
        }

        HashSet<LessonRuleItem> usedItems = new();
        List<RuleDefinition> correctRules = new();
        List<RuleDefinition> distractorRules = new();

        for (int i = 0; i < requiredItems.Count; i++)
        {
            string distractorId = null;
            if (i < distractors.Count &&
                LessonRuleItemCatalog.TryGet(distractors[i], out LessonRuleDescriptor distractorDescriptor))
            {
                distractorId = distractorDescriptor.RuleId;
            }

            if (!TryAddItem(requiredItems[i], false, usedItems, distractorId, out RuleDefinition rule)) continue;
            correctRules.Add(rule);
            expectedRuleIds.Add(rule.RuleId);
            if (rule.RuntimeEvent != Events.Empty) requiredRuntimeEvents.Add(rule.RuntimeEvent);
        }

        for (int i = 0; i < distractors.Count; i++)
        {
            string correctRuleId = i < expectedRuleIds.Count ? expectedRuleIds[i] : null;
            if (!TryAddItem(distractors[i], true, usedItems, correctRuleId, out RuleDefinition rule)) continue;
            distractorRules.Add(rule);
        }

        // Keep each correct rule directly next to its paired distractor in the
        // rule builder. Extra distractors are appended after the complete pairs.
        for (int i = 0; i < correctRules.Count; i++)
        {
            availableRules.Add(correctRules[i]);
            if (i < distractorRules.Count)
            {
                availableRules.Add(distractorRules[i]);
            }
        }

        for (int i = correctRules.Count; i < distractorRules.Count; i++)
        {
            availableRules.Add(distractorRules[i]);
        }
    }

    private bool TryAddItem(
        LessonRuleItem item,
        bool isDistractor,
        HashSet<LessonRuleItem> usedItems,
        string exclusiveRuleId,
        out RuleDefinition rule)
    {
        rule = null;
        if (!usedItems.Add(item))
        {
            Debug.LogError($"Level '{levelId}' contains duplicate item '{item}'.", this);
            return false;
        }

        if (!LessonRuleItemCatalog.TryGet(item, out LessonRuleDescriptor descriptor))
        {
            Debug.LogError($"Level '{levelId}' contains unmapped item '{item}'.", this);
            return false;
        }

        if (descriptor.Mode != mode)
        {
            Debug.LogError($"Item '{item}' belongs to {descriptor.Mode}, but level '{levelId}' uses {mode}.", this);
            return false;
        }

        rule = descriptor.CreateRule(isDistractor, exclusiveRuleId);
        return true;
    }

    private void EnsureRuntimeData()
    {
        if (!runtimeDataBuilt) RebuildRuntimeData();
    }

    private void OnEnable()
    {
        runtimeDataBuilt = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        runtimeDataBuilt = false;
        if (!string.IsNullOrWhiteSpace(levelId)) RebuildRuntimeData();
    }

    public void ConfigureEditor(
        string id,
        string displayTitle,
        string levelBriefing,
        string levelObjective,
        ModeName targetMode,
        Sprite levelThumbnail,
        IEnumerable<LessonRuleItem> correctItems,
        IEnumerable<LessonRuleItem> incorrectItems,
        IEnumerable<Events> openingEvents)
    {
        levelId = id;
        title = displayTitle;
        briefing = levelBriefing;
        objective = levelObjective;
        mode = targetMode;
        thumbnail = levelThumbnail;
        requiredItems = new List<LessonRuleItem>(correctItems);
        distractors = new List<LessonRuleItem>(incorrectItems);
        openingRuntimeEvents = new List<Events>(openingEvents);
        runtimeDataBuilt = false;
        RebuildRuntimeData();
    }
#endif
}
