using UnityEngine;

public enum LessonRuleItem
{
    [InspectorName("Go Bag / Water")] GoBagWater = 0,
    [InspectorName("Go Bag / Flashlight")] GoBagFlashlight = 1,
    [InspectorName("Go Bag / Books")] GoBagBooks = 2,
    [InspectorName("Go Bag / Candles")] GoBagCandles = 3,
    [InspectorName("Go Bag / Scissors")] GoBagScissors = 4,
    [InspectorName("Go Bag / Ball")] GoBagBall = 5,

    [InspectorName("Kitchen / Canned Food")] KitchenCannedFood = 6,
    [InspectorName("Kitchen / Crackers")] KitchenCrackers = 7,
    [InspectorName("Kitchen / Water")] KitchenWater = 8,
    [InspectorName("Kitchen / Cheese")] KitchenCheese = 9,
    [InspectorName("Kitchen / Eggs")] KitchenEggs = 10,
    [InspectorName("Kitchen / Chicken")] KitchenChicken = 11,
    [InspectorName("Kitchen / Fish")] KitchenFish = 12,

    [InspectorName("Bedroom / Clothes")] BedroomClothes = 13,
    [InspectorName("Bedroom / Water")] BedroomWater = 14,
    [InspectorName("Bedroom / Flashlight")] BedroomFlashlight = 15,
    [InspectorName("Bedroom / Toy")] BedroomToy = 16,
    [InspectorName("Bedroom / Fruit")] BedroomFruit = 17,
    [InspectorName("Bedroom / Lamp")] BedroomLamp = 18,
    [InspectorName("Bedroom / Aquarium")] BedroomAquarium = 19
}

