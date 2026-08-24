using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LessonButtonView : MonoBehaviour
{
    [Header("Card Visuals")]
    [Tooltip("Image that receives the background selected in Level Definition.")]
    [SerializeField] private Image backgroundImage;
    [Tooltip("Optional fallback used while a lesson has no assigned card background.")]
    [SerializeField] private Sprite fallbackBackground;

    [Header("Card Content")]
    [SerializeField] private Text numberLabel;
    [SerializeField] private Text titleLabel;
    [SerializeField] private Text descriptionLabel;
    [SerializeField] private Button openButton;
    [SerializeField] private Image backgroundScrim;
    [SerializeField] private RectTransform cardContent;

    private void Awake()
    {
        ValidateReferences();
        GameFlowUITheme.ApplyDynamic(gameObject);

        ApplyCardLayout();
    }

    private void ValidateReferences()
    {
        if (backgroundImage == null || numberLabel == null || titleLabel == null ||
            descriptionLabel == null || openButton == null || backgroundScrim == null ||
            cardContent == null)
        {
            Debug.LogError(
                "LessonButtonView requires all visual references to be assigned in LessonButton.prefab.",
                this);
        }
    }

    public void Bind(int lessonNumber, string title, string description, Sprite thumbnail, Action onOpen)
    {
        if (backgroundImage != null)
        {
            Sprite selectedBackground = thumbnail != null ? thumbnail : fallbackBackground;
            backgroundImage.sprite = selectedBackground;
            backgroundImage.color = selectedBackground != null
                ? Color.white
                : new Color(0.95686275f, 0.92156863f, 0.8509804f, 1f);
        }

        numberLabel.text = $"LESSON {lessonNumber:00}";
        titleLabel.text = title;
        descriptionLabel.text = description;
        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(() => onOpen());
        GameFlowUITheme.ApplyDynamic(gameObject);
        ApplyCardLayout();
    }

    private void ApplyCardLayout()
    {
        if (backgroundScrim != null)
        {
            RectTransform scrimRect = backgroundScrim.rectTransform;
            scrimRect.anchorMin = new Vector2(0f, 0f);
            scrimRect.anchorMax = new Vector2(0.62f, 1f);
            scrimRect.offsetMin = Vector2.zero;
            scrimRect.offsetMax = Vector2.zero;
            backgroundScrim.color = new Color(0.05f, 0.18f, 0.28f, 0.84f);
            backgroundScrim.raycastTarget = false;
        }

        if (cardContent != null)
        {
            cardContent.anchorMin = new Vector2(0.05f, 0.08f);
            cardContent.anchorMax = new Vector2(0.59f, 0.92f);
            cardContent.offsetMin = Vector2.zero;
            cardContent.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = cardContent.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.padding = new RectOffset(12, 12, 8, 8);
                layout.spacing = 3f;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
            }
        }

        StyleCardText(numberLabel, 15, 18, FontStyle.Bold);
        StyleCardText(titleLabel, 22, 28, FontStyle.Bold);
        StyleCardText(descriptionLabel, 17, 22, FontStyle.Normal);

        if (openButton != null)
        {
            LayoutElement buttonLayout = openButton.GetComponent<LayoutElement>();
            if (buttonLayout != null)
            {
                buttonLayout.preferredWidth = 142f;
                buttonLayout.preferredHeight = 42f;
                buttonLayout.flexibleWidth = 0f;
            }
        }
    }

    private static void StyleCardText(Text text, int minSize, int maxSize, FontStyle style)
    {
        if (text == null) return;

        text.color = Color.white;
        text.fontStyle = style;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
    }

#if UNITY_EDITOR
    public void Configure(Image background, Sprite fallback, Text number, Text title, Text description, Button button)
    {
        backgroundImage = background;
        fallbackBackground = fallback;
        numberLabel = number;
        titleLabel = title;
        descriptionLabel = description;
        openButton = button;
    }
#endif
}
