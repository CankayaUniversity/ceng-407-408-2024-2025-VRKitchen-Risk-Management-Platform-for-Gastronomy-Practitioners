using UnityEngine;

public class HeatZone : MonoBehaviour
{
    public OvenController ovenController; 
    private GameObject currentPan;
    private bool flag;
    
    private void Update()
    {
        
        if (ovenController != null && ovenController.isOvenOn && currentPan != null)
        {
            HeatObject(currentPan); 
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pan")) 
        {
            
            currentPan = collision.gameObject;
            
            flag = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Pan")) 
        {
            Debug.Log("Pan removed from heat zone: " + collision.gameObject.name);
            
            currentPan = null;
        }
    }
    public void HeatObject(GameObject obj)
    {
        if (obj != null)
        {
            Debug.Log("Heating object: " + obj.name);
            

            Pan pan = obj.GetComponent<Pan>();
            if (pan != null)
            {
                pan.Heat(10);
            }
        }
    }

    public void StopHeatingObject(GameObject obj)
    {
        if (obj != null)
        {
            Debug.Log("Stopped heating object: " + obj.name);
            

            Pan pan = obj.GetComponent<Pan>();
            if (pan != null)
            {
                pan.Cool(10);
            }
        }
    }
}