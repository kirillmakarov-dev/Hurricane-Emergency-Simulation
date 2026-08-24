using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class RuleOptionView : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Button button;

    public void Bind(string text, Action onClick)
    {
        label.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
    }

    public void SetInteractable(bool interactable)
    {
        button.interactable = interactable;
    }

#if UNITY_EDITOR
    public void Configure(Text ruleLabel, Button ruleButton)
    {
        label = ruleLabel;
        button = ruleButton;
    }
#endif
}
