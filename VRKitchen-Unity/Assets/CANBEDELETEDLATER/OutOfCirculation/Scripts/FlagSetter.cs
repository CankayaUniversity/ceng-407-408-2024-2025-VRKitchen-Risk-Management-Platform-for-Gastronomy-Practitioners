using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Simple helper that allow to set a flag in the GameStateManager. Useful to be called by an Event in the inspector
/// (e.g. SpawnPoint OnSpawned event)
/// </summary>
[DefaultExecutionOrder(100)] //allow to make sure it run after all other flag check
public class FlagSetter : MonoBehaviour
{
    public bool SetOnStart = false;
    public string Key;

    private void Start()
    {
        if(SetOnStart)
            SetFlag();
    }

    public void SetFlag()
    {
        GameStateManager.Instance.SetValue(Key, true);
    }
}
