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

    public void Bind(string text, Action onClick, bool compact = false)
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
        label.fontSize = compact ? 22 : 24;
        label.resizeTextForBestFit = false;
        label.rectTransform.offsetMin = compact ? new Vector2(14f, 6f) : new Vector2(20f, 10f);
        label.rectTransform.offsetMax = compact ? new Vector2(-14f, -6f) : new Vector2(-20f, -10f);
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
