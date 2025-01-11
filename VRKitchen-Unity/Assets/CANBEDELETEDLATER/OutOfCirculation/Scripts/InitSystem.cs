using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitSystem
{
    private static System.Action m_OnInit = null;
    private static bool m_WasInit = false;
    
    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        ControlManager.Init();
        SaveSystem.Init();
        VisualManager.Init();
        
        m_WasInit = true;

        m_OnInit?.Invoke();
    }
    
    public static void RegisterOnInitEvent(System.Action action)
    {
        if (m_WasInit)
        {
            action();
        }
        else
        {
            m_OnInit += action;
        }
    }

    //Either called by the start screen when creating a new game or by the SpawnPoint when entering a scene and no player
    //exist in the editor (to allow testing any scene at any time without going through the startup process)
    public static void SpawnGameplayData()
    {
        DataReference.Instance.SpawnGameData();
    }
    
}
