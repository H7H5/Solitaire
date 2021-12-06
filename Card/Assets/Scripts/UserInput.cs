using UnityEngine;
using System.Linq;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance;
    public GameObject slot1;  // выбраная карта
    private Solitaire solitaire;
    private float timer;
    private float doubleClickTime = 0.3f;
    private int clickCount = 0;
    private bool bottomFlag = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        solitaire = FindObjectOfType<Solitaire>();
        slot1 = this.gameObject;
    }

    void Update()
    {
        GetMouseClick(); //обрабатываем нажатие мышки и выделяем выбраную карту
    }

    void GetMouseClick() //обрабатываем нажатие мышки и выделяем выбраную карту
    {
        if (Input.GetMouseButtonDown(0))
        {
            clickCount++;

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -10));
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit)
            {
                // what has been hit? Deck/Card/EmptySlot...
                if (hit.collider.CompareTag("Deck"))
                {
                    //clicked deck
                    Deck();
                }
                /*
                else if (hit.collider.CompareTag("Card"))
                {
                    // clicked card
                    Card(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Top"))
                {
                    // clicked top
                    Top(hit.collider.gameObject);
                }
                else if (hit.collider.CompareTag("Bottom"))
                {
                    // clicked bottom
                    Bottom(hit.collider.gameObject);
                }
                */
            }
        }
    }

    void Deck() // обработка нажатия на колоду
    {
        solitaire.DealFromDeck(); // обработка роздачи карт по три
        slot1 = this.gameObject; //сброс выделеной карты

    }

    public bool CardMove(GameObject currentCard, GameObject downObj)
    {
        if (downObj.CompareTag("Card"))
        {
            slot1 = currentCard;
            return Card(downObj);
        }
        else if (downObj.CompareTag("Bottom"))
        {
            bottomFlag = true;
            slot1 = currentCard;
            return Bottom(downObj);
        }
        else if (downObj.CompareTag("Top"))
        {
            bottomFlag = true;
            slot1 = currentCard;
            return Top(downObj);
        }
        return false;
    }

    bool Card(GameObject selected)
    {
        if (!selected.GetComponent<Selectable>().faceUp) // если карта, на которую нажали, лежит лицевой стороной вниз
        {
            if (!Blocked(selected)) // если нажатая карта не заблокирована
            {
                selected.GetComponent<Selectable>().faceUp = true; //переворот карты
                slot1 = this.gameObject;
            }
            return false;
        }
        else if (selected.GetComponent<Selectable>().inDeckPule) // если нажатая карта находится в колоде
        {
            if (!Blocked(selected))
            {
                if (slot1 == selected) // если на одну и ту же карту щелкнули дважды
                {
                    if (DoubleClick())
                    {
                        AutoStack(selected);
                    }
                }
                else
                {
                    slot1 = selected;
                }                
            }
            return false;
        }
        else
        {
            // если карта открыта
            // если в данный момент не выбрана ни одна карта
            // выбираем карту
            if (slot1 == this.gameObject) //не null, потому что мы передаем этот gameObject вместо
            {
                slot1 = selected;
                return false;
            }
            // если уже выбрана карта (и это не та же карта)
            else if (slot1 != selected)
            {
                // если новая карта может быть уложена на старую карту
                if (Stackable(selected))
                { 
                    slot1.GetComponent<MoveCard>().ReturnCard();
                    Stack(selected);
                    return true;
                }
                else
                {
                    slot1 = selected;
                }
                return false;
            }

            else if (slot1 == selected) 
            {
                if (DoubleClick())
                {
                    AutoStack(selected);
                }
                return false;
            }
        }
        return false;
    }

    bool Top(GameObject selected) // обработка нажатия на верхнюю позицию (без карт)
    {
        if (slot1.CompareTag("Card"))
        {
            if (slot1.GetComponent<Selectable>().value == 1)//если туз то можно положить на пустое место
            {
                slot1.GetComponent<MoveCard>().ReturnCard();
                Stack(selected);
                return true;
            } 
        }
        return false;
    }

    bool Bottom(GameObject selected)// обработка нажатия на нижнею позицию (без карт)
    {
        if (selected.transform.childCount > 0)
        {
            return false;
        }
        if (slot1.CompareTag("Card"))
        {
            //if (slot1.GetComponent<Selectable>().value == 13)//если король то можно положить на пустое место 
            //{
                slot1.GetComponent<MoveCard>().ReturnCard();
                Stack(selected);
                return true;
            //}
        }
        return false;
    }

    bool Stackable(GameObject selected) // проверка можно ли стекать карты
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        if (!s2.inDeckPule)
        {
            if (s2.top) 
            {
                if (s1.suit == s2.suit || (s1.value == 1 && s2.suit == null))
                {
                    if (s1.value == s2.value + 1)
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
            else  //  если в нижней стопке должны стоять чередующиеся цвета от короля до туза
            {
                if (s1.value == s2.value - 1)
                {
                    bool card1Red = true;
                    bool card2Red = true;
                    if (s1.suit == "C" || s1.suit == "S")
                    {
                        card1Red = false;
                    }
                    if (s2.suit == "C" || s2.suit == "S")
                    {
                        card2Red = false;
                    }
                    if (card1Red == card2Red)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    void Stack(GameObject selected) //стекает карты
    {
        Selectable s1 = slot1.GetComponent<Selectable>();
        Selectable s2 = selected.GetComponent<Selectable>();
        float yOffset = 0.3f;
        if (s2.top || (!s2.top && bottomFlag == true))
        {
            bottomFlag = false;
            yOffset = 0;
        }
        slot1.transform.position = new Vector3(selected.transform.position.x, selected.transform.position.y - yOffset, selected.transform.position.z - 0.01f);
        slot1.transform.parent = selected.transform; // это заставляет детей двигаться вместе с родителями

        if (s1.inDeckPule) // удаляет карты из верхней стопки, чтобы предотвратить дублирование карт
        {
            solitaire.tripsOnDisplay.Remove(slot1.name);
        }
        else if (s1.top && s2.top && s1.value == 1) // позволяет перемещать карты между верхними позициями
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = 0;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = null;
        }
        else if (s1.top) // отслеживает текущее значение верхних колод при удалении карты
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value - 1;
        }
        else // удаляет строку карты из соответствующего нижнего списка
        {
            solitaire.bottoms[s1.row].Remove(slot1.name);
        }
        s1.inDeckPule = false; 
        s1.row = s2.row;
        if (s2.top) // перемещает карту наверх и присваивает значение и масть вершине
        {
            solitaire.topPos[s1.row].GetComponent<Selectable>().value = s1.value;
            solitaire.topPos[s1.row].GetComponent<Selectable>().suit = s1.suit;
            s1.top = true;
        }
        else
        {
            s1.top = false;
        }
        // после завершения перемещения сбросить слот1, чтобы он был по существу нулевым, поскольку нулевое значение нарушит логику
        slot1 = this.gameObject;
    }
    public bool Blocked(GameObject selected)// проверка на блокировку другими картами
    {
        Selectable s2 = selected.GetComponent<Selectable>();
        if (s2.inDeckPule == true)
        {
            if (s2.name == solitaire.tripsOnDisplay.Last()) 
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            if (s2.faceUp == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    bool DoubleClick()//проверка на двойной клик
    {
        if (timer < doubleClickTime && clickCount == 2)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void AutoStack(GameObject selected)
    {
        for (int i = 0; i < solitaire.topPos.Length; i++)
        {
            Selectable stack = solitaire.topPos[i].GetComponent<Selectable>();
            if (selected.GetComponent<Selectable>().value == 1) 
            {
                if (solitaire.topPos[i].GetComponent<Selectable>().value == 0) 
                {
                    slot1 = selected;
                    Stack(stack.gameObject); 
                    break;                 
                }
            }
            else
            {
                if ((solitaire.topPos[i].GetComponent<Selectable>().suit == slot1.GetComponent<Selectable>().suit) && (solitaire.topPos[i].GetComponent<Selectable>().value == slot1.GetComponent<Selectable>().value - 1))
                {
                    if (HasNoChildren(slot1))
                    {
                        slot1 = selected;
                        string lastCardname = stack.suit + stack.value.ToString();
                        if (stack.value == 1)
                        {
                            lastCardname = stack.suit + "A";
                        }
                        if (stack.value == 11)
                        {
                            lastCardname = stack.suit + "J";
                        }
                        if (stack.value == 12)
                        {
                            lastCardname = stack.suit + "Q";
                        }
                        if (stack.value == 13)
                        {
                            lastCardname = stack.suit + "K";
                        }
                        GameObject lastCard = GameObject.Find(lastCardname);
                        Stack(lastCard);
                        break;
                    }
                }
            }
        }
    }

    bool HasNoChildren(GameObject card)  // определяем есть ли в объекта дочерние объекты
    {
        int i = 0;
        foreach (Transform child in card.transform)
        {
            i++;
        }
        if (i == 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public void Exit()
    {
        Application.Quit();
    }

}
