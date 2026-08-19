using System.Collections.Generic;
using UnityEngine;

public static class LessonLaunchContext
{
    private static readonly List<string> selectedRuleIds = new();

    public static string LevelId { get; private set; }
    public static IReadOnlyList<string> SelectedRuleIds => selectedRuleIds;
    public static bool HasLesson => !string.IsNullOrEmpty(LevelId) && selectedRuleIds.Count > 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        Clear();
    }

    public static void SetLesson(string levelId, IEnumerable<RuleDefinition> selectedRules)
    {
        LevelId = levelId;
        selectedRuleIds.Clear();

        foreach (RuleDefinition rule in selectedRules)
        {
            selectedRuleIds.Add(rule.RuleId);
        }
    }

    public static void Clear()
    {
        LevelId = null;
        selectedRuleIds.Clear();
    }
}
