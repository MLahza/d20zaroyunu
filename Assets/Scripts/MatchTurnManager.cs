using System;
using System.Collections.Generic;
using UnityEngine;

public class MatchTurnManager : MonoBehaviour
{
    public const int HandSize = 5;

    public AttackPhase CurrentPhase { get; private set; } = AttackPhase.Phase1_Prep;
    public Possession CurrentPossession { get; private set; } = Possession.Player;

    public CardSlot[] slots = new CardSlot[HandSize];
    public List<CardData> deck = new List<CardData>();
    public List<CardData> hand = new List<CardData>();

    public event Action<int, int, bool> OnDiceRolled;   // roll, slotIndex, success
    public event Action<AttackPhase> OnPhaseChanged;
    public event Action<Possession> OnPossessionChanged;
    public event Action OnGoalScored;

    static readonly int[,] Ranges = { { 0, 4 }, { 5, 9 }, { 10, 13 }, { 14, 17 }, { 18, 20 } };

    void Awake() => InitSlots();

    void InitSlots()
    {
        slots = new CardSlot[HandSize];
        for (int i = 0; i < HandSize; i++)
            slots[i] = new CardSlot(i, Ranges[i, 0], Ranges[i, 1]);
    }

    public void StartTurn()
    {
        foreach (var s in slots) s.Clear();
        hand.Clear();

        for (int i = 0; i < HandSize && deck.Count > 0; i++)
            hand.Add(deck[UnityEngine.Random.Range(0, deck.Count)]);
    }

    public bool PlaceCard(CardData card, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= HandSize || !hand.Contains(card)) return false;
        if (!slots[slotIndex].TryPlaceCard(card, CurrentPhase)) return false;

        hand.Remove(card);
        return true;
    }

    public bool AllSlotsFilled
    {
        get
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].currentCard == null) return false;
            return true;
        }
    }

    public bool RemoveCardFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= HandSize || slots[slotIndex].currentCard == null) return false;

        hand.Add(slots[slotIndex].currentCard);
        slots[slotIndex].Clear();
        return true;
    }

    public void RollDiceAndExecute() => RollDiceAndExecute(UnityEngine.Random.Range(1, 21));

    public void RollDiceAndExecute(int roll)
    {
        roll = Mathf.Clamp(roll, 1, 20);
        CardSlot slot = FindSlot(roll);
        bool success = slot != null && slot.ExecuteEffect();

        OnDiceRolled?.Invoke(roll, slot != null ? slot.slotIndex : -1, success);

        if (success) AdvancePhase();
        else SwitchPossession(Other(CurrentPossession));
    }

    static Possession Other(Possession p) => p == Possession.Player ? Possession.Opponent : Possession.Player;

    CardSlot FindSlot(int roll)
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].Contains(roll)) return slots[i];
        return null;
    }

    void AdvancePhase()
    {
        if (CurrentPhase == AttackPhase.Phase3_Finish)
        {
            OnGoalScored?.Invoke();
            SwitchPossession(Other(CurrentPossession));
            return;
        }

        CurrentPhase++;
        OnPhaseChanged?.Invoke(CurrentPhase);
        StartTurn();
    }

    void SwitchPossession(Possession to)
    {
        CurrentPossession = to;
        ResetAttack();
        OnPossessionChanged?.Invoke(to);
    }

    public void ResetAttack()
    {
        CurrentPhase = AttackPhase.Phase1_Prep;
        OnPhaseChanged?.Invoke(CurrentPhase);
        StartTurn();
    }
}
