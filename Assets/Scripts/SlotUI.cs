using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour, IDropHandler
{
    public int slotIndex;
    public MatchTurnManager manager;
    public Image background;
    public Text label;
    public float labelInset = 36f;

    public CardDragDrop Occupant { get; private set; }

    static readonly Color Idle = new Color(0.15f, 0.25f, 0.45f, 0.95f);
    static readonly Color Empty = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    static readonly Color HitSuccess = new Color(0.2f, 0.8f, 0.3f, 1f);
    static readonly Color HitFail = new Color(0.85f, 0.2f, 0.2f, 1f);

    Color? highlight;

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
        background.color = highlight ?? (Occupant != null ? Idle : Empty);
    }
}
