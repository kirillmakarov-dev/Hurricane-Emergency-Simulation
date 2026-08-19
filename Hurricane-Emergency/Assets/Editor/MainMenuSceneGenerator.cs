using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MainMenuSceneGenerator
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameplayScenePath = "Assets/Scenes/SampleScene.unity";
    private const string PrefabFolder = "Assets/prefabs/GameFlow";
    private const string UiPrefabPath = PrefabFolder + "/GameFlowUI.prefab";
    private const string RuleOptionPrefabPath = PrefabFolder + "/RuleOptionButton.prefab";
    private const string SelectedRulePrefabPath = PrefabFolder + "/SelectedRuleRow.prefab";
    private const string LessonButtonPrefabPath = PrefabFolder + "/LessonButton.prefab";

    private static readonly Color Ink = Hex("17252D");
    private static readonly Color DeepTeal = Hex("184E57");
    private static readonly Color Teal = Hex("238A8D");
    private static readonly Color Aqua = Hex("65D1C8");
    private static readonly Color Cream = Hex("F4EBD9");
    private static readonly Color Paper = Hex("FFF9EE");
    private static readonly Color Coral = Hex("F47C65");
    private static readonly Color Success = Hex("2D9D78");
    private static Font font;

    [MenuItem("Tools/Hurricane/Rebuild Game Flow UI Assets")]
    public static void RebuildGameFlowUiAssets()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsurePrefabFolder();
        LevelCatalog levelCatalog = LessonDataAssetGenerator.EnsureLessonDataAssets();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        RuleOptionView optionPrefab = BuildRuleOptionPrefab();
        SelectedRuleRowView selectedPrefab = BuildSelectedRulePrefab();
        LessonButtonView lessonPrefab = BuildLessonButtonPrefab();
        BuildGameFlowUiPrefab(optionPrefab, selectedPrefab, lessonPrefab);

        WireScene(MenuScenePath, true, levelCatalog);
        WireScene(GameplayScenePath, false, levelCatalog);
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
        Debug.Log("Game flow prefabs rebuilt and serialized into MainMenu and SampleScene.");
    }

    [MenuItem("Tools/Hurricane/Repair Build Scene List")]
    public static void RepairBuildSceneList()
    {
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Build scene list repaired: MainMenu is scene 0 and SampleScene is scene 1.");
    }

    private static RuleOptionView BuildRuleOptionPrefab()
    {
        GameObject root = CreateImage("RuleOptionButton", Paper);
        Button button = root.AddComponent<Button>();
        ConfigureButtonColors(button, Paper);
        AddLayout(root, 56f);
        Text label = CreateText("Rule action", root.transform, 17, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        Stretch(label.rectTransform, 18f);
        RuleOptionView view = root.AddComponent<RuleOptionView>();
        view.Configure(label, button);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RuleOptionPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<RuleOptionView>();
    }

    private static SelectedRuleRowView BuildSelectedRulePrefab()
    {
        GameObject root = CreateHorizontalGroup("SelectedRuleRow", null, 8f);
        AddLayout(root, 58f);
        Text label = CreateText("1. Rule action", root.transform, 17, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddFlexible(label.gameObject, 1f);
        Button up = CreateButton("UP", root.transform, Cream, DeepTeal, 48f, 56f);
        Button down = CreateButton("DOWN", root.transform, Cream, DeepTeal, 48f, 68f);
        Button remove = CreateButton("X", root.transform, Coral, Color.white, 48f, 48f);
        SelectedRuleRowView view = root.AddComponent<SelectedRuleRowView>();
        view.Configure(label, up, down, remove);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SelectedRulePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<SelectedRuleRowView>();
    }

    private static LessonButtonView BuildLessonButtonPrefab()
    {
        GameObject root = CreateImage("LessonButton", Cream);
        VerticalLayoutGroup layout = AddVertical(root, 3f, 8f);
        layout.childAlignment = TextAnchor.MiddleLeft;
        AddLayout(root, 145f);
        Text number = FixedText("LESSON 01", root.transform, 12, FontStyle.Bold, Coral, TextAnchor.MiddleLeft, 16f);
        Text title = FixedText("Lesson title", root.transform, 21, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, 24f);
        Text description = FixedText("Lesson objective", root.transform, 14, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft, 25f);
        Button open = CreateButton("OPEN LESSON", root.transform, Teal, Color.white, 34f);
        LessonButtonView view = root.AddComponent<LessonButtonView>();
        view.Configure(number, title, description, open);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, LessonButtonPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<LessonButtonView>();
    }

    private static GameFlowView BuildGameFlowUiPrefab(
        RuleOptionView optionPrefab,
        SelectedRuleRowView selectedPrefab,
        LessonButtonView lessonPrefab)
    {
        GameObject root = new("GameFlowUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameFlowView));
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject mainMenu = CreateScreen("MainMenuScreen", root.transform, Ink);
        GameObject mainContent = CreateContent(mainMenu.transform, Paper);
        VerticalLayoutGroup mainLayout = AddVertical(mainContent, 18f, 38f);
        mainLayout.childAlignment = TextAnchor.MiddleLeft;
        FixedText("HURRICANE READY  /  UNITY", mainContent.transform, 17, FontStyle.Bold, Teal, TextAnchor.MiddleLeft, 30f);
        FixedText("Choose a lesson", mainContent.transform, 52, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, 72f);
        FixedText("Learn emergency preparation by building rules, then watch the simulation follow your plan.", mainContent.transform, 22, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft, 72f);
        GameObject lessonContainer = CreateVerticalGroup("LessonButtons", mainContent.transform, 10f);
        AddFlexible(lessonContainer, 1f);

        GameObject briefing = CreateScreen("BriefingScreen", root.transform, DeepTeal);
        GameObject briefingContent = CreateContent(briefing.transform, Paper);
        VerticalLayoutGroup briefingLayout = AddVertical(briefingContent, 18f, 40f);
        briefingLayout.childAlignment = TextAnchor.MiddleLeft;
        FixedText("LESSON BRIEFING", briefingContent.transform, 17, FontStyle.Bold, Coral, TextAnchor.MiddleLeft, 30f);
        Text briefingTitle = FixedText("Lesson title", briefingContent.transform, 46, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, 66f);
        Text briefingBody = FixedText("Lesson briefing", briefingContent.transform, 23, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft, 140f);
        GameObject objective = CreateImage("Objective", Cream);
        objective.transform.SetParent(briefingContent.transform, false);
        AddLayout(objective, 100f);
        Text objectiveText = CreateText("OBJECTIVE  /  Lesson objective", objective.transform, 19, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        Stretch(objectiveText.rectTransform, 20f);
        GameObject briefingActions = CreateHorizontalGroup("Actions", briefingContent.transform, 14f);
        AddLayout(briefingActions, 64f);
        Button briefingBack = CreateButton("BACK TO LESSONS", briefingActions.transform, Cream, DeepTeal, 64f);
        AddFlexible(briefingBack.gameObject, 1f);
        Button buildRules = CreateButton("BUILD RULES", briefingActions.transform, Coral, Color.white, 64f);
        AddFlexible(buildRules.gameObject, 1.4f);

        GameObject builder = CreateScreen("RuleBuilderScreen", root.transform, Ink);
        GameObject builderContent = CreatePanel("Content", builder.transform, new Color(1f, 1f, 1f, 0.97f), new Vector2(0.08f, 0.07f), new Vector2(0.92f, 0.93f));
        VerticalLayoutGroup builderLayout = AddVertical(builderContent, 12f, 28f);
        builderLayout.childAlignment = TextAnchor.UpperLeft;
        FixedText("MY RULES", builderContent.transform, 16, FontStyle.Bold, Teal, TextAnchor.MiddleLeft, 26f);
        Text ruleBuilderTitle = FixedText("Build the action sequence", builderContent.transform, 38, FontStyle.Bold, Ink, TextAnchor.MiddleLeft, 52f);
        FixedText("Add actions from the left. Use UP and DOWN to match the objective.", builderContent.transform, 19, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft, 34f);
        GameObject columns = CreateHorizontalGroup("Columns", builderContent.transform, 18f);
        AddFlexible(columns, 1f);

        GameObject availablePanel = CreateImage("AvailableRules", Cream);
        availablePanel.transform.SetParent(columns.transform, false);
        AddFlexible(availablePanel, 1f);
        AddVertical(availablePanel, 9f, 18f).childAlignment = TextAnchor.UpperLeft;
        FixedText("AVAILABLE ACTIONS", availablePanel.transform, 16, FontStyle.Bold, Teal, TextAnchor.MiddleLeft, 30f);
        GameObject availableList = CreateVerticalGroup("AvailableList", availablePanel.transform, 8f);
        AddFlexible(availableList, 1f);

        GameObject selectedPanel = CreateImage("SelectedRules", Paper);
        selectedPanel.transform.SetParent(columns.transform, false);
        AddFlexible(selectedPanel, 1.15f);
        AddVertical(selectedPanel, 9f, 18f).childAlignment = TextAnchor.UpperLeft;
        FixedText("YOUR SEQUENCE", selectedPanel.transform, 16, FontStyle.Bold, Coral, TextAnchor.MiddleLeft, 30f);
        GameObject selectedList = CreateVerticalGroup("SelectedList", selectedPanel.transform, 8f);
        AddFlexible(selectedList, 1f);
        Text emptySelection = CreateText("No actions selected yet", selectedList.transform, 18, FontStyle.Italic, DeepTeal, TextAnchor.MiddleCenter);
        AddLayout(emptySelection.gameObject, 56f);

        Text ruleFeedback = FixedText("Build the sequence, then press Check.", builderContent.transform, 18, FontStyle.Bold, DeepTeal, TextAnchor.MiddleLeft, 40f);
        GameObject builderActions = CreateHorizontalGroup("Actions", builderContent.transform, 14f);
        AddLayout(builderActions, 62f);
        Button builderBack = CreateButton("BACK TO BRIEFING", builderActions.transform, Cream, DeepTeal, 62f);
        AddFlexible(builderBack.gameObject, 1f);
        Button check = CreateButton("CHECK", builderActions.transform, Teal, Color.white, 62f);
        AddFlexible(check.gameObject, 1.5f);

        GameObject gameplay = CreatePanel("GameplayHUD", root.transform, Color.clear, Vector2.zero, Vector2.one);
        gameplay.GetComponent<Image>().raycastTarget = false;
        GameObject topBar = CreatePanel("TopBar", gameplay.transform, new Color(Ink.r, Ink.g, Ink.b, 0.94f), new Vector2(0.03f, 0.89f), new Vector2(0.97f, 0.975f));
        HorizontalLayoutGroup topLayout = AddHorizontal(topBar, 14f, 18f);
        topLayout.childAlignment = TextAnchor.MiddleLeft;
        Text liveLabel = CreateText("LESSON  /  LIVE CHECK", topBar.transform, 17, FontStyle.Bold, Aqua, TextAnchor.MiddleLeft);
        AddFlexible(liveLabel.gameObject, 1f);
        Text progress = CreateText("0 / 4 steps complete", topBar.transform, 19, FontStyle.Bold, Color.white, TextAnchor.MiddleRight);
        AddLayout(progress.gameObject, 50f, 360f);
        GameObject feedbackPanel = CreatePanel("Feedback", gameplay.transform, new Color(DeepTeal.r, DeepTeal.g, DeepTeal.b, 0.95f), new Vector2(0.26f, 0.055f), new Vector2(0.74f, 0.13f));
        Text gameplayFeedback = CreateText("The family is getting ready...", feedbackPanel.transform, 20, FontStyle.Bold, Cream, TextAnchor.MiddleCenter);
        Stretch(gameplayFeedback.rectTransform, 18f);

        GameObject result = CreateScreen("ResultScreen", root.transform, new Color(Ink.r, Ink.g, Ink.b, 0.98f));
        GameObject resultContent = CreateContent(result.transform, Paper);
        VerticalLayoutGroup resultLayout = AddVertical(resultContent, 18f, 40f);
        resultLayout.childAlignment = TextAnchor.MiddleCenter;
        FixedText("LEVEL COMPLETE", resultContent.transform, 18, FontStyle.Bold, Success, TextAnchor.MiddleCenter, 32f);
        Text resultTitle = FixedText("Lesson complete", resultContent.transform, 50, FontStyle.Bold, Ink, TextAnchor.MiddleCenter, 72f);
        Text resultSummary = FixedText("Every essential item was packed in the planned order.", resultContent.transform, 23, FontStyle.Normal, DeepTeal, TextAnchor.MiddleCenter, 110f);
        GameObject badge = CreateImage("SuccessBadge", Aqua);
        badge.transform.SetParent(resultContent.transform, false);
        AddLayout(badge, 88f, 360f);
        Text badgeText = CreateText("RULES VERIFIED", badge.transform, 21, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        Stretch(badgeText.rectTransform, 12f);
        GameObject resultActions = CreateHorizontalGroup("Actions", resultContent.transform, 14f);
        AddLayout(resultActions, 64f);
        Button backToLessons = CreateButton("BACK TO LESSONS", resultActions.transform, Cream, DeepTeal, 64f);
        AddFlexible(backToLessons.gameObject, 1f);
        Button playAgain = CreateButton("PLAY AGAIN", resultActions.transform, Coral, Color.white, 64f);
        AddFlexible(playAgain.gameObject, 1f);

        GameFlowView view = root.GetComponent<GameFlowView>();
        view.Configure(mainMenu, briefing, builder, gameplay, result, briefingBack, buildRules,
            builderBack, check, backToLessons, playAgain, lessonContainer.transform, lessonPrefab,
            briefingTitle, briefingBody, objectiveText, ruleBuilderTitle, liveLabel, resultTitle,
            availableList.transform, selectedList.transform,
            emptySelection.gameObject, ruleFeedback, optionPrefab, selectedPrefab, progress, gameplayFeedback, resultSummary);
        mainMenu.SetActive(true);
        briefing.SetActive(false);
        builder.SetActive(false);
        gameplay.SetActive(false);
        result.SetActive(false);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, UiPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<GameFlowView>();
    }

    private static void WireScene(string path, bool ensureCamera, LevelCatalog levelCatalog)
    {
        Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        GameFlowController controller = Object.FindFirstObjectByType<GameFlowController>(FindObjectsInactive.Include);
        if (controller == null)
        {
            controller = new GameObject("GameFlowController").AddComponent<GameFlowController>();
            SceneManager.MoveGameObjectToScene(controller.gameObject, scene);
        }

        GameFlowView[] existingViews = Object.FindObjectsByType<GameFlowView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameFlowView existingView in existingViews) Object.DestroyImmediate(existingView.gameObject);
        Transform oldCanvas = controller.transform.Find("UnityNativeLessonFlow");
        if (oldCanvas != null) Object.DestroyImmediate(oldCanvas.gameObject);

        GameObject uiPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(UiPrefabPath);
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(uiPrefab, scene);
        instance.transform.SetParent(controller.transform, false);
        GameFlowView view = instance.GetComponent<GameFlowView>();
        controller.Configure(view, levelCatalog);
        EditorUtility.SetDirty(controller);

        if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        if (ensureCamera && Camera.main == null)
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.15f, 0.18f);
            camera.orthographic = true;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void EnsurePrefabFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/prefabs")) AssetDatabase.CreateFolder("Assets", "prefabs");
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/prefabs", "GameFlow");
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new()
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameplayScenePath, true)
        };
        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (existing.path == MenuScenePath || existing.path == GameplayScenePath) continue;
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(existing.path) != null) scenes.Add(existing);
        }
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static GameObject CreateScreen(string name, Transform parent, Color color) => CreatePanel(name, parent, color, Vector2.zero, Vector2.one);
    private static GameObject CreateContent(Transform parent, Color color) => CreatePanel("Content", parent, color, new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.86f));

    private static GameObject CreatePanel(string name, Transform parent, Color color, Vector2 min, Vector2 max)
    {
        GameObject panel = CreateImage(name, color);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private static GameObject CreateImage(string name, Color color)
    {
        GameObject result = new(name, typeof(RectTransform), typeof(Image));
        result.GetComponent<Image>().color = color;
        return result;
    }

    private static Text CreateText(string value, Transform parent, int size, FontStyle style, Color color, TextAnchor alignment)
    {
        GameObject textObject = new("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>();
        text.text = value;
        text.font = font;
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    private static Text FixedText(string value, Transform parent, int size, FontStyle style, Color color, TextAnchor alignment, float height)
    {
        Text text = CreateText(value, parent, size, style, color, alignment);
        AddLayout(text.gameObject, height);
        return text;
    }

    private static Button CreateButton(string label, Transform parent, Color background, Color foreground, float height, float width = -1f)
    {
        GameObject root = CreateImage(label + "Button", background);
        root.transform.SetParent(parent, false);
        Button button = root.AddComponent<Button>();
        ConfigureButtonColors(button, background);
        Text text = CreateText(label, root.transform, 17, FontStyle.Bold, foreground, TextAnchor.MiddleCenter);
        Stretch(text.rectTransform, 10f);
        AddLayout(root, height, width);
        return button;
    }

    private static void ConfigureButtonColors(Button button, Color background)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = Color.Lerp(background, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
        colors.disabledColor = new Color(background.r, background.g, background.b, 0.35f);
        button.colors = colors;
    }

    private static GameObject CreateVerticalGroup(string name, Transform parent, float spacing)
    {
        GameObject group = new(name, typeof(RectTransform), typeof(VerticalLayoutGroup));
        group.transform.SetParent(parent, false);
        VerticalLayoutGroup layout = group.GetComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        return group;
    }

    private static GameObject CreateHorizontalGroup(string name, Transform parent, float spacing)
    {
        GameObject group = new(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        if (parent != null) group.transform.SetParent(parent, false);
        HorizontalLayoutGroup layout = group.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        return group;
    }

    private static VerticalLayoutGroup AddVertical(GameObject target, float spacing, float padding)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        int value = Mathf.RoundToInt(padding);
        layout.padding = new RectOffset(value, value, value, value);
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        return layout;
    }

    private static HorizontalLayoutGroup AddHorizontal(GameObject target, float spacing, float padding)
    {
        HorizontalLayoutGroup layout = target.AddComponent<HorizontalLayoutGroup>();
        int value = Mathf.RoundToInt(padding);
        layout.padding = new RectOffset(value, value, value, value);
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        return layout;
    }

    private static void AddLayout(GameObject target, float height, float width = -1f)
    {
        LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
        element.preferredHeight = height;
        if (width >= 0f) element.preferredWidth = width;
    }

    private static void AddFlexible(GameObject target, float width)
    {
        LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
        element.flexibleWidth = width;
        element.flexibleHeight = 1f;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}
