using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenButton : MonoBehaviour
{
    public HeatZone targetHeatZone; // Baðlý HeatZone

    private XRSimpleInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<XRSimpleInteractable>();
        interactable.hoverEntered.AddListener(OnButtonTouched);
    }

    private void OnButtonTouched(HoverEnterEventArgs args)
    {
        if (targetHeatZone != null)
        {
            targetHeatZone.ToggleZone(); // HeatZone'u aç/kapat
        }
    }
}
