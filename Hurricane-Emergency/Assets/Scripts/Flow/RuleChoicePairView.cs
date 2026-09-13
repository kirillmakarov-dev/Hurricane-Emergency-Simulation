using UnityEngine;
using UnityEngine.UI;

/// <summary>Prefab-owned side-by-side layout for two mutually exclusive actions.</summary>
public sealed class RuleChoicePairView : MonoBehaviour
{
    [SerializeField] private RuleOptionView firstOption;
    [SerializeField] private RuleOptionView secondOption;
    [SerializeField] private Text orLabel;

    public RuleOptionView FirstOption => firstOption;
    public RuleOptionView SecondOption => secondOption;

#if UNITY_EDITOR
    public void Configure(RuleOptionView first, RuleOptionView second, Text separator)
    {
        firstOption = first;
        secondOption = second;
        orLabel = separator;
    }
#endif
}
