using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : SingletonBehaviour<FireController>
{
    [System.Serializable]
    public class FireSource
    {
        public Transform spawnPoint;  // Where fire should appear
        public GameObject firePrefab; // Fire effect prefab
        public OvenController ovenController; // Reference to oven
        public HeatZone heatZone; // Reference to heating zone
        public float heatingTime = 0f; // Time heating zone is active
        public float maxHeatingTime = 600f; // 10 minutes before fire starts
        [HideInInspector] public GameObject activeFire; // Stores fire object
    }

    public UnityToAPI toAPI; // 🔗 API reference for sending queries
    public List<FireSource> fireSources = new List<FireSource>(); // List of heating zones
    public float checkInterval = 5f; // Time interval for fire check

    public OvenController ovenController;
    private void Start()
    {
        StartCoroutine(CheckForFireRisk());
    }

    private IEnumerator CheckForFireRisk()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            MonitorHeatingZones();
        }
    }

    private void MonitorHeatingZones()
    {
        foreach (var source in fireSources)
        {
            // Check if oven is ON
            if (source.ovenController != null && source.heatZone != null)
            {
                if (source.ovenController.isOvenOn && source.heatZone.gameObject.activeSelf)
                {
                    source.heatingTime += checkInterval;

                    // Fire appears if heating zone is active too long
                    if (source.heatingTime >= source.maxHeatingTime && source.activeFire == null)
                    {
                        Debug.Log($"🔥 Fire started at {source.spawnPoint.name} (Heating zone on too long!)");
                        SpawnFire(source);
                    }
                }
                else
                {
                    source.heatingTime = 0f; // Reset timer if oven is off or no pan is present
                }
            }
        }
    }

    private void SpawnFire(FireSource source)
    {
        if (source.firePrefab != null && source.spawnPoint != null)
        {
            source.activeFire = Instantiate(source.firePrefab, source.spawnPoint.position, Quaternion.identity);
            Debug.Log($"🔥 Fire instantiated at {source.spawnPoint.name}");

            // 🔥 Send query to API
            if (toAPI != null)
            {
                toAPI.queryText = "A general fire has started in the game. What are the steps to handle this situation?";
                toAPI.SubmitQuery();
            }
            
        }
    }

    public void ExtinguishFire(FireSource source)
    {
        if (source.activeFire != null)
        {
            Destroy(source.activeFire);
            source.activeFire = null;
            Debug.Log("🚒 Fire extinguished!");
        }
    }
}
