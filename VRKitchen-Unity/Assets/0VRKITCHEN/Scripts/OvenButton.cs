using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenButton : MonoBehaviour
{
    public OvenController ovenController; // Assign the OvenController script

    private XRSimpleInteractable interactable;

   private void Start()
{
    interactable = GetComponent<XRSimpleInteractable>();
    if (interactable != null)
    {
        interactable.hoverEntered.AddListener(OnButtonTouched);
        Debug.Log("Listener added to button.");
    }
    else
    {
        Debug.LogError("XR Simple Interactable component not found on button!");
    }
}
   

    private void OnButtonTouched(HoverEnterEventArgs args)
    {
        Debug.Log("aaaaaa");
        if (ovenController != null)
        {
            ovenController.ToggleOven();
        }
    }
    
}