using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PadUseHandler : MonoBehaviour
{
    public void Use()
    {
        UIRoot.Instance.EndScreenUI.gameObject.SetActive(true);
    }
}
