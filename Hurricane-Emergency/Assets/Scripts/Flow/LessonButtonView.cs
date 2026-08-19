using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class LessonButtonView : MonoBehaviour
{
    [SerializeField] private Text numberLabel;
    [SerializeField] private Text titleLabel;
    [SerializeField] private Text descriptionLabel;
    [SerializeField] private Button openButton;

    public void Bind(int lessonNumber, string title, string description, Action onOpen)
    {
        numberLabel.text = $"LESSON {lessonNumber:00}";
        titleLabel.text = title;
        descriptionLabel.text = description;
        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(() => onOpen());
    }

#if UNITY_EDITOR
    public void Configure(Text number, Text title, Text description, Button button)
    {
        numberLabel = number;
        titleLabel = title;
        descriptionLabel = description;
        openButton = button;
    }
#endif
}
