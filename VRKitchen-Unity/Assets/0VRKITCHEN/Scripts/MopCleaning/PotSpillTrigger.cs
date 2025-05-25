using UnityEngine;
using System.Collections;

public class PotSpillTrigger : MonoBehaviour
{
    [Header("Spill Settings")]
    public GameObject spillPrefab;
    public GameObject particleEffectPrefab;
    public Transform spillPoint; // Centered under the pot
    public float tiltDotThreshold = 0.4f;

    [Header("Water Mesh Reference")]
    public GameObject potWaterObject;

    private Animation waterAnimation;
    private bool hasSpilled = false;
    private float animationActiveTime = 0f;

    public UnityToAPI toAPI;

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
            AudioController.Instance.PlayWaterSplashSound();
        }

        yield return new WaitForSeconds(1f);

        if (spillPrefab != null)
        {
            // Spawn puddles
            GameObject puddle1 = Instantiate(spillPrefab, floorPos, Quaternion.Euler(90f, 0f, 0f));
            puddle1.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject puddle2 = Instantiate(spillPrefab, floorPos + new Vector3(0.2f, 0f, 0.2f), Quaternion.Euler(90f, 0f, 0f));
            puddle2.transform.localScale = new Vector3(1f, 1f, 1f);

            // Assign chef to both puddles
            ChefMovement chef = FindObjectOfType<ChefMovement>();
            if (chef != null)
            {
                AssignChefToSpill(puddle1, chef);
                AssignChefToSpill(puddle2, chef);

                // Tell chef to approach spill
                Vector3 approachPoint = floorPos + new Vector3(0.5f, 0f, 0.5f);
                chef.GoToLocation(approachPoint);
            }
        }
        else
        {
            Debug.LogWarning("[SpillTrigger] ⚠ No spill prefab assigned.");
        }

        if (potWaterObject != null)
        {
            potWaterObject.SetActive(false);
            Debug.Log("[SpillTrigger] 🚫 Water hidden.");
        }

        if (toAPI != null)
        {
            toAPI.queryText = "Water spill happened. What should I do?";
            toAPI.SubmitQuery();
            Debug.Log("[SpillTrigger] RAG query sent for spill.");
        }

        WaterFillTrigger fillTrigger = GetComponentInChildren<WaterFillTrigger>();
        if (fillTrigger != null)
        {
            fillTrigger.ResetState();
            Debug.Log("[SpillTrigger] WaterFillTrigger reset.");
        }

        hasSpilled = false;
    }

    void AssignChefToSpill(GameObject puddle, ChefMovement chef)
    {
        CleanableSpill cs = puddle.GetComponent<CleanableSpill>();
        if (cs != null)
        {
            cs.chefMovement = chef;
        }
    }
}
