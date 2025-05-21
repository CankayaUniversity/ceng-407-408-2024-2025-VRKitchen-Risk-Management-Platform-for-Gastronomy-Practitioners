using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OvenCheck : MonoBehaviour
{
    public bool canOpen;
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name=="XR Origin")
        {
            canOpen = true;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "XR Origin")
        {
            canOpen = true;
        }
    }
}
