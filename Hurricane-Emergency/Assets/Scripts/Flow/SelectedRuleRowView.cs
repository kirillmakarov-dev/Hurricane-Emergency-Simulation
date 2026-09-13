using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class SelectedRuleRowView : MonoBehaviour
{
    [SerializeField] private Text orderLabel;
    [SerializeField] private Text actionLabel;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button removeButton;

    private void Awake()
    {
        GameFlowUITheme.ApplyDynamic(gameObject);
    }

    public void Bind(
        string text,
        bool allowReordering,
        bool canMoveUp,
        bool canMoveDown,
        Action onUp,
        Action onDown,
        Action onRemove)
    {
        int separator = text.IndexOf(".  ", StringComparison.Ordinal);
        orderLabel.text = separator >= 0 ? text.Substring(0, separator) : string.Empty;
        actionLabel.text = separator >= 0 ? text.Substring(separator + 3) : text;
        upButton.gameObject.SetActive(allowReordering);
        downButton.gameObject.SetActive(allowReordering);
        upButton.interactable = allowReordering && canMoveUp;
        downButton.interactable = allowReordering && canMoveDown;
        upButton.onClick.RemoveAllListeners();
        downButton.onClick.RemoveAllListeners();
        removeButton.onClick.RemoveAllListeners();
        upButton.onClick.AddListener(() => onUp());
        downButton.onClick.AddListener(() => onDown());
        removeButton.onClick.AddListener(() => onRemove());
        GameFlowUITheme.ApplyDynamic(gameObject);
    }

#if UNITY_EDITOR
    public void Configure(Text rowOrder, Text rowAction, Button up, Button down, Button remove)
    {
        orderLabel = rowOrder;
        actionLabel = rowAction;
        upButton = up;
        downButton = down;
        removeButton = remove;
    }
#endif
}
