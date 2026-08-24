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

        // Keep the action label readable after the shared button theme runs.
        label.text = text;
        label.color = new Color(0.10f, 0.20f, 0.30f, 1f);
        label.fontStyle = FontStyle.Bold;
        label.fontSize = 22;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 17;
        label.resizeTextMaxSize = 28;
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
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
