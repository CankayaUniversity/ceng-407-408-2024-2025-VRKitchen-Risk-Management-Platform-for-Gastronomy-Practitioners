using UnityEngine;

public class HeatZone : MonoBehaviour
{
    public OvenController ovenController;
    private FireController.FireSource myFireSource; //  FireSource referans�
    private GameObject currentPan;
    [SerializeField] private bool isZoneOn = false;

    public void ToggleZone()
    {
        isZoneOn = !isZoneOn;

        Debug.Log($"{gameObject.name} HeatZone is {(isZoneOn ? "ON" : "OFF")}");

        if (ovenController != null)
        {
            ovenController.RegisterZoneStateChange(isZoneOn);
        }
    }

    private void Start()
    {
        //  Bu HeatZone�a ba�l� olan FireSource�u bir kere bul ve sakla
        foreach (var source in FireController.Instance.fireSources)
        {
            if (source.heatZone == this)
            {
                myFireSource = source;
                break;
            }
        }
    }

    private void Update()
    {
        bool fireIsActive = myFireSource != null && myFireSource.activeFire != null;

        if ((isZoneOn || fireIsActive) && currentPan != null)
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
