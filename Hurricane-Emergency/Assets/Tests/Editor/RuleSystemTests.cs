using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class RuleSystemTests
{
    private const string CatalogPath = "Assets/Data/Lessons/LevelCatalog.asset";

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

    [Test]
    public void ScriptableObjectCatalog_ContainsAllSixLessons()
    {
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);

        Assert.That(catalog, Is.Not.Null);
        Assert.That(catalog.Levels.Count, Is.EqualTo(6));
        Assert.That(catalog.Levels[0].LevelId, Is.EqualTo("go-bag-prototype"));
        Assert.That(catalog.Levels[1].LevelId, Is.EqualTo("kitchen-lesson"));
        Assert.That(catalog.Levels[2].LevelId, Is.EqualTo("bedroom-lesson"));
        Assert.That(catalog.Levels[3].LevelId, Is.EqualTo("shelter-lesson"));
        Assert.That(catalog.Levels[4].LevelId, Is.EqualTo("after-the-hurricane-lesson"));
        Assert.That(catalog.Levels[5].LevelId, Is.EqualTo("garden-view-lesson"));
        Assert.That(catalog.Levels[1].Mode, Is.EqualTo(ModeName.KitchenLesson));
        Assert.That(catalog.Levels[2].Mode, Is.EqualTo(ModeName.ChildrenRoom));
        Assert.That(catalog.Levels[3].Mode, Is.EqualTo(ModeName.Shelter));
        Assert.That(catalog.Levels[4].Mode, Is.EqualTo(ModeName.AfterTheHurricane));
        Assert.That(catalog.Levels[5].Mode, Is.EqualTo(ModeName.GardenView));
    }

    [Test]
    public void KitchenLesson_UsesOnlyExistingRuntimeEvents()
    {
        LevelDefinition level = LoadLevel(1);
        level.RebuildRuntimeData();

        Assert.That(level.RequiredItems, Is.EqualTo(new[]
        {
            LessonRuleItem.KitchenCannedFood,
            LessonRuleItem.KitchenCrackers,
            LessonRuleItem.KitchenWater
        }));
        Assert.That(level.ExpectedRuleIds, Is.EqualTo(new[]
        {
            "kitchen-canned-food",
            "kitchen-crackers",
            "kitchen-water"
        }));
        Assert.That(level.RequiredRuntimeEvents, Is.EqualTo(new[]
        {
            Events.GobagReminder,
            Events.PackCannedFood,
            Events.PackCrackers,
            Events.PackWater
        }));
    }

    [Test]
    public void BedroomLesson_UsesChildrenRoomEventSequence()
    {
        LevelDefinition level = LoadLevel(2);
        level.RebuildRuntimeData();

        Assert.That(level.RequiredItems, Is.EqualTo(new[]
        {
            LessonRuleItem.BedroomClothes,
            LessonRuleItem.BedroomWater,
            LessonRuleItem.BedroomFlashlight,
            LessonRuleItem.BedroomToy
        }));
        Assert.That(level.ExpectedRuleIds, Is.EqualTo(new[]
        {
            "bedroom-clothes",
            "bedroom-water",
            "bedroom-flashlight",
            "bedroom-toy"
        }));
        Assert.That(level.RequiredRuntimeEvents, Is.EqualTo(new[]
        {
            Events.HurricaneWatch,
            Events.PackClothes,
            Events.PackWater,
            Events.PackFlashlight,
            Events.PackToys
        }));
    }

    [Test]
    public void ShelterLesson_UsesShelterEventSequence()
    {
        LevelDefinition level = LoadLevel(3);
        level.RebuildRuntimeData();

        Assert.That(level.RequiredItems, Is.EqualTo(new[]
        {
            LessonRuleItem.ShelterColoursABook,
            LessonRuleItem.ShelterPlaysWithToy
        }));
        Assert.That(level.ExpectedRuleIds, Is.EqualTo(new[]
        {
            "shelter-colours-a-book",
            "shelter-plays-with-toy"
        }));
        Assert.That(level.RequiredRuntimeEvents, Is.EqualTo(new[]
        {
            Events.ColorBook,
            Events.PlayToy
        }));
    }

    [Test]
    public void AfterTheHurricaneLesson_UsesCleanupEventSequence()
    {
        LevelDefinition level = LoadLevel(4);
        level.RebuildRuntimeData();

        Assert.That(level.RequiredItems, Is.EqualTo(new[]
        {
            LessonRuleItem.AfterHurricanePickBranches,
            LessonRuleItem.AfterHurricanePickBottles,
            LessonRuleItem.AfterHurricaneMotherCutWood
        }));
        Assert.That(level.ExpectedRuleIds, Is.EqualTo(new[]
        {
            "after-hurricane-pick-branches",
            "after-hurricane-pick-bottles",
            "after-hurricane-mother-cut-wood"
        }));
        Assert.That(level.RequiredRuntimeEvents, Is.EqualTo(new[]
        {
            Events.PickBranches,
            Events.PickBottles,
            Events.CutBranches
        }));
    }

    [Test]
    public void GardenViewLesson_UsesGardenEventSequence()
    {
        LevelDefinition level = LoadLevel(5);
        level.RebuildRuntimeData();

        Assert.That(level.RequiredItems, Is.EqualTo(new[]
        {
            LessonRuleItem.GardenViewTakeToys,
            LessonRuleItem.GardenViewTakeBall,
            LessonRuleItem.GardenViewTakeBicycle
        }));
        Assert.That(level.ExpectedRuleIds, Is.EqualTo(new[]
        {
            "garden-view-take-toys",
            "garden-view-take-ball",
            "garden-view-take-bicycle"
        }));
        Assert.That(level.RequiredRuntimeEvents, Is.EqualTo(new[]
        {
            Events.HurricaneWarning,
            Events.GetToys,
            Events.GetBall,
            Events.GetBicycle
        }));
    }

    [Test]
    public void EveryLesson_SeparatesRequiredItemsAndDistractors()
    {
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
        Assert.That(catalog, Is.Not.Null);

        for (int levelIndex = 0; levelIndex < catalog.Levels.Count; levelIndex++)
        {
            LevelDefinition level = catalog.Levels[levelIndex];
            level.RebuildRuntimeData();

            Assert.That(level.AvailableRules.Count,
                Is.EqualTo(level.RequiredItems.Count + level.Distractors.Count),
                level.LevelId);
            Assert.That(level.Distractors.Count,
                Is.GreaterThanOrEqualTo(level.RequiredItems.Count),
                $"{level.LevelId}: every required rule must have one distractor");

            List<RuleDefinition> correctRules = new();
            List<RuleDefinition> distractorRules = new();
            for (int ruleIndex = 0; ruleIndex < level.AvailableRules.Count; ruleIndex++)
            {
                RuleDefinition rule = level.AvailableRules[ruleIndex];
                (rule.IsDistractor ? distractorRules : correctRules).Add(rule);
            }

            Assert.That(correctRules.Count, Is.EqualTo(level.RequiredItems.Count), level.LevelId);
            for (int ruleIndex = 0; ruleIndex < correctRules.Count; ruleIndex++)
            {
                RuleDefinition correct = correctRules[ruleIndex];
                RuleDefinition distractor = null;
                for (int distractorIndex = 0; distractorIndex < distractorRules.Count; distractorIndex++)
                {
                    if (distractorRules[distractorIndex].RuleId == correct.ExclusiveRuleId)
                    {
                        distractor = distractorRules[distractorIndex];
                        break;
                    }
                }

                Assert.That(distractor, Is.Not.Null,
                    $"{level.LevelId}: {correct.RuleId} must have an adjacent distractor pair");
                Assert.That(correct.IsExclusiveWith(distractor), Is.True,
                    $"{level.LevelId}: {correct.RuleId} must be exclusive with {distractor.RuleId}");
                Assert.That(distractor.IsExclusiveWith(correct), Is.True,
                    $"{level.LevelId}: {distractor.RuleId} must be exclusive with {correct.RuleId}");
            }

            for (int ruleIndex = 0; ruleIndex < correctRules.Count; ruleIndex++)
            {
                int availableIndex = ruleIndex * 2;
                Assert.That(level.AvailableRules[availableIndex].IsDistractor, Is.False, level.LevelId);
                Assert.That(level.AvailableRules[availableIndex + 1].IsDistractor, Is.True, level.LevelId);
            }
        }
    }

    [Test]
    public void RuleValidator_RejectsDistractorSelection()
    {
        RuleDefinition correct = new("radio", "Radio", "", "RadioAnnouncement", Events.RadioBroadcast,
            false, "music");
        RuleDefinition distractor = new("music", "Music", "", "PlayRadioSong", Events.Empty,
            true, "radio");

        RuleValidationResult result = RuleValidator.Validate(
            new[] { distractor },
            new[] { correct.RuleId });

        Assert.That(result.IsValid, Is.False);
        Assert.That(result.UnexpectedRuleIds, Contains.Item(distractor.RuleId));
        Assert.That(correct.IsExclusiveWith(distractor), Is.True);
    }

    private static LevelDefinition LoadLevel(int index)
    {
        LevelCatalog catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
        Assert.That(catalog, Is.Not.Null);
        Assert.That(catalog.Levels.Count, Is.GreaterThan(index));
        return catalog.Levels[index];
    }
}
