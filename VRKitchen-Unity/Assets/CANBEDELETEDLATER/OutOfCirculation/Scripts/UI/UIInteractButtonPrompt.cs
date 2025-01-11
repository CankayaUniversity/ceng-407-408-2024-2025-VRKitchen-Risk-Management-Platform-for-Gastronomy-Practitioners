using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIInteractButtonPrompt : MonoBehaviour, UIRoot.IUIInitHandler
{
    public static UIInteractButtonPrompt Instance { get; protected set; }
    
    public TextMeshProUGUI InteractiveName;
    public TextMeshProUGUI ButtonInputName;
    public Image ButtonInputIcon;


    public void Init()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void Hide()
    {
        if(gameObject == null)
            return;

        gameObject.SetActive(false);
    }

    public void Show(Transform root, string interactiveName)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        
        Bounds totalBound = new Bounds();
        foreach (var r in renderers)
        {
            if (totalBound.size.sqrMagnitude < 0.001f)
                totalBound = r.bounds;
            else
                totalBound.Encapsulate(r.bounds);
        }

        Vector3 pos;
        if (totalBound.size.sqrMagnitude < 0.001f)
        {
            pos = root.position + Vector3.up * 0.5f;
        }
        else
        {
            pos = totalBound.center;
            pos.y = Mathf.Max(totalBound.max.y + 0.4f, 1.5f);
        }
        
        gameObject.SetActive(true);

        transform.position = Camera.main.WorldToScreenPoint(pos);
        InteractiveName.text = interactiveName;

        //TODO : store that uniquely somewhere to avoid the query?
        var interactAction = ControlManager.CurrentInput.FindAction("Gameplay/Interact");
        int index = interactAction.GetBindingIndex(ControlManager.CurrentControlScheme.bindingGroup);

        if (index == -1)
        {//no bidning for the curent control scheme for Button Interact, so just display the name hide all button prompt
            ButtonInputName.transform.parent.gameObject.SetActive(false);
            ButtonInputIcon.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            var displayName = interactAction.GetBindingDisplayString(index, out string deviceLayoutName, out string controlPath);
            var mapping = ControlManager.ControlIconSelector.GetMappingForSetAndPath(SaveSystem.CurrentSettings.IconeSet,
                controlPath);
            
            if (mapping != null && mapping.Icone != null)
            {
                ButtonInputName.transform.parent.gameObject.SetActive(false);

                ButtonInputIcon.transform.parent.gameObject.SetActive(true);
                ButtonInputIcon.sprite = mapping.Icone;
                ButtonInputIcon.color = mapping.Color;
            }
            else
            {
                ButtonInputIcon.transform.parent.gameObject.SetActive(false);

                ButtonInputName.transform.parent.gameObject.SetActive(true);
                ButtonInputName.text = displayName;
            }
        }
    }
}
