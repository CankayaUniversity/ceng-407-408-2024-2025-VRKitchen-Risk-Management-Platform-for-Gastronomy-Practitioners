using UnityEngine;

public class HeatZone : MonoBehaviour
{
    public OvenController ovenController;
    private GameObject currentPan;
    private bool isZoneOn = false;
    public FireController.FireSource linkedFireSource;
    public void ToggleZone()
    {
        isZoneOn = !isZoneOn;

        Debug.Log($"{gameObject.name} HeatZone is {(isZoneOn ? "ON" : "OFF")}");

        if (ovenController != null)
        {
            ovenController.RegisterZoneStateChange(isZoneOn);
        }
    }

    private void Update()
    {
        if (isZoneOn || linkedFireSource.activeFire != null)
        {
            HeatObject(currentPan);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pan"))
        {
            currentPan = collision.gameObject;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pan"))
        {
            currentPan = null;
        }
    }

    public void HeatObject(GameObject obj)
    {
        Pan pan = obj.GetComponent<Pan>();
        if (pan != null)
        {
            pan.Heat(10);
        }
    }
}
