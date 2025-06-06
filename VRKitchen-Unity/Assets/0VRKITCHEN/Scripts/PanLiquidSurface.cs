using UnityEngine;

public class PanLiquidSurface : MonoBehaviour
{
    public GameObject liquidQuad;         // Su/yağ yüzeyi gösterecek quad
    public Material waterMaterial;
    public Material oilMaterial;
    public UnityToAPI toAPI;
    private bool hasActivated = false;

    private void Start()
    {
        if (liquidQuad != null)
        {
            liquidQuad.SetActive(false);
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (hasActivated || liquidQuad == null) return;

        if (other.CompareTag("WaterParticle"))
        {
            ActivateLiquid(waterMaterial);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasActivated || liquidQuad == null) return;

        if (other.CompareTag("Oil")) // yağ objesine bu tag ver
        {
            toAPI.queryText = "I have put oil in frying pan in game. What now?";
            toAPI.SubmitQuery();
            ActivateLiquid(oilMaterial);
        }
    }

    private void ActivateLiquid(Material mat)
    {
        Renderer renderer = liquidQuad.GetComponent<Renderer>();
        if (renderer != null && mat != null)
        {
            renderer.material = mat;
        }

        liquidQuad.SetActive(true);
        hasActivated = true;
    }
}