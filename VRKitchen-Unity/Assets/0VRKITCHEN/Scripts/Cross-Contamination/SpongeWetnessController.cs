using UnityEngine;

public class SpongeWetnessController : MonoBehaviour
{
    public float wetDuration = 5f;
    [HideInInspector] public bool isWet;

    private float wetTimer;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WaterSource"))
        {
            isWet = true;
            wetTimer = wetDuration;
            Debug.Log("Sponge is now wet.");
        }
    }

    private void Update()
    {
        if (!isWet) return;

        wetTimer -= Time.deltaTime;
        if (wetTimer <= 0f)
        {
            isWet = false;
            Debug.Log("Sponge has dried.");
        }
    }
}
