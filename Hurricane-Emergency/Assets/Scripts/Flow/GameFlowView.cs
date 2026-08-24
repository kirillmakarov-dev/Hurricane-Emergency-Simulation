using UnityEngine;
using UnityEngine.UI;

public sealed class GameFlowView : MonoBehaviour
{
    private void Awake()
    {
        GameFlowUITheme.Apply(gameObject);
    }

    [Header("Screens")]
    [SerializeField] private GameObject mainMenuScreen;
    [SerializeField] private GameObject briefingScreen;
    [SerializeField] private GameObject ruleBuilderScreen;
    [SerializeField] private GameObject gameplayHud;
    [SerializeField] private GameObject resultScreen;

    [Header("Navigation")]
    [SerializeField] private Button briefingBackButton;
    [SerializeField] private Button buildRulesButton;
    [SerializeField] private Button ruleBuilderBackButton;
    [SerializeField] private Button checkButton;
    [SerializeField] private Button backToLessonsButton;
    [SerializeField] private Button playAgainButton;

    [Header("Lesson Selection")]
    [SerializeField] private Transform lessonButtonsContainer;
    [SerializeField] private LessonButtonView lessonButtonPrefab;

    [Header("Selected Lesson")]
    [SerializeField] private Text briefingTitleText;
    [SerializeField] private Text briefingBodyText;
    [SerializeField] private Text objectiveText;
    [SerializeField] private Text ruleBuilderTitleText;
    [SerializeField] private Text gameplayLessonText;
    [SerializeField] private Text resultTitleText;

    [Header("Rule Builder")]
    [SerializeField] private Transform availableRulesContainer;
    [SerializeField] private Transform selectedRulesContainer;
    [SerializeField] private GameObject emptySelectionMessage;
    [SerializeField] private Text ruleFeedbackText;
    [SerializeField] private RuleOptionView ruleOptionPrefab;
    [SerializeField] private SelectedRuleRowView selectedRuleRowPrefab;

    [Header("Runtime")]
    [SerializeField] private Text progressText;
    [SerializeField] private Text gameplayFeedbackText;
    [SerializeField] private Text resultSummaryText;

    public Button BriefingBackButton => briefingBackButton;
    public Button BuildRulesButton => buildRulesButton;
    public Button RuleBuilderBackButton => ruleBuilderBackButton;
    public Button CheckButton => checkButton;
    public Button BackToLessonsButton => backToLessonsButton;
    public Button PlayAgainButton => playAgainButton;
    public Transform LessonButtonsContainer => lessonButtonsContainer;
    public Text BriefingTitleText => briefingTitleText;
    public Text BriefingBodyText => briefingBodyText;
    public Text ObjectiveText => objectiveText;
    public Text RuleBuilderTitleText => ruleBuilderTitleText;
    public Text GameplayLessonText => gameplayLessonText;
    public Text ResultTitleText => resultTitleText;
    public Transform AvailableRulesContainer => availableRulesContainer;
    public Transform SelectedRulesContainer => selectedRulesContainer;
    public Text RuleFeedbackText => ruleFeedbackText;
    public Text ProgressText => progressText;
    public Text GameplayFeedbackText => gameplayFeedbackText;
    public Text ResultSummaryText => resultSummaryText;

    public void ShowOnly(GameObject activeScreen)
    {
        mainMenuScreen.SetActive(activeScreen == mainMenuScreen);
        briefingScreen.SetActive(activeScreen == briefingScreen);
        ruleBuilderScreen.SetActive(activeScreen == ruleBuilderScreen);
        gameplayHud.SetActive(activeScreen == gameplayHud);
        resultScreen.SetActive(activeScreen == resultScreen);
    }

    public void ShowMainMenu() => ShowOnly(mainMenuScreen);
    public void ShowBriefing() => ShowOnly(briefingScreen);
    public void ShowRuleBuilder() => ShowOnly(ruleBuilderScreen);
    public void ShowGameplay() => ShowOnly(gameplayHud);
    public void ShowResult() => ShowOnly(resultScreen);

    public void SetEmptySelectionVisible(bool isVisible)
    {
        emptySelectionMessage.SetActive(isVisible);
    }

    public RuleOptionView CreateRuleOption(Transform parent)
    {
        return Instantiate(ruleOptionPrefab, parent);
    }

    public SelectedRuleRowView CreateSelectedRuleRow(Transform parent)
    {
        return Instantiate(selectedRuleRowPrefab, parent);
    }

    public LessonButtonView CreateLessonButton()
    {
        return Instantiate(lessonButtonPrefab, lessonButtonsContainer);
    }

#if UNITY_EDITOR
    public void Configure(
        GameObject mainMenu,
        GameObject briefing,
        GameObject ruleBuilder,
        GameObject gameplay,
        GameObject result,
        Button briefingBack,
        Button buildRules,
        Button builderBack,
        Button check,
        Button backToLessons,
        Button playAgain,
        Transform lessonContainer,
        LessonButtonView lessonPrefab,
        Text briefingTitle,
        Text briefingBody,
        Text objective,
        Text ruleBuilderTitle,
        Text gameplayLesson,
        Text resultTitle,
        Transform availableContainer,
        Transform selectedContainer,
        GameObject emptySelection,
        Text ruleFeedback,
        RuleOptionView optionPrefab,
        SelectedRuleRowView selectedPrefab,
        Text progress,
        Text gameplayFeedback,
        Text resultSummary)
    {
        mainMenuScreen = mainMenu;
        briefingScreen = briefing;
        ruleBuilderScreen = ruleBuilder;
        gameplayHud = gameplay;
        resultScreen = result;
        briefingBackButton = briefingBack;
        buildRulesButton = buildRules;
        ruleBuilderBackButton = builderBack;
        checkButton = check;
        backToLessonsButton = backToLessons;
        playAgainButton = playAgain;
        lessonButtonsContainer = lessonContainer;
        lessonButtonPrefab = lessonPrefab;
        briefingTitleText = briefingTitle;
        briefingBodyText = briefingBody;
        objectiveText = objective;
        ruleBuilderTitleText = ruleBuilderTitle;
        gameplayLessonText = gameplayLesson;
        resultTitleText = resultTitle;
        availableRulesContainer = availableContainer;
        selectedRulesContainer = selectedContainer;
        emptySelectionMessage = emptySelection;
        ruleFeedbackText = ruleFeedback;
        ruleOptionPrefab = optionPrefab;
        selectedRuleRowPrefab = selectedPrefab;
        progressText = progress;
        gameplayFeedbackText = gameplayFeedback;
        resultSummaryText = resultSummary;
    }
#endif
}
