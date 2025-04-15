using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [SerializeField] private GameObject oilStream; // The oil object inside the bottle
    [SerializeField] private Transform pourPoint;  // The tip of the bottle
    [SerializeField] private LayerMask dishLayer;  // Layer of dishes
    [SerializeField] private float oilRate = 0.5f; // Time between oil applications
    [SerializeField] private float pourTreshold = 0.7f; // angle of bottle
    
    private bool isPouring = false;
    private Coroutine pourCoroutine;

    private void Start()
    {
        if (oilStream != null)
        {
            oilStream.SetActive(false);
        }
        else
        {
            Debug.LogError("Oil stream object not assigned!");
        }
    }

    private void Update()
    {
        Vector3 pourDirection = -transform.up;
        
        float tiltAmount = Vector3.Dot(Vector3.down, pourDirection);
        
        if (tiltAmount > pourTreshold)
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

        if (oilStream != null)
        {
            oilStream.SetActive(true); // Show oil stream mesh or object
        }

        pourCoroutine = StartCoroutine(PourOil(direction));
    }

    private void StopPouring()
    {
        isPouring = false;

        if (oilStream != null)
        {
            oilStream.SetActive(false); // Hide oil stream
        }

        if (pourCoroutine != null)
        {
            StopCoroutine(pourCoroutine);
        }
    }

    private IEnumerator PourOil(Vector3 pourDirection)
    {
        while (isPouring)
        {
            RaycastHit hit;

            // Şişenin ucundan eğildiği yöne doğru bir ray at
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
