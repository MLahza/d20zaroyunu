using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image background;
    public Text label;

    public CardData Data { get; private set; }
    public SlotUI CurrentSlot { get; private set; }
    public bool DroppedOnSlot { get; private set; }

    static readonly Color Positive = new Color(0.25f, 0.45f, 0.3f, 0.95f);
    static readonly Color Negative = new Color(0.5f, 0.25f, 0.25f, 0.95f);
    static readonly Color EmptyColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    RectTransform rt;
    CanvasGroup group;
    RectTransform dragLayer;
    Transform homeParent;
    Vector2 homeAnchorMin, homeAnchorMax, homeOffsetMin, homeOffsetMax;
    int homeSibling;
    Vector2 cardSize;

    void Awake()
    {
        rt = (RectTransform)transform;
        group = GetComponent<CanvasGroup>();
        dragLayer = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
        homeParent = rt.parent;
        homeAnchorMin = rt.anchorMin;
        homeAnchorMax = rt.anchorMax;
        homeOffsetMin = rt.offsetMin;
        homeOffsetMax = rt.offsetMax;
        homeSibling = rt.GetSiblingIndex();
    }

    public void Bind(CardData data)
    {
        Data = data;
        if (data == null)
        {
            label.text = "";
            background.color = EmptyColor;
            return;
        }

        label.text = $"<b>{data.cardName}</b>\n\n{data.description}\n\nGüç: {data.powerValue}";
        background.color = data.isNegative ? Negative : Positive;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Data == null) { eventData.pointerDrag = null; return; }

        if (CurrentSlot == null) cardSize = rt.rect.size;
        else CurrentSlot.Release(this);

        CurrentSlot = null;
        DroppedOnSlot = false;

        Vector3 worldPos = rt.position;
        rt.SetParent(dragLayer, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cardSize;
        rt.localScale = Vector3.one;
        rt.position = worldPos;
        rt.SetAsLastSibling();

        group.blocksRaycasts = false;
        group.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Data == null) return;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragLayer, eventData.position, eventData.pressEventCamera, out Vector3 wp))
            rt.position = wp;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
        group.alpha = 1f;
        if (Data != null && !DroppedOnSlot) ResetToHome();
    }

    public void SnapTo(SlotUI slot)
    {
        var slotRect = (RectTransform)slot.transform;
        rt.SetParent(slotRect, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cardSize;

        float availH = slotRect.rect.height - slot.labelInset;
        float s = Mathf.Min(slotRect.rect.width / cardSize.x, availH / cardSize.y) * 0.95f;
        rt.localScale = Vector3.one * s;
        rt.anchoredPosition = new Vector2(0f, -slot.labelInset * 0.5f);

        CurrentSlot = slot;
        DroppedOnSlot = true;
    }

    public void ResetToHome()
    {
        rt.SetParent(homeParent, false);
        rt.anchorMin = homeAnchorMin;
        rt.anchorMax = homeAnchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = homeOffsetMin;
        rt.offsetMax = homeOffsetMax;
        rt.localScale = Vector3.one;
        rt.SetSiblingIndex(homeSibling);

        CurrentSlot = null;
        DroppedOnSlot = false;
        group.blocksRaycasts = true;
        group.alpha = 1f;
    }
}
