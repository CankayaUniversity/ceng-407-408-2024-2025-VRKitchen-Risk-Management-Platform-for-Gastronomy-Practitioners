using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenButton : MonoBehaviour
{
    public OvenController ovenController; // Assign the OvenController script

    private XRSimpleInteractable interactable;

   private void Start()
{
    interactable = GetComponent<XRSimpleInteractable>();
    
        interactable.hoverEntered.AddListener(OnButtonTouched);
       
    
}
   

    private void OnButtonTouched(HoverEnterEventArgs args)
    {
        
        if (ovenController != null)
        {
            ovenController.ToggleOven();
        }
    }
    
}