using UnityEngine;
using System.Collections;

public class PotSpillTrigger : MonoBehaviour
{
    [Header("Spill Settings")]
    public GameObject spillPrefab;
    public GameObject particleEffectPrefab;
    public Transform spillPoint; // Centered under the pot
    public float tiltDotThreshold = 0.4f; // Dot threshold instead of angle

    [Header("Water Mesh Reference")]
    public GameObject potWaterObject; // Assign your animated water mesh (e.g. potwater)

    private Animation waterAnimation;
    private bool hasSpilled = false;
    private float animationActiveTime = 0f;

    public UnityToAPI toAPI; // Drag your UnityToAPI object in the inspector

    void Start()
    {
        if (potWaterObject != null)
        {
            waterAnimation = potWaterObject.GetComponent<Animation>();
            Debug.Log("[SpillTrigger] Found Animation component: " + (waterAnimation != null));
        }
        else
        {
            Debug.LogError("[SpillTrigger] potWaterObject is not assigned!");
        }
    }

    void Update()
    {
        if (!CanSpill())
        {
            animationActiveTime = 0f;
            return;
        }

        animationActiveTime += Time.deltaTime;

        float tiltDot = Vector3.Dot(transform.forward.normalized, Vector3.down);
        Debug.Log($"[SpillTrigger] Animation time: {animationActiveTime:F2}, Tilt Dot: {tiltDot:F2}");

        if (!hasSpilled && animationActiveTime > 2f && tiltDot > tiltDotThreshold)
        {
            Debug.Log("[SpillTrigger] Spill triggered!");
            StartCoroutine(PlaySpillEffect());
            hasSpilled = true;
        }
    }

    bool CanSpill()
    {
        if (potWaterObject == null || !potWaterObject.activeSelf || waterAnimation == null)
            return false;

        if (!waterAnimation.IsPlaying("water_rise"))
            return false;

        float progress = waterAnimation["water_rise"].normalizedTime;
        Debug.Log($"[SpillTrigger] Animation progress: {progress:F2}");

        return progress >= 1f;
    }

    IEnumerator PlaySpillEffect()
    {
        Vector3 floorPos = new Vector3(spillPoint.position.x, 0.01f, spillPoint.position.z);

        if (particleEffectPrefab != null)
        {
            Debug.Log("[SpillTrigger] Spawning splash at: " + floorPos);
            GameObject splash = Instantiate(particleEffectPrefab, floorPos, Quaternion.identity);
            Destroy(splash, 2f);

            // Play water splash sound
            AudioController.Instance.PlayWaterSplashSound();
        }
        else
        {
            Debug.LogWarning("[SpillTrigger] No particle effect prefab assigned.");
        }

        yield return new WaitForSeconds(1f);

        if (spillPrefab != null)
        {
            Debug.Log("[SpillTrigger] Spawning puddle at: " + floorPos);
            GameObject puddle = Instantiate(spillPrefab, floorPos, Quaternion.Euler(90f, 0f, 0f));
            puddle.transform.localScale = new Vector3(1f, 1f, 1f); // Adjust scale if needed

            // Slightly offset second puddle
            Vector3 offset = new Vector3(0.2f, 0f, 0.2f); // You can adjust the offset as needed
            GameObject puddle2 = Instantiate(spillPrefab, floorPos + offset, Quaternion.Euler(90f, 0f, 0f));
            puddle2.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else
        {
            Debug.LogWarning("[SpillTrigger] ⚠ No spill prefab assigned.");
        }

        if (potWaterObject != null)
        {
            potWaterObject.SetActive(false);
            Debug.Log("[SpillTrigger] 🚫 Water hidden (SetActive false).");
        }

        if (toAPI != null)
        {
            Debug.Log("[SpillTrigger] Sending RAG query: Water spill happened.");
            toAPI.queryText = "Water spill happened. What should I do?";
            toAPI.SubmitQuery();
        }
        else
        {
            Debug.LogWarning("[SpillTrigger] ⚠ UnityToAPI reference not set.");
        }

        // ✅ Reset the fill state by calling the WaterFillTrigger on a child (e.g., PanBottom)
        WaterFillTrigger fillTrigger = GetComponentInChildren<WaterFillTrigger>();
        if (fillTrigger != null)
        {
            fillTrigger.ResetState();
            Debug.Log("[SpillTrigger] WaterFillTrigger reset after spill.");
        }

        hasSpilled = false;
    }
}
