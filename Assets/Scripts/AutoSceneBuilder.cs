#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class AutoSceneBuilder
{
    static Font font;

    [MenuItem("Tools/Setup Full Game Scene")]
    public static void SetupFullGameScene()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            es.AddComponent<StandaloneInputModule>();
#endif
        }

        var oldUi = canvas.transform.Find("DraftUI");
        if (oldUi != null) Object.DestroyImmediate(oldUi.gameObject);
        var oldRoot = GameObject.Find("GameRoot");
        if (oldRoot != null) Object.DestroyImmediate(oldRoot);

        var root = Panel(canvas.transform, "DraftUI", Vector2.zero, Vector2.one, new Color(0.05f, 0.35f, 0.12f, 1f));

        var gameRoot = new GameObject("GameRoot", typeof(MatchTurnManager), typeof(DraftUIController));
        var manager = gameRoot.GetComponent<MatchTurnManager>();
        var ctrl = gameRoot.GetComponent<DraftUIController>();

        ctrl.phaseText = Label(root, "PhaseText", "FAZ", 36, new Vector2(0.02f, 0.93f), new Vector2(0.45f, 0.99f), TextAnchor.MiddleLeft);
        ctrl.possessionText = Label(root, "PossessionText", "TOP", 36, new Vector2(0.5f, 0.93f), new Vector2(0.75f, 0.99f), TextAnchor.MiddleLeft);

        Label(root, "SlotsTitle", "TAKTİK SLOTLARI", 28, new Vector2(0.05f, 0.87f), new Vector2(0.75f, 0.92f), TextAnchor.MiddleCenter);
        string[] ranges = { "0-4", "5-9", "10-13", "14-17", "18-20" };
        for (int i = 0; i < 5; i++)
        {
            float x0 = 0.05f + i * 0.14f;
            var box = Panel(root, $"Slot_{i}", new Vector2(x0, 0.62f), new Vector2(x0 + 0.13f, 0.86f), new Color(0.2f, 0.2f, 0.2f, 0.9f));
            var slot = box.gameObject.AddComponent<SlotUI>();
            slot.slotIndex = i;
            slot.manager = manager;
            slot.background = box.GetComponent<Image>();
            slot.label = Label(box, "Text", $"[{ranges[i]}]", 28, new Vector2(0f, 0.86f), Vector2.one, TextAnchor.MiddleCenter);
            slot.labelInset = 0.14f * 0.24f * 1080f;
            ctrl.slotUIs[i] = slot;
        }

        ctrl.resultText = Label(root, "ResultText", "", 40, new Vector2(0.05f, 0.47f), new Vector2(0.75f, 0.61f), TextAnchor.MiddleCenter);

        Label(root, "HandTitle", "ELİNDEKİ KARTLAR (slota sürükle)", 28, new Vector2(0.05f, 0.4f), new Vector2(0.75f, 0.46f), TextAnchor.MiddleCenter);
        for (int i = 0; i < 5; i++)
        {
            float x0 = 0.05f + i * 0.14f;
            var card = Panel(root, $"Card_{i}", new Vector2(x0, 0.04f), new Vector2(x0 + 0.13f, 0.39f), new Color(0.2f, 0.2f, 0.2f, 0.9f));
            card.gameObject.AddComponent<CanvasGroup>();
            var drag = card.gameObject.AddComponent<CardDragDrop>();
            drag.background = card.GetComponent<Image>();
            var t = Label(card, "Text", "", 22, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), TextAnchor.UpperCenter);
            t.supportRichText = true;
            drag.label = t;
            ctrl.handCards[i] = drag;
        }

        var dicePanel = Panel(root, "DicePanel", new Vector2(0.78f, 0.2f), new Vector2(0.97f, 0.8f), new Color(0.08f, 0.08f, 0.12f, 0.95f));
        Label(dicePanel, "DiceTitle", "D20 ZAR", 40, new Vector2(0f, 0.86f), new Vector2(1f, 0.98f), TextAnchor.MiddleCenter);
        Label(dicePanel, "DiceHint", "Zarı geri çek ve bırak", 22, new Vector2(0f, 0.8f), new Vector2(1f, 0.86f), TextAnchor.MiddleCenter);

        var area = Panel(dicePanel, "DiceArea", new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.8f), new Color(0.12f, 0.2f, 0.14f, 1f));

        var diceRt = Panel(area, "Dice", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.95f, 0.95f, 0.95f, 1f));
        diceRt.sizeDelta = new Vector2(110f, 110f);
        diceRt.anchoredPosition = Vector2.zero;
        var dt = Label(diceRt, "DiceValue", "-", 64, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        dt.color = Color.black;
        var dice = diceRt.gameObject.AddComponent<ThrowableDice2D>();
        dice.area = area;
        dice.valueText = dt;
        dice.manager = manager;
        ctrl.dice = dice;

        var btnGo = Panel(dicePanel, "RollButton", new Vector2(0.1f, 0.06f), new Vector2(0.9f, 0.24f), new Color(0.85f, 0.65f, 0.1f, 1f));
        var btn = btnGo.gameObject.AddComponent<Button>();
        btn.targetGraphic = btnGo.GetComponent<Image>();
        Label(btnGo, "Text", "ZAR AT", 44, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter).color = Color.black;
        ctrl.rollButton = btn;

        EditorUtility.SetDirty(ctrl);
        Selection.activeGameObject = gameRoot;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    static RectTransform Panel(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = color;
        return rt;
    }

    static Text Label(Transform parent, string name, string text, int size, Vector2 aMin, Vector2 aMax, TextAnchor anchor)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var t = go.GetComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
        t.raycastTarget = false;
        return t;
    }
}
#endif
