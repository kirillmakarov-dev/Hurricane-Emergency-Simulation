using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared visual language for the lesson flow UI.
/// The game uses legacy Unity UI.Text, so the theme keeps the existing
/// serialized contracts while providing rounded, high-contrast controls.
/// </summary>
public static class GameFlowUITheme
{
    private static readonly Color Ink = Hex("19324D");
    private static readonly Color Sky = Hex("EAF7FF");
    private static readonly Color Surface = Hex("FFFFFF");
    private static readonly Color Teal = Hex("0F766E");
    private static readonly Color TealPressed = Hex("0B5F59");
    private static readonly Color Purple = Hex("5B4EDB");
    private static readonly Color Coral = Hex("F06D62");
    private static readonly Color Gold = Hex("FFCA5C");
    private static readonly Color Disabled = Hex("CBD6E2");

    private static Sprite roundedSprite;
    private static Font uiFont;
    private static bool uiFontResolved;

    public static void Apply(GameObject root)
    {
        if (root == null) return;

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++) StyleButton(buttons[i]);

        Text[] texts = root.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++) StyleText(texts[i]);

        for (int i = 0; i < buttons.Length; i++) StyleButtonText(buttons[i]);

        Image[] images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++) StyleSurface(images[i]);
    }

    public static void ApplyTo(GameObject target)
    {
        if (target == null) return;

        Button[] buttons = target.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++) StyleButton(buttons[i]);

        Text[] texts = target.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++) StyleText(texts[i]);

        for (int i = 0; i < buttons.Length; i++) StyleButtonText(buttons[i]);

        Image[] images = target.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++) StyleSurface(images[i]);
    }

    /// <summary>
    /// Styles runtime-created cards without touching their content images.
    /// Lesson cards receive their thumbnail after instantiation, so replacing
    /// those images here would make the card look empty.
    /// </summary>
    public static void ApplyDynamic(GameObject target)
    {
        if (target == null) return;

        Button[] buttons = target.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++) StyleButton(buttons[i]);

        Text[] texts = target.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++) StyleText(texts[i]);

        for (int i = 0; i < buttons.Length; i++) StyleButtonText(buttons[i]);
    }

    private static void StyleButton(Button button)
    {
        if (button == null) return;

        Image image = button.targetGraphic as Image;
        if (image == null) image = button.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = GetRoundedSprite();
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
            image.color = ButtonColor(button.gameObject.name);
        }

        Color normal = ButtonColor(button.gameObject.name);
        ColorBlock colors = button.colors;
        colors.normalColor = normal;
        colors.highlightedColor = Lighten(normal, 0.08f);
        colors.pressedColor = TealPressed;
        colors.selectedColor = Lighten(normal, 0.12f);
        colors.disabledColor = new Color(Disabled.r, Disabled.g, Disabled.b, 0.7f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.16f;
        button.colors = colors;

        Shadow shadow = button.GetComponent<Shadow>();
        if (shadow != null)
        {
            shadow.effectColor = new Color(0.08f, 0.2f, 0.3f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -3f);
            shadow.useGraphicAlpha = true;
        }
    }

    private static void StyleText(Text text)
    {
        if (text == null) return;

        Font resolvedFont = GetUIFont();
        if (resolvedFont != null)
        {
            text.font = resolvedFont;
        }
        text.color = Ink;
        string hierarchy = GetHierarchyName(text.transform).ToLowerInvariant();
        if (hierarchy.Contains("successbadge"))
        {
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
        }
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 16;
        text.resizeTextMaxSize = IsHeading(text) ? 38 : 30;

        if (IsHeading(text))
        {
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleLeft;
        }
        else
        {
            Button parentButton = text.GetComponentInParent<Button>();
            if (parentButton != null)
            {
                Color buttonColor = ButtonColor(parentButton.gameObject.name);
                float luminance = buttonColor.r * 0.2126f + buttonColor.g * 0.7152f + buttonColor.b * 0.0722f;
                if (luminance < 0.55f)
                {
                    text.color = Color.white;
                }

                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
            }
            else
            {
                if (!hierarchy.Contains("successbadge"))
                {
                    text.fontStyle = FontStyle.Normal;
                }
            }
        }
    }

    private static void StyleButtonText(Button button)
    {
        if (button == null) return;

        Color buttonColor = ButtonColor(button.gameObject.name);
        float luminance = buttonColor.r * 0.2126f + buttonColor.g * 0.7152f + buttonColor.b * 0.0722f;
        Text[] texts = button.GetComponentsInChildren<Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            Text text = texts[i];
            text.color = luminance < 0.55f ? Color.white : Ink;
            text.fontStyle = FontStyle.Bold;
            text.fontSize = 24;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 20;
            text.resizeTextMaxSize = 32;
            text.alignment = TextAnchor.MiddleCenter;
        }
    }

    private static void StyleSurface(Image image)
    {
        if (image == null || image.GetComponent<Button>() != null ||
            image.GetComponentInParent<LessonButtonView>() != null) return;

        string hierarchy = GetHierarchyName(image.transform).ToLowerInvariant();
        bool isSurface = hierarchy.Contains("screen") ||
                         hierarchy.Contains("content") ||
                         hierarchy.Contains("feedback") ||
                         hierarchy.Contains("objective") ||
                         hierarchy.Contains("available") ||
                         hierarchy.Contains("selected") ||
                         hierarchy.Contains("navigation") ||
                         hierarchy.Contains("actions") ||
                         hierarchy.Contains("topappbar");
        if (!isSurface) return;

        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.fillCenter = true;
        image.color = hierarchy.Contains("screen") ? Sky : Surface;
    }

    private static Color ButtonColor(string objectName)
    {
        string name = objectName.ToLowerInvariant();
        if (name.Contains("check") || name.Contains("build") || name.Contains("play again")) return Teal;
        if (name.Contains("remove") || name.Contains("delete")) return Coral;
        if (name.Contains("up") || name.Contains("down")) return Purple;
        if (name.Contains("back")) return new Color(0.93f, 0.96f, 0.99f, 1f);
        if (name.Contains("lesson") || name.Contains("card")) return Surface;
        return Gold;
    }

    private static bool IsHeading(Text text)
    {
        string hierarchy = GetHierarchyName(text.transform).ToLowerInvariant();
        return hierarchy.Contains("title") || hierarchy.Contains("objective") || hierarchy.Contains("topappbar");
    }

    private static string GetHierarchyName(Transform current)
    {
        string value = string.Empty;
        while (current != null)
        {
            value += "/" + current.name;
            current = current.parent;
        }
        return value;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null) return roundedSprite;

        const int size = 64;
        const float radius = 14f;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "GameFlow Rounded UI Sprite",
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, 0f, x - (size - 1 - radius));
                float dy = Mathf.Max(radius - y, 0f, y - (size - 1 - radius));
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius + 0.75f - distance);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        roundedSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
        roundedSprite.name = "GameFlow Rounded UI Sprite";
        roundedSprite.hideFlags = HideFlags.HideAndDontSave;
        return roundedSprite;
    }

    private static Color Lighten(Color color, float amount)
    {
        return Color.Lerp(color, Color.white, amount);
    }

    private static Font GetUIFont()
    {
        if (uiFontResolved) return uiFont;

        uiFontResolved = true;
        try
        {
            uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load the optional UI font: {exception.Message}");
        }

        return uiFont;
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString("#" + value, out Color color);
        return color;
    }
}
