using System.Collections;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject oilStream;           // Yellow curved oil mesh
    [SerializeField] private Transform pourPoint;            // Spout or intersection point
    [SerializeField] private Transform bottleTransform;      // Bottle reference
    [SerializeField] private LayerMask dishLayer;            // Raycast target layer

    [Header("Pouring Settings")]
    [SerializeField] private float oilRate = 0.5f;
    [SerializeField] private float minTiltAngle = 60f;
    [SerializeField] private float curveIntensity = 0.5f;    // How much the oil curves when tilted

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

            UpdateOilDirection(tiltAngle);
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

    private void UpdateOilDirection(float tiltAngle)
    {
        if (pourPoint == null || bottleTransform == null)
            return;

        // Value from -1 (left tilt) to +1 (right tilt)
        float sideTilt = Vector3.Dot(bottleTransform.right, Vector3.down);

        // Clamp rotation angle
        float zRotation = -sideTilt * minTiltAngle;

        // Apply to sprinkle point Z-axis
        Vector3 currentEuler = pourPoint.localEulerAngles;
        pourPoint.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, zRotation);
    }


    private IEnumerator PourOil()
    {
        while (isPouring)
        {
            // Base direction
            Vector3 pourDirection = -bottleTransform.up;

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
