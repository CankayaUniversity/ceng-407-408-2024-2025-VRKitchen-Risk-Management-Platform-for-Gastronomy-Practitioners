using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScannerUseHandler : MonoBehaviour
{
    public GrabbableItem SecretMemo;
    public GrabbableItem CommsPad;

    public void Scanned()
    {
        GameStateManager.Instance.SetValue("Lib_Scanned", true);
        
        if(!GameStateManager.Instance.Contains(SecretMemo.FlagName))
            SecretMemo.gameObject.SetActive(true);
        
        if(!GameStateManager.Instance.Contains(CommsPad.FlagName))
            CommsPad.gameObject.SetActive(true);
        
        UIItemUsePopup.DisplayMessage("The machine makes a ping sound! Its display indicates energy disturbances on the bookshelves and on one of the tables.");
    }
    
}
