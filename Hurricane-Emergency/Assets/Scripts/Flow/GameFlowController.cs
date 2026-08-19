using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameFlowState
{
    MainMenu,
    Briefing,
    RuleBuilder,
    Playing,
    LevelResult
}

public sealed class GameFlowController : MonoBehaviour
{
    private static readonly Color Ink = Hex("17252D");
    private static readonly Color DeepTeal = Hex("184E57");
    private static readonly Color Teal = Hex("238A8D");
    private static readonly Color Aqua = Hex("65D1C8");
    private static readonly Color Cream = Hex("F4EBD9");
    private static readonly Color Paper = Hex("FFF9EE");
    private static readonly Color Coral = Hex("F47C65");
    private static readonly Color Success = Hex("2D9D78");
    private static readonly Color Warning = Hex("D95D50");

    private static bool reopenLessonAfterReload;

    private readonly List<RuleDefinition> selectedRules = new();

    private LevelDefinition level;
    private LevelSessionController session;
    private Font font;
    private GameFlowState state;
    private int mistakes;

    private GameObject mainMenuScreen;
    private GameObject briefingScreen;
    private GameObject ruleBuilderScreen;
    private GameObject gameplayHud;
    private GameObject resultScreen;

    private Transform availableRulesContainer;
    private Transform selectedRulesContainer;
    private Text ruleFeedbackText;
    private Text progressText;
    private Text gameplayFeedbackText;
    private Text resultSummaryText;

    public GameFlowState State => state;

    private void Awake()
    {
        level = GoBagPrototypeLevelFactory.Create();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildInterface();
    }

