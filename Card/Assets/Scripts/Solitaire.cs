using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Solitaire : MonoBehaviour
{
    public Sprite[] cardFaces;               // спрайты карт
    public GameObject cardPref;              // префаб карты
    public GameObject deckButton;            // кнопка колоды карт
    public GameObject[] bottomsPos;          // нижние позиции карт
    public GameObject[] topPos;              // верхние позиции карт

    public static string[] suits = new string[] { "C", "D", "H", "S" };                                        // масть
    public static string[] values = new string[] { "A", "2", "3", "4","5","6","7" ,"8","9","10","J","Q","K"};  //номинал
    public List<string>[] bottoms;                                       //
    public List<string>[] tops;                                          //
    public List<string> tripsOnDisplay = new List<string>();             // три карты из колоды
    public List<List<string>> deckTrips = new List<List<string>>();      // масив по три карты из колоды
    public List<string> deck;                                            // карты в оставшейся колоде которые не смотрели
    public List<string> discardPile = new List<string>();                // карты в оставшейся колоде которые уже посмотрели

    private List<string> bottom0 = new List<string>();                   //
    private List<string> bottom1 = new List<string>();                   //
    private List<string> bottom2 = new List<string>();                   //
    private List<string> bottom3 = new List<string>();                   //
    private List<string> bottom4 = new List<string>();                   //
    private List<string> bottom5 = new List<string>();                   //
    private List<string> bottom6 = new List<string>();                   //

    private int deckLocation;
    private int trips;     // количество стопок карт по 3
    private int tripsRemainder;  // остаток от стопок карт по 3
    void Start()
    {
        bottoms = new List<string>[] {bottom0, bottom1, bottom2, bottom3, bottom4, bottom5, bottom6};
        PlayCards();
    }

    public void PlayCards()   //роздать карты
    {
        foreach (List<string> list in bottoms)
        {
            list.Clear();              // очистить все нижние позиции карт
        }
        deck = GenerateDeck();  // генерируем колоду карт
        Shuffle(deck);          // перетасовка карт
        SolitaireSort();        // заполнение масивов для нижних позиций карт
        StartCoroutine(SolitaireDeal());// создание объектов карт и назмещение их по нижних позициях
        SortDeckIntoTrips();  // добавление карт в колоду по три
    }
    public static List<string> GenerateDeck() // генератор карт
    {
        List<string> newDeck = new List<string>();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                newDeck.Add(s + v);
            }
        }
        return newDeck;
    }
    public void Shuffle<T>(List<T> list) // перетасовка карт
    {
        System.Random random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            T temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }
    IEnumerator SolitaireDeal()  // создание объектов карт и назмещение их по нижних позициях
    {
        for (int i = 0; i < 7; i++)
        {
            float yOffset = 0;
            float zOffset = 0.03f;
            foreach (string card in bottoms[i])
            {
                yield return new WaitForSeconds(0.01f);
                Vector3 tempPosition = new Vector3(bottomsPos[i].transform.position.x,
                    bottomsPos[i].transform.position.y - yOffset, bottomsPos[i].transform.position.z - zOffset);
                GameObject newCard = Instantiate(cardPref, tempPosition, Quaternion.identity, bottomsPos[i].transform);
                newCard.name = card;
                newCard.GetComponent<Selectable>().row = i;
                if (card == bottoms[i][bottoms[i].Count - 1])
                {
                    newCard.GetComponent<Selectable>().faceUp = true;
                }
                yOffset += 0.3f;
                zOffset += 0.03f;
                discardPile.Add(card); 
            }
        }
        foreach (string card in discardPile) // удаление дубликатов карт из оставшейся колоды 
        {
            if (deck.Contains(card))
            {
                deck.Remove(card);
            }
        }
        discardPile.Clear();
    }
    void SolitaireSort()    //добавление карт в нихние позиции
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = i; j < 7; j++)
            {
                bottoms[j].Add(deck.Last<string>());
                deck.RemoveAt(deck.Count-1);
            }
        }
    }
    public void SortDeckIntoTrips() // добавление карт в колоду по три
    {
        trips = deck.Count / 3;
        tripsRemainder = deck.Count % 3;
        deckTrips.Clear();
        int modifier = 0;
        for (int i = 0; i < trips; i++)
        {
            List<string> myTrips = new List<string>();
            for (int j = 0; j < 3; j++)
            {
                myTrips.Add(deck[j + modifier]);
            }
            deckTrips.Add(myTrips);
            modifier += 3;
        }
        if (tripsRemainder !=0)
        {
            List<string> myRemainders = new List<string>();
            modifier = 0;
            for (int k = 0; k < tripsRemainder; k++)
            {
                myRemainders.Add(deck[deck.Count - tripsRemainder + modifier]);
                modifier++;
            }
            deckTrips.Add(myRemainders);
            trips++;
        }
        deckLocation = 0;
    }
    public void DealFromDeck() // обработка роздачи карт по три
    {
        foreach (Transform child in deckButton.transform)
        {
            if (child.CompareTag("Card"))
            {
                deck.Remove(child.name);
                discardPile.Add(child.name);
                Destroy(child.gameObject);
            }
        }
        if (deckLocation < trips)
        {
            tripsOnDisplay.Clear();
            float xOffset = 2.5f;
            float zOffset = -0.2f;
            foreach (string card in deckTrips[deckLocation])
            {
                Vector3 pos = new Vector3(deckButton.transform.position.x + xOffset,
                    deckButton.transform.position.y, deckButton.transform.position.z+zOffset);
                GameObject newToCard = Instantiate(cardPref, pos, Quaternion.identity, deckButton.transform);
                xOffset += 0.5f;
                zOffset -= 0.2f;
                newToCard.name = card;
                tripsOnDisplay.Add(card);
                newToCard.GetComponent<Selectable>().faceUp = true;
                newToCard.GetComponent<Selectable>().inDeckPule = true;
            }
            deckLocation++;
        }
        else
        {
            ReskackTopDeck();
        }
    }
    void ReskackTopDeck() // перемещение карт по три назад в колоду
    {
        
        deck.Clear();
        foreach (string card in discardPile)
        {
            deck.Add(card);

        }
        discardPile.Clear();
        SortDeckIntoTrips();
    }
}

