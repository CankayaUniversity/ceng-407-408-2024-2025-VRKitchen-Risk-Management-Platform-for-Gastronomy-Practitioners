using UnityEngine;

public class VisualFeedbackController : MonoBehaviour
{
    public GameObject exclamationPrefab;

    private GameObject instance;

    public void ShowExclamation(Vector3 spawnPosition)
    {
        Debug.Log("📌 Spawning exclamation at: " + spawnPosition);

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

        // Play exclamation sound using AudioController
        AudioController.Instance.PlayExclamationSound();
    }

    public void HideExclamation()
    {
        if (instance != null)
        {
            instance.SetActive(false); // Hides the alert
        }
    }
}
