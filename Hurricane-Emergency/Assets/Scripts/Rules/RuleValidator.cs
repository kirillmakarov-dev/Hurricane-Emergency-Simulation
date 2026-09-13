using System.Collections.Generic;

public sealed class RuleValidationResult
{
    public bool IsValid { get; }
    public string Message { get; }
    public IReadOnlyList<string> MissingRuleIds { get; }
    public IReadOnlyList<string> UnexpectedRuleIds { get; }
    public IReadOnlyList<string> MisorderedRuleIds { get; }

    public RuleValidationResult(
        bool isValid,
        string message,
        IReadOnlyList<string> missingRuleIds,
        IReadOnlyList<string> unexpectedRuleIds,
        IReadOnlyList<string> misorderedRuleIds)
    {
        IsValid = isValid;
        Message = message;
        MissingRuleIds = missingRuleIds;
        UnexpectedRuleIds = unexpectedRuleIds;
        MisorderedRuleIds = misorderedRuleIds;
    }
}

public static class RuleValidator
{
    public static RuleValidationResult Validate(
        IReadOnlyList<RuleDefinition> selectedRules,
        IReadOnlyList<string> expectedRuleIds,
        bool requireOrder = true)
    {
        List<string> selectedIds = new();
        for (int i = 0; i < selectedRules.Count; i++)
        {
            selectedIds.Add(selectedRules[i].RuleId);
        }

        List<string> missing = new();
        List<string> unexpected = new();
        List<string> misordered = new();

        for (int i = 0; i < expectedRuleIds.Count; i++)
        {
            if (!selectedIds.Contains(expectedRuleIds[i]))
            {
                missing.Add(expectedRuleIds[i]);
            }
        }

        for (int i = 0; i < selectedIds.Count; i++)
        {
            if (!Contains(expectedRuleIds, selectedIds[i]))
            {
                unexpected.Add(selectedIds[i]);
            }
        }

        if (requireOrder && missing.Count == 0 && unexpected.Count == 0 && selectedIds.Count == expectedRuleIds.Count)
        {
            for (int i = 0; i < expectedRuleIds.Count; i++)
            {
                if (selectedIds[i] != expectedRuleIds[i])
                {
                    misordered.Add(selectedIds[i]);
                }
            }
        }

        if (missing.Count > 0)
        {
            return new RuleValidationResult(false, "Some essential actions are still missing.", missing, unexpected, misordered);
        }

        if (unexpected.Count > 0)
        {
            return new RuleValidationResult(false, "Remove the unsafe or unnecessary actions.", missing, unexpected, misordered);
        }

        if (selectedIds.Count != expectedRuleIds.Count)
        {
            return new RuleValidationResult(false, "Use exactly the required number of actions.", missing, unexpected, misordered);
        }

        if (misordered.Count > 0)
        {
            return new RuleValidationResult(false, "The actions are correct, but their order needs attention.", missing, unexpected, misordered);
        }

        return new RuleValidationResult(true, "Rules accepted. The simulation is ready.", missing, unexpected, misordered);
    }

    private static bool Contains(IReadOnlyList<string> values, string candidate)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == candidate)
            {
                return true;
            }
        }

        return false;
    }
}
