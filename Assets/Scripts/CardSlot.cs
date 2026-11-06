using System;
using UnityEngine;

[Serializable]
public class CardSlot
{
    public int slotIndex;
    public int minDiceValue;
    public int maxDiceValue;
    public CardData currentCard;

    public CardSlot(int index, int min, int max)
    {
        slotIndex = index;
        minDiceValue = min;
        maxDiceValue = max;
    }

    public bool Contains(int roll) => roll >= minDiceValue && roll <= maxDiceValue;

    public bool CardPlacementValidation(CardData card, AttackPhase currentPhase)
    {
        return card != null && card.allowedPhase == currentPhase;
    }

    public bool TryPlaceCard(CardData card, AttackPhase currentPhase)
    {
        if (!CardPlacementValidation(card, currentPhase)) return false;
        currentCard = card;
        return true;
    }

    public void Clear() => currentCard = null;

    // true = olumlu etki uygulandı, false = boş slot / olumsuz kart
    public bool ExecuteEffect()
    {
        if (currentCard == null) return false;

        Debug.Log($"[Slot {slotIndex}] {currentCard.cardName} ({currentCard.cardType}) power:{currentCard.powerValue}");
        return !currentCard.isNegative;
    }
}
