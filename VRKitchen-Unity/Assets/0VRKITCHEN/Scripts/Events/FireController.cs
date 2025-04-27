using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : SingletonBehaviour<FireController>
{
    [System.Serializable]
    public class FireSource
    {
        public Transform spawnPoint;          // Where fire should appear
        public GameObject firePrefab;         // Fire effect prefab
        public OvenController ovenController; // Reference to oven
        public HeatZone heatZone;             // Reference to heating zone
        public float heatingTime = 0f;        // Time heating zone is active
        public float maxHeatingTime = 600f;   // 10 minutes before fire starts
        [HideInInspector] public GameObject activeFire; // Stores fire object
        [HideInInspector] public AudioSource fireAudioSource; // Track the fire looping sound
    }

    public UnityToAPI toAPI;                            // 🔗 API reference
    public VisualFeedbackController fireAlertFeedback;  // 🔊👀 Visual alert controller
    public Transform fireAlertMarkerTransform;          // 📍 Where to spawn the alert icon
    public List<FireSource> fireSources = new List<FireSource>(); // List of heating zones
    public float checkInterval = 20f;                   // Interval to check heat zones

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
                bool isZoneOn = source.heatZone.IsZoneOn();

                if (isZoneOn)
                {
                    source.heatingTime += checkInterval;

                    if (source.heatingTime >= source.maxHeatingTime && source.activeFire == null)
                    {
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

            FireInstance fireInstance = source.activeFire.GetComponent<FireInstance>();
            if (fireInstance == null)
                fireInstance = source.activeFire.AddComponent<FireInstance>();

            fireInstance.source = source;
            source.fireAudioSource = AudioController.Instance.PlayLoopingSound(AudioController.Instance.GetFireClip(), source.spawnPoint.position);


            // 🧠 Submit RAG fire query
            if (toAPI != null)
            {
                Debug.Log("🔥 Fire triggered - sending query to RAG");
                toAPI.queryText = "A general fire has started in the game. What are the steps to handle this situation? Just give me the steps.";
                toAPI.SubmitQuery();
            }

            // 🔔 Show visual alert
            if (fireAlertFeedback != null && fireAlertMarkerTransform != null)
            {
                fireAlertFeedback.ShowExclamation(fireAlertMarkerTransform.position);
                Debug.Log("🚨 Fire alert icon displayed.");
            }
        }
    }

    public void ExtinguishFire(FireSource source)
    {
        if (source.activeFire != null)
        {
            Destroy(source.activeFire);
            source.activeFire = null;
            source.heatingTime = 0f;

            if (source.fireAudioSource != null)
            {
                AudioController.Instance.StopLoopingSound(source.fireAudioSource);
                source.fireAudioSource = null;
            }

            Debug.Log("✅ Fire extinguished.");

            fireAlertFeedback?.HideExclamation(); // 🔕 Hide the fire alert

        }
    }
}
