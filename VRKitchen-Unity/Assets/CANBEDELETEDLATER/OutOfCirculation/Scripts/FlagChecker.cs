using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class will check for the existence of a given flag in the Game State Manager. If it is not present, it will
/// simply disable the object, otherwise it leave it untouched.
/// This behaviour can be inverted (only leave it untouched if flag is NOT set) through an option on the component
/// </summary>
public class FlagChecker : MonoBehaviour
{
    public bool CheckOnStart;
    public bool Invert;
    public string Key;

    // Start is called before the first frame update
    void Start()
    {
        if(CheckOnStart)
            PerformCheck();
    }

    public void PerformCheck()
    {
        if (GameStateManager.Instance.Contains(Key))
        {
            if(Invert) gameObject.SetActive(false);
        }
        else
        {
            if(!Invert) gameObject.SetActive(false);
        }
    }
}
