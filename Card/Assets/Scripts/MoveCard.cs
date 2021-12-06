using UnityEngine;

public class MoveCard : MonoBehaviour
{
    private bool isDragging;
    private Vector3 oldTransform;
    private GameObject downCard;
    private void OnMouseDown()
    {
        if (UserInput.Instance.Blocked(gameObject)==false)
        {
            downCard = GetDownCard();
            oldTransform = gameObject.transform.position;
            isDragging = true;
        }
        else
        {
            isDragging = false;
        }
    }
    private void OnMouseUp()
    {
        if (isDragging)
        {
            gameObject.SetActive(false);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
            gameObject.SetActive(true);
            if (hit.collider != null)
            {
                GameObject obj = hit.collider.transform.gameObject;
                if (UserInput.Instance.CardMove(gameObject, obj) == false)
                {
                    transform.position = oldTransform;
                }          
            }
            else
            {
                transform.position = oldTransform;
            }
        }
        isDragging = false;
    }
    void Update()
    {
        if (isDragging)
        {
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
           Input.mousePosition.y, 1));
        }
    }
    GameObject GetDownCard()
    {
        gameObject.SetActive(false);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up);
        gameObject.SetActive(true);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Card"))
            {
                GameObject obj = hit.collider.transform.gameObject;
                return obj;
            }
        }
        return null;
    }
    public void ReturnCard()
    {
        if (downCard!=null)
        {
            downCard.GetComponent<Selectable>().faceUp = true;
        } 
    }
}
