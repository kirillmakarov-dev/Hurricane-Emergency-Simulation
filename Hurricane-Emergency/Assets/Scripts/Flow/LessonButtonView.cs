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

    private void Awake()
    {
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
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
