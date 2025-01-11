using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


//Add this to an Image that act as a catch all for pointer click and can act on that click (e.g. used by the item popup
//to get closed on clicking outside of the item window.
public class UICloseBlocker : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent OnClickedEvent;


    public void OnPointerClick(PointerEventData eventData)
    {
        OnClickedEvent?.Invoke();
    }
}
