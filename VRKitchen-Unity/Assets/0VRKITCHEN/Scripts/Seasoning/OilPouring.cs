using System.Collections;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject oilStream;           // Yellow curved oil mesh
    [SerializeField] private Transform pourPoint;            // Mesh origin (bottle mouth)
    [SerializeField] private Transform bottleTransform;      // Bottle reference
    [SerializeField] private LayerMask dishLayer;            // Raycast target layer

    [Header("Pouring Settings")]
    [SerializeField] private float oilRate = 0.5f;
    [SerializeField] private float minTiltAngle = 60f;

    private bool isPouring = false;
    private Coroutine pourCoroutine;

    private void Start()
    {
        if (oilStream != null)
        {
            oilStream.SetActive(false);
        }
    }

    private void Update()
    {
        Vector3 pourDirection = -bottleTransform.up;
        
        float angleZ = Vector3.SignedAngle(Vector3.forward, pourDirection, Vector3.forward);
        
        pourPoint.localRotation = Quaternion.AngleAxis(angleZ, Vector3.forward);
        
        float tiltAngle = Vector3.Angle(bottleTransform.up, Vector3.up);
        if (tiltAngle > minTiltAngle)
        {
            if (!isPouring)
            {
                StartPouring(pourDirection);
            }
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }

    
    private void StartPouring(Vector3 direction)
    {
        isPouring = true;
        if (oilStream != null) oilStream.SetActive(true);
        pourCoroutine = StartCoroutine(PourOil(direction));
    }

    private void StopPouring()
    {
        isPouring = false;
        if (oilStream != null) oilStream.SetActive(false);
        if (pourCoroutine != null) StopCoroutine(pourCoroutine);
    }

    private IEnumerator PourOil(Vector3 pourDirection)
    {
        while (isPouring)
        {
            RaycastHit hit;
            if (Physics.Raycast(pourPoint.position, pourDirection, out hit, 2f, dishLayer))
            {
                Dish dish = hit.collider.GetComponent<Dish>();
                if (dish != null)
                {
                    dish.AddSeasoning("Oil", 1);
                }
            }

            yield return new WaitForSeconds(oilRate);
        }
    }
}
