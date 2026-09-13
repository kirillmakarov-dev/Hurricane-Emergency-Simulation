using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameFlowState { MainMenu, Briefing, RuleBuilder, Playing, LevelResult }

public sealed class GameFlowController : MonoBehaviour
{
    public const string MainMenuSceneName = "MainMenu";
    public const string GameplaySceneName = "SampleScene";

    private static readonly Color DeepTeal = Hex("19324D");
    private static readonly Color Aqua = Hex("0F766E");
    private static readonly Color Cream = Hex("19324D");
    private static readonly Color Coral = Hex("C2413A");
    private static readonly Color Success = Hex("177A5D");
    private static readonly Color Warning = Hex("B93832");

    [SerializeField] private GameFlowView view;
    [SerializeField] private LevelCatalog levelCatalog;

    private readonly List<LevelDefinition> levels = new();
    private readonly List<RuleDefinition> selectedRules = new();
    private readonly List<GameObject> generatedLessonViews = new();
    private readonly List<GameObject> generatedRuleViews = new();
    private LevelDefinition level;
    private LevelSessionController session;
    private GameFlowState state;
    private int mistakes;
    private bool missionFailed;
    private bool resultScheduled;
    private bool isMenuScene;

    public GameFlowState State => state;

    private void Awake()
    {
        isMenuScene = SceneManager.GetActiveScene().name == MainMenuSceneName;
        if (levelCatalog == null)
        {
            Debug.LogError("GameFlowController requires a serialized LevelCatalog asset.", this);
            enabled = false;
            return;
        }

        levels.AddRange(levelCatalog.Levels);
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i] != null) levels[i].RebuildRuntimeData();
        }

        if (!isMenuScene && LessonLaunchContext.HasLesson)
        {
            level = FindLevel(LessonLaunchContext.LevelId);
        }
        if (view == null) view = GetComponentInChildren<GameFlowView>(true);
        if (view == null)
        {
            Debug.LogError("GameFlowController requires a serialized GameFlowView prefab instance.", this);
            enabled = false;
            return;
        }
        BindViewEvents();
    }

    private IEnumerator Start()
    {
        if (!enabled) yield break;
        if (isMenuScene)
        {
            LessonLaunchContext.Clear();
            ShowMainMenu();
            yield break;
        }
        while (SimulationManager.Instance == null) yield return null;
        if (!RestoreSelectedRules())
        {
            SceneManager.LoadScene(MainMenuSceneName);
            yield break;
        }
        ApplyLevelContent();
        yield return StartGameplay();
    }

    private void OnDestroy()
    {
        UnbindViewEvents();
        session?.Dispose();
    }

    private void BindViewEvents()
    {
        view.BriefingBackButton.onClick.AddListener(ShowMainMenu);
        view.BuildRulesButton.onClick.AddListener(ShowRuleBuilder);
        view.RuleBuilderBackButton.onClick.AddListener(ShowBriefing);
        view.CheckButton.onClick.AddListener(CheckRules);
        view.BackToLessonsButton.onClick.AddListener(ReturnToLessons);
        view.PlayAgainButton.onClick.AddListener(PlayAgain);
    }

    private void UnbindViewEvents()
    {
        if (view == null) return;
        view.BriefingBackButton.onClick.RemoveListener(ShowMainMenu);
        view.BuildRulesButton.onClick.RemoveListener(ShowRuleBuilder);
        view.RuleBuilderBackButton.onClick.RemoveListener(ShowBriefing);
        view.CheckButton.onClick.RemoveListener(CheckRules);
        view.BackToLessonsButton.onClick.RemoveListener(ReturnToLessons);
        view.PlayAgainButton.onClick.RemoveListener(PlayAgain);
    }

    private void ShowMainMenu()
    {
        state = GameFlowState.MainMenu;
        level = null;
        selectedRules.Clear();
        RefreshLessonList();
        view.ShowMainMenu();
    }

    private void ShowBriefing() { state = GameFlowState.Briefing; view.ShowBriefing(); }

    private void SelectLevel(LevelDefinition selectedLevel)
    {
        level = selectedLevel;
        selectedRules.Clear();
        ApplyLevelContent();
        ShowBriefing();
    }

    private void ApplyLevelContent()
    {
        view.BriefingTitleText.text = level.Title;
        view.BriefingBodyText.text = level.Briefing;
        view.ObjectiveText.text = "OBJECTIVE  /  " + level.Objective;
        view.RuleBuilderTitleText.text = level.Title;
        view.GameplayLessonText.text = level.Title.ToUpperInvariant() + "  /  LIVE CHECK";
        view.ResultTitleText.text = level.Title + " complete";
    }

    private void ShowRuleBuilder()
    {
        state = GameFlowState.RuleBuilder;
        view.RuleFeedbackText.text = "Arrange your actions, then select Check plan.";
        view.RuleFeedbackText.color = DeepTeal;
        view.ShowRuleBuilder();
        RefreshRuleLists();
    }

    private void CheckRules()
    {
        if (level == null) return;
        view.RuleFeedbackText.text = "Launching your selected rules. You will see the result during play.";
        view.RuleFeedbackText.color = Success;
        StartCoroutine(StartGameplayAfterConfirmation());
    }

    private IEnumerator StartGameplayAfterConfirmation()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        LessonLaunchContext.SetLesson(level.LevelId, selectedRules);
        SceneManager.LoadScene(GameplaySceneName);
    }

    private IEnumerator StartGameplay()
    {
        state = GameFlowState.Playing;
        mistakes = 0;
        missionFailed = false;
        resultScheduled = false;
        view.ProgressText.text = $"0 / {level.RequiredRuntimeEvents.Count} steps complete";
        view.GameplayFeedbackText.text = "The lesson is getting ready...";
        view.GameplayFeedbackText.color = Cream;
        view.ShowGameplay();

        session?.Dispose();
        session = new LevelSessionController(level.Mode, level.RequiredRuntimeEvents);
        session.StepEvaluated += HandleRuntimeStep;
        session.Start();
        SimulationManager.Instance.SwitchMode(level.Mode);

        missionFailed = !RuleValidator.Validate(selectedRules, level.ExpectedRuleIds).IsValid;

        List<string> commands = new();
        for (int i = 0; i < selectedRules.Count; i++) commands.Add(selectedRules[i].AnimationCommand);

        if (!LaunchConfiguredSequence(commands, ScheduleResultAfterAnimationSettles))
        {
            ShowFatalResult(level.Title + " is not available in this scene.");
        }

        yield break;
    }

    private bool LaunchConfiguredSequence(IReadOnlyList<string> commands, System.Action onCompleted)
    {
        IConfiguredSequenceMode configuredMode =
            SimulationManager.Instance.GetMode(level.Mode) as IConfiguredSequenceMode;
        if (configuredMode == null) return false;

        configuredMode.PlayConfiguredSequence(commands, onCompleted);
        return true;
    }

    private bool RestoreSelectedRules()
    {
        if (!LessonLaunchContext.HasLesson || level == null || LessonLaunchContext.LevelId != level.LevelId) return false;
        selectedRules.Clear();
        for (int i = 0; i < LessonLaunchContext.SelectedRuleIds.Count; i++)
        {
            RuleDefinition match = null;
            for (int j = 0; j < level.AvailableRules.Count; j++)
            {
                if (level.AvailableRules[j].RuleId == LessonLaunchContext.SelectedRuleIds[i])
                {
                    match = level.AvailableRules[j];
                    break;
                }
            }
            if (match == null) return false;
            selectedRules.Add(match);
        }
        return true;
    }

    private void HandleRuntimeStep(RuntimeStepResult result)
    {
        view.ProgressText.text = $"{result.CompletedSteps} / {result.TotalSteps} steps complete";
        switch (result.Type)
        {
            case RuntimeStepResultType.Correct:
                view.GameplayFeedbackText.text = FriendlyEventName(result.ReceivedEvent) + " completed correctly.";
                view.GameplayFeedbackText.color = Aqua;
                break;
            case RuntimeStepResultType.LevelCompleted:
                if (missionFailed)
                {
                    view.GameplayFeedbackText.text = "The mission was not completed because an incorrect action was selected or performed.";
                    view.GameplayFeedbackText.color = Coral;
                }
                else
                {
                    view.GameplayFeedbackText.text = "All lesson actions completed correctly.";
                    view.GameplayFeedbackText.color = Aqua;
                }
                break;
            case RuntimeStepResultType.Incorrect:
                mistakes++;
                missionFailed = true;
                view.GameplayFeedbackText.text = "That action is not part of this lesson.";
                view.GameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.OutOfOrder:
                mistakes++;
                missionFailed = true;
                view.GameplayFeedbackText.text = "Correct action, but it happened out of order.";
                view.GameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.Duplicate:
                // Animation clips may report the same physical action more than once.
                // Keep the last meaningful feedback visible instead of replacing it.
                break;
        }
    }

    private IEnumerator ShowResultAfterAnimationSettles()
    {
        yield return new WaitForSecondsRealtime(2.25f);
        state = GameFlowState.LevelResult;
        view.ResultTitleText.text = missionFailed ? "Mission failed" : level.Title + " complete";
        view.ResultSummaryText.text = missionFailed
            ? "Mission not completed. At least one selected or performed action was incorrect."
            : mistakes == 0
            ? "Perfect run. Every required action was completed in the planned order."
            : $"The lesson finished with {mistakes} recorded mistake(s).";
        view.ShowResult();
    }

    private void ScheduleResultAfterAnimationSettles()
    {
        if (resultScheduled) return;
        resultScheduled = true;
        StartCoroutine(ShowResultAfterAnimationSettles());
    }

    private void ShowFatalResult(string message)
    {
        session?.Dispose();
        state = GameFlowState.LevelResult;
        view.ResultSummaryText.text = message;
        view.ShowResult();
    }

    private void AddRule(RuleDefinition rule)
    {
        if (selectedRules.Contains(rule)) return;

        for (int i = 0; i < selectedRules.Count; i++)
        {
            if (!selectedRules[i].IsExclusiveWith(rule)) continue;

            view.RuleFeedbackText.text = "Choose either this action or its distractor, not both.";
            view.RuleFeedbackText.color = Warning;
            return;
        }

        selectedRules.Add(rule);
        RefreshRuleLists();
    }

    private void RemoveRule(RuleDefinition rule) { selectedRules.Remove(rule); RefreshRuleLists(); }

    private void MoveRule(int index, int offset)
    {
        int target = index + offset;
        if (index < 0 || index >= selectedRules.Count || target < 0 || target >= selectedRules.Count) return;
        RuleDefinition moving = selectedRules[index];
        selectedRules[index] = selectedRules[target];
        selectedRules[target] = moving;
        RefreshRuleLists();
    }

    private void RefreshRuleLists()
    {
        ClearGeneratedRuleViews();
        IReadOnlyList<RuleDefinition> available = level.AvailableRules;
        List<RuleDefinition> displayed = new();
        for (int i = 0; i < available.Count; i++)
        {
            RuleDefinition first = available[i];
            if (displayed.Contains(first)) continue;

            RuleDefinition alternative = null;
            for (int j = i + 1; j < available.Count; j++)
            {
                if (displayed.Contains(available[j]) || !first.IsExclusiveWith(available[j])) continue;
                alternative = available[j];
                break;
            }

            if (alternative != null)
            {
                RuleChoicePairView pair = view.CreateRuleChoicePair(view.AvailableRulesContainer);
                BindAvailableRule(first, pair.FirstOption, true);
                BindAvailableRule(alternative, pair.SecondOption, true);
                displayed.Add(alternative);
                generatedRuleViews.Add(pair.gameObject);
            }
            else
            {
                BindAvailableRule(first, view.CreateRuleOption(view.AvailableRulesContainer));
            }

            displayed.Add(first);
        }

        view.SetEmptySelectionVisible(selectedRules.Count == 0);
        for (int i = 0; i < selectedRules.Count; i++)
        {
            int index = i;
            RuleDefinition rule = selectedRules[i];
            SelectedRuleRowView row = view.CreateSelectedRuleRow(view.SelectedRulesContainer);
            row.Bind($"{i + 1}.  {rule.DisplayName}", i > 0, i < selectedRules.Count - 1,
                () => MoveRule(index, -1), () => MoveRule(index, 1), () => RemoveRule(rule));
            generatedRuleViews.Add(row.gameObject);
        }
    }

    private void BindAvailableRule(RuleDefinition rule, RuleOptionView option, bool compact = false)
    {
        option.Bind(rule.DisplayName, () => AddRule(rule), compact);
        option.SetInteractable(!selectedRules.Contains(rule) && !HasExclusiveSelection(rule));
        generatedRuleViews.Add(option.gameObject);
    }

    private bool HasExclusiveSelection(RuleDefinition candidate)
    {
        for (int i = 0; i < selectedRules.Count; i++)
        {
            if (selectedRules[i].IsExclusiveWith(candidate)) return true;
        }

        return false;
    }

    private void RefreshLessonList()
    {
        ClearGeneratedLessonViews();

        for (int i = 0; i < levels.Count; i++)
        {
            int lessonNumber = i + 1;
            LevelDefinition lesson = levels[i];
            LessonButtonView lessonView = view.CreateLessonButton();
            lessonView.Bind(lessonNumber, lesson.CardTitle, lesson.CardDescription, lesson.CardBackground, () => SelectLevel(lesson));
            generatedLessonViews.Add(lessonView.gameObject);
        }
    }

    private void ClearGeneratedLessonViews()
    {
        ClearGeneratedViews(generatedLessonViews);
    }

    private void ClearGeneratedRuleViews()
    {
        ClearGeneratedViews(generatedRuleViews);
    }

    private static void ClearGeneratedViews(List<GameObject> generatedViews)
    {
        for (int i = generatedViews.Count - 1; i >= 0; i--)
        {
            if (generatedViews[i] != null) Destroy(generatedViews[i]);
        }

        generatedViews.Clear();
    }

    private void ReturnToLessons()
    {
        session?.Dispose();
        LessonLaunchContext.Clear();
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void PlayAgain() { session?.Dispose(); SceneManager.LoadScene(GameplaySceneName); }

#if UNITY_EDITOR
    public void Configure(GameFlowView gameFlowView, LevelCatalog catalog)
    {
        view = gameFlowView;
        levelCatalog = catalog;
    }
#endif

    private LevelDefinition FindLevel(string levelId)
    {
        for (int i = 0; i < levels.Count; i++)
        {
            if (levels[i].LevelId == levelId) return levels[i];
        }

        return null;
    }

    private static string FriendlyEventName(Events eventType) => eventType switch
    {
        Events.GobagReminder => "Parents' reminder",
        Events.RadioBroadcast => "Radio announcement",
        Events.ReviewEmergencyPlan => "Emergency plan",
        Events.CheckGoBag => "Go Bag check",
        Events.CleanYard => "Yard cleanup",
        Events.CollectPlywood => "Plywood",
        Events.GetCannedFood => "Canned food",
        Events.GetCrackers => "Crackers",
        Events.GetWater => "Water",
        Events.ColorBook => "Color book",
        Events.PlayToy => "Play with toy",
        Events.HurricaneWarning => "Hurricane warning",
        Events.GetBicycle => "Bicycle",
        Events.GetToys => "Toys",
        Events.GetBall => "Ball",
        Events.PickBranches => "Pick up branches",
        Events.PickBottles => "Pick up bottles",
        Events.PickGlass => "Pick up broken glass",
        Events.CutBranches => "Cut wood",
        Events.PackWater => "Water",
        Events.PackFlashlight => "Flashlight",
        Events.PackBook => "Books",
        Events.PackCannedFood => "Canned food",
        Events.PackCrackers => "Crackers",
        Events.HurricaneWatch => "Hurricane watch",
        Events.PackClothes => "Clothes",
        Events.PackToys => "Toy",
        _ => eventType.ToString()
    };

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}
