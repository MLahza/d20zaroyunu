using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MatchTurnManager))]
public class DraftUIController : MonoBehaviour
{
    public SlotUI[] slotUIs = new SlotUI[5];
    public CardDragDrop[] handCards = new CardDragDrop[5];
    public OpponentCardUI[] opponentCards = new OpponentCardUI[5];
    public ThrowableDice2D dice;
    public Text phaseText;
    public Text possessionText;
    public Text resultText;
    public Text warningText;

    MatchTurnManager manager;
    readonly Dictionary<AttackPhase, List<CardData>> decks = new Dictionary<AttackPhase, List<CardData>>();
    Coroutine aiRoutine;
    string message = "";

    void Start()
    {
        manager = GetComponent<MatchTurnManager>();
        if (!ResolveReferences())
        {
            Debug.LogError("DraftUIController: UI eksik. Tools > Setup Full Game Scene menüsünü tekrar çalıştırın.", this);
            enabled = false;
            return;
        }
        BuildTestDecks();

        manager.OnDiceRolled += HandleDiceRolled;
        manager.OnPhaseChanged += phase => manager.deck = decks[phase];
        manager.OnPossessionChanged += p => message += p == Possession.Opponent ? "\nTop rakibe geçti!" : "\nTop sana geçti!";
        manager.OnGoalScored += () => message += "\nGOOOL!";

        dice.OnThrown += HandleThrown;
        dice.OnResolved += HandleResolved;

        manager.deck = decks[AttackPhase.Phase1_Prep];
        manager.StartTurn();
        DealHand();
        UpdateTexts();
    }

    bool ResolveReferences()
    {
        if (dice == null) dice = FindFirstObjectByType<ThrowableDice2D>(FindObjectsInactive.Include);

        var slots = FindObjectsByType<SlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (slotUIs == null || slotUIs.Length != 5) slotUIs = new SlotUI[5];
        foreach (var s in slots)
            if (s.slotIndex >= 0 && s.slotIndex < 5 && slotUIs[s.slotIndex] == null) slotUIs[s.slotIndex] = s;

        var cards = FindObjectsByType<CardDragDrop>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(cards, (a, b) => string.CompareOrdinal(a.name, b.name));
        if (handCards == null || handCards.Length != 5) handCards = new CardDragDrop[5];
        for (int i = 0; i < 5 && i < cards.Length; i++)
            if (handCards[i] == null) handCards[i] = cards[i];

        var opp = FindObjectsByType<OpponentCardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(opp, (a, b) => string.CompareOrdinal(a.name, b.name));
        if (opponentCards == null || opponentCards.Length != 5) opponentCards = new OpponentCardUI[5];
        for (int i = 0; i < 5 && i < opp.Length; i++)
            if (opponentCards[i] == null) opponentCards[i] = opp[i];

        if (phaseText == null) phaseText = FindText("PhaseText");
        if (possessionText == null) possessionText = FindText("PossessionText");
        if (resultText == null) resultText = FindText("ResultText");
        if (warningText == null) warningText = FindText("WarningText");

        if (dice != null && dice.manager == null) dice.manager = manager;
        foreach (var s in slotUIs)
            if (s != null && s.manager == null) s.manager = manager;

        if (dice == null || phaseText == null || possessionText == null || resultText == null) return false;
        foreach (var s in slotUIs) if (s == null) return false;
        foreach (var c in handCards) if (c == null) return false;
        foreach (var o in opponentCards) if (o == null) return false;
        return true;
    }

    static Text FindText(string objectName)
    {
        var go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<Text>() : null;
    }

    void Update()
    {
        bool playerTurn = manager.CurrentPossession == Possession.Player;
        bool filled = manager.AllSlotsFilled;

        dice.SetLocked(!(filled && playerTurn));

        if (warningText != null)
        {
            warningText.gameObject.SetActive(!playerTurn || !filled);
            warningText.text = playerTurn ? "Tüm slotlara kart yerleştirin!" : "Rakip oynuyor...";
        }
    }

    void HandleThrown()
    {
        CardDragDrop.DragLocked = true;
        foreach (var s in slotUIs) s.ClearHighlight();
        message = "";
        resultText.text = "";
    }

    void HandleDiceRolled(int roll, int slotIndex, bool success)
    {
        if (slotIndex >= 0) slotUIs[slotIndex].SetHighlight(success);

        string who = manager.CurrentPossession == Possession.Player ? "Sen" : "Rakip";
        var card = slotIndex >= 0 ? manager.slots[slotIndex].currentCard : null;
        message = $"{who} - Zar: {roll} → Slot {slotIndex}: {(card != null ? card.cardName : "BOŞ")}\n" +
                  (success ? "BAŞARILI! Faz ilerliyor." : "BAŞARISIZ!");
    }

