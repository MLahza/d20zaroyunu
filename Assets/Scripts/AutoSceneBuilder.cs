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


        const float start = 0.07f, colW = 0.11f, step = 0.13f;
        const float rowLeft = 0.03f, rowRight = 0.74f;
        const float centerX = 0.385f;
        var panelDark = new Color(0.2f, 0.2f, 0.2f, 0.9f);

        ctrl.phaseText = Label(root, "PhaseText", "FAZ", 32, new Vector2(rowLeft, 0.945f), new Vector2(0.40f, 0.995f), TextAnchor.MiddleLeft);
        ctrl.possessionText = Label(root, "PossessionText", "SIRA", 32, new Vector2(0.42f, 0.945f), new Vector2(rowRight, 0.995f), TextAnchor.MiddleLeft);

        Label(root, "OpponentTitle", "RAKİP KARTLARI", 22, new Vector2(rowLeft, 0.915f), new Vector2(rowRight, 0.945f), TextAnchor.MiddleCenter);
        const float oW = 0.075f, oStep = 0.062f;
        for (int i = 0; i < 5; i++)
        {
            float t = i - 2;
            float x0 = centerX - oW * 0.5f + t * oStep;
            float yOff = t * t * 0.0075f;
            var oc = Panel(root, $"Opp_{i}", new Vector2(x0, 0.695f + yOff), new Vector2(x0 + oW, 0.895f + yOff), Color.white);
            oc.localRotation = Quaternion.Euler(0f, 0f, t * 5f);
            var ui = oc.gameObject.AddComponent<OpponentCardUI>();
            ui.view = BuildCardFace(oc, 0.85f);
            ctrl.opponentCards[i] = ui;
        }

        Label(root, "SlotsTitle", "TAKTİK SLOTLARI", 22, new Vector2(rowLeft, 0.665f), new Vector2(rowRight, 0.695f), TextAnchor.MiddleCenter);
        string[] ranges = { "0-4", "5-9", "10-13", "14-17", "18-20" };
        for (int i = 0; i < 5; i++)
        {
            float x0 = start + i * step;
            var box = Panel(root, $"Slot_{i}", new Vector2(x0, 0.43f), new Vector2(x0 + colW, 0.665f), panelDark);
            var slot = box.gameObject.AddComponent<SlotUI>();
            slot.slotIndex = i;
            slot.manager = manager;
            slot.background = box.GetComponent<Image>();
            slot.aiText = Label(box, "AIText", "", 20, new Vector2(0.05f, 0.1f), new Vector2(0.95f, 0.85f), TextAnchor.MiddleCenter);
            slot.aiText.supportRichText = true;
            slot.label = Label(box, "Text", $"[{ranges[i]}]", 22, new Vector2(0f, 0.87f), Vector2.one, TextAnchor.MiddleCenter);
            slot.labelFraction = 0.13f;
            ctrl.slotUIs[i] = slot;
        }

        ctrl.resultText = Label(root, "ResultText", "", 30, new Vector2(rowLeft, 0.36f), new Vector2(rowRight, 0.425f), TextAnchor.MiddleCenter);

        Label(root, "HandTitle", "ELİNDEKİ KARTLAR (slota sürükle)", 22, new Vector2(rowLeft, 0.325f), new Vector2(rowRight, 0.36f), TextAnchor.MiddleCenter);
        const float hW = 0.088f, hStep = 0.072f;
        for (int i = 0; i < 5; i++)
        {
            float t = i - 2;
            float x0 = centerX - hW * 0.5f + t * hStep;
            float yOff = -t * t * 0.0075f;
            var card = Panel(root, $"Card_{i}", new Vector2(x0, 0.10f + yOff), new Vector2(x0 + hW, 0.32f + yOff), Color.white);
            card.localRotation = Quaternion.Euler(0f, 0f, -t * 5f);
            card.gameObject.AddComponent<CanvasGroup>();
            var drag = card.gameObject.AddComponent<CardDragDrop>();
            drag.view = BuildCardFace(card, 1f);
            ctrl.handCards[i] = drag;
        }

        var dicePanel = Panel(root, "DicePanel", new Vector2(0.76f, 0.08f), new Vector2(0.98f, 0.92f), new Color(0.08f, 0.08f, 0.12f, 0.95f));
        Label(dicePanel, "DiceTitle", "D20 ZAR", 36, new Vector2(0f, 0.92f), new Vector2(1f, 0.99f), TextAnchor.MiddleCenter);
        Label(dicePanel, "DiceHint", "Zara basılı tut, sürükle ve bırak", 20, new Vector2(0f, 0.87f), new Vector2(1f, 0.92f), TextAnchor.MiddleCenter);

        var area = Panel(dicePanel, "DiceArea", new Vector2(0.05f, 0.2f), new Vector2(0.95f, 0.86f), new Color(0.12f, 0.2f, 0.14f, 1f));

        var diceRt = Panel(area, "Dice", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Color.white);
        diceRt.sizeDelta = new Vector2(130f, 130f);
        diceRt.anchoredPosition = Vector2.zero;
        var dt = Label(diceRt, "DiceValue", "-", 44, Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        dt.color = new Color(0.1f, 0.1f, 0.2f, 1f);
        dt.fontStyle = FontStyle.Bold;
        var dice = diceRt.gameObject.AddComponent<ThrowableDice2D>();
        dice.area = area;
        dice.valueText = dt;
        dice.manager = manager;
        ctrl.dice = dice;

        var warn = Label(dicePanel, "WarningText", "Tüm slotlara kart yerleştirin!", 26, new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.19f), TextAnchor.MiddleCenter);
        warn.color = new Color(1f, 0.35f, 0.3f, 1f);
        warn.fontStyle = FontStyle.Bold;
        ctrl.warningText = warn;

        EditorUtility.SetDirty(ctrl);
        Selection.activeGameObject = gameRoot;
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    static CardView BuildCardFace(RectTransform card, float fs)
    {
        var view = card.gameObject.AddComponent<CardView>();
        view.border = card.GetComponent<Image>();
        view.border.color = new Color(0.85f, 0.7f, 0.2f, 1f);

        var front = Part(card, "Front", Vector2.zero, Vector2.one, new Color(0.12f, 0.12f, 0.16f, 1f));
        front.offsetMin = new Vector2(5f, 5f);
        front.offsetMax = new Vector2(-5f, -5f);
        view.front = front.gameObject;

        var nameBar = Part(front, "NameBar", new Vector2(0f, 0.86f), Vector2.one, new Color(0.18f, 0.18f, 0.26f, 1f));
        view.nameText = Label(nameBar, "Name", "", Mathf.RoundToInt(16 * fs), new Vector2(0.04f, 0f), new Vector2(0.96f, 1f), TextAnchor.MiddleCenter);
        view.nameText.fontStyle = FontStyle.Bold;
        BestFit(view.nameText, 9, Mathf.RoundToInt(16 * fs));

        view.art = Part(front, "Art", new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.84f), new Color(0.4f, 0.5f, 0.6f, 1f)).GetComponent<Image>();
        view.typeText = Label(view.art.transform, "Type", "", Mathf.RoundToInt(20 * fs), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        view.typeText.fontStyle = FontStyle.Bold;
        BestFit(view.typeText, 8, Mathf.RoundToInt(20 * fs));

        var desc = Part(front, "DescBox", new Vector2(0.05f, 0.14f), new Vector2(0.95f, 0.47f), new Color(0.95f, 0.92f, 0.82f, 1f));
        view.descText = Label(desc, "Desc", "", Mathf.RoundToInt(12 * fs), new Vector2(0.05f, 0.03f), new Vector2(0.95f, 0.97f), TextAnchor.MiddleCenter);
        view.descText.color = new Color(0.15f, 0.12f, 0.08f, 1f);
        BestFit(view.descText, 8, Mathf.RoundToInt(12 * fs));

        var powerBar = Part(front, "PowerBar", new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.12f), new Color(0.07f, 0.07f, 0.1f, 1f));
        view.powerText = Label(powerBar, "Power", "", Mathf.RoundToInt(14 * fs), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        view.powerText.color = new Color(1f, 0.85f, 0.3f, 1f);
        view.powerText.fontStyle = FontStyle.Bold;
        BestFit(view.powerText, 8, Mathf.RoundToInt(14 * fs));

        var back = Part(card, "Back", Vector2.zero, Vector2.one, new Color(0.15f, 0.2f, 0.45f, 1f));
        back.offsetMin = new Vector2(5f, 5f);
        back.offsetMax = new Vector2(-5f, -5f);
        view.back = back.gameObject;
        var frame = Part(back, "Frame", new Vector2(0.1f, 0.07f), new Vector2(0.9f, 0.93f), new Color(0.22f, 0.3f, 0.65f, 1f));
        var bt = Label(frame, "BackText", "D20", Mathf.RoundToInt(30 * fs), Vector2.zero, Vector2.one, TextAnchor.MiddleCenter);
        bt.fontStyle = FontStyle.Bold;
        back.gameObject.SetActive(false);

        return view;
    }

    static void BestFit(Text t, int min, int max)
    {
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = min;
        t.resizeTextMaxSize = max;
    }

    static RectTransform Part(Transform parent, string name, Vector2 aMin, Vector2 aMax, Color color)
    {
        var rt = Panel(parent, name, aMin, aMax, color);
        rt.GetComponent<Image>().raycastTarget = false;
        return rt;
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