    private IEnumerator Start()
    {
        while (SimulationManager.Instance == null)
        {
            yield return null;
        }

        SimulationManager.Instance.SwitchMode(ModeName.EntryScreen);

        if (reopenLessonAfterReload)
        {
            reopenLessonAfterReload = false;
            ShowBriefing();
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void OnDestroy()
    {
        session?.Dispose();

        if (level != null)
        {
            Destroy(level);
        }
    }

    private void ShowMainMenu()
    {
        state = GameFlowState.MainMenu;
        ShowOnly(mainMenuScreen);
    }

    private void ShowBriefing()
    {
        state = GameFlowState.Briefing;
        ShowOnly(briefingScreen);
    }

    private void ShowRuleBuilder()
    {
        state = GameFlowState.RuleBuilder;
        ruleFeedbackText.text = "Build the sequence, then press Check.";
        ruleFeedbackText.color = DeepTeal;
        ShowOnly(ruleBuilderScreen);
        RefreshRuleLists();
    }

    private void CheckRules()
    {
        RuleValidationResult result = RuleValidator.Validate(selectedRules, level.ExpectedRuleIds);
        ruleFeedbackText.text = result.Message;
        ruleFeedbackText.color = result.IsValid ? Success : Warning;

        if (result.IsValid)
        {
            StartCoroutine(StartGameplayAfterConfirmation());
        }
    }

    private IEnumerator StartGameplayAfterConfirmation()
    {
        yield return new WaitForSecondsRealtime(0.6f);

        state = GameFlowState.Playing;
        mistakes = 0;
        progressText.text = $"0 / {level.RequiredRuntimeEvents.Count} steps complete";
        gameplayFeedbackText.text = "The family is getting ready...";
        gameplayFeedbackText.color = Cream;
        ShowOnly(gameplayHud);

        session?.Dispose();
        session = new LevelSessionController(level.Mode, level.RequiredRuntimeEvents);
        session.StepEvaluated += HandleRuntimeStep;
        session.Start();

        SimulationManager.Instance.SwitchMode(level.Mode);
        GoBagLesson goBagLesson = SimulationManager.Instance.GetMode<GoBagLesson>();

        if (goBagLesson == null)
        {
            ShowFatalResult("The Go Bag lesson is not available in this scene.");
            yield break;
        }

        List<string> animationCommands = new();
        for (int i = 0; i < selectedRules.Count; i++)
        {
            animationCommands.Add(selectedRules[i].AnimationCommand);
        }

        goBagLesson.PlayConfiguredSequence(animationCommands);
    }

    private void HandleRuntimeStep(RuntimeStepResult result)
    {
        progressText.text = $"{result.CompletedSteps} / {result.TotalSteps} steps complete";

        switch (result.Type)
        {
            case RuntimeStepResultType.Correct:
                gameplayFeedbackText.text = FriendlyEventName(result.ReceivedEvent) + " completed correctly.";
                gameplayFeedbackText.color = Aqua;
                break;
            case RuntimeStepResultType.LevelCompleted:
                gameplayFeedbackText.text = "All rules completed. The go bag is ready.";
                gameplayFeedbackText.color = Aqua;
                StartCoroutine(ShowResultAfterAnimationSettles());
                break;
            case RuntimeStepResultType.Incorrect:
                mistakes++;
                gameplayFeedbackText.text = "That item does not belong in this emergency bag.";
                gameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.OutOfOrder:
                mistakes++;
                gameplayFeedbackText.text = "Correct item, but this action happened out of order.";
                gameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.Duplicate:
                gameplayFeedbackText.text = "Duplicate event ignored.";
                gameplayFeedbackText.color = Cream;
                break;
        }
    }

    private IEnumerator ShowResultAfterAnimationSettles()
    {
        yield return new WaitForSecondsRealtime(2.25f);

        state = GameFlowState.LevelResult;
        resultSummaryText.text = mistakes == 0
            ? "Perfect run. Every essential item was packed in the planned order."
            : $"The bag is ready with {mistakes} recorded mistake(s).";
        ShowOnly(resultScreen);
    }

    private void ShowFatalResult(string message)
    {
        session?.Dispose();
        state = GameFlowState.LevelResult;
        resultSummaryText.text = message;
        ShowOnly(resultScreen);
    }

    private void AddRule(RuleDefinition rule)
    {
        if (!selectedRules.Contains(rule))
        {
            selectedRules.Add(rule);
            RefreshRuleLists();
        }
    }

    private void RemoveRule(RuleDefinition rule)
    {
        selectedRules.Remove(rule);
        RefreshRuleLists();
    }

    private void MoveRule(int index, int offset)
    {
        int targetIndex = index + offset;
        if (index < 0 || index >= selectedRules.Count || targetIndex < 0 || targetIndex >= selectedRules.Count)
        {
            return;
        }

        RuleDefinition movingRule = selectedRules[index];
        selectedRules[index] = selectedRules[targetIndex];
        selectedRules[targetIndex] = movingRule;
        RefreshRuleLists();
    }

    private void RefreshRuleLists()
    {
        ClearChildren(availableRulesContainer);
        ClearChildren(selectedRulesContainer);

        for (int i = 0; i < level.AvailableRules.Count; i++)
        {
            RuleDefinition rule = level.AvailableRules[i];
            if (selectedRules.Contains(rule))
            {
                continue;
            }

            Button button = CreateButton(
                rule.DisplayName,
                availableRulesContainer,
                Paper,
                Ink,
                56f,
                TextAnchor.MiddleLeft);
            button.onClick.AddListener(() => AddRule(rule));
        }

        if (selectedRules.Count == 0)
        {
            Text empty = CreateText("No actions selected yet", selectedRulesContainer, 18, FontStyle.Italic, DeepTeal, TextAnchor.MiddleCenter);
            AddLayoutElement(empty.gameObject, 56f);
        }

        for (int i = 0; i < selectedRules.Count; i++)
        {
            int selectedIndex = i;
            RuleDefinition rule = selectedRules[i];
            GameObject row = CreateHorizontalGroup("SelectedRule", selectedRulesContainer, 8f);
            AddLayoutElement(row, 58f);

            Text label = CreateText($"{i + 1}.  {rule.DisplayName}", row.transform, 18, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
            AddFlexibleLayout(label.gameObject, 1f);

            Button up = CreateButton("UP", row.transform, Cream, DeepTeal, 48f);
            AddLayoutElement(up.gameObject, 48f, 56f);
            up.interactable = i > 0;
            up.onClick.AddListener(() => MoveRule(selectedIndex, -1));

            Button down = CreateButton("DOWN", row.transform, Cream, DeepTeal, 48f);
            AddLayoutElement(down.gameObject, 48f, 68f);
            down.interactable = i < selectedRules.Count - 1;
            down.onClick.AddListener(() => MoveRule(selectedIndex, 1));

            Button remove = CreateButton("X", row.transform, Coral, Color.white, 48f);
            AddLayoutElement(remove.gameObject, 48f, 48f);
            remove.onClick.AddListener(() => RemoveRule(rule));
        }
    }

    private void RestartFromMenu(bool reopenLesson)
    {
        reopenLessonAfterReload = reopenLesson;
        session?.Dispose();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void BuildInterface()
    {
        EnsureEventSystem();

        GameObject canvasObject = new("UnityNativeLessonFlow", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        mainMenuScreen = BuildMainMenu(canvasObject.transform);
        briefingScreen = BuildBriefing(canvasObject.transform);
        ruleBuilderScreen = BuildRuleBuilder(canvasObject.transform);
        gameplayHud = BuildGameplayHud(canvasObject.transform);
        resultScreen = BuildResult(canvasObject.transform);
    }

    private GameObject BuildMainMenu(Transform parent)
    {
        GameObject screen = CreateScreen("MainMenu", parent, Ink);
        AddDecorativeShapes(screen.transform);

        GameObject content = CreateContentPanel(screen.transform, new Color(1f, 1f, 1f, 0.96f));
        VerticalLayoutGroup layout = AddVerticalLayout(content, 18f, 28f);
        layout.childAlignment = TextAnchor.MiddleLeft;

        Text eyebrow = CreateText("HURRICANE READY  /  UNITY PROTOTYPE", content.transform, 17, FontStyle.Bold, Teal, TextAnchor.MiddleLeft);
        AddLayoutElement(eyebrow.gameObject, 30f);
        Text title = CreateText("Choose a lesson", content.transform, 52, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddLayoutElement(title.gameObject, 72f);
        Text subtitle = CreateText("Learn the rules first. Then watch your plan guide the simulation.", content.transform, 23, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft);
        AddLayoutElement(subtitle.gameObject, 64f);

        GameObject divider = CreateImage("Divider", content.transform, Aqua);
        AddLayoutElement(divider, 5f);

        GameObject card = CreateImage("GoBagCard", content.transform, Cream);
        AddLayoutElement(card, 260f);
        VerticalLayoutGroup cardLayout = AddVerticalLayout(card, 10f, 24f);
        cardLayout.childAlignment = TextAnchor.MiddleLeft;
        Text number = CreateText("LESSON 01", card.transform, 16, FontStyle.Bold, Coral, TextAnchor.MiddleLeft);
        AddLayoutElement(number.gameObject, 26f);
        Text cardTitle = CreateText(level.Title, card.transform, 34, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddLayoutElement(cardTitle.gameObject, 48f);
        Text cardBody = CreateText("Prepare an emergency bag by building a safe action sequence.", card.transform, 20, FontStyle.Normal, DeepTeal, TextAnchor.UpperLeft);
        AddFlexibleLayout(cardBody.gameObject, 1f);
        Button startButton = CreateButton("OPEN LESSON", card.transform, Teal, Color.white, 58f);
        AddLayoutElement(startButton.gameObject, 58f, 260f);
        startButton.onClick.AddListener(ShowBriefing);

        Text footer = CreateText("Additional lessons will appear here as their rule definitions are approved.", content.transform, 16, FontStyle.Italic, DeepTeal, TextAnchor.MiddleLeft);
        AddLayoutElement(footer.gameObject, 42f);
        return screen;
    }

    private GameObject BuildBriefing(Transform parent)
    {
        GameObject screen = CreateScreen("Briefing", parent, DeepTeal);
        AddDecorativeShapes(screen.transform);
        GameObject content = CreateContentPanel(screen.transform, Paper);
        VerticalLayoutGroup layout = AddVerticalLayout(content, 18f, 30f);
        layout.childAlignment = TextAnchor.MiddleLeft;

        Text eyebrow = CreateText("BEFORE YOU START", content.transform, 17, FontStyle.Bold, Coral, TextAnchor.MiddleLeft);
        AddLayoutElement(eyebrow.gameObject, 30f);
        Text title = CreateText(level.Title, content.transform, 48, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddLayoutElement(title.gameObject, 66f);
        Text briefing = CreateText(level.Briefing, content.transform, 23, FontStyle.Normal, DeepTeal, TextAnchor.UpperLeft);
        AddLayoutElement(briefing.gameObject, 110f);

        GameObject objectiveCard = CreateImage("Objective", content.transform, Cream);
        AddLayoutElement(objectiveCard, 150f);
        VerticalLayoutGroup objectiveLayout = AddVerticalLayout(objectiveCard, 8f, 22f);
        objectiveLayout.childAlignment = TextAnchor.MiddleLeft;
        Text objectiveLabel = CreateText("YOUR OBJECTIVE", objectiveCard.transform, 15, FontStyle.Bold, Teal, TextAnchor.MiddleLeft);
        AddLayoutElement(objectiveLabel.gameObject, 24f);
        Text objective = CreateText(level.Objective, objectiveCard.transform, 22, FontStyle.Bold, Ink, TextAnchor.UpperLeft);
        AddFlexibleLayout(objective.gameObject, 1f);

        Text flow = CreateText("1  Read the objective     2  Build the rules     3  Press Check     4  Watch the simulation", content.transform, 18, FontStyle.Bold, DeepTeal, TextAnchor.MiddleLeft);
        AddLayoutElement(flow.gameObject, 60f);

        GameObject actions = CreateHorizontalGroup("Actions", content.transform, 14f);
        AddLayoutElement(actions, 62f);
        Button back = CreateButton("BACK", actions.transform, Cream, DeepTeal, 62f);
        AddFlexibleLayout(back.gameObject, 1f);
        back.onClick.AddListener(ShowMainMenu);
        Button continueButton = CreateButton("BUILD RULES", actions.transform, Coral, Color.white, 62f);
        AddFlexibleLayout(continueButton.gameObject, 2f);
        continueButton.onClick.AddListener(ShowRuleBuilder);
        return screen;
    }

    private GameObject BuildRuleBuilder(Transform parent)
    {
        GameObject screen = CreateScreen("RuleBuilder", parent, Ink);
        GameObject content = CreatePanel("RuleWorkspace", screen.transform, new Color(1f, 1f, 1f, 0.98f), new Vector2(0.055f, 0.06f), new Vector2(0.945f, 0.94f));
        VerticalLayoutGroup layout = AddVerticalLayout(content, 14f, 24f);
        layout.childAlignment = TextAnchor.UpperLeft;

        Text eyebrow = CreateText("MY RULES  /  LESSON 01", content.transform, 16, FontStyle.Bold, Teal, TextAnchor.MiddleLeft);
        AddLayoutElement(eyebrow.gameObject, 26f);
        Text title = CreateText("Build the action sequence", content.transform, 38, FontStyle.Bold, Ink, TextAnchor.MiddleLeft);
        AddLayoutElement(title.gameObject, 52f);
        Text instruction = CreateText("Add actions from the left. Use UP and DOWN to match the objective.", content.transform, 19, FontStyle.Normal, DeepTeal, TextAnchor.MiddleLeft);
        AddLayoutElement(instruction.gameObject, 38f);

        GameObject columns = CreateHorizontalGroup("Columns", content.transform, 18f);
        AddFlexibleLayout(columns, 1f);

        GameObject availablePanel = CreateImage("AvailableRules", columns.transform, Cream);
        AddFlexibleLayout(availablePanel, 1f);
        VerticalLayoutGroup availableLayout = AddVerticalLayout(availablePanel, 9f, 18f);
        availableLayout.childAlignment = TextAnchor.UpperLeft;
        Text availableTitle = CreateText("AVAILABLE ACTIONS", availablePanel.transform, 16, FontStyle.Bold, Teal, TextAnchor.MiddleLeft);
        AddLayoutElement(availableTitle.gameObject, 30f);
        availableRulesContainer = CreateVerticalGroup("AvailableList", availablePanel.transform, 8f).transform;
        AddFlexibleLayout(availableRulesContainer.gameObject, 1f);

        GameObject selectedPanel = CreateImage("SelectedRules", columns.transform, Paper);
        AddFlexibleLayout(selectedPanel, 1.15f);
        VerticalLayoutGroup selectedLayout = AddVerticalLayout(selectedPanel, 9f, 18f);
        selectedLayout.childAlignment = TextAnchor.UpperLeft;
        Text selectedTitle = CreateText("YOUR SEQUENCE", selectedPanel.transform, 16, FontStyle.Bold, Coral, TextAnchor.MiddleLeft);
        AddLayoutElement(selectedTitle.gameObject, 30f);
        selectedRulesContainer = CreateVerticalGroup("SelectedList", selectedPanel.transform, 8f).transform;
        AddFlexibleLayout(selectedRulesContainer.gameObject, 1f);

        ruleFeedbackText = CreateText("Build the sequence, then press Check.", content.transform, 18, FontStyle.Bold, DeepTeal, TextAnchor.MiddleLeft);
        AddLayoutElement(ruleFeedbackText.gameObject, 40f);

        GameObject actions = CreateHorizontalGroup("Actions", content.transform, 14f);
        AddLayoutElement(actions, 62f);
        Button back = CreateButton("BACK TO BRIEFING", actions.transform, Cream, DeepTeal, 62f);
        AddFlexibleLayout(back.gameObject, 1f);
        back.onClick.AddListener(ShowBriefing);
        Button check = CreateButton("CHECK", actions.transform, Teal, Color.white, 62f);
        AddFlexibleLayout(check.gameObject, 1.5f);
        check.onClick.AddListener(CheckRules);
        return screen;
    }

    private GameObject BuildGameplayHud(Transform parent)
    {
        GameObject screen = CreatePanel("GameplayHUD", parent, Color.clear, Vector2.zero, Vector2.one);
        Image background = screen.GetComponent<Image>();
        background.raycastTarget = false;

        GameObject bar = CreatePanel("TopBar", screen.transform, new Color(Ink.r, Ink.g, Ink.b, 0.94f), new Vector2(0.03f, 0.89f), new Vector2(0.97f, 0.975f));
        HorizontalLayoutGroup layout = AddHorizontalLayout(bar, 14f, 18f);
        layout.childAlignment = TextAnchor.MiddleLeft;

        Text lesson = CreateText("GO BAG  /  LIVE CHECK", bar.transform, 17, FontStyle.Bold, Aqua, TextAnchor.MiddleLeft);
        AddFlexibleLayout(lesson.gameObject, 1f);
        progressText = CreateText("0 / 4 steps complete", bar.transform, 19, FontStyle.Bold, Color.white, TextAnchor.MiddleRight);
        AddLayoutElement(progressText.gameObject, 50f, 360f);

        GameObject feedback = CreatePanel("Feedback", screen.transform, new Color(DeepTeal.r, DeepTeal.g, DeepTeal.b, 0.95f), new Vector2(0.26f, 0.055f), new Vector2(0.74f, 0.13f));
        gameplayFeedbackText = CreateText("The family is getting ready...", feedback.transform, 20, FontStyle.Bold, Cream, TextAnchor.MiddleCenter);
        Stretch(gameplayFeedbackText.rectTransform, 18f);
        return screen;
    }

    private GameObject BuildResult(Transform parent)
    {
        GameObject screen = CreateScreen("Result", parent, new Color(Ink.r, Ink.g, Ink.b, 0.98f));
        AddDecorativeShapes(screen.transform);
        GameObject content = CreateContentPanel(screen.transform, Paper);
        VerticalLayoutGroup layout = AddVerticalLayout(content, 18f, 34f);
        layout.childAlignment = TextAnchor.MiddleCenter;

        Text eyebrow = CreateText("LEVEL COMPLETE", content.transform, 18, FontStyle.Bold, Success, TextAnchor.MiddleCenter);
        AddLayoutElement(eyebrow.gameObject, 32f);
        Text title = CreateText("Go bag ready", content.transform, 50, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        AddLayoutElement(title.gameObject, 72f);
        resultSummaryText = CreateText("Every essential item was packed in the planned order.", content.transform, 23, FontStyle.Normal, DeepTeal, TextAnchor.MiddleCenter);
        AddLayoutElement(resultSummaryText.gameObject, 110f);

        GameObject badge = CreateImage("SuccessBadge", content.transform, Aqua);
        AddLayoutElement(badge, 88f, 360f);
        Text badgeText = CreateText("RULES VERIFIED", badge.transform, 21, FontStyle.Bold, Ink, TextAnchor.MiddleCenter);
        Stretch(badgeText.rectTransform, 12f);

        GameObject actions = CreateHorizontalGroup("Actions", content.transform, 14f);
        AddLayoutElement(actions, 64f);
        Button menu = CreateButton("BACK TO LESSONS", actions.transform, Cream, DeepTeal, 64f);
        AddFlexibleLayout(menu.gameObject, 1f);
        menu.onClick.AddListener(() => RestartFromMenu(false));
        Button replay = CreateButton("PLAY AGAIN", actions.transform, Coral, Color.white, 64f);
        AddFlexibleLayout(replay.gameObject, 1f);
        replay.onClick.AddListener(() => RestartFromMenu(true));
        return screen;
    }

    private void ShowOnly(GameObject activeScreen)
    {
        mainMenuScreen.SetActive(activeScreen == mainMenuScreen);
        briefingScreen.SetActive(activeScreen == briefingScreen);
        ruleBuilderScreen.SetActive(activeScreen == ruleBuilderScreen);
        gameplayHud.SetActive(activeScreen == gameplayHud);
        resultScreen.SetActive(activeScreen == resultScreen);
    }

    private void AddDecorativeShapes(Transform parent)
    {
        GameObject stripe = CreatePanel("AccentStripe", parent, Teal, new Vector2(0f, 0.91f), new Vector2(1f, 1f));
        stripe.GetComponent<Image>().raycastTarget = false;

        GameObject circle = CreateImage("WeatherMark", parent, new Color(Aqua.r, Aqua.g, Aqua.b, 0.18f));
        RectTransform rect = circle.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.76f, 0.57f);
        rect.anchorMax = new Vector2(1.08f, 1.13f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localRotation = Quaternion.Euler(0f, 0f, -12f);
        circle.GetComponent<Image>().raycastTarget = false;
    }

    private GameObject CreateScreen(string objectName, Transform parent, Color color)
    {
        return CreatePanel(objectName, parent, color, Vector2.zero, Vector2.one);
    }

    private GameObject CreateContentPanel(Transform parent, Color color)
    {
        return CreatePanel("Content", parent, color, new Vector2(0.15f, 0.12f), new Vector2(0.85f, 0.86f));
    }

    private GameObject CreatePanel(string objectName, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject panel = CreateImage(objectName, parent, color);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return panel;
    }

    private GameObject CreateImage(string objectName, Transform parent, Color color)
    {
        GameObject imageObject = new(objectName, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private Text CreateText(string value, Transform parent, int size, FontStyle style, Color color, TextAnchor alignment)
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

    private Button CreateButton(
        string label,
        Transform parent,
        Color background,
        Color foreground,
        float preferredHeight,
        TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject buttonObject = CreateImage(label + "Button", parent, background);
        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = background;
        colors.highlightedColor = Color.Lerp(background, Color.white, 0.16f);
        colors.pressedColor = Color.Lerp(background, Color.black, 0.12f);
        colors.disabledColor = new Color(background.r, background.g, background.b, 0.35f);
        button.colors = colors;

        Text text = CreateText(label, buttonObject.transform, 17, FontStyle.Bold, foreground, alignment);
        Stretch(text.rectTransform, alignment == TextAnchor.MiddleLeft ? 18f : 10f);
        AddLayoutElement(buttonObject, preferredHeight);
        return button;
    }

    private GameObject CreateVerticalGroup(string objectName, Transform parent, float spacing)
    {
        GameObject group = new(objectName, typeof(RectTransform), typeof(VerticalLayoutGroup));
        group.transform.SetParent(parent, false);
        VerticalLayoutGroup layout = group.GetComponent<VerticalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        return group;
    }

    private GameObject CreateHorizontalGroup(string objectName, Transform parent, float spacing)
    {
        GameObject group = new(objectName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
        group.transform.SetParent(parent, false);
        HorizontalLayoutGroup layout = group.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        return group;
    }

    private VerticalLayoutGroup AddVerticalLayout(GameObject target, float spacing, float padding)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        int roundedPadding = Mathf.RoundToInt(padding);
        layout.padding = new RectOffset(roundedPadding, roundedPadding, roundedPadding, roundedPadding);
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        return layout;
    }

    private HorizontalLayoutGroup AddHorizontalLayout(GameObject target, float spacing, float padding)
    {
        HorizontalLayoutGroup layout = target.AddComponent<HorizontalLayoutGroup>();
        int roundedPadding = Mathf.RoundToInt(padding);
        layout.padding = new RectOffset(roundedPadding, roundedPadding, roundedPadding, roundedPadding);
        layout.spacing = spacing;
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = true;
        layout.childForceExpandWidth = false;
        return layout;
    }

    private static void AddLayoutElement(GameObject target, float preferredHeight, float preferredWidth = -1f)
    {
        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = target.AddComponent<LayoutElement>();
        }

        element.preferredHeight = preferredHeight;
        if (preferredWidth >= 0f)
        {
            element.preferredWidth = preferredWidth;
        }
    }

    private static void AddFlexibleLayout(GameObject target, float flexibleWidth)
    {
        LayoutElement element = target.GetComponent<LayoutElement>();
        if (element == null)
        {
            element = target.AddComponent<LayoutElement>();
        }

        element.flexibleWidth = flexibleWidth;
        element.flexibleHeight = 1f;
    }

    private static void Stretch(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child = parent.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }

    private static void EnsureEventSystem()
    {
        if (EventSystem.current != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static string FriendlyEventName(Events eventType)
    {
        return eventType switch
        {
            Events.GobagReminder => "Parents' reminder",
            Events.PackWater => "Water",
            Events.PackFlashlight => "Flashlight",
            Events.PackBook => "Books",
            _ => eventType.ToString()
        };
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}

public static class GameFlowBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterSceneBootstrap()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        CreateFlow();
    }

    private static void CreateFlow()
    {
        if (Object.FindFirstObjectByType<GameFlowController>() != null)
        {
            return;
        }

        GameObject flowObject = new("GameFlowController");
        flowObject.AddComponent<GameFlowController>();
    }
}
