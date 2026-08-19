using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameFlowState { MainMenu, Briefing, RuleBuilder, Playing, LevelResult }

public sealed class GameFlowController : MonoBehaviour
{
    public const string MainMenuSceneName = "MainMenu";
    public const string GameplaySceneName = "SampleScene";

    private static readonly Color DeepTeal = Hex("184E57");
    private static readonly Color Aqua = Hex("65D1C8");
    private static readonly Color Cream = Hex("F4EBD9");
    private static readonly Color Coral = Hex("F47C65");
    private static readonly Color Success = Hex("2D9D78");
    private static readonly Color Warning = Hex("D95D50");

    [SerializeField] private GameFlowView view;

    private readonly List<RuleDefinition> selectedRules = new();
    private readonly List<GameObject> generatedRuleViews = new();
    private LevelDefinition level;
    private LevelSessionController session;
    private GameFlowState state;
    private int mistakes;
    private bool isMenuScene;

    public GameFlowState State => state;

    private void Awake()
    {
        isMenuScene = SceneManager.GetActiveScene().name == MainMenuSceneName;
        level = GoBagPrototypeLevelFactory.Create();
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
        yield return StartGameplay();
    }

    private void OnDestroy()
    {
        UnbindViewEvents();
        session?.Dispose();
        if (level != null) Destroy(level);
    }

    private void BindViewEvents()
    {
        view.OpenLessonButton.onClick.AddListener(ShowBriefing);
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
        view.OpenLessonButton.onClick.RemoveListener(ShowBriefing);
        view.BriefingBackButton.onClick.RemoveListener(ShowMainMenu);
        view.BuildRulesButton.onClick.RemoveListener(ShowRuleBuilder);
        view.RuleBuilderBackButton.onClick.RemoveListener(ShowBriefing);
        view.CheckButton.onClick.RemoveListener(CheckRules);
        view.BackToLessonsButton.onClick.RemoveListener(ReturnToLessons);
        view.PlayAgainButton.onClick.RemoveListener(PlayAgain);
    }

    private void ShowMainMenu() { state = GameFlowState.MainMenu; view.ShowMainMenu(); }
    private void ShowBriefing() { state = GameFlowState.Briefing; view.ShowBriefing(); }

    private void ShowRuleBuilder()
    {
        state = GameFlowState.RuleBuilder;
        view.RuleFeedbackText.text = "Build the sequence, then press Check.";
        view.RuleFeedbackText.color = DeepTeal;
        view.ShowRuleBuilder();
        RefreshRuleLists();
    }

    private void CheckRules()
    {
        RuleValidationResult result = RuleValidator.Validate(selectedRules, level.ExpectedRuleIds);
        view.RuleFeedbackText.text = result.Message;
        view.RuleFeedbackText.color = result.IsValid ? Success : Warning;
        if (result.IsValid) StartCoroutine(StartGameplayAfterConfirmation());
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
        view.ProgressText.text = $"0 / {level.RequiredRuntimeEvents.Count} steps complete";
        view.GameplayFeedbackText.text = "The family is getting ready...";
        view.GameplayFeedbackText.color = Cream;
        view.ShowGameplay();

        session?.Dispose();
        session = new LevelSessionController(level.Mode, level.RequiredRuntimeEvents);
        session.StepEvaluated += HandleRuntimeStep;
        session.Start();
        SimulationManager.Instance.SwitchMode(level.Mode);
        GoBagLesson lesson = SimulationManager.Instance.GetMode<GoBagLesson>();
        if (lesson == null)
        {
            ShowFatalResult("The Go Bag lesson is not available in this scene.");
            yield break;
        }

        List<string> commands = new();
        for (int i = 0; i < selectedRules.Count; i++) commands.Add(selectedRules[i].AnimationCommand);
        lesson.PlayConfiguredSequence(commands);
    }

    private bool RestoreSelectedRules()
    {
        if (!LessonLaunchContext.HasLesson || LessonLaunchContext.LevelId != level.LevelId) return false;
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
        return RuleValidator.Validate(selectedRules, level.ExpectedRuleIds).IsValid;
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
                view.GameplayFeedbackText.text = "All rules completed. The go bag is ready.";
                view.GameplayFeedbackText.color = Aqua;
                StartCoroutine(ShowResultAfterAnimationSettles());
                break;
            case RuntimeStepResultType.Incorrect:
                mistakes++;
                view.GameplayFeedbackText.text = "That item does not belong in this emergency bag.";
                view.GameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.OutOfOrder:
                mistakes++;
                view.GameplayFeedbackText.text = "Correct item, but this action happened out of order.";
                view.GameplayFeedbackText.color = Coral;
                break;
            case RuntimeStepResultType.Duplicate:
                view.GameplayFeedbackText.text = "Duplicate event ignored.";
                view.GameplayFeedbackText.color = Cream;
                break;
        }
    }

    private IEnumerator ShowResultAfterAnimationSettles()
    {
        yield return new WaitForSecondsRealtime(2.25f);
        state = GameFlowState.LevelResult;
        view.ResultSummaryText.text = mistakes == 0
            ? "Perfect run. Every essential item was packed in the planned order."
            : $"The bag is ready with {mistakes} recorded mistake(s).";
        view.ShowResult();
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
        for (int i = 0; i < level.AvailableRules.Count; i++)
        {
            RuleDefinition rule = level.AvailableRules[i];
            if (selectedRules.Contains(rule)) continue;
            RuleOptionView option = view.CreateRuleOption(view.AvailableRulesContainer);
            option.Bind(rule.DisplayName, () => AddRule(rule));
            generatedRuleViews.Add(option.gameObject);
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

    private void ClearGeneratedRuleViews()
    {
        for (int i = generatedRuleViews.Count - 1; i >= 0; i--)
            if (generatedRuleViews[i] != null) Destroy(generatedRuleViews[i]);
        generatedRuleViews.Clear();
    }

    private void ReturnToLessons()
    {
        session?.Dispose();
        LessonLaunchContext.Clear();
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void PlayAgain() { session?.Dispose(); SceneManager.LoadScene(GameplaySceneName); }

#if UNITY_EDITOR
    public void ConfigureView(GameFlowView gameFlowView) => view = gameFlowView;
#endif

    private static string FriendlyEventName(Events eventType) => eventType switch
    {
        Events.GobagReminder => "Parents' reminder",
        Events.PackWater => "Water",
        Events.PackFlashlight => "Flashlight",
        Events.PackBook => "Books",
        _ => eventType.ToString()
    };

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}
