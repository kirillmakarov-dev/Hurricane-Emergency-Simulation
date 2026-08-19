using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LessonButtonView : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
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
            backgroundImage.sprite = thumbnail;
            backgroundImage.color = thumbnail != null
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
    public void Configure(Image background, Text number, Text title, Text description, Button button)
    {
        backgroundImage = background;
        numberLabel = number;
        titleLabel = title;
        descriptionLabel = description;
        openButton = button;
    }
#endif
}
