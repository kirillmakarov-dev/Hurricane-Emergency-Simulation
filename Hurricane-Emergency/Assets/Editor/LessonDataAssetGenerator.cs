using UnityEditor;
using UnityEngine;

public static class LessonDataAssetGenerator
{
    public const string CatalogPath = "Assets/Data/Lessons/LevelCatalog.asset";

    private const string DataFolder = "Assets/Data";
    private const string LessonFolder = DataFolder + "/Lessons";
    private const string GoBagPath = LessonFolder + "/GoBagLesson.asset";
    private const string KitchenPath = LessonFolder + "/KitchenLesson.asset";
    private const string BedroomPath = LessonFolder + "/BedroomLesson.asset";
    private const string ShelterPath = LessonFolder + "/ShelterLesson.asset";
    private const string AfterTheHurricanePath = LessonFolder + "/AfterTheHurricaneLesson.asset";
    private const string GardenViewPath = LessonFolder + "/GardenViewLesson.asset";
    private const string BathroomPath = LessonFolder + "/BathroomLesson.asset";
    private const string GoBagThumbnailPath = "Assets/GoBag Simulation/Background room.png";
    private const string KitchenThumbnailPath = "Assets/GoBag Simulation/Kitchen lesson/kitchen.png";
    private const string BedroomThumbnailPath = "Assets/Sprites/Mission 5/ChildrensRoom/Kelan Bedroom no items.png";
    private const string ShelterThumbnailPath = "Assets/Shelter/Shelter background.png";
    private const string AfterTheHurricaneThumbnailPath = "Assets/Mission 8/Background.png";
    private const string GardenViewThumbnailPath = "Assets/Sprites/outside garden view.jpg";
    private const string BathroomThumbnailPath = "Assets/GoBag Simulation/BathRoom/Bathroom.png";

