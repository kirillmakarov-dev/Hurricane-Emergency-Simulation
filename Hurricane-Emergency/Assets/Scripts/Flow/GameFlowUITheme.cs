using UnityEngine;
using UnityEngine.UI;

/// <summary>Shared colors and interaction states. Layout, sprites and typography belong to the prefabs.</summary>
public static class GameFlowUITheme
{
    public static readonly Color Ink = Hex("183B47");
    public static readonly Color Muted = Hex("49636C");
    public static readonly Color Canvas = Hex("EEF4F3");
    public static readonly Color Soft = Hex("E3EFED");
    public static readonly Color Teal = Hex("087F75");
    public static readonly Color Border = Hex("C6D8D5");

    public static void Apply(GameObject root)
    {
        if (root == null) return;
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.GetComponent<Button>() != null || image.GetComponentInParent<LessonButtonView>() != null) continue;
            switch (image.name)
            {
                case "MainMenuScreen": case "BriefingScreen": case "RuleBuilderScreen": case "ResultScreen":
                    image.color = Canvas; image.sprite = null; break;
                case "Content": case "SelectedRules": case "SelectedRuleRow": case "PairRow": case "TopBar": case "Feedback":
                case "LessonScrollArea": case "Viewport": image.color = Color.white; break;
                case "TopAppBar": image.color = Canvas; break;
                case "AvailableRules": case "Objective": case "SuccessBadge": image.color = Soft; break;
                case "OrderBadge": case "ORBadge": case "Accent": case "LiveAccent": case "Handle": image.color = Teal; break;
                case "VerticalScrollbar": image.color = Soft; break;
            }
        }
        ApplyDynamic(root);
    }

    public static void ApplyTo(GameObject root) => Apply(root);

    public static void ApplyDynamic(GameObject root)
    {
        if (root == null) return;
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            string name = button.name.ToLowerInvariant();
            bool primary = name.Contains("build") || name.Contains("check") || name.Contains("play again") || name.Contains("open");
            bool remove = name.Contains("remove") || name == "xbutton";
            Color normal = primary ? Teal : remove ? Hex("FCE8E4") : Soft;
            Color foreground = primary ? Color.white : remove ? Hex("9F3328") : Ink;
            if (button.targetGraphic is Image image)
            {
                // Selectable tint multiplies Image.color; keep the base white.
                image.color = Color.white;
            }
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = Color.Lerp(normal, primary ? Color.black : Border, .16f);
            colors.selectedColor = Color.Lerp(normal, primary ? Color.black : Border, .22f);
            colors.pressedColor = Color.Lerp(normal, Color.black, .12f);
            colors.disabledColor = Hex("DCE3E2");
            colors.colorMultiplier = 1;
            colors.fadeDuration = .16f;
            button.transition = Selectable.Transition.ColorTint;
            button.colors = colors;
            foreach (Text text in button.GetComponentsInChildren<Text>(true)) text.color = foreground;
        }
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}
