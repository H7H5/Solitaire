using UnityEngine;

public class ScoreKepper : MonoBehaviour
{
    public GameObject highScorePanel; //панель победы
    public Selectable[] topStack; // верхние позиции

    void Update()
    {
        if (HasWon())// проверка на победу
        {
            Win();
        }
    }
    public bool HasWon() // проверка на победу
    {
        int i = 0;
        foreach (Selectable topstack in topStack)
        {
            i += topstack.value;
        }
        
        if (i>=52)
        {
            return true;
        }
        else
        {
            highScorePanel.SetActive(false);
            return false;
        }
    }
    void Win() // включение победы
    {
        highScorePanel.SetActive(true);
        print("You Win");
    }
}