    void HandleResolved()
    {
        DealHand();
        UpdateTexts();

        if (manager.CurrentPossession == Possession.Opponent)
        {
            if (aiRoutine != null) StopCoroutine(aiRoutine);
            aiRoutine = StartCoroutine(OpponentTurn());
        }
    }

    IEnumerator OpponentTurn()
    {
        yield return new WaitForSeconds(1.2f);

        var hand = new List<CardData>(manager.hand);
        var order = new List<int>();
        for (int i = 0; i < slotUIs.Length; i++) order.Add(i);
        for (int i = order.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (order[i], order[j]) = (order[j], order[i]);
        }

        for (int k = 0; k < hand.Count && k < order.Count; k++)
        {
            int slot = order[k];
            if (manager.PlaceCard(hand[k], slot)) slotUIs[slot].SetAICard(hand[k]);
            yield return new WaitForSeconds(0.35f);
        }

        yield return new WaitForSeconds(0.7f);
        dice.AutoThrow();
    }

    void DealHand()
    {
        var dealt = new List<CardData>(manager.hand);
        bool playerTurn = manager.CurrentPossession == Possession.Player;

        foreach (var s in slotUIs) s.ClearOccupant();

        for (int i = 0; i < handCards.Length; i++)
        {
            handCards[i].ResetToHome();
            if (playerTurn) handCards[i].Bind(i < dealt.Count ? dealt[i] : null);
            else handCards[i].ShowBack();

            if (playerTurn) opponentCards[i].ShowBack();
            else opponentCards[i].ShowFace(i < dealt.Count ? dealt[i] : null);
        }

        CardDragDrop.DragLocked = !playerTurn;
    }

    void UpdateTexts()
    {
        bool playerTurn = manager.CurrentPossession == Possession.Player;
        phaseText.text = $"FAZ: {manager.CurrentPhase}";
        possessionText.text = $"SIRA: {(playerTurn ? "SENDE" : "RAKİPTE")}";
        resultText.text = message;
    }

    void BuildTestDecks()
    {
        decks[AttackPhase.Phase1_Prep] = MakeDeck(AttackPhase.Phase1_Prep, new[]
        {
            ("Kısa Pas", "Yakındaki oyuncuya güvenli pas.", CardType.Pass, 3, false),
            ("Savunmadan Çıkış", "Topu orta sahaya taşı.", CardType.Tactic, 4, false),
            ("Pres Kırma", "Rakip baskısını aş.", CardType.Dribble, 5, false),
            ("Geri Pas", "Top kaybetme riski.", CardType.Pass, 1, true),
            ("Hatalı Kontrol", "Top ayağından açıldı.", CardType.Foul, 2, true),
        });
        decks[AttackPhase.Phase2_BuildUp] = MakeDeck(AttackPhase.Phase2_BuildUp, new[]
        {
            ("Ara Pası", "Savunma arkasına ara pası.", CardType.Pass, 6, false),
            ("Çalım", "Rakibi ters köşe yap.", CardType.Dribble, 5, false),
            ("Kanat Ortası", "Ceza sahasına orta.", CardType.Pass, 7, false),
            ("Top Kaybı", "Top rakibe kaptırıldı.", CardType.Defense, 2, true),
            ("Ofsayt", "Bayrak kalktı.", CardType.Foul, 1, true),
        });
        decks[AttackPhase.Phase3_Finish] = MakeDeck(AttackPhase.Phase3_Finish, new[]
        {
            ("Plase Şut", "Köşeye yerden şut.", CardType.Shot, 8, false),
            ("Röveşata", "Akrobatik vuruş.", CardType.Shot, 10, false),
            ("Vole", "Havadan sert şut.", CardType.Shot, 9, false),
            ("Auta Şut", "Top auta gitti.", CardType.Shot, 0, true),
            ("Kaleci Kurtardı", "Kaleci topu çeldi.", CardType.Defense, 0, true),
        });
    }

    static List<CardData> MakeDeck(AttackPhase phase, (string name, string desc, CardType type, int power, bool neg)[] defs)
    {
        var list = new List<CardData>();
        foreach (var d in defs)
        {
            var c = ScriptableObject.CreateInstance<CardData>();
            c.id = $"{phase}_{d.name}";
            c.cardName = d.name;
            c.description = d.desc;
            c.cardType = d.type;
            c.allowedPhase = phase;
            c.isNegative = d.neg;
            c.powerValue = d.power;
            list.Add(c);
        }
        return list;
    }
}
