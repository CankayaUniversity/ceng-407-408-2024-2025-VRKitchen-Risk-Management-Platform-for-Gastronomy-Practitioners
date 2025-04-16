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
    public float checkInterval = 20f; // Time interval for fire check

    //public OvenController ovenController;
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
            if (source.heatZone != null)
            {
                bool isZoneOn = source.heatZone.IsZoneOn(); // bu metodu HeatZone.cs içine ekleyeceğiz

                //Debug.Log($"Checking {source.heatZone.name} - IsOn: {isZoneOn}");

                if (isZoneOn)
                {
                    source.heatingTime += checkInterval;
                    Debug.Log($"Heating time increased: {source.heatZone.name} = {source.heatingTime}");

                    if (source.heatingTime >= source.maxHeatingTime && source.activeFire == null)
                    {
                        Debug.Log($"Fire started at {source.spawnPoint.name} (Heating zone on too long!)");
                        SpawnFire(source);
                    }
                }
                else
                {
                    if (source.heatingTime != 0)
                        Debug.Log($"Reset heating time for {source.heatZone.name}");

                    source.heatingTime = 0f;
                }
            }
        }
    }


    private void SpawnFire(FireSource source)
    {
        if (source.firePrefab != null && source.spawnPoint != null)
        {
            source.activeFire = Instantiate(source.firePrefab, source.spawnPoint.position, Quaternion.identity);
            
            FireInstance fireInstance = source.activeFire.AddComponent<FireInstance>();
            fireInstance.source = source;

            Debug.Log($" Fire instantiated at {source.spawnPoint.name}");

            if (toAPI != null)
            {
                Debug.Log($" Query submitted!");
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
            Debug.Log(" Fire extinguished!");
            source.heatingTime = 0f;
        }
    }
}
