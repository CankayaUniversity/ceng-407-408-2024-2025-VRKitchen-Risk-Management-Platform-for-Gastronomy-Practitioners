using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoUseHandler : MonoBehaviour
{
    public void Use()
    {
        UIItemUsePopup.DisplayMessage("A man in my position has to delegate to those he can really trust. I knew you wouldn’t risk betraying that trust.\n\nWell done. Let’s proceed to the next stage.");
    }
}
