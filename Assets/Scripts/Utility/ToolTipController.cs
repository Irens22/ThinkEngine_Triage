using UnityEngine;
using UnityEngine.EventSystems;

public class ToolTipController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipObject;

    void Start()
    {
        if(tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(tooltipObject != null)
        {
            tooltipObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }
}