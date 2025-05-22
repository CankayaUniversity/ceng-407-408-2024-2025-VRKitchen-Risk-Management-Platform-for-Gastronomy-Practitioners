using UnityEngine;

public class HeatZone : MonoBehaviour
{
    public OvenController ovenController;
    private GameObject currentPan;
    
    public GameObject burnerFireParticles;
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

    private void Update()
    {
        if (isZoneOn)
        {
            if (burnerFireParticles != null)
                burnerFireParticles.SetActive(true);

            HeatObject(currentPan);
        }
        else
        {
            if (burnerFireParticles != null)
                burnerFireParticles.SetActive(false);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pan"))
        {
            currentPan = collision.gameObject;
            Debug.Log("AAAAAAAAAAAAA");
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
        if (obj == null) return;

        Pan pan = obj.GetComponent<Pan>();
        if (pan != null)
        {
            pan.Heat(10f * Time.deltaTime); 
        }
    }

    public bool IsZoneOn()
    {
        return isZoneOn;
    }
}