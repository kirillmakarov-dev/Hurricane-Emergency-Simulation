using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LessonButtonView : MonoBehaviour
{
    [Header("Card Visuals")]
    [SerializeField] private Image backgroundImage;
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
        if (backgroundImage == null || numberLabel == null || titleLabel == null ||
            descriptionLabel == null || openButton == null || backgroundScrim == null || cardContent == null)
            Debug.LogError("LessonButtonView requires all visual references in LessonButton.prefab.", this);
        GameFlowUITheme.ApplyDynamic(gameObject);
    }

    public void Bind(int lessonNumber, string title, string description, Sprite thumbnail, Action onOpen)
    {
        Sprite selected = thumbnail != null ? thumbnail : fallbackBackground;
        backgroundImage.sprite = selected;
        backgroundImage.color = selected != null ? Color.white : GameFlowUITheme.Soft;
        numberLabel.text = $"LESSON {lessonNumber:00}";
        titleLabel.text = title;
        descriptionLabel.text = description;
        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(() => onOpen());
        GameFlowUITheme.ApplyDynamic(gameObject);
    }

#if UNITY_EDITOR
    public void Configure(Image background, Sprite fallback, Text number, Text title, Text description, Button button,
        Image scrim = null, RectTransform content = null)
    {
        backgroundImage = background;
        fallbackBackground = fallback;
        numberLabel = number;
        titleLabel = title;
        descriptionLabel = description;
        openButton = button;
        backgroundScrim = scrim;
        cardContent = content;
    }
#endif
}
