using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private RectTransform confettiLayer;
    [SerializeField] private Image[] confettiPieces;

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
    [SerializeField] private RuleChoicePairView ruleChoicePairPrefab;

    [Header("Runtime")]
    [SerializeField] private Text progressText;
    [SerializeField] private Text gameplayFeedbackText;
    [SerializeField] private Text resultSummaryText;

    private Coroutine confettiCoroutine;

    private static readonly Color[] ConfettiPalette =
    {
        new Color32(255, 190, 74, 255),
        new Color32(31, 166, 150, 255),
        new Color32(247, 112, 89, 255),
        new Color32(82, 148, 226, 255),
        new Color32(177, 130, 224, 255),
        new Color32(246, 218, 105, 255)
    };

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
        if (activeScreen != resultScreen) StopConfetti();
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

    public void PlaySuccessConfetti()
    {
        if (confettiLayer == null || confettiPieces == null || confettiPieces.Length == 0)
        {
            Debug.LogWarning("GameFlowView needs a configured confetti layer and pieces.", this);
            return;
        }

        StopConfetti();
        confettiCoroutine = StartCoroutine(AnimateConfetti());
    }

    private IEnumerator AnimateConfetti()
    {
        Canvas.ForceUpdateCanvases();
        Rect rect = confettiLayer.rect;
        float width = rect.width;
        float height = rect.height;
        if (width <= 0f || height <= 0f)
        {
            confettiCoroutine = null;
            yield break;
        }

        List<ConfettiParticle> particles = new(confettiPieces.Length);
        for (int i = 0; i < confettiPieces.Length; i++)
        {
            Image image = confettiPieces[i];
            if (image == null) continue;

            RectTransform piece = image.rectTransform;
            piece.anchorMin = new Vector2(0.5f, 0.5f);
            piece.anchorMax = new Vector2(0.5f, 0.5f);
            piece.pivot = new Vector2(0.5f, 0.5f);
            piece.sizeDelta = new Vector2(Random.Range(7f, 13f), Random.Range(10f, 22f));
            piece.anchoredPosition = new Vector2(
                Random.Range(-width * 0.48f, width * 0.48f),
                Random.Range(height * 0.08f, height * 0.34f));
            piece.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));

            Color color = ConfettiPalette[Random.Range(0, ConfettiPalette.Length)];
            color.a = Random.Range(0.82f, 1f);
            image.color = color;
            image.gameObject.SetActive(true);

            particles.Add(new ConfettiParticle(
                piece,
                image,
                new Vector2(Random.Range(-230f, 230f), Random.Range(90f, 340f)),
                Random.Range(-420f, 420f),
                Random.Range(2.4f, 3.4f)));
        }

        const float gravity = 560f;
        float elapsed = 0f;
        while (elapsed < 3.5f)
        {
            float deltaTime = Time.unscaledDeltaTime;
            elapsed += deltaTime;

            for (int i = 0; i < particles.Count; i++)
            {
                ConfettiParticle particle = particles[i];
                particle.Age += deltaTime;
                if (particle.Age >= particle.Lifetime)
                {
                    particle.Image.gameObject.SetActive(false);
                    continue;
                }

                particle.Velocity.y -= gravity * deltaTime;
                particle.Rect.anchoredPosition += particle.Velocity * deltaTime;
                particle.Rect.Rotate(0f, 0f, particle.RotationSpeed * deltaTime);

                Color color = particle.BaseColor;
                float fadeStart = particle.Lifetime - 0.65f;
                if (particle.Age > fadeStart)
                {
                    color.a *= Mathf.Clamp01((particle.Lifetime - particle.Age) / 0.65f);
                }
                particle.Image.color = color;
            }

            yield return null;
        }

        for (int i = 0; i < confettiPieces.Length; i++)
        {
            if (confettiPieces[i] != null) confettiPieces[i].gameObject.SetActive(false);
        }
        confettiCoroutine = null;
    }

    private void StopConfetti()
    {
        if (confettiCoroutine != null)
        {
            StopCoroutine(confettiCoroutine);
            confettiCoroutine = null;
        }

        if (confettiPieces == null) return;
        for (int i = 0; i < confettiPieces.Length; i++)
        {
            if (confettiPieces[i] != null) confettiPieces[i].gameObject.SetActive(false);
        }
    }

    private sealed class ConfettiParticle
    {
        public readonly RectTransform Rect;
        public readonly Image Image;
        public readonly float RotationSpeed;
        public readonly float Lifetime;
        public readonly Color BaseColor;
        public Vector2 Velocity;
        public float Age;

        public ConfettiParticle(RectTransform rect, Image image, Vector2 velocity, float rotationSpeed, float lifetime)
        {
            Rect = rect;
            Image = image;
            Velocity = velocity;
            RotationSpeed = rotationSpeed;
            Lifetime = lifetime;
            BaseColor = image.color;
        }
    }

    public void SetEmptySelectionVisible(bool isVisible)
    {
        emptySelectionMessage.SetActive(isVisible);
    }

    public RuleOptionView CreateRuleOption(Transform parent)
    {
        return Instantiate(ruleOptionPrefab, parent);
    }

    public RuleChoicePairView CreateRuleChoicePair(Transform parent)
    {
        return Instantiate(ruleChoicePairPrefab, parent);
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
        RectTransform celebrationLayer,
        Image[] celebrationPieces,
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
        RuleChoicePairView pairPrefab,
        Text progress,
        Text gameplayFeedback,
        Text resultSummary)
    {
        mainMenuScreen = mainMenu;
        briefingScreen = briefing;
        ruleBuilderScreen = ruleBuilder;
        gameplayHud = gameplay;
        resultScreen = result;
        confettiLayer = celebrationLayer;
        confettiPieces = celebrationPieces;
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
        ruleChoicePairPrefab = pairPrefab;
        progressText = progress;
        gameplayFeedbackText = gameplayFeedback;
        resultSummaryText = resultSummary;
    }
#endif
}
