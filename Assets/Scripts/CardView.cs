using UnityEngine;
using UnityEngine.UI;

public class CardView : MonoBehaviour
{
    public Image border;
    public GameObject front;
    public GameObject back;
    public Image art;
    public Text nameText;
    public Text typeText;
    public Text descText;
    public Text powerText;

    static readonly Color BorderPositive = new Color(0.85f, 0.7f, 0.2f, 1f);
    static readonly Color BorderNegative = new Color(0.75f, 0.2f, 0.2f, 1f);
    static readonly Color BorderBack = new Color(0.85f, 0.85f, 0.9f, 1f);

    public void Show(CardData card)
    {
        front.SetActive(true);
        back.SetActive(false);

        border.color = card.isNegative ? BorderNegative : BorderPositive;
        art.color = TypeColor(card.cardType);
        nameText.text = card.cardName;
        typeText.text = TypeLabel(card.cardType);
        descText.text = card.description;
        powerText.text = $"GÜÇ {card.powerValue}";
    }

    public void ShowBack()
    {
        front.SetActive(false);
        back.SetActive(true);
        border.color = BorderBack;
    }

    static Color TypeColor(CardType t)
    {
        switch (t)
        {
            case CardType.Pass: return new Color(0.25f, 0.5f, 0.85f);
            case CardType.Dribble: return new Color(0.25f, 0.7f, 0.35f);
            case CardType.Shot: return new Color(0.95f, 0.55f, 0.15f);
            case CardType.Tactic: return new Color(0.6f, 0.35f, 0.8f);
            case CardType.Defense: return new Color(0.4f, 0.5f, 0.6f);
            default: return new Color(0.8f, 0.25f, 0.25f);
        }
    }

    static string TypeLabel(CardType t)
    {
        switch (t)
        {
            case CardType.Pass: return "PAS";
            case CardType.Dribble: return "ÇALIM";
            case CardType.Shot: return "ŞUT";
            case CardType.Tactic: return "TAKTİK";
            case CardType.Defense: return "SAVUNMA";
            default: return "FAUL";
        }
    }
}
