using NUnit.Framework;

public sealed class RuleSystemTests
{
    [TearDown]
    public void TearDown()
    {
        LessonLaunchContext.Clear();
    }

    private static RuleDefinition Rule(string id, Events eventType)
    {
        return new RuleDefinition(id, id, id, id, eventType);
    }

    [Test]
    public void RuleValidator_AcceptsExactSequence()
    {
        RuleDefinition water = Rule("water", Events.PackWater);
        RuleDefinition flashlight = Rule("flashlight", Events.PackFlashlight);

        RuleValidationResult result = RuleValidator.Validate(
            new[] { water, flashlight },
            new[] { "water", "flashlight" });

        Assert.That(result.IsValid, Is.True);
    }

    [Test]
    public void RuleValidator_ReportsMissingRule()
    {
        RuleDefinition water = Rule("water", Events.PackWater);

        RuleValidationResult result = RuleValidator.Validate(
            new[] { water },
            new[] { "water", "flashlight" });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.MissingRuleIds, Contains.Item("flashlight"));
    }

    [Test]
    public void RuleValidator_RejectsCorrectRulesInWrongOrder()
    {
        RuleDefinition water = Rule("water", Events.PackWater);
        RuleDefinition flashlight = Rule("flashlight", Events.PackFlashlight);

        RuleValidationResult result = RuleValidator.Validate(
            new[] { flashlight, water },
            new[] { "water", "flashlight" });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.MisorderedRuleIds, Is.Not.Empty);
    }

    [Test]
    public void RuntimeEvaluator_CompletesExpectedSequence()
    {
        RuntimeStepEvaluator evaluator = new(new[]
        {
            Events.GobagReminder,
            Events.PackWater
        });

        RuntimeStepResult first = evaluator.Evaluate(Events.GobagReminder);
        RuntimeStepResult second = evaluator.Evaluate(Events.PackWater);

        Assert.That(first.Type, Is.EqualTo(RuntimeStepResultType.Correct));
        Assert.That(second.Type, Is.EqualTo(RuntimeStepResultType.LevelCompleted));
        Assert.That(evaluator.IsComplete, Is.True);
    }

    [Test]
    public void RuntimeEvaluator_ReportsOutOfOrderAndDuplicateEvents()
    {
        RuntimeStepEvaluator evaluator = new(new[]
        {
            Events.GobagReminder,
            Events.PackWater,
            Events.PackBook
        });

        RuntimeStepResult outOfOrder = evaluator.Evaluate(Events.PackBook);
        RuntimeStepResult accepted = evaluator.Evaluate(Events.GobagReminder);
        RuntimeStepResult duplicate = evaluator.Evaluate(Events.GobagReminder);

        Assert.That(outOfOrder.Type, Is.EqualTo(RuntimeStepResultType.OutOfOrder));
        Assert.That(accepted.Type, Is.EqualTo(RuntimeStepResultType.Correct));
        Assert.That(duplicate.Type, Is.EqualTo(RuntimeStepResultType.Duplicate));
    }

    [Test]
    public void LessonLaunchContext_PreservesSelectedRuleOrder()
    {
        RuleDefinition water = Rule("water", Events.PackWater);
        RuleDefinition flashlight = Rule("flashlight", Events.PackFlashlight);

        LessonLaunchContext.SetLesson("go-bag", new[] { water, flashlight });

        Assert.That(LessonLaunchContext.HasLesson, Is.True);
        Assert.That(LessonLaunchContext.LevelId, Is.EqualTo("go-bag"));
        Assert.That(LessonLaunchContext.SelectedRuleIds, Is.EqualTo(new[] { "water", "flashlight" }));
    }
}
