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

    private static readonly Color Ink = Hex("172A38");
    private static readonly Color DeepBlue = Hex("006D96");
    private static readonly Color Blue = Hex("2CAEF4");
    private static readonly Color Purple = Hex("A920B8");
    private static readonly Color Yellow = Hex("FFD447");
    private static readonly Color CanvasColor = Hex("F4F7FC");
    private static readonly Color Soft = Hex("E9EEF6");
    private static readonly Color Paper = Hex("FFFFFF");
    private static readonly Color Danger = Hex("D91F2B");
    private static readonly Color Success = Hex("00D97E");
    private static readonly Color Cream = Hex("FFF8E8");
    private static readonly Color Muted = Hex("40515E");
    private static Font font;
    private static Sprite roundedSprite;

    [MenuItem("Tools/Hurricane/Rebuild Game Flow UI Assets")]
    public static void RebuildGameFlowUiAssets()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        EnsurePrefabFolder();
        LevelCatalog levelCatalog = LessonDataAssetGenerator.EnsureLessonDataAssets();
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf")
            ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        roundedSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        RuleOptionView optionPrefab = BuildRuleOptionPrefab();
        SelectedRuleRowView selectedPrefab = BuildSelectedRulePrefab();
        LessonButtonView lessonPrefab = BuildLessonButtonPrefab();
        BuildGameFlowUiPrefab(optionPrefab, selectedPrefab, lessonPrefab);
        GameFlowVisualRefresh.Apply();

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
        Round(root);
        AddOutline(root, Hex("C7D1DD"), new Vector2(1.5f, -1.5f));
        AddShadow(root, new Color(0.05f, 0.18f, 0.28f, 0.12f), new Vector2(0f, -3f));
        Button button = root.AddComponent<Button>();
        ConfigureButtonColors(button, Paper);
        AddLayout(root, 60f);
        Text label = CreateText("Rule action", root.transform, 17, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        Stretch(label.rectTransform, 20f);
        RuleOptionView view = root.AddComponent<RuleOptionView>();
        view.Configure(label, button);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RuleOptionPrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<RuleOptionView>();
    }

    private static SelectedRuleRowView BuildSelectedRulePrefab()
    {
        GameObject root = CreateImage("SelectedRuleRow", Paper);
        Round(root);
        AddShadow(root, new Color(0.05f, 0.18f, 0.28f, 0.14f), new Vector2(0f, -3f));
        AddLayout(root, 58f);
        HorizontalLayoutGroup rowLayout = AddHorizontal(root, 0f, 0f);
        rowLayout.padding = new RectOffset(12, 0, 6, 6);
        rowLayout.childAlignment = TextAnchor.MiddleLeft;
        rowLayout.childForceExpandHeight = false;

        GameObject badge = CreateImage("OrderBadge", DeepBlue);
        badge.transform.SetParent(root.transform, false);
        Round(badge);
        AddLayout(badge, 42f, 42f);
        Text order = CreateText("1", badge.transform, 17, FontStyle.Bold, Color.white, TextAnchor.MiddleCenter);
        Stretch(order.rectTransform, 4f);
        Text action = CreateText("Rule action", root.transform, 17, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddLayout(action.gameObject, 46f);
        AddFlexibleWidth(action.gameObject, 1f);
        Button up = CreateButton("UP", root.transform, Blue, Color.white, 46f, 58f, false);
        Button down = CreateButton("DN", root.transform, Yellow, Ink, 46f, 58f, false);
        Button remove = CreateButton("X", root.transform, Danger, Color.white, 46f, 54f, false);
        SelectedRuleRowView view = root.AddComponent<SelectedRuleRowView>();
        view.Configure(order, action, up, down, remove);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, SelectedRulePrefabPath);
        Object.DestroyImmediate(root);
        return prefab.GetComponent<SelectedRuleRowView>();
    }

    private static LessonButtonView BuildLessonButtonPrefab()
    {
        GameObject root = CreateImage("LessonButton", Soft);
        Round(root);
        AddOutline(root, DeepBlue, new Vector2(2f, -2f));
        AddShadow(root, new Color(0.04f, 0.16f, 0.25f, 0.18f), new Vector2(0f, -5f));
        AddLayout(root, 206f, 560f);

        GameObject scrim = CreatePanel("BackgroundScrim", root.transform,
            new Color(0.12f, 0.55f, 0.78f, 0.42f), Vector2.zero, Vector2.one);
        Round(scrim);
        scrim.GetComponent<Image>().raycastTarget = false;

        GameObject content = new("CardContent", typeof(RectTransform));
        content.transform.SetParent(root.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(22f, 18f);
        contentRect.offsetMax = new Vector2(-22f, -18f);
        VerticalLayoutGroup layout = AddVertical(content, 4f, 0f);
        layout.childAlignment = TextAnchor.MiddleLeft;

        Text number = FixedText("LESSON 01", content.transform, 12, FontStyle.Bold, Blue, TextAnchor.MiddleLeft, 18f);
        Text title = FixedText("Lesson title", content.transform, 24, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft, 34f);
        Text description = FixedText("Lesson objective", content.transform, 14, FontStyle.Normal, Color.white, TextAnchor.UpperLeft, 54f);
        GameObject actionRow = CreateHorizontalGroup("CardAction", content.transform, 0f);
        AddLayout(actionRow, 44f);
        Button open = CreateButton("OPEN LESSON", actionRow.transform, Yellow, Ink, 44f, 174f);
        GameObject spacer = new("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(actionRow.transform, false);
        AddFlexible(spacer, 1f);

        LessonButtonView view = root.AddComponent<LessonButtonView>();
        view.Configure(root.GetComponent<Image>(), null, number, title, description, open, scrim.GetComponent<Image>(), contentRect);
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

        GameObject mainMenu = CreateScreen("MainMenuScreen", root.transform, CanvasColor);
        GameObject topAppBar = CreatePanel("TopAppBar", mainMenu.transform, Paper, new Vector2(0f, 0.91f), Vector2.one);
        AddShadow(topAppBar, new Color(0.04f, 0.16f, 0.25f, 0.12f), new Vector2(0f, -4f));
        HorizontalLayoutGroup headerLayout = AddHorizontal(topAppBar, 26f, 34f);
        headerLayout.childAlignment = TextAnchor.MiddleCenter;
        Text logo = CreateText("HERO READY", topAppBar.transform, 31, FontStyle.Bold, Purple, TextAnchor.MiddleLeft);
        AddLayout(logo.gameObject, 54f, 330f);
        GameObject nav = CreateHorizontalGroup("Navigation", topAppBar.transform, 24f);
        AddFlexible(nav, 1f);
        nav.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        Text missions = CreateText("MISSIONS", nav.transform, 14, FontStyle.Bold, DeepBlue, TextAnchor.MiddleCenter);
        AddLayout(missions.gameObject, 48f, 120f);
        Text badges = CreateText("BADGES", nav.transform, 14, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        AddLayout(badges.gameObject, 48f, 100f);
        Text map = CreateText("MAP", nav.transform, 14, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        AddLayout(map.gameObject, 48f, 80f);
        Text utilities = CreateText("SETTINGS     HELP", topAppBar.transform, 13, FontStyle.Bold, DeepBlue, TextAnchor.MiddleRight);
        AddLayout(utilities.gameObject, 54f, 330f);

        GameObject mainContent = CreatePanel("Content", mainMenu.transform, Paper, new Vector2(0.06f, 0.045f), new Vector2(0.94f, 0.88f));
        Round(mainContent);
        AddShadow(mainContent, new Color(0.04f, 0.16f, 0.25f, 0.1f), new Vector2(0f, -5f));
        VerticalLayoutGroup mainLayout = AddVertical(mainContent, 12f, 42f);
        mainLayout.childAlignment = TextAnchor.MiddleLeft;
        FixedText("HURRICANE READY  /  UNITY", mainContent.transform, 14, FontStyle.Bold, Purple, TextAnchor.MiddleLeft, 24f);
        FixedText("Choose a lesson", mainContent.transform, 42, FontStyle.Bold, DeepBlue, TextAnchor.MiddleLeft, 56f);
        FixedText("Learn emergency preparation by building rules, then watch the simulation follow your plan.", mainContent.transform, 19, FontStyle.Normal, Muted, TextAnchor.MiddleLeft, 52f);
        GameObject lessonScrollArea = CreateImage("LessonScrollArea", Paper);
        Round(lessonScrollArea);
        lessonScrollArea.transform.SetParent(mainContent.transform, false);
        AddFlexible(lessonScrollArea, 1f);
        ScrollRect lessonScrollRect = lessonScrollArea.AddComponent<ScrollRect>();
        lessonScrollRect.horizontal = false;
        lessonScrollRect.vertical = true;
        lessonScrollRect.movementType = ScrollRect.MovementType.Clamped;
        lessonScrollRect.scrollSensitivity = 34f;

        GameObject viewport = CreateImage("Viewport", Paper);
        viewport.transform.SetParent(lessonScrollArea.transform, false);
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-24f, 0f);

        GameObject lessonContainer = new("LessonButtons", typeof(RectTransform), typeof(GridLayoutGroup));
        lessonContainer.transform.SetParent(viewport.transform, false);
        RectTransform lessonContainerRect = lessonContainer.GetComponent<RectTransform>();
        lessonContainerRect.anchorMin = new Vector2(0f, 1f);
        lessonContainerRect.anchorMax = new Vector2(1f, 1f);
        lessonContainerRect.pivot = new Vector2(0.5f, 1f);
        lessonContainerRect.anchoredPosition = Vector2.zero;
        lessonContainerRect.sizeDelta = Vector2.zero;
        GridLayoutGroup lessonLayout = lessonContainer.GetComponent<GridLayoutGroup>();
        lessonLayout.padding = new RectOffset(6, 6, 8, 18);
        lessonLayout.cellSize = new Vector2(560f, 206f);
        lessonLayout.spacing = new Vector2(22f, 20f);
        lessonLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        lessonLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        lessonLayout.childAlignment = TextAnchor.UpperCenter;
        lessonLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        lessonLayout.constraintCount = 2;
        ContentSizeFitter lessonFitter = lessonContainer.AddComponent<ContentSizeFitter>();
        lessonFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        lessonFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        GameObject scrollbarObject = CreateImage("VerticalScrollbar", Soft);
        Round(scrollbarObject);
        scrollbarObject.transform.SetParent(lessonScrollArea.transform, false);
        RectTransform scrollbarRect = scrollbarObject.GetComponent<RectTransform>();
        scrollbarRect.anchorMin = new Vector2(1f, 0f);
        scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = new Vector2(1f, 1f);
        scrollbarRect.anchoredPosition = Vector2.zero;
        scrollbarRect.sizeDelta = new Vector2(14f, 0f);
        Scrollbar scrollbar = scrollbarObject.AddComponent<Scrollbar>();
        GameObject slidingArea = new("SlidingArea", typeof(RectTransform));
        slidingArea.transform.SetParent(scrollbarObject.transform, false);
        Stretch(slidingArea.GetComponent<RectTransform>(), 2f);
        GameObject handle = CreateImage("Handle", Purple);
        Round(handle);
        handle.transform.SetParent(slidingArea.transform, false);
        Stretch(handle.GetComponent<RectTransform>(), 0f);
        scrollbar.handleRect = handle.GetComponent<RectTransform>();
        scrollbar.targetGraphic = handle.GetComponent<Image>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.size = 0.35f;

        lessonScrollRect.viewport = viewportRect;
        lessonScrollRect.content = lessonContainer.GetComponent<RectTransform>();
        lessonScrollRect.verticalScrollbar = scrollbar;
        lessonScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;

        GameObject briefing = CreateScreen("BriefingScreen", root.transform, DeepBlue);
        GameObject briefingContent = CreatePanel("Content", briefing.transform, CanvasColor, new Vector2(0.07f, 0.07f), new Vector2(0.93f, 0.93f));
        Round(briefingContent);
        AddShadow(briefingContent, new Color(0f, 0.18f, 0.29f, 0.24f), new Vector2(0f, -7f));
        VerticalLayoutGroup briefingLayout = AddVertical(briefingContent, 16f, 48f);
        briefingLayout.childAlignment = TextAnchor.MiddleLeft;
        FixedText("LESSON BRIEFING", briefingContent.transform, 14, FontStyle.Bold, Purple, TextAnchor.MiddleLeft, 26f);
        Text briefingTitle = FixedText("Lesson title", briefingContent.transform, 44, FontStyle.Bold, DeepBlue, TextAnchor.MiddleLeft, 62f);
        Text briefingBody = FixedText("Lesson briefing", briefingContent.transform, 21, FontStyle.Normal, Ink, TextAnchor.UpperLeft, 120f);
        GameObject objective = CreateImage("Objective", Soft);
        Round(objective);
        objective.transform.SetParent(briefingContent.transform, false);
        AddLayout(objective, 92f);
        HorizontalLayoutGroup objectiveLayout = AddHorizontal(objective, 16f, 0f);
        objectiveLayout.padding = new RectOffset(0, 24, 14, 14);
        objectiveLayout.childAlignment = TextAnchor.MiddleLeft;
        GameObject objectiveAccent = CreateImage("Accent", Blue);
        objectiveAccent.transform.SetParent(objective.transform, false);
        AddLayout(objectiveAccent, 64f, 8f);
        Text objectiveText = CreateText("OBJECTIVE  /  Lesson objective", objective.transform, 17, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddFlexible(objectiveText.gameObject, 1f);
        GameObject briefingActions = CreateHorizontalGroup("Actions", briefingContent.transform, 14f);
        AddFlexible(briefingActions, 1f);
        briefingActions.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        Button briefingBack = CreateButton("BACK TO LESSONS", briefingActions.transform, Soft, Ink, 118f);
        AddFlexible(briefingBack.gameObject, 1f);
        Button buildRules = CreateButton("BUILD RULES", briefingActions.transform, Blue, Ink, 118f);
        AddFlexible(buildRules.gameObject, 1f);

        GameObject builder = CreateScreen("RuleBuilderScreen", root.transform, CanvasColor);
        GameObject builderContent = CreatePanel("Content", builder.transform, Paper, new Vector2(0.055f, 0.055f), new Vector2(0.945f, 0.945f));
        Round(builderContent);
        AddShadow(builderContent, new Color(0.04f, 0.16f, 0.25f, 0.14f), new Vector2(0f, -6f));
        VerticalLayoutGroup builderLayout = AddVertical(builderContent, 10f, 30f);
        builderLayout.childAlignment = TextAnchor.UpperLeft;
        FixedText("MY RULES", builderContent.transform, 13, FontStyle.Bold, Purple, TextAnchor.MiddleCenter, 22f);
        Text ruleBuilderTitle = FixedText("Build the action sequence", builderContent.transform, 38, FontStyle.Bold, DeepBlue, TextAnchor.MiddleCenter, 50f);
        FixedText("Add actions from the left. Use UP and DOWN to match the objective.", builderContent.transform, 18, FontStyle.Normal, Muted, TextAnchor.MiddleCenter, 32f);
        GameObject columns = CreateHorizontalGroup("Columns", builderContent.transform, 18f);
        AddFlexible(columns, 1f);

        GameObject availablePanel = CreateImage("AvailableRules", Soft);
        Round(availablePanel);
        availablePanel.transform.SetParent(columns.transform, false);
        AddFlexible(availablePanel, 0.72f);
        AddVertical(availablePanel, 9f, 18f).childAlignment = TextAnchor.UpperLeft;
        FixedText("AVAILABLE ACTIONS", availablePanel.transform, 14, FontStyle.Bold, Ink, TextAnchor.MiddleCenter, 28f);
        GameObject availableList = CreateVerticalGroup("AvailableList", availablePanel.transform, 8f);
        AddFlexible(availableList, 1f);

        GameObject selectedPanel = CreateImage("SelectedRules", Paper);
        Round(selectedPanel);
        AddOutline(selectedPanel, Blue, new Vector2(2f, -2f));
        selectedPanel.transform.SetParent(columns.transform, false);
        AddFlexible(selectedPanel, 1.42f);
        AddVertical(selectedPanel, 9f, 18f).childAlignment = TextAnchor.UpperLeft;
        FixedText("YOUR SEQUENCE", selectedPanel.transform, 14, FontStyle.Bold, Blue, TextAnchor.MiddleCenter, 28f);
        GameObject selectedList = CreateVerticalGroup("SelectedList", selectedPanel.transform, 8f);
        AddFlexible(selectedList, 1f);
        Text emptySelection = CreateText("No actions selected yet", selectedList.transform, 18, FontStyle.Italic, Muted, TextAnchor.MiddleCenter);
        AddLayout(emptySelection.gameObject, 56f);

        Text ruleFeedback = FixedText("Build the sequence, then press Check.", builderContent.transform, 17, FontStyle.Normal, Muted, TextAnchor.MiddleCenter, 34f);
        GameObject builderActions = CreateHorizontalGroup("Actions", builderContent.transform, 14f);
        AddLayout(builderActions, 62f);
        builderActions.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        GameObject actionSpacerLeft = new("SpacerLeft", typeof(RectTransform));
        actionSpacerLeft.transform.SetParent(builderActions.transform, false);
        AddFlexible(actionSpacerLeft, 1f);
        Button builderBack = CreateButton("BACK TO BRIEFING", builderActions.transform, Soft, Ink, 58f, 250f);
        Button check = CreateButton("CHECK", builderActions.transform, Purple, Color.white, 58f, 190f);
        GameObject actionSpacerRight = new("SpacerRight", typeof(RectTransform));
        actionSpacerRight.transform.SetParent(builderActions.transform, false);
        AddFlexible(actionSpacerRight, 1f);

        GameObject gameplay = CreatePanel("GameplayHUD", root.transform, Color.clear, Vector2.zero, Vector2.one);
        gameplay.GetComponent<Image>().raycastTarget = false;
        GameObject topBar = CreatePanel("TopBar", gameplay.transform, new Color(1f, 1f, 1f, 0.96f), new Vector2(0.03f, 0.89f), new Vector2(0.97f, 0.975f));
        Round(topBar);
        AddShadow(topBar, new Color(0.02f, 0.1f, 0.16f, 0.2f), new Vector2(0f, -5f));
        HorizontalLayoutGroup topLayout = AddHorizontal(topBar, 14f, 18f);
        topLayout.childAlignment = TextAnchor.MiddleLeft;
        GameObject liveAccent = CreateImage("LiveAccent", Blue);
        liveAccent.transform.SetParent(topBar.transform, false);
        AddLayout(liveAccent, 52f, 8f);
        Text liveLabel = CreateText("LESSON  /  LIVE CHECK", topBar.transform, 17, FontStyle.Bold, DeepBlue, TextAnchor.MiddleLeft);
        AddFlexible(liveLabel.gameObject, 1f);
        Text progress = CreateText("0 / 4 steps complete", topBar.transform, 19, FontStyle.Bold, Ink, TextAnchor.MiddleRight);
        AddLayout(progress.gameObject, 50f, 360f);
        GameObject feedbackPanel = CreatePanel("Feedback", gameplay.transform, new Color(1f, 1f, 1f, 0.96f), new Vector2(0.24f, 0.045f), new Vector2(0.76f, 0.13f));
        Round(feedbackPanel);
        AddOutline(feedbackPanel, Blue, new Vector2(2f, -2f));
        AddShadow(feedbackPanel, new Color(0.02f, 0.1f, 0.16f, 0.22f), new Vector2(0f, -5f));
        Text gameplayFeedback = CreateText("The family is getting ready...", feedbackPanel.transform, 20, FontStyle.Bold, DeepBlue, TextAnchor.MiddleCenter);
        Stretch(gameplayFeedback.rectTransform, 18f);

        GameObject result = CreateScreen("ResultScreen", root.transform, Cream);
        CreatePanel("LeftAccent", result.transform, Hex("5ED0C4"), new Vector2(0.045f, 0.39f), new Vector2(0.35f, 0.65f));
        CreatePanel("RightAccent", result.transform, Hex("F33022"), new Vector2(0.65f, 0.18f), new Vector2(0.955f, 0.57f));
        GameObject resultContent = CreatePanel("Content", result.transform, CanvasColor, new Vector2(0.18f, 0.17f), new Vector2(0.82f, 0.83f));
        Round(resultContent);
        AddShadow(resultContent, new Color(0.18f, 0.13f, 0.05f, 0.18f), new Vector2(0f, -8f));
        VerticalLayoutGroup resultLayout = AddVertical(resultContent, 14f, 48f);
        resultLayout.childAlignment = TextAnchor.MiddleCenter;
        FixedText("LEVEL COMPLETE", resultContent.transform, 14, FontStyle.Bold, Purple, TextAnchor.MiddleCenter, 26f);
        Text resultTitle = FixedText("Lesson complete", resultContent.transform, 45, FontStyle.Bold, DeepBlue, TextAnchor.MiddleCenter, 64f);
        Text resultSummary = FixedText("Every essential item was packed in the planned order.", resultContent.transform, 20, FontStyle.Normal, Ink, TextAnchor.MiddleCenter, 74f);
        GameObject badge = CreateImage("SuccessBadge", Success);
        Round(badge);
        badge.transform.SetParent(resultContent.transform, false);
        AddLayout(badge, 72f, 740f);
        Text badgeText = CreateText("RULES VERIFIED", badge.transform, 18, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        Stretch(badgeText.rectTransform, 12f);
        GameObject resultActions = CreateHorizontalGroup("Actions", resultContent.transform, 14f);
        AddFlexible(resultActions, 1f);
        resultActions.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        Button backToLessons = CreateButton("BACK TO LESSONS", resultActions.transform, Soft, Ink, 70f);
        AddFlexible(backToLessons.gameObject, 1f);
        Button playAgain = CreateButton("PLAY AGAIN", resultActions.transform, Blue, Ink, 70f);
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

        Transform existingUi = controller.transform.Find("GameFlowUI");
        if (existingUi == null) existingUi = controller.transform.Find("UnityNativeLessonFlow");
        if (existingUi != null) Object.DestroyImmediate(existingUi.gameObject);

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

    private static Button CreateButton(
        string label,
        Transform parent,
        Color background,
        Color foreground,
        float height,
        float width = -1f,
        bool rounded = true)
    {
        GameObject root = CreateImage(label + "Button", background);
        root.transform.SetParent(parent, false);
        if (rounded)
        {
            Round(root);
            AddShadow(root, new Color(0.04f, 0.15f, 0.24f, 0.2f), new Vector2(0f, -4f));
        }
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

    private static void Round(GameObject target)
    {
        Image image = target.GetComponent<Image>();
        if (image == null || roundedSprite == null) return;
        image.sprite = roundedSprite;
        image.type = Image.Type.Sliced;
    }

    private static void AddShadow(GameObject target, Color color, Vector2 distance)
    {
        Shadow shadow = target.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    private static void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        Outline outline = target.AddComponent<Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
        outline.useGraphicAlpha = true;
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
        element.flexibleHeight = 0f;
        if (width >= 0f) element.preferredWidth = width;
    }

    private static void AddFlexible(GameObject target, float width)
    {
        LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
        element.flexibleWidth = width;
        element.flexibleHeight = 1f;
    }

    private static void AddFlexibleWidth(GameObject target, float width)
    {
        LayoutElement element = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
        element.flexibleWidth = width;
        element.flexibleHeight = 0f;
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
