using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This will call Init on all the children of this script having a specific interface. Allow to initialize reference to
/// Some UI part when they are disable (useful to be able to disable thing in editor when designing them so they don't
/// get in the way and don't have to remember re-enabling everything before building/pressing play)
/// </summary>
public class UIRoot : MonoBehaviour
{
    public static UIRoot Instance { get; protected set; }
    
    public CanvasScaler Scaler;

    [Header("Game UI Reference")] 
    public GameObject OptionSidebar;
    public GameObject InventorySidebar;

    public GameObject EndScreenUI;

    public interface IUIInitHandler
    {
        void Init();
    }

    private void Awake()
    {
        Instance = this;
        
        EnableGameUI(false);

        InitSystem.RegisterOnInitEvent(() =>
        {
            VisualManager.InitUIData();
            
            var handlers = GetComponentsInChildren<IUIInitHandler>(true);

            foreach (var handler in handlers)
            {
                handler.Init();
            }
        });
    }

    /// <summary>
    /// Called to hide/display the two sidebars used for gameplay. They are disabled by default and will be enabled only
    /// when going into game.
    /// </summary>
    /// <param name="on"></param>
    public void EnableGameUI(bool on)
    {
        OptionSidebar.SetActive(on);
        InventorySidebar.SetActive(on);
    }
}
