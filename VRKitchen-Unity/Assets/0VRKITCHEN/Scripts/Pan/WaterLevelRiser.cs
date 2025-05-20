using UnityEngine;

public class WaterFillTrigger : MonoBehaviour
{
    public GameObject potWaterObject;
    public float fillDuration = 5f;
    public Material waterMaterial; // Inspector'dan atanacak
    public Material oilMaterial;
    private Animation waterAnimation;
    private float fillTime = 0f;
    private bool isFilling = false;
    private bool hasActivatedWater = false;

    public UnityToAPI toAPI; // Add to inspector
    private bool hasSentQuery = false;


    private void Start()
    {
        // Ba�ta potwater'� kapal� b�rak
        if (potWaterObject != null)
        {
            potWaterObject.SetActive(false); // g�venlik i�in yine de
        }
    }

    private void OnParticleCollision(GameObject other)
    {

        if (other.CompareTag("WaterParticle") || other.CompareTag("Oil"))
        {

            isFilling = true;

            if (!hasActivatedWater && potWaterObject != null)
            {

                potWaterObject.SetActive(true);
                //hasActivatedWater = true;
                Renderer potRenderer = potWaterObject.GetComponent<Renderer>();
                if (potRenderer != null)
                {
                    if (other.CompareTag("WaterParticle") && waterMaterial != null)
                        potRenderer.material = waterMaterial;

                    else if (other.CompareTag("Oil") && oilMaterial != null)
                        potRenderer.material = oilMaterial;
                }
                // Aktif hale gelince Animation component'ine eri�
                waterAnimation = potWaterObject.GetComponent<Animation>();

                if (waterAnimation != null && waterAnimation.GetClip("water_rise") != null)
                {
                    waterAnimation.Play("water_rise");
                    waterAnimation["water_rise"].speed = 0f;
                    waterAnimation["water_rise"].normalizedTime = 0f;
                }
                else
                {
                    Debug.LogWarning("Potwater objesinde 'water_rise' animasyonu bulunamad�.");
                }
            }
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

            // ✅ Check if filling is complete and query hasn't been sent
            if (fillTime >= fillDuration && !hasSentQuery && toAPI != null)
            {
                toAPI.queryText = "The pot has been filled with water. What is the next step in the game? Provide only the next in-game step.";
                toAPI.SubmitQuery();
                hasSentQuery = true;
            }
        }

        isFilling = false; // reset each frame
    }

}