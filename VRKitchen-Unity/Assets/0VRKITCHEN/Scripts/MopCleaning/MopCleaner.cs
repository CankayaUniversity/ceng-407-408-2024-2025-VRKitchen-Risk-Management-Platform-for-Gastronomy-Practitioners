using UnityEngine;

public class MopCleaner : MonoBehaviour
{
    public float cleaningPower = 1f; // How fast it cleans

    private void OnTriggerStay(Collider other)
    {
        // Check if we are overlapping a cleanable spill
        CleanableSpill spill = other.GetComponent<CleanableSpill>();
        if (spill != null)
        {
            spill.Clean(cleaningPower);
        }
    }
}
