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
    }
}