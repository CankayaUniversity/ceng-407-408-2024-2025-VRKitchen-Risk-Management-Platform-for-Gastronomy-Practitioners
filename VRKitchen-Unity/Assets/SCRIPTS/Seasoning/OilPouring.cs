using System.Collections;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
    [SerializeField] private GameObject oilPrefab; // Prefab containing the particle system
    private ParticleSystem oilParticleSystem;
    [SerializeField] private Transform pourPoint; // The point from which the oil is poured
    [SerializeField] private LayerMask dishLayer; // Layer of dishes where oil should be detected
    [SerializeField] private float oilRate = 0.5f; // Time interval between oil additions

    private bool isPouring = false;
    private GameObject oilInstance;

    private void Start()
    {
        // Instantiate the oil prefab
        if (oilPrefab != null)
        {
            oilInstance = Instantiate(oilPrefab, pourPoint.position, Quaternion.identity);
            oilParticleSystem = oilInstance.GetComponentInChildren<ParticleSystem>();

            if (oilParticleSystem == null)
            {
                Debug.LogError("No Particle System found in the Oil prefab!");
            }
            else
            {
                oilParticleSystem.Stop(); // Ensure it's stopped at the start
            }


            oilInstance.transform.SetParent(pourPoint);
            oilInstance.SetActive(false);
        }
        else
        {
            Debug.LogError("Oil prefab is not assigned!");
        }
    }

    private void Update()
    {
        if (transform.up.y < -0.7f)
        {
            if (!isPouring)
            {
                StartPouring();
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

    private void StartPouring()
    {
        isPouring = true;
        if (oilInstance != null)
        {
            oilInstance.SetActive(true); // Make oil visible
        }

        if (oilParticleSystem != null && !oilParticleSystem.isPlaying)
        {
            oilParticleSystem.Play();
        }

        StartCoroutine(PourOil());
    }

    private void StopPouring()
    {
        isPouring = false;
        if (oilParticleSystem != null && oilParticleSystem.isPlaying)
        {
            oilParticleSystem.Stop();
        }

        StartCoroutine(DisableOilAfterParticles());
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

    private IEnumerator DisableOilAfterParticles()
    {
        yield return new WaitForSeconds(1f); // Small delay to allow particles to fade
        if (!isPouring)
        {
            oilInstance.SetActive(false); // Hide oil completely
        }
    }
}