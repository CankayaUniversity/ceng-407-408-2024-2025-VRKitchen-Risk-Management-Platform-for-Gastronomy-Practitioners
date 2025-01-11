using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIBottomBar : MonoBehaviour
{
    public Button SaveButton;
    public Button UndoButton;
    public Button CloseButton;

    public TextMeshProUGUI SaveButtonText;
    public TextMeshProUGUI CancelButtonText;

    public void SetupBar(System.Action onSave, System.Action onUndo)
    {
        SaveButton.onClick.RemoveAllListeners();
        SaveButton.onClick.AddListener(() => { onSave(); });
        
        UndoButton.onClick.RemoveAllListeners();
        UndoButton.onClick.AddListener(() => { onUndo(); });

        NotifyInteractable(false);
    }

    public void NotifyInteractable(bool interactable)
    {
        //Navigation system doesn't skip non interactable element when in explicit mode. So we have to manually remove
        //the navigation link from close to Save when disabling the buttons. 
        if (interactable)
        {
            var nav = CloseButton.navigation;
            nav.selectOnRight = SaveButton;
            CloseButton.navigation = nav;
        }
        else
        {
            var nav = CloseButton.navigation;
            nav.selectOnRight = null;
            CloseButton.navigation = nav;

            if (EventSystem.current.currentSelectedGameObject == SaveButton.gameObject ||
                EventSystem.current.currentSelectedGameObject == UndoButton.gameObject)
            {
                //if the selected element was the save/close button we select the close button instead
                EventSystem.current.SetSelectedGameObject(CloseButton.gameObject);
            }
        }

        SaveButton.interactable = interactable;
        UndoButton.interactable = interactable;

        SaveButtonText.color = interactable ? Color.black : Color.grey;
        CancelButtonText.color = interactable ? Color.black : Color.grey;
    }
}
