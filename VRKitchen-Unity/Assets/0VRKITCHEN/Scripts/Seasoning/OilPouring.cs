using System.Collections;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject oilStream;
    [SerializeField] private Transform pourPoint;
    [SerializeField] private Transform bottleTransform;
    [SerializeField] private LayerMask dishLayer;

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
        float tiltAngle = Vector3.Angle(bottleTransform.up, Vector3.up);

        if (tiltAngle > minTiltAngle)
        {
            if (!isPouring)
            {
                StartPouring();
            }

            UpdateOilDirection();
        }
        else
        {
            if (isPouring)
            {
                StopPouring();
            }
        }
    }

    private void StartPouring()
    {
        isPouring = true;
        if (oilStream != null) oilStream.SetActive(true);
        pourCoroutine = StartCoroutine(PourOil());
    }

    private void StopPouring()
    {
        isPouring = false;
        if (oilStream != null) oilStream.SetActive(false);
        if (pourCoroutine != null) StopCoroutine(pourCoroutine);
    }

    private void UpdateOilDirection()
    {
        if (pourPoint == null || bottleTransform == null)
            return;

        Vector3 sideVector = bottleTransform.right; // Sideways in world
        float dot = Vector3.Dot(sideVector.normalized, Vector3.down);

        float zRotation;

        if (dot > 0.5f)
        {
            zRotation = 107.9f;  // Tilted left (pouring left)
        }
        else if (dot < -0.5f)
        {
            zRotation = -17.2f; // Tilted right (pouring right)
        }
        else
        {
            zRotation = 17.2f; // Default forward pour or upright
        }

        pourPoint.localRotation = Quaternion.Euler(0f, 0f, zRotation);
    }
    private IEnumerator PourOil()
    {
        while (isPouring)
        {
            Vector3 pourDirection = -bottleTransform.up;

            if (Physics.Raycast(pourPoint.position, pourDirection, out RaycastHit hit, 2f, dishLayer))
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
