using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject oilStream;     // The oil visual effect or stream object
    [SerializeField] private Transform pourPoint;      // Tip of the bottle where oil comes out
    [SerializeField] private LayerMask dishLayer;      // Layer to detect dishes

    [Header("Pouring Settings")]
    [SerializeField] private float oilRate = 0.5f;      // Time between oil applications
    [SerializeField] private float minTiltAngle = 60f;  // Minimum tilt angle to start pouring

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
        float tiltAngle = Vector3.Angle(transform.up, Vector3.up);

        if (tiltAngle > minTiltAngle)
        {
            if (!isPouring)
            {
                StartPouring(-transform.up);
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
