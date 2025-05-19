using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class OvenButton : MonoBehaviour
{
    public HeatZone targetHeatZone;

    private XRBaseInteractable interactable;

    private void Start()
    {
        interactable = GetComponent<XRBaseInteractable>();

        // Artık hover yerine select (trigger basılması) kullanılıyor
        interactable.selectEntered.AddListener(OnButtonPressed);
    }

    private void OnButtonPressed(SelectEnterEventArgs args)
    {
        if (targetHeatZone != null)
        {
            targetHeatZone.ToggleZone(); // HeatZone'u aç/kapat
        }
    }
}