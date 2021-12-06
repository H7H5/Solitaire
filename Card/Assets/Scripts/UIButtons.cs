using UnityEngine;

public class UIButtons : MonoBehaviour
{
    public GameObject highScorePanel;// панель победы
    public void PlayAgain() // новая игра после победы
    {
        highScorePanel.SetActive(false);
        ResetScene();
    }
    public void ResetScene()
    {
        UpdateSprite[] cards = FindObjectsOfType<UpdateSprite>();
        foreach (UpdateSprite card in cards)
        {
            Destroy(card.gameObject);
        }
        ClearTopValues();
        FindObjectOfType<Solitaire>().PlayCards();
    }
    public void ClearTopValues()
    {
        Selectable[] selectables = FindObjectsOfType<Selectable>();
        foreach (Selectable selectable in selectables)
        {
            if (selectable.CompareTag("Top"))
            {
                selectable.suit = null;
                selectable.value = 0;
            }
        }
    }
}
