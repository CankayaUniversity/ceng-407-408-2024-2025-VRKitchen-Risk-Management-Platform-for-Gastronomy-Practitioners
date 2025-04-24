using UnityEngine;

public class VisualFeedbackController : MonoBehaviour
{
    public GameObject exclamationPrefab;
    public Transform fixedSpawnPoint; // Drag the ContaminationMarker here!

    private GameObject instance;

    public void ShowExclamation(Vector3 ignoredPosition)
    {
        if (fixedSpawnPoint == null)
        {
            Debug.LogError("❌ No fixedSpawnPoint assigned! Drag the ContaminationMarker in the Inspector.");
            return;
        }

        Vector3 spawnPosition = fixedSpawnPoint.position;
        Debug.Log("📌 Spawning exclamation at marker: " + spawnPosition);

        if (instance == null)
        {
            instance = Instantiate(exclamationPrefab, spawnPosition, Quaternion.identity);

            BillboardUI billboard = instance.GetComponent<BillboardUI>();
            if (billboard == null)
                billboard = instance.AddComponent<BillboardUI>();

            billboard.cameraToLookAt = Camera.main;
        }
        else
        {
            instance.transform.position = spawnPosition;
            instance.SetActive(true);
        }
    }
}
