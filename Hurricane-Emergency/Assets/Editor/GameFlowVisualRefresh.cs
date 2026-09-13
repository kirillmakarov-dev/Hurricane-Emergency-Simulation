using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Explicit editor-only migration of the existing UI assets; preserves scene and lesson references.</summary>
public static class GameFlowVisualRefresh
{
    const string Folder = "Assets/prefabs/GameFlow/";
    static Sprite panel;

    [MenuItem("Tools/Hurricane/Apply UI Visual Refresh")]
    public static void Apply()
    {
        panel = CreatePanelSprite();
        RepairCardBackgrounds();
        Edit("LessonButton", Card);
        Edit("RuleOptionButton", root =>
        {
            Height(root, 80);
            Text label = root.GetComponentInChildren<Text>();
            Type(label, 25, FontStyle.Bold);
            Insets(label.rectTransform, 20, 10);
        });
        Edit("SelectedRuleRow", root =>
        {
            Height(root, 88);
            var layout = root.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 10, 10); layout.spacing = 10;
            foreach (Text text in root.GetComponentsInChildren<Text>()) Type(text, 24, FontStyle.Bold);
            foreach (Button button in root.GetComponentsInChildren<Button>())
            {
                var le = button.GetComponent<LayoutElement>();
                le.preferredWidth = button.name == "XButton" ? 106 : 78;
                le.preferredHeight = 60;
                button.GetComponentInChildren<Text>().text = button.name == "XButton" ? "Remove" : button.name == "DNButton" ? "Down" : "Up";
                Type(button.GetComponentInChildren<Text>(), 21, FontStyle.Bold);
            }
            root.transform.Find("OrderBadge").GetComponentInChildren<Text>().color = Color.white;
        });
        Edit("GameFlowUI", Screens);
        AssetDatabase.SaveAssets();
        Debug.Log("UI_VISUAL_REFRESH_COMPLETE");
    }

    static void RepairCardBackgrounds()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:LevelDefinition", new[] { "Assets/Data/Lessons" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(path);
            if (level.CardBackground != null && level.Title != "Cleaning Garden") continue;
            string yaml = File.ReadAllText(path);
            var match = System.Text.RegularExpressions.Regex.Match(yaml, @"thumbnail: .*guid: (\w+)");
            if (!match.Success) continue;
            string imagePath = level.Title == "Cleaning Garden" ? "Assets/Sprites/outside garden view.jpg" : AssetDatabase.GUIDToAssetPath(match.Groups[1].Value);
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(imagePath))
            {
                if (!(asset is Sprite sprite)) continue;
                var data = new SerializedObject(level); data.FindProperty("thumbnail").objectReferenceValue = sprite;
                data.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(level); break;
            }
        }
    }

    static void Edit(string name, Action<GameObject> edit)
    {
        string path = Folder + name + ".prefab";
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (Text text in root.GetComponentsInChildren<Text>(true))
            {
                text.raycastTarget = false;
                text.color = GameFlowUITheme.Ink;
                text.resizeTextForBestFit = false;
                text.fontSize = Mathf.Max(24, text.fontSize);
                text.lineSpacing = 1.05f;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                var line = text.GetComponent<LayoutElement>();
                if (line != null && line.preferredHeight > 0) line.preferredHeight = Mathf.Max(line.preferredHeight, text.fontSize * 1.35f);
            }
            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                image.sprite = panel; image.type = Image.Type.Sliced;
            }
            foreach (Shadow shadow in root.GetComponentsInChildren<Shadow>(true))
            {
                shadow.effectColor = shadow is Outline ? GameFlowUITheme.Border : new Color(.07f, .2f, .2f, .09f);
                shadow.effectDistance = shadow is Outline ? new Vector2(1, -1) : new Vector2(0, -3);
            }
            edit(root);
            GameFlowUITheme.Apply(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    static void Card(GameObject root)
    {
        Height(root, 280);
        var view = root.GetComponent<LessonButtonView>();
        var so = new SerializedObject(view);
        Text number = so.FindProperty("numberLabel").objectReferenceValue as Text;
        Text title = so.FindProperty("titleLabel").objectReferenceValue as Text;
        Text description = so.FindProperty("descriptionLabel").objectReferenceValue as Text;
        Button open = so.FindProperty("openButton").objectReferenceValue as Button;
        var content = so.FindProperty("cardContent").objectReferenceValue as RectTransform;
        var scrim = so.FindProperty("backgroundScrim").objectReferenceValue as Image;
        root.GetComponent<Image>().color = Color.white;
        scrim.color = Color.white;
        Anchors(scrim.rectTransform, Vector2.zero, new Vector2(.70f, 1));
        Anchors(content, Vector2.zero, new Vector2(.70f, 1));
        var layout = content.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 18, 18); layout.spacing = 7;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childForceExpandHeight = false;
        Type(number, 20, FontStyle.Bold); number.color = GameFlowUITheme.Teal; Height(number.gameObject, 25);
        Type(title, 32, FontStyle.Bold); Height(title.gameObject, 40);
        Type(description, 23, FontStyle.Normal); description.color = GameFlowUITheme.Muted; Height(description.gameObject, 96);
        number.alignment = title.alignment = description.alignment = TextAnchor.UpperLeft;
        Height(open.transform.parent.gameObject, 52); Height(open.gameObject, 52);
        open.GetComponent<LayoutElement>().preferredWidth = 188;
        open.GetComponentInChildren<Text>().text = "Open lesson";
        Type(open.GetComponentInChildren<Text>(), 23, FontStyle.Bold);
        Transform previous = root.transform.Find("LessonArtwork");
        Image artwork = previous != null ? previous.GetComponent<Image>() : new GameObject("LessonArtwork", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        artwork.transform.SetParent(root.transform, false);
        Anchors(artwork.rectTransform, new Vector2(.72f, .08f), new Vector2(.98f, .92f));
        artwork.preserveAspect = true; artwork.type = Image.Type.Simple; artwork.sprite = null; artwork.color = GameFlowUITheme.Soft; artwork.raycastTarget = false;
        so.FindProperty("backgroundImage").objectReferenceValue = artwork;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void Screens(GameObject root)
    {
        Transform t = root.transform;
        var view = root.GetComponent<GameFlowView>();
        Transform bar = t.Find("MainMenuScreen/TopAppBar");
        // These are decorative labels copied from the reference, not working controls.
        foreach (Transform child in bar)
            if (child.name == "Navigation" || (child.GetComponent<Text>() != null && child.GetComponent<Text>().text.Contains("SETTINGS")))
                child.gameObject.SetActive(false);
        Text logo = bar.GetComponentInChildren<Text>(); logo.text = "HURRICANE READY"; Type(logo, 28, FontStyle.Bold);
        logo.GetComponent<LayoutElement>().preferredWidth = 600;
        var headerLayout = bar.GetComponent<HorizontalLayoutGroup>(); headerLayout.padding = new RectOffset(104, 104, 12, 12); headerLayout.childAlignment = TextAnchor.MiddleLeft;
        foreach (Shadow s in bar.GetComponents<Shadow>()) Object.DestroyImmediate(s);
        Transform main = t.Find("MainMenuScreen/Content");
        Anchors((RectTransform)main, new Vector2(.045f, .04f), new Vector2(.955f, .90f));
        main.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(40, 40, 28, 24);
        foreach (Text text in main.GetComponentsInChildren<Text>())
        {
            if (text.text.Contains("HURRICANE READY")) { text.text = "LEARN. PLAN. BE READY."; Type(text, 20, FontStyle.Bold); text.color = GameFlowUITheme.Teal; }
            if (text.text == "Choose a lesson") Type(text, 48, FontStyle.Bold);
            if (text.text.StartsWith("Learn emergency")) { text.text = "Choose a lesson, build your action plan, then see it in practice."; Type(text, 26, FontStyle.Normal); }
        }
        var grid = view.LessonButtonsContainer.GetComponent<GridLayoutGroup>(); grid.cellSize = new Vector2(740, 280); grid.spacing = new Vector2(24, 24);
        (grid.GetComponent<LessonGridLayout>() ?? grid.gameObject.AddComponent<LessonGridLayout>()).Configure(grid);
        var viewport = (RectTransform)grid.transform.parent; viewport.offsetMax = new Vector2(-24, 0);
        Type(view.BriefingTitleText, 52, FontStyle.Bold); Height(view.BriefingTitleText.gameObject, 68);
        Type(view.BriefingBodyText, 29, FontStyle.Normal); Height(view.BriefingBodyText.gameObject, 150);
        Type(view.ObjectiveText, 28, FontStyle.Bold); Height(view.ObjectiveText.transform.parent.gameObject, 160);
        Transform briefing = t.Find("BriefingScreen/Content");
        Anchors((RectTransform)briefing, new Vector2(.09f, .14f), new Vector2(.91f, .86f));
        briefing.GetComponent<VerticalLayoutGroup>().spacing = 24;
        FixActions(t.Find("BriefingScreen/Content/Actions"), 76);
        FixActions(t.Find("ResultScreen/Content/Actions"), 76);
        FixActions(t.Find("RuleBuilderScreen/Content/Actions"), 72);
        Type(view.RuleBuilderTitleText, 42, FontStyle.Bold);
        Transform builder = t.Find("RuleBuilderScreen/Content");
        Anchors((RectTransform)builder, new Vector2(.035f, .035f), new Vector2(.965f, .965f));
        foreach (Text text in builder.GetComponentsInChildren<Text>(true))
        {
            if (text.text == "MY RULES") { text.text = "BUILD YOUR PLAN"; Type(text, 20, FontStyle.Bold); text.color = GameFlowUITheme.Teal; }
            if (text.text.StartsWith("Add actions")) { text.text = "Choose actions on the left. Arrange them in the order you want to try."; Type(text, 25, FontStyle.Normal); }
            if (text.text == "AVAILABLE ACTIONS" || text.text == "YOUR SEQUENCE") { Type(text, 24, FontStyle.Bold); Height(text.gameObject, 36); text.alignment = TextAnchor.MiddleLeft; }
        }
        var availablePanel = view.AvailableRulesContainer.parent.GetComponent<LayoutElement>(); availablePanel.flexibleWidth = .9f;
        var selectedPanel = view.SelectedRulesContainer.parent.GetComponent<LayoutElement>(); selectedPanel.flexibleWidth = 1.4f;
        ScrollList(view.AvailableRulesContainer); ScrollList(view.SelectedRulesContainer);
        view.RuleFeedbackText.text = "Arrange your actions, then select Check plan.";
        Type(view.RuleFeedbackText, 24, FontStyle.Normal); Height(view.RuleFeedbackText.gameObject, 42);
        Type(view.GameplayLessonText, 28, FontStyle.Bold); Type(view.ProgressText, 26, FontStyle.Bold);
        Type(view.GameplayFeedbackText, 28, FontStyle.Bold);
        Anchors(t.Find("GameplayHUD/Feedback") as RectTransform, new Vector2(.16f, .035f), new Vector2(.84f, .145f));
        Type(view.ResultTitleText, 48, FontStyle.Bold); Type(view.ResultSummaryText, 28, FontStyle.Normal);
        Height(view.ResultSummaryText.gameObject, 116);
        Transform result = t.Find("ResultScreen/Content");
        foreach (Text label in result.GetComponentsInChildren<Text>())
            if (label.text == "LEVEL COMPLETE") label.text = "LESSON REVIEW";
        Anchors((RectTransform)result, new Vector2(.15f, .15f), new Vector2(.85f, .85f));
        var badge = result.Find("SuccessBadge"); Type(badge.GetComponentInChildren<Text>(), 25, FontStyle.Bold); badge.GetComponentInChildren<Text>().color = GameFlowUITheme.Teal;
        // The result can include mistakes, so the badge must not claim every rule passed.
        badge.GetComponentInChildren<Text>().text = "PRACTICE COMPLETE";
        t.Find("ResultScreen/LeftAccent").gameObject.SetActive(false); t.Find("ResultScreen/RightAccent").gameObject.SetActive(false);
        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            Text label = button.GetComponentInChildren<Text>(); Type(label, 26, FontStyle.Bold);
            label.text = button == view.CheckButton ? "Check plan" : char.ToUpperInvariant(label.text[0]) + label.text.Substring(1).ToLowerInvariant();
        }
    }

    static void ScrollList(Transform list)
    {
        if (list.parent.name == "ListViewport") return;
        if (list.GetComponent<GridLayoutGroup>() != null) Object.DestroyImmediate(list.GetComponent<GridLayoutGroup>());
        var vertical = list.GetComponent<VerticalLayoutGroup>() ?? list.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.spacing = 12; vertical.childControlHeight = true; vertical.childControlWidth = true; vertical.childForceExpandHeight = false; vertical.childForceExpandWidth = true;
        var scroll = new GameObject("ListViewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D), typeof(ScrollRect), typeof(LayoutElement));
        scroll.transform.SetParent(list.parent, false); scroll.transform.SetSiblingIndex(list.GetSiblingIndex());
        scroll.GetComponent<Image>().color = new Color(1, 1, 1, .01f);
        scroll.GetComponent<LayoutElement>().flexibleHeight = 1;
        list.SetParent(scroll.transform, false);
        var rect = list as RectTransform;
        rect.anchorMin = new Vector2(0, 1); rect.anchorMax = Vector2.one; rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = Vector2.zero; rect.sizeDelta = new Vector2(-8, 0);
        var fitter = list.GetComponent<ContentSizeFitter>() ?? list.gameObject.AddComponent<ContentSizeFitter>(); fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var sr = scroll.GetComponent<ScrollRect>(); sr.content = rect; sr.viewport = scroll.transform as RectTransform; sr.horizontal = false; sr.vertical = true; sr.scrollSensitivity = 36; sr.movementType = ScrollRect.MovementType.Clamped;
    }

    static void FixActions(Transform actions, float height)
    {
        Height(actions.gameObject, height);
        var layout = actions.GetComponent<HorizontalLayoutGroup>(); layout.childForceExpandHeight = false; layout.childAlignment = TextAnchor.MiddleCenter; layout.spacing = 20;
        foreach (Button button in actions.GetComponentsInChildren<Button>()) { Height(button.gameObject, height); button.GetComponent<LayoutElement>().minWidth = 260; }
    }
    static void Height(GameObject go, float height)
    {
        var le = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>(); le.preferredHeight = height; le.flexibleHeight = 0;
    }
    static void Type(Text text, int size, FontStyle style)
    {
        text.fontSize = size; text.fontStyle = style; text.resizeTextForBestFit = false;
    }
    static void Anchors(RectTransform rect, Vector2 min, Vector2 max) { rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    static void Insets(RectTransform rect, float x, float y) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(x, y); rect.offsetMax = new Vector2(-x, -y); }
    static Sprite CreatePanelSprite()
    {
        const string path = Folder + "RoundedPanel.png";
        if (!File.Exists(path))
        {
            var texture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
            {
                float dx = Mathf.Max(14 - x, 0, x - 49); float dy = Mathf.Max(14 - y, 0, y - 49);
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(14.5f - Mathf.Sqrt(dx * dx + dy * dy))));
            }
            texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG()); Object.DestroyImmediate(texture); AssetDatabase.ImportAsset(path);
        }
        var importer = (TextureImporter)AssetImporter.GetAtPath(path); importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single; importer.spriteBorder = new Vector4(16, 16, 16, 16); importer.mipmapEnabled = false; importer.alphaIsTransparency = true; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
}
