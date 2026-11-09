using UnityEngine;

public class OpponentCardUI : MonoBehaviour
{
    public CardView view;

    public void ShowBack() => view.ShowBack();

    public void ShowFace(CardData card)
    {
        if (card == null) view.ShowBack();
        else view.Show(card);
    }
}