public readonly struct LessonRuleDescriptor
{
    public string RuleId { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string AnimationCommand { get; }
    public Events RuntimeEvent { get; }
    public ModeName Mode { get; }

    public LessonRuleDescriptor(
        string ruleId,
        string displayName,
        string description,
        string animationCommand,
        Events runtimeEvent,
        ModeName mode)
    {
        RuleId = ruleId;
        DisplayName = displayName;
        Description = description;
        AnimationCommand = animationCommand;
        RuntimeEvent = runtimeEvent;
        Mode = mode;
    }

    public RuleDefinition CreateRule(bool isDistractor)
    {
        string ruleDescription = isDistractor
            ? "This item is not required for this emergency lesson."
            : Description;

        return new RuleDefinition(
            RuleId,
            DisplayName,
            ruleDescription,
            AnimationCommand,
            RuntimeEvent,
            isDistractor);
    }
}

public static class LessonRuleItemCatalog
{
    public static bool TryGet(LessonRuleItem item, out LessonRuleDescriptor descriptor)
    {
        descriptor = item switch
        {
            LessonRuleItem.GoBagWater => Create(
                "pack-water", "Pack water", "Add drinking water to the emergency bag.",
                GoBagLesson.GaBagAnimations.KelenTakeWater, Events.PackWater, ModeName.GoBagLesson),
            LessonRuleItem.GoBagFlashlight => Create(
                "pack-flashlight", "Pack a flashlight", "Add a flashlight in case the power goes out.",
                GoBagLesson.GaBagAnimations.KelenTakeFlashlight, Events.PackFlashlight, ModeName.GoBagLesson),
            LessonRuleItem.GoBagBooks => Create(
                "pack-books", "Pack books", "Add a book for a longer stay in the shelter.",
                GoBagLesson.GaBagAnimations.KelenTakeBooks, Events.PackBook, ModeName.GoBagLesson),
            LessonRuleItem.GoBagCandles => Create(
                "pack-candles", "Pack candles", "An open flame is unsafe during an emergency.",
                GoBagLesson.GaBagAnimations.KelenTakeCandels, Events.Empty, ModeName.GoBagLesson),
            LessonRuleItem.GoBagScissors => Create(
                "pack-scissors", "Pack scissors", "This is not required for this lesson's emergency bag.",
                GoBagLesson.GaBagAnimations.KelenTakeScissors, Events.Empty, ModeName.GoBagLesson),
            LessonRuleItem.GoBagBall => Create(
                "pack-ball", "Pack a ball", "The ball takes space needed by essential supplies.",
                GoBagLesson.GaBagAnimations.KelanTakeBall, Events.Empty, ModeName.GoBagLesson),

            LessonRuleItem.KitchenCannedFood => Create(
                "kitchen-canned-food", "Pack canned food", "Choose shelf-stable food for the emergency supply.",
                KitchenLesson.KitchenAnimations.KayTakeCannedFood, Events.PackCannedFood, ModeName.KitchenLesson),
            LessonRuleItem.KitchenCrackers => Create(
                "kitchen-crackers", "Pack crackers", "Add dry food that is easy to store and carry.",
                KitchenLesson.KitchenAnimations.KayTakeCrackers, Events.PackCrackers, ModeName.KitchenLesson),
            LessonRuleItem.KitchenWater => Create(
                "kitchen-water", "Pack water", "Add drinking water to the emergency supplies.",
                KitchenLesson.KitchenAnimations.KayTakeWater, Events.PackWater, ModeName.KitchenLesson),
            LessonRuleItem.KitchenCheese => Create(
                "kitchen-cheese", "Pack cheese", "Choose food that can travel safely without refrigeration.",
                KitchenLesson.KitchenAnimations.KayTakeChees, Events.Empty, ModeName.KitchenLesson),
            LessonRuleItem.KitchenEggs => Create(
                "kitchen-eggs", "Pack eggs", "Choose food that can travel safely without refrigeration.",
                KitchenLesson.KitchenAnimations.KayTakeEggs, Events.Empty, ModeName.KitchenLesson),
            LessonRuleItem.KitchenChicken => Create(
                "kitchen-chicken", "Pack chicken", "Choose food that can travel safely without refrigeration.",
                KitchenLesson.KitchenAnimations.KayTakeChicken, Events.Empty, ModeName.KitchenLesson),
            LessonRuleItem.KitchenFish => Create(
                "kitchen-fish", "Pack fish", "Choose food that can travel safely without refrigeration.",
                KitchenLesson.KitchenAnimations.KayTakeFish, Events.Empty, ModeName.KitchenLesson),

            LessonRuleItem.BedroomClothes => Create(
                "bedroom-clothes", "Pack clothes", "Add a change of clothes for a possible shelter stay.",
                Animations.KelenTakeTshirt, Events.PackClothes, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomWater => Create(
                "bedroom-water", "Pack water", "Add drinking water to the emergency bag.",
                Animations.KelenTakeWater, Events.PackWater, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomFlashlight => Create(
                "bedroom-flashlight", "Pack a flashlight", "Add a flashlight in case the power goes out.",
                Animations.KelenTakeFlashlight, Events.PackFlashlight, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomToy => Create(
                "bedroom-toy", "Pack one toy", "Add one comfort item for the shelter.",
                Animations.KelenTakeToy, Events.PackToys, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomFruit => Create(
                "bedroom-fruit", "Pack fruit", "This item is not required for the bedroom lesson.",
                Animations.KelenTakeFruits, Events.Empty, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomLamp => Create(
                "bedroom-lamp", "Pack a lamp", "This item is not required for the bedroom lesson.",
                Animations.KelenTakeLamp, Events.Empty, ModeName.ChildrenRoom),
            LessonRuleItem.BedroomAquarium => Create(
                "bedroom-aquarium", "Pack the aquarium", "This item is not required for the bedroom lesson.",
                Animations.KelenTakeAquarium, Events.Empty, ModeName.ChildrenRoom),
            _ => default
        };

        return !string.IsNullOrEmpty(descriptor.RuleId);
    }

    private static LessonRuleDescriptor Create<TAnimation>(
        string id,
        string displayName,
        string description,
        TAnimation animation,
        Events runtimeEvent,
        ModeName mode)
        where TAnimation : System.Enum
    {
        return new LessonRuleDescriptor(
            id,
            displayName,
            description,
            animation.ToString(),
            runtimeEvent,
            mode);
    }
}
