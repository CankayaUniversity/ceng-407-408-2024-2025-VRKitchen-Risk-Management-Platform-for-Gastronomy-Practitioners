using UnityEngine;
using System.Collections;

public class WaterFillTrigger : MonoBehaviour
{
    public GameObject potWaterObject;
    public float fillDuration = 5f;
    public Material waterMaterial;
    public Material oilMaterial;
    public UnityToAPI toAPI;

    private Animation waterAnimation;
    private float fillTime = 0f;
    private bool isFilling = false;
    private bool hasActivatedWater = false;
    private bool hasSentQuery = false;

    private void Start()
    {
        if (potWaterObject != null)
        {
            potWaterObject.SetActive(false);
        }
    }

    // ✅ SU için (Particle)
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("WaterParticle"))
        {
            HandleLiquidActivation(waterMaterial);
        }
    }

    // ✅ YAĞ için (Obje ile fiziksel temas)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Oil"))
        {
            StartCoroutine(HandleOilTrigger());
        }
    }

    private IEnumerator HandleOilTrigger()
    {
        potWaterObject.SetActive(true);
        yield return null; // bir frame bekle ki Unity potWater'ı tam aktif etsin

        isFilling = true;
        hasActivatedWater = true;

        Renderer potRenderer = potWaterObject.GetComponent<Renderer>();
        if (potRenderer != null && oilMaterial != null)
        {
            potRenderer.material = oilMaterial;
        }

        waterAnimation = potWaterObject.GetComponent<Animation>();
        if (waterAnimation != null && waterAnimation.GetClip("water_rise") != null)
        {
            waterAnimation.Stop();
            waterAnimation["water_rise"].normalizedTime = 0f;
            waterAnimation["water_rise"].speed = 1f;
            waterAnimation.Play("water_rise");

            yield return new WaitForSecondsRealtime(0.05f);
            waterAnimation["water_rise"].speed = 0f;
        }
        else
        {
            Debug.LogWarning("⚠ Animation or 'water_rise' clip not found on potWaterObject.");
        }
    }

    private void HandleLiquidActivation(Material mat)
    {
        isFilling = true;

        if (potWaterObject != null)
        {
            potWaterObject.SetActive(true);
            hasActivatedWater = true;

            Renderer potRenderer = potWaterObject.GetComponent<Renderer>();
            if (potRenderer != null && mat != null)
            {
                potRenderer.material = mat;
            }

            waterAnimation = potWaterObject.GetComponent<Animation>();
            if (waterAnimation != null && waterAnimation.GetClip("water_rise") != null)
            {
                waterAnimation.Stop();
                waterAnimation["water_rise"].normalizedTime = 0f;
                waterAnimation["water_rise"].speed = 1f;
                waterAnimation.Play("water_rise");

                StartCoroutine(DelayedPause());
            }
            else
            {
                Debug.LogWarning("Potwater objesinde 'water_rise' animasyonu bulunamadı.");
            }
        }
    }

    private IEnumerator DelayedPause()
    {
        yield return new WaitForSecondsRealtime(0.05f);
        if (waterAnimation != null)
        {
            waterAnimation["water_rise"].speed = 0f;
        }
    }

    private void Update()
    {
        if (isFilling && waterAnimation != null && waterAnimation.IsPlaying("water_rise"))
        {
            fillTime += Time.deltaTime;
            fillTime = Mathf.Clamp(fillTime, 0f, fillDuration);
            float progress = fillTime / fillDuration;

            waterAnimation["water_rise"].normalizedTime = progress;
            waterAnimation["water_rise"].speed = 0f;

            if (fillTime >= fillDuration && !hasSentQuery && toAPI != null)
            {
                toAPI.queryText = "The pot has been filled with water. What now?";
                toAPI.SubmitQuery();
                hasSentQuery = true;
            }
        }

        isFilling = false;
    }

    public void ResetState()
    {
        Debug.Log("ResetState called");
        fillTime = 0f;
        isFilling = false;
        hasActivatedWater = false;
        hasSentQuery = false;

        if (potWaterObject != null)
        {
            // Visually hide water if needed (based on animation type)
            //potWaterObject.transform.localScale = new Vector3(1f, 0f, 1f); // If scale-based
             potWaterObject.transform.localPosition = new Vector3(0f, 0f, 0f); // If position-based

            potWaterObject.SetActive(false);

            waterAnimation = potWaterObject.GetComponent<Animation>();
            if (waterAnimation != null && waterAnimation.GetClip("water_rise") != null)
            {
                waterAnimation.Stop();
                waterAnimation["water_rise"].normalizedTime = 0f;
                waterAnimation["water_rise"].speed = 0f;
                Debug.Log("[WaterFillTrigger] Animation reset, NOT playing until triggered.");
            }
        }
    }
}
