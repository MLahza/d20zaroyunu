using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "D20Football/Card")]
public class CardData : ScriptableObject
{
    public string id;
    public string cardName;
    [TextArea] public string description;
    public CardType cardType;
    public AttackPhase allowedPhase;
    public bool isNegative;
    public int powerValue;
}
