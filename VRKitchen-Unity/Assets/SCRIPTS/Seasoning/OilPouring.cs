using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilPouring : MonoBehaviour
{
   [SerializeField] private GameObject oilPrefab; 
    private ParticleSystem oilParticleSystem; 
    [SerializeField] private Transform pourPoint; 
    [SerializeField] private LayerMask dishLayer; 
    [SerializeField] private float oilRate = 0.5f; 

    private bool isPouring = false;

    private void Start()
    {
        oilParticleSystem = oilPrefab.GetComponentInChildren<ParticleSystem>();

        if (oilParticleSystem == null)
        {
            Debug.LogError("No Particle System found in the Oil prefab!");
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
        if (oilParticleSystem != null)
        {
            oilParticleSystem.Play();
        }
        StartCoroutine(PourOil());
    }

    private void StopPouring()
    {
        isPouring = false;
        if (oilParticleSystem != null)
        {
            oilParticleSystem.Stop();
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
