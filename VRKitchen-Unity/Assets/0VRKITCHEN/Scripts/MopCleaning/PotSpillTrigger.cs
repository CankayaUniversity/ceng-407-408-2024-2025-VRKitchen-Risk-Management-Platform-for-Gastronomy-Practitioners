using UnityEngine;
using System.Collections;

public class PotSpillTrigger : MonoBehaviour
{
    [Header("Spill Settings")]
    public GameObject spillPrefab;
    public GameObject particleEffectPrefab;
    public Transform[] spillPoints;
    public float tiltThreshold = 60f;

    [Header("Water Mesh Reference")]
    public GameObject potWaterObject; // Assign the "potwater" object here

    private Animation waterAnimation;
    private bool hasSpilled = false;

    void Start()
    {
        // Get animation from pot water mesh (not from WaterFillTrigger)
        if (potWaterObject != null)
        {
            waterAnimation = potWaterObject.GetComponent<Animation>();
        }
    }

    void Update()
    {
        if (!IsWaterFullyFilled())
            return;

        float tiltX = NormalizeAngle(transform.eulerAngles.x);
        float tiltZ = NormalizeAngle(transform.eulerAngles.z);

        if (!hasSpilled && (Mathf.Abs(tiltX) > tiltThreshold || Mathf.Abs(tiltZ) > tiltThreshold))
        {
            StartCoroutine(PlaySpillEffect());
            hasSpilled = true;
        }
    }

    bool IsWaterFullyFilled()
    {
        if (potWaterObject == null || !potWaterObject.activeSelf || waterAnimation == null)
            return false;

        if (!waterAnimation.IsPlaying("water_rise"))
            return false;

        float progress = waterAnimation["water_rise"].normalizedTime;

        return progress >= 1f;
    }

    IEnumerator PlaySpillEffect()
    {
        foreach (Transform point in spillPoints)
        {
            Vector3 floorPos = new Vector3(point.position.x, 0.01f, point.position.z);
            GameObject splash = Instantiate(particleEffectPrefab, floorPos, Quaternion.identity);
            Destroy(splash, 2f);
        }

        yield return new WaitForSeconds(1f);

        foreach (Transform point in spillPoints)
        {
            Vector3 floorPos = new Vector3(point.position.x, 0.01f, point.position.z);
            Instantiate(spillPrefab, floorPos, Quaternion.Euler(90f, 0f, 0f));
        }
    }

    float NormalizeAngle(float angle)
    {
        angle = angle % 360;
        return (angle > 180f) ? angle - 360f : angle;
    }
}
