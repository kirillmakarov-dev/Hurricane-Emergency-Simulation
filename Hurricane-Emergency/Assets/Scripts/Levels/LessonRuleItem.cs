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
    [InspectorName("Bedroom / Aquarium")] BedroomAquarium = 19,

    [InspectorName("Shelter / Colours a book")] ShelterColoursABook = 20,
    [InspectorName("Shelter / Plays with toy")] ShelterPlaysWithToy = 21,
    [InspectorName("Shelter / Plays outside")] ShelterPlaysOutside = 22,
    [InspectorName("Shelter / Talks to a stranger")] ShelterTalksToAStranger = 23,

    [InspectorName("After the Hurricane / Pick up branches")] AfterHurricanePickBranches = 24,
    [InspectorName("After the Hurricane / Pick up bottles")] AfterHurricanePickBottles = 25,
    [InspectorName("After the Hurricane / Pick up broken glass")] AfterHurricanePickBrokenGlass = 26,
    [InspectorName("After the Hurricane / Pick up electric wires")] AfterHurricanePickElectricWires = 27,
    [InspectorName("After the Hurricane / Father picks up broken glass")] AfterHurricaneFatherPickBrokenGlass = 28,
    [InspectorName("After the Hurricane / Father picks up electric wires")] AfterHurricaneFatherPickElectricWires = 29,
    [InspectorName("After the Hurricane / Go for a walk")] AfterHurricaneGoForWalk = 30,
    [InspectorName("After the Hurricane / Mother cuts wood")] AfterHurricaneMotherCutWood = 31,

    [InspectorName("Garden View / Hurricane warning")] GardenViewHurricaneWarning = 32,
    [InspectorName("Garden View / Take toys")] GardenViewTakeToys = 33,
    [InspectorName("Garden View / Take ball")] GardenViewTakeBall = 34,
    [InspectorName("Garden View / Take bicycle")] GardenViewTakeBicycle = 35,
    [InspectorName("Garden View / Go for a walk")] GardenViewGoForWalk = 36,
    [InspectorName("Garden View / Pick flowers")] GardenViewPickFlowers = 37,

    [InspectorName("Bathroom / First aid kit")] BathroomFirstAidKit = 38,
    [InspectorName("Bathroom / Toothbrush")] BathroomToothbrush = 39,
    [InspectorName("Bathroom / Wipes")] BathroomWipes = 40,
    [InspectorName("Bathroom / Soap")] BathroomSoap = 41,
    [InspectorName("Bathroom / Hair dryer")] BathroomHairDryer = 42,
    [InspectorName("Bathroom / Pump")] BathroomPump = 43,
    [InspectorName("Bathroom / Washing gel")] BathroomWashingGel = 44,
    [InspectorName("Bathroom / Cleaning spray")] BathroomCleaningSpray = 45,

    [InspectorName("House / Radio announcement")] HouseRadioAnnouncement = 46,
    [InspectorName("House / Review emergency plan")] HouseReviewEmergencyPlan = 47,
    [InspectorName("House / Check Go Bag")] HouseCheckGoBag = 48,
    [InspectorName("House / Play radio song")] HousePlayRadioSong = 49,
    [InspectorName("House / Parents panic")] HouseParentsPanic = 50,

    [InspectorName("Cleaning Garden / Clear yard")] CleaningGardenClearYard = 51,
    [InspectorName("Cleaning Garden / Gather plywood")] CleaningGardenGatherPlywood = 52,
    [InspectorName("Cleaning Garden / Water flowers")] CleaningGardenWaterFlowers = 53,
    [InspectorName("Cleaning Garden / Go for a walk")] CleaningGardenGoForWalk = 54,

    [InspectorName("Supermarket / Canned food")] SupermarketCannedFood = 55,
    [InspectorName("Supermarket / Crackers")] SupermarketCrackers = 56,
    [InspectorName("Supermarket / Water")] SupermarketWater = 57,
    [InspectorName("Supermarket / Cheese")] SupermarketCheese = 58,
    [InspectorName("Supermarket / Eggs")] SupermarketEggs = 59,
    [InspectorName("Supermarket / Chicken")] SupermarketChicken = 60,
    [InspectorName("Supermarket / Fish")] SupermarketFish = 61,

    [InspectorName("House / Watch TV")] HouseWatchTV = 62,

    [InspectorName("Garden View / Water flowers")] GardenViewWaterFlowers = 63,
    [InspectorName("Bedroom / Chicken")] BedroomChicken = 64
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

    public RuleDefinition CreateRule(bool isDistractor, string exclusiveRuleId = null)
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
            isDistractor,
            exclusiveRuleId);
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
            LessonRuleItem.BedroomChicken => Create(
                "bedroom-chicken", "Pack chicken", "This item is not required for the bedroom lesson.",
                Animations.KelenTakeChicken, Events.Empty, ModeName.ChildrenRoom),

            LessonRuleItem.ShelterColoursABook => Create(
                "shelter-colours-a-book", "Colour a book", "Keep Kay calm with a quiet indoor activity while waiting in shelter.",
                ShelterMod.ShelterAnimations.ColoursABook, Events.ColorBook, ModeName.Shelter),
            LessonRuleItem.ShelterPlaysWithToy => Create(
                "shelter-plays-with-toy", "Play with a toy", "A simple toy is a safe shelter activity.",
                ShelterMod.ShelterAnimations.PlaysWithToy, Events.PlayToy, ModeName.Shelter),
            LessonRuleItem.ShelterPlaysOutside => Create(
                "shelter-plays-outside", "Play outside", "Going outside is not safe while sheltering.",
                ShelterMod.ShelterAnimations.PlaysOutside, Events.Empty, ModeName.Shelter),
            LessonRuleItem.ShelterTalksToAStranger => Create(
                "shelter-talks-to-a-stranger", "Talk to a stranger", "Do not leave shelter to talk to strangers.",
                ShelterMod.ShelterAnimations.TalksToAStranger, Events.Empty, ModeName.Shelter),

            LessonRuleItem.AfterHurricanePickBranches => Create(
                "after-hurricane-pick-branches", "Pick up branches", "Clear safe debris from the yard after the storm.",
                AfterHurricaneAnimations.picksUpBranches, Events.PickBranches, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricanePickBottles => Create(
                "after-hurricane-pick-bottles", "Pick up bottles", "Collect safe debris from the yard after the storm.",
                AfterHurricaneAnimations.picksUpBottles, Events.PickBottles, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricanePickBrokenGlass => Create(
                "after-hurricane-pick-broken-glass", "Pick up broken glass", "Broken glass is unsafe and should not be handled by the child.",
                AfterHurricaneAnimations.picksUpBrockenGlass, Events.PickGlass, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricanePickElectricWires => Create(
                "after-hurricane-pick-electric-wires", "Pick up electric wires", "Electric wires are unsafe and must be avoided.",
                AfterHurricaneAnimations.picksUpElectricWires, Events.Empty, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricaneFatherPickBrokenGlass => Create(
                "after-hurricane-father-pick-broken-glass", "Father picks up broken glass", "An adult can clear broken glass carefully if the lesson calls for it.",
                AfterHurricaneAnimations.FatherPicksUpBrockenGlass, Events.PickGlass, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricaneFatherPickElectricWires => Create(
                "after-hurricane-father-pick-electric-wires", "Father picks up electric wires", "Electric wires are unsafe and must be avoided.",
                AfterHurricaneAnimations.FatherPicksUpElectricWires, Events.Empty, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricaneGoForWalk => Create(
                "after-hurricane-go-for-walk", "Go for a walk", "This is not part of the cleanup lesson.",
                AfterHurricaneAnimations.GoForWalk, Events.Empty, ModeName.AfterTheHurricane),
            LessonRuleItem.AfterHurricaneMotherCutWood => Create(
                "after-hurricane-mother-cut-wood", "Mother cuts wood", "Clear storm debris safely as part of the cleanup.",
                AfterHurricaneAnimations.MotherCutWood, Events.CutBranches, ModeName.AfterTheHurricane),

            LessonRuleItem.GardenViewHurricaneWarning => Create(
                "garden-view-hurricane-warning", "Hurricane warning", "The warning should be heard before the yard actions begin.",
                Animations.HurricaneWatchAnnouncement, Events.HurricaneWarning, ModeName.GardenView),
            LessonRuleItem.GardenViewTakeToys => Create(
                "garden-view-take-toys", "Take toys", "Gather outdoor toys before the storm reaches the yard.",
                Animations.kelanTaketoys, Events.GetToys, ModeName.GardenView),
            LessonRuleItem.GardenViewTakeBall => Create(
                "garden-view-take-ball", "Take ball", "Gather the ball before the storm reaches the yard.",
                Animations.kelanTakeBall, Events.GetBall, ModeName.GardenView),
            LessonRuleItem.GardenViewTakeBicycle => Create(
                "garden-view-take-bicycle", "Take bicycle", "Move the bicycle to safety before the storm reaches the yard.",
                Animations.keyTakesBicycle, Events.GetBicycle, ModeName.GardenView),
            LessonRuleItem.GardenViewGoForWalk => Create(
                "garden-view-go-for-walk", "Go for a walk", "Going for a walk is not safe during the warning.",
                Animations.kelanGoforWalk, Events.Empty, ModeName.GardenView),
            LessonRuleItem.GardenViewPickFlowers => Create(
                "garden-view-pick-flowers", "Pick flowers", "Pick flowers is not part of the safety lesson.",
                Animations.keyPickFlowers, Events.Empty, ModeName.GardenView),
            LessonRuleItem.GardenViewWaterFlowers => Create(
                "garden-view-water-flowers", "Water flowers", "Watering flowers is not part of preparing the yard for a hurricane.",
                Animations.GardenWaterFlowers, Events.Empty, ModeName.GardenView),

            LessonRuleItem.BathroomFirstAidKit => Create(
                "bathroom-first-aid-kit", "Pack first aid kit", "Add first aid supplies for small injuries and emergencies.",
                BathRoomLesson.BathRoomAnimations.PackFirstAid, Events.PackFirstAid, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomToothbrush => Create(
                "bathroom-toothbrush", "Pack toothbrush", "A toothbrush belongs in the bathroom emergency kit.",
                BathRoomLesson.BathRoomAnimations.PackToothbrush, Events.PackToothbrush, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomWipes => Create(
                "bathroom-wipes", "Pack wipes", "Pack wipes so the family can stay clean while away from home.",
                BathRoomLesson.BathRoomAnimations.PackWipes, Events.PackWipes, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomSoap => Create(
                "bathroom-soap", "Pack soap", "Soap helps the family stay clean and healthy.",
                BathRoomLesson.BathRoomAnimations.PackSoap, Events.PackSoap, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomHairDryer => Create(
                "bathroom-hair-dryer", "Pack hair dryer", "This is not required for the bathroom lesson.",
                BathRoomLesson.BathRoomAnimations.PuckHairDryer, Events.Empty, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomPump => Create(
                "bathroom-pump", "Pack pump", "This is not required for the bathroom lesson.",
                BathRoomLesson.BathRoomAnimations.PackPump, Events.Empty, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomWashingGel => Create(
                "bathroom-washing-gel", "Pack washing gel", "This is not required for the bathroom lesson.",
                BathRoomLesson.BathRoomAnimations.PackWashingGel, Events.Empty, ModeName.BathRoomLesson),
            LessonRuleItem.BathroomCleaningSpray => Create(
                "bathroom-cleaning-spray", "Pack cleaning spray", "This is not required for the bathroom lesson.",
                BathRoomLesson.BathRoomAnimations.PackCleaningSpray, Events.Empty, ModeName.BathRoomLesson),

            LessonRuleItem.HouseRadioAnnouncement => Create(
                "house-radio-announcement", "Listen to the radio announcement", "Listen for official hurricane information.",
                HouseAnimations.RadioAnnouncement, Events.RadioBroadcast, ModeName.House),
            LessonRuleItem.HouseReviewEmergencyPlan => Create(
                "house-review-emergency-plan", "Review the emergency plan", "Review the family plan before the storm arrives.",
                HouseAnimations.ReviewEmergencyPlan, Events.ReviewEmergencyPlan, ModeName.House),
            LessonRuleItem.HouseCheckGoBag => Create(
                "house-check-go-bag", "Check the Go Bag", "Make sure the emergency bag is ready to take.",
                HouseAnimations.CheckGoBag, Events.CheckGoBag, ModeName.House),
            LessonRuleItem.HousePlayRadioSong => Create(
                "house-play-radio-song", "Play music on the radio", "Music does not provide the emergency information the family needs.",
                HouseAnimations.PlayRadioSong, Events.Empty, ModeName.House),
            LessonRuleItem.HouseParentsPanic => Create(
                "house-parents-panic", "Parents panic", "Stay calm and follow the emergency plan.",
                HouseAnimations.ParentsPanic, Events.Empty, ModeName.House),
            LessonRuleItem.HouseWatchTV => Create(
                "house-watch-tv", "Watch TV", "Watching TV does not replace reviewing the emergency plan.",
                HouseAnimations.WatchTV, Events.Empty, ModeName.House),

            LessonRuleItem.CleaningGardenClearYard => Create(
                "cleaning-garden-clear-yard", "Clear loose yard debris", "Remove loose leaves and debris before strong winds arrive.",
                ClearingGardenAnimations.ClearYard, Events.CleanYard, ModeName.ClearingGarden),
            LessonRuleItem.CleaningGardenGatherPlywood => Create(
                "cleaning-garden-gather-plywood", "Gather plywood", "Prepare plywood for protecting the house.",
                ClearingGardenAnimations.GatherPlywood, Events.CollectPlywood, ModeName.ClearingGarden),
            LessonRuleItem.CleaningGardenWaterFlowers => Create(
                "cleaning-garden-water-flowers", "Water the flowers", "Watering flowers is not part of hurricane preparation.",
                ClearingGardenAnimations.WaterFlowers, Events.Empty, ModeName.ClearingGarden),
            LessonRuleItem.CleaningGardenGoForWalk => Create(
                "cleaning-garden-go-for-walk", "Go for a walk", "The family should prepare the property instead of leaving for a walk.",
                ClearingGardenAnimations.GoForWalk, Events.Empty, ModeName.ClearingGarden),

            LessonRuleItem.SupermarketCannedFood => Create(
                "supermarket-canned-food", "Get canned food", "Choose shelf-stable food for the emergency supply.",
                AnimationsInSuper.GetCannedFood, Events.GetCannedFood, ModeName.SuperMarket),
            LessonRuleItem.SupermarketCrackers => Create(
                "supermarket-crackers", "Get crackers", "Choose dry food that is easy to store and carry.",
                AnimationsInSuper.GetCrackers, Events.GetCrackers, ModeName.SuperMarket),
            LessonRuleItem.SupermarketWater => Create(
                "supermarket-water", "Get water", "Add drinking water to the family emergency supply.",
                AnimationsInSuper.GetWater, Events.GetWater, ModeName.SuperMarket),
            LessonRuleItem.SupermarketCheese => Create(
                "supermarket-cheese", "Get cheese", "Choose food that can stay safe without refrigeration.",
                AnimationsInSuper.GetCheese, Events.Empty, ModeName.SuperMarket),
            LessonRuleItem.SupermarketEggs => Create(
                "supermarket-eggs", "Get eggs", "Choose food that can stay safe without refrigeration.",
                AnimationsInSuper.GetEggs, Events.Empty, ModeName.SuperMarket),
            LessonRuleItem.SupermarketChicken => Create(
                "supermarket-chicken", "Get chicken", "Choose food that can stay safe without refrigeration.",
                AnimationsInSuper.GetChicken, Events.Empty, ModeName.SuperMarket),
            LessonRuleItem.SupermarketFish => Create(
                "supermarket-fish", "Get fish", "Choose food that can stay safe without refrigeration.",
                AnimationsInSuper.GetFish, Events.Empty, ModeName.SuperMarket),
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
