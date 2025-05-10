using UnityEngine;

public class WaterFillTrigger : MonoBehaviour
{
    public GameObject potWaterObject;
    public float fillDuration = 5f;

    private Animation waterAnimation;
    private float fillTime = 0f;
    private bool isFilling = false;
    private bool hasActivatedWater = false;

    private void Start()
    {
        // Baþta potwater'ý kapalý býrak
        if (potWaterObject != null)
        {
            potWaterObject.SetActive(false); // güvenlik için yine de
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("aaaaaaa");
        if (other.CompareTag("WaterParticle"))
        {
            
            isFilling = true;

            if (!hasActivatedWater && potWaterObject != null)
            {
                potWaterObject.SetActive(true);
                hasActivatedWater = true;

                // Aktif hale gelince Animation component'ine eriþ
                waterAnimation = potWaterObject.GetComponent<Animation>();

                if (waterAnimation != null && waterAnimation.GetClip("water_rise") != null)
                {
                    waterAnimation.Play("water_rise");
                    waterAnimation["water_rise"].speed = 0f;
                    waterAnimation["water_rise"].normalizedTime = 0f;
                }
                else
                {
                    Debug.LogWarning("Potwater objesinde 'water_rise' animasyonu bulunamadý.");
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
        }

        isFilling = false; // her frame sýfýrlanmalý
    }
}
