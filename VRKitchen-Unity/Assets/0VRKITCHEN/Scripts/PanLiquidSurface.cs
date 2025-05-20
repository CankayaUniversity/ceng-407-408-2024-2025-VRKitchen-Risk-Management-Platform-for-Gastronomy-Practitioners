using UnityEngine;

public class PanLiquidSurface : MonoBehaviour
{
    public GameObject liquidQuad;         // ��inde su/ya� efekti olacak Quad
    public Material waterMaterial;
    public Material oilMaterial;

    private bool hasActivated = false;

    private void Start()
    {
        if (liquidQuad != null)
        {
            liquidQuad.SetActive(false); // Ba�ta g�r�nmesin
        }
    }

    private void OnParticleCollision(GameObject other)
    {

        Debug.Log("aaaaa");
        if (other.CompareTag("WaterParticle") || other.CompareTag("Oil"))
        {
            liquidQuad.SetActive(true);
            Renderer renderer = liquidQuad.GetComponent<Renderer>();
            if (renderer != null)
            {
                if (other.CompareTag("WaterParticle") && waterMaterial != null)
                    renderer.material = waterMaterial;

                else if (other.CompareTag("Oil") && oilMaterial != null)
                    renderer.material = oilMaterial;
            }

            
        }
    }
}
