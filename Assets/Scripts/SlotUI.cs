using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public MatchTurnManager manager;
    public Image background;
    public Text label;
    public Text aiText;
    public float labelFraction = 0.13f;

    public CardDragDrop Occupant { get; private set; }
    public float InsetPixels => ((RectTransform)transform).rect.height * labelFraction;

    static readonly Color Idle = new Color(0.15f, 0.25f, 0.45f, 0.95f);
    static readonly Color Empty = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    static readonly Color HitSuccess = new Color(0.2f, 0.8f, 0.3f, 1f);
    static readonly Color HitFail = new Color(0.85f, 0.2f, 0.2f, 1f);

    Color? highlight;
    bool aiOccupied;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        var card = eventData.pointerDrag.GetComponent<CardDragDrop>();
        if (card == null || card.Data == null || Occupant != null) return;
        if (!manager.PlaceCard(card.Data, slotIndex)) return;

        Occupant = card;
        card.SnapTo(this);
        Refresh();
    }

    public void Release(CardDragDrop card)
    {
        if (Occupant != card) return;
        manager.RemoveCardFromSlot(slotIndex);
        Occupant = null;
        Refresh();
    }

    public void ClearOccupant()
    {
        Occupant = null;
        aiOccupied = false;
        if (aiText != null) aiText.text = "";
        Refresh();
    }

    public void SetAICard(CardData card)
    {
        aiOccupied = true;
        if (aiText != null) aiText.text = $"<b>{card.cardName}</b>\nGüç: {card.powerValue}";
        Refresh();
    }

    public void SetHighlight(bool success)
    {
        highlight = success ? HitSuccess : HitFail;
        Refresh();
    }

    public void ClearHighlight()
    {
        highlight = null;
        Refresh();
    }

    void Refresh()
    {
        background.color = highlight ?? (Occupant != null || aiOccupied ? Idle : Empty);
    }
}
