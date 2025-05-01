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
        public FirePool firePool; // Reference in inspector
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

    [Header("Fire Limits")]
    public int maxSimultaneousFires = 3;


    [Header("Random Fire Settings")]
    public bool enableRandomFires = true;
    public float randomFireInterval = 60f; // every 60 seconds try spawning a random fire
    [Range(0f, 1f)]
    public float randomFireChance = 0.1f; // 10% chance each interval

    [Header("Difficulty Scaling Settings")]
    public float chanceIncreaseRate = 0.01f;   // +1% every 30 seconds
    public float intervalDecreaseRate = 1f;    // -1 second every 30 seconds
    public float minimumFireInterval = 10f;    // minimum 10 seconds between random fires
    public float maxRandomFireChance = 0.5f;   // max 50% chance

    private void Start()
    {
        StartCoroutine(CheckForFireRisk());

        if (enableRandomFires)
        {
            StartCoroutine(RandomFireSpawner());
            StartCoroutine(IncreaseFireDifficultyOverTime());
        }
    }

    private int GetActiveFireCount()
    {
        int count = 0;
        foreach (var source in fireSources)
        {
            if (source.activeFire != null)
                count++;
        }
        return count;
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

    private IEnumerator RandomFireSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(randomFireInterval);

            if (Random.value <= randomFireChance)
            {
                FireSource randomSource = GetRandomAvailableSource();
                if (randomSource != null && randomSource.activeFire == null)
                {
                    Debug.Log("🔥 Random fire triggered!");
                    SpawnFire(randomSource);
                }
            }
        }
    }

    private IEnumerator IncreaseFireDifficultyOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(30f);

            // Increase random fire chance
            randomFireChance = Mathf.Min(maxRandomFireChance, randomFireChance + chanceIncreaseRate);

            // Decrease random fire interval
            randomFireInterval = Mathf.Max(minimumFireInterval, randomFireInterval - intervalDecreaseRate);

            Debug.Log($"🔥 Fire difficulty increased: Chance={randomFireChance * 100f}%, Interval={randomFireInterval} seconds");
        }
    }

    private FireSource GetRandomAvailableSource()
    {
        List<FireSource> availableSources = new List<FireSource>();

        foreach (var source in fireSources)
        {
            if (source.firePrefab != null && source.spawnPoint != null && source.activeFire == null)
            {
                availableSources.Add(source);
            }
        }

        if (availableSources.Count == 0)
            return null;

        return availableSources[Random.Range(0, availableSources.Count)];
    }

    private void SpawnFire(FireSource source)
    {

        if (GetActiveFireCount() >= maxSimultaneousFires)
        {
            Debug.Log("Fire limit reached — skipping spawn.");
            return;
        }
        
        if (source.firePool == null || source.spawnPoint == null) return;

        source.activeFire = source.firePool.GetFire(source.spawnPoint.position);

        FireInstance fireInstance = source.activeFire.GetComponent<FireInstance>();
        if (fireInstance == null)
            fireInstance = source.activeFire.AddComponent<FireInstance>();

        fireInstance.source = source;

        source.fireAudioSource = AudioController.Instance.PlayLoopingSound(
            AudioController.Instance.GetFireClip(), source.spawnPoint.position);

        if (toAPI != null)
        {
            Debug.Log("🔥 Fire triggered - sending query to RAG");
            toAPI.queryText = "A general fire has started in the game. What are the steps to handle this situation? Just give me the steps.";
            toAPI.SubmitQuery();
        }

        if (fireAlertFeedback != null && fireAlertMarkerTransform != null)
        {
            fireAlertFeedback.ShowExclamation(fireAlertMarkerTransform.position);
            Debug.Log("🚨 Fire alert icon displayed.");
        }
    }


    public void ExtinguishFire(FireSource source)
    {
        if (source.activeFire != null)
        {
            if (source.firePool != null)
                source.firePool.ReturnFire(source.activeFire); // Return to correct pool

            source.activeFire = null;
            source.heatingTime = 0f;

            if (source.fireAudioSource != null)
            {
                AudioController.Instance.StopLoopingSound(source.fireAudioSource);
                source.fireAudioSource = null;
            }

            Debug.Log("✅ Fire extinguished.");

            fireAlertFeedback?.HideExclamation();
        }
    }

}
