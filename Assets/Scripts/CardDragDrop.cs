using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class CardDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public CardView view;

    public CardData Data { get; private set; }
    public SlotUI CurrentSlot { get; private set; }
    public bool DroppedOnSlot { get; private set; }
    public static bool DragLocked;

    bool dragging;

    Quaternion homeRotation;
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
        homeRotation = rt.localRotation;
    }

    public void ShowBack()
    {
        Data = null;
        view.ShowBack();
    }

    public void Bind(CardData data)
    {
        Data = data;
        if (data == null) view.ShowBack();
        else view.Show(data);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Data == null || DragLocked) { eventData.pointerDrag = null; return; }

        StopAllCoroutines();
        dragging = true;

        if (CurrentSlot == null) cardSize = rt.rect.size;
        else CurrentSlot.Release(this);

        CurrentSlot = null;
        DroppedOnSlot = false;

        Vector3 worldPos = rt.position;
        rt.SetParent(dragLayer, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cardSize;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.position = worldPos;
        rt.SetAsLastSibling();

        group.blocksRaycasts = false;
        group.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(dragLayer, eventData.position, eventData.pressEventCamera, out Vector3 wp))
            rt.position = wp;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        dragging = false;
        group.blocksRaycasts = true;
        group.alpha = 1f;
        if (!DroppedOnSlot) ResetToHome();
    }

    public void SnapTo(SlotUI slot)
    {
        var slotRect = (RectTransform)slot.transform;
        Vector3 worldPos = rt.position;

        rt.SetParent(slotRect, false);
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = cardSize;
        rt.localScale = Vector3.one;
        rt.position = worldPos;

        float inset = slot.InsetPixels;
        float availW = slotRect.rect.width - 12f;
        float availH = slotRect.rect.height - 2f * inset;
        float s = Mathf.Min(availW / cardSize.x, availH / cardSize.y);

        CurrentSlot = slot;
        DroppedOnSlot = true;

        StopAllCoroutines();
        StartCoroutine(SnapRoutine(Vector2.zero, s));
    }

    IEnumerator SnapRoutine(Vector2 targetPos, float targetScale)
    {
        const float duration = 0.15f;
        Vector2 startPos = rt.anchoredPosition;
        float startScale = rt.localScale.x;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / duration);
            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, k);
            rt.localScale = Vector3.one * Mathf.Lerp(startScale, targetScale, k);
            yield return null;
        }

        rt.anchoredPosition = targetPos;
        rt.localScale = Vector3.one * targetScale;
    }

    public void ResetToHome()
    {
        StopAllCoroutines();
        rt.SetParent(homeParent, false);
        rt.anchorMin = homeAnchorMin;
        rt.anchorMax = homeAnchorMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = homeOffsetMin;
        rt.offsetMax = homeOffsetMax;
        rt.localScale = Vector3.one;
        rt.localRotation = homeRotation;
        rt.SetSiblingIndex(homeSibling);

        CurrentSlot = null;
        DroppedOnSlot = false;
        group.blocksRaycasts = true;
        group.alpha = 1f;
    }
}
