using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class RuleOptionView : MonoBehaviour
{
    [SerializeField] private Text label;
    [SerializeField] private Button button;

    private void Awake()
    {
        GameFlowUITheme.ApplyDynamic(gameObject);
    }

    public void Bind(string text, Action onClick)
    {
        if (label == null || button == null)
        {
            Debug.LogError("RuleOptionView requires a label and a button reference.", this);
            return;
        }

        label.text = text;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick());
        GameFlowUITheme.ApplyDynamic(gameObject);

    }

    public void SetInteractable(bool interactable)
    {
        if (button != null) button.interactable = interactable;
    }

#if UNITY_EDITOR
    public void Configure(Text ruleLabel, Button ruleButton)
    {
        label = ruleLabel;
        button = ruleButton;
    }
#endif
}