    [MenuItem("Tools/Hurricane/Rebuild Lesson Data Assets")]
    public static void RebuildLessonDataAssets()
    {
        EnsureLessonDataAssets(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
        Debug.Log("Lesson ScriptableObject assets rebuilt for Go Bag, Kitchen, Bedroom, Bathroom, Shelter, After the Hurricane, and Garden View.");
    }

    public static LevelCatalog EnsureLessonDataAssets(bool overwriteExisting = false)
    {
        EnsureFolders();

        LevelDefinition goBag = LoadOrCreate<LevelDefinition>(GoBagPath);
        if (overwriteExisting || string.IsNullOrEmpty(goBag.LevelId))
        {
            goBag.ConfigureEditor(
                "go-bag-prototype",
                "Build a Go Bag",
                "Kelan's family is preparing for a hurricane. Choose the essential items and arrange the actions before the simulation begins.",
                "After the parents give a reminder, pack water, a flashlight, and books in that order.",
                ModeName.GoBagLesson,
                LoadSprite(GoBagThumbnailPath),
                new[]
                {
                    LessonRuleItem.GoBagWater,
                    LessonRuleItem.GoBagFlashlight,
                    LessonRuleItem.GoBagBooks
                },
                new[]
                {
                    LessonRuleItem.GoBagCandles,
                    LessonRuleItem.GoBagScissors,
                    LessonRuleItem.GoBagBall
                },
                new[] { Events.GobagReminder });
            EditorUtility.SetDirty(goBag);
        }

        LevelDefinition kitchen = LoadOrCreate<LevelDefinition>(KitchenPath);
        if (overwriteExisting || string.IsNullOrEmpty(kitchen.LevelId))
        {
            kitchen.ConfigureEditor(
                "kitchen-lesson",
                "Kitchen supplies",
                "Kay is checking the kitchen for food and water that can safely travel with the family during a hurricane.",
                "After the reminder, pack canned food, crackers, and water in that order.",
                ModeName.KitchenLesson,
                LoadSprite(KitchenThumbnailPath),
                new[]
                {
                    LessonRuleItem.KitchenCannedFood,
                    LessonRuleItem.KitchenCrackers,
                    LessonRuleItem.KitchenWater
                },
                new[]
                {
                    LessonRuleItem.KitchenCheese,
                    LessonRuleItem.KitchenEggs,
                    LessonRuleItem.KitchenChicken,
                    LessonRuleItem.KitchenFish
                },
                new[] { Events.GobagReminder });
            EditorUtility.SetDirty(kitchen);
        }

        LevelDefinition bedroom = LoadOrCreate<LevelDefinition>(BedroomPath);
        if (overwriteExisting || string.IsNullOrEmpty(bedroom.LevelId))
        {
            bedroom.ConfigureEditor(
                "bedroom-lesson",
                "Prepare the bedroom",
                "A hurricane watch has been announced. Kelan needs to choose useful bedroom items before the family leaves.",
                "After the hurricane watch, pack clothes, water, a flashlight, and one toy in that order.",
                ModeName.ChildrenRoom,
                LoadSprite(BedroomThumbnailPath),
                new[]
                {
                    LessonRuleItem.BedroomClothes,
                    LessonRuleItem.BedroomWater,
                    LessonRuleItem.BedroomFlashlight,
                    LessonRuleItem.BedroomToy
                },
                new[]
                {
                    LessonRuleItem.BedroomFruit,
                    LessonRuleItem.BedroomLamp,
                    LessonRuleItem.BedroomAquarium
                },
                new[] { Events.HurricaneWatch });
            EditorUtility.SetDirty(bedroom);
        }

        LevelDefinition shelter = LoadOrCreate<LevelDefinition>(ShelterPath);
        if (overwriteExisting || string.IsNullOrEmpty(shelter.LevelId))
        {
            shelter.ConfigureEditor(
                "shelter-lesson",
                "Shelter",
                "Kay is staying in shelter while the storm passes. Choose the calm, safe actions that belong in this scene.",
                "Select the shelter actions in order, then press Check to launch the scene.",
                ModeName.Shelter,
                LoadSprite(ShelterThumbnailPath),
                new[]
                {
                    LessonRuleItem.ShelterColoursABook,
                    LessonRuleItem.ShelterPlaysWithToy
                },
                new[]
                {
                    LessonRuleItem.ShelterPlaysOutside,
                    LessonRuleItem.ShelterTalksToAStranger
                },
                System.Array.Empty<Events>());
            EditorUtility.SetDirty(shelter);
        }

        LevelDefinition afterTheHurricane = LoadOrCreate<LevelDefinition>(AfterTheHurricanePath);
        if (overwriteExisting || string.IsNullOrEmpty(afterTheHurricane.LevelId))
        {
            afterTheHurricane.ConfigureEditor(
                "after-the-hurricane-lesson",
                "After the Hurricane",
                "The storm has passed and the family is cleaning up the yard. Choose only the safe cleanup actions.",
                "Select the cleanup actions in order, then press Check to launch the scene.",
                ModeName.AfterTheHurricane,
                LoadSprite(AfterTheHurricaneThumbnailPath),
                new[]
                {
                    LessonRuleItem.AfterHurricanePickBranches,
                    LessonRuleItem.AfterHurricanePickBottles,
                    LessonRuleItem.AfterHurricaneMotherCutWood
                },
                new[]
                {
                    LessonRuleItem.AfterHurricanePickBrokenGlass,
                    LessonRuleItem.AfterHurricanePickElectricWires,
                    LessonRuleItem.AfterHurricaneFatherPickBrokenGlass,
                    LessonRuleItem.AfterHurricaneFatherPickElectricWires,
                    LessonRuleItem.AfterHurricaneGoForWalk
                },
                System.Array.Empty<Events>());
            EditorUtility.SetDirty(afterTheHurricane);
        }

        LevelDefinition gardenView = LoadOrCreate<LevelDefinition>(GardenViewPath);
        if (overwriteExisting || string.IsNullOrEmpty(gardenView.LevelId))
        {
            gardenView.ConfigureEditor(
                "garden-view-lesson",
                "Garden View",
                "The family is outside when the hurricane warning arrives. Choose the safe yard actions before the storm gets close.",
                "After the warning, take the toys, ball, and bicycle in that order.",
                ModeName.GardenView,
                LoadSprite(GardenViewThumbnailPath),
                new[]
                {
                    LessonRuleItem.GardenViewTakeToys,
                    LessonRuleItem.GardenViewTakeBall,
                    LessonRuleItem.GardenViewTakeBicycle
                },
                new[]
                {
                    LessonRuleItem.GardenViewGoForWalk,
                    LessonRuleItem.GardenViewPickFlowers
                },
                new[] { Events.HurricaneWarning });
            EditorUtility.SetDirty(gardenView);
        }

        LevelDefinition bathroom = LoadOrCreate<LevelDefinition>(BathroomPath);
        if (overwriteExisting || string.IsNullOrEmpty(bathroom.LevelId))
        {
            bathroom.ConfigureEditor(
                "bathroom-lesson",
                "Bathroom supplies",
                "Kelan is preparing the bathroom emergency kit before the family leaves for shelter.",
                "After the reminder, pack the first aid kit, toothbrush, wipes, and soap in that order.",
                ModeName.BathRoomLesson,
                LoadSprite(BathroomThumbnailPath),
                new[]
                {
                    LessonRuleItem.BathroomFirstAidKit,
                    LessonRuleItem.BathroomToothbrush,
                    LessonRuleItem.BathroomWipes,
                    LessonRuleItem.BathroomSoap
                },
                new[]
                {
                    LessonRuleItem.BathroomHairDryer,
                    LessonRuleItem.BathroomPump,
                    LessonRuleItem.BathroomWashingGel,
                    LessonRuleItem.BathroomCleaningSpray
                },
                new[] { Events.GobagReminder });
            EditorUtility.SetDirty(bathroom);
        }

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(CatalogPath);
        if (overwriteExisting || catalog.Levels.Count != 7)
        {
            catalog.ConfigureEditor(new[] { goBag, kitchen, bedroom, bathroom, shelter, afterTheHurricane, gardenView });
            EditorUtility.SetDirty(catalog);
        }
        return catalog;
    }

    private static T LoadOrCreate<T>(string path) where T : ScriptableObject
    {
        T asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder(DataFolder)) AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder(LessonFolder)) AssetDatabase.CreateFolder(DataFolder, "Lessons");
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
