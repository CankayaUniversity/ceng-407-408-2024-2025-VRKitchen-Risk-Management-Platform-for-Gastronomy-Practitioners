using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInteractiveObject : MonoBehaviour
{
    private static BaseInteractiveObject s_CurrentlyHighlighted;

    public Transform interactionLocation;

    public Vector3 FlatPosition => new Vector3(transform.position.x, 0f, transform.position.z);
    public Collider Col => m_Collider;
    
    public AudioClip UseClip;

    protected Collider m_Collider;

    protected virtual void Awake()
    {
        m_Collider = GetComponentInChildren<Collider>();
        SetLayers(false);
    }

    public virtual void Interact(NavMeshAgentController controller)
    {
        if(UseClip != null)
            AudioManager.Instance.PlayPointSFX(UseClip, transform.position, false);
    }

    public virtual bool IsBlockingNavigationInput()
    {
        return false;
    }

    public virtual void SetLayers(bool highlighted)
    {
        RecursiveLayerSet(transform, highlighted? 30 : 29);
    }

    private void OnDestroy()
    {
        if (s_CurrentlyHighlighted == this)
        {
            s_CurrentlyHighlighted.Highlight(false);
        }
    }

    // Highlighting is made through custom render feature on the render pipeline object.
    // You can find two renderer : UniversalRenderPipelineAsset_Renderer and ObjectHighlightRenderer in the Settings folder.
    // The first one is used to only highlight on hover, the second when always highlighted.
    // They work by rendering object in specific layers (InteractiveOutlined only for the 1st, and Interactive also for the 2nd)
    // with some replaced shader : first render them normally, but writing in the stencil buffer a value of 10, then render
    // them with the Highlight material, that scale them up along their normal and in a solid color. But only pixel for
    // which the stencil is not 10 will be rendered, rendering only things that are "outside" the object silhouette.
    //
    // So that function only toggle the layer of our interactive object, placing them in the InteractiveOutline layer
    // when outlined. That way they are picked up by the rendering feature and the outline is rendered.
    public void Highlight(bool on)
    {
        SetLayers(on);

        //hide the prompt when we're in a dialog
        if (on && !UIDialogueWindow.Instance.gameObject.activeSelf)
        {

            //TODO : only queried at runtime, make that dynamic?
            UIInteractButtonPrompt.Instance?.Show(transform, GetName());
            s_CurrentlyHighlighted = this;
        }
        else
        {
            UIInteractButtonPrompt.Instance?.Hide();
        }
    }
    
    public virtual string GetName()
    {
        return gameObject.name;
    }

    static public void RecursiveLayerSet(Transform t, int layer)
    {
        t.gameObject.layer = layer;

        foreach (Transform child in t)
        {
            RecursiveLayerSet(child, layer);
        }
    }
}