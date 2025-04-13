using System.Collections;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [SerializeField] private GameObject oilStream; // The oil object inside the bottle
    [SerializeField] private Transform pourPoint;  // The tip of the bottle
    [SerializeField] private LayerMask dishLayer;  // Layer of dishes
    [SerializeField] private float oilRate = 0.5f; // Time between oil applications

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
        // Check if bottle is tilted upside down (or however you want to check pouring state)
        if (transform.up.y < -0.7f && !isPouring)
        {
            StartPouring();
        }
        else if (transform.up.y >= -0.7f && isPouring)
        {
            StopPouring();
        }
    }

    private void StartPouring()
    {
        isPouring = true;

        if (oilStream != null)
        {
            oilStream.SetActive(true); // Show oil stream mesh or object
        }

        pourCoroutine = StartCoroutine(PourOil());
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

    private IEnumerator PourOil()
    {
        while (isPouring)
        {
            RaycastHit hit;
            if (Physics.Raycast(pourPoint.position, Vector3.down, out hit, 2f, dishLayer))
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
