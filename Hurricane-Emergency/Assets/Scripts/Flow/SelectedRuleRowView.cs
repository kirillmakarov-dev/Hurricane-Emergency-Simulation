using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SelectedRuleRowView : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button removeButton;

    public void Bind(string text, bool canMoveUp, bool canMoveDown, Action onUp, Action onDown, Action onRemove)
    {
        label.text = text;
        upButton.interactable = canMoveUp;
        downButton.interactable = canMoveDown;
        upButton.onClick.RemoveAllListeners();
        downButton.onClick.RemoveAllListeners();
        removeButton.onClick.RemoveAllListeners();
        upButton.onClick.AddListener(() => onUp());
        downButton.onClick.AddListener(() => onDown());
        removeButton.onClick.AddListener(() => onRemove());
    }

#if UNITY_EDITOR
    public void Configure(Text rowLabel, Button up, Button down, Button remove)
    {
        label = rowLabel;
        upButton = up;
        downButton = down;
        removeButton = remove;
    }
#endif
}
