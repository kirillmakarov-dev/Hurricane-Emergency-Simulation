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

    [MenuItem("Tools/Hurricane/Rebuild Lesson Data Assets")]
    public static void RebuildLessonDataAssets()
    {
        EnsureLessonDataAssets(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogPath);
        Debug.Log("Lesson ScriptableObject assets rebuilt for Go Bag, Kitchen, and Bedroom.");
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

        LevelCatalog catalog = LoadOrCreate<LevelCatalog>(CatalogPath);
        if (overwriteExisting || catalog.Levels.Count == 0)
        {
            catalog.ConfigureEditor(new[] { goBag, kitchen, bedroom });
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
}
