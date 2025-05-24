using UnityEngine;

public class BurgerStackDetector : MonoBehaviour
{
    private GameObject lastItemBelow;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Prevent duplicate or self-collision logs
        if (other == lastItemBelow || other.name == gameObject.name)
            return;

        // Only process if this object is above the other
        if (transform.position.y > other.transform.position.y)
        {
            lastItemBelow = other;

            Vector3 bottomPos = other.transform.position;
            Vector3 currentPos = transform.position;

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Disable physics temporarily
            }

            // Center top item in X and Z over the bottom one
            transform.position = new Vector3(
                bottomPos.x,
                currentPos.y, // Keep current Y
                bottomPos.z
            );

            // Apply rotation correction if needed
            string itemName = gameObject.name.ToLower();

            if (itemName.Contains("tomato") || itemName.Contains("lettuce") || itemName.Contains("cheese"))
            {
                // Flatten slice items
                transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }
            else
            {
                // Default upright placement
                transform.rotation = Quaternion.identity;
            }

            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics
            }

            // Clean names
            string topName = GetCleanName(gameObject.name);
            string bottomName = GetCleanName(other.name);

            // Log and send action
            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction($"I placed the {topName} on the {bottomName}. What now?");
            }

            Debug.Log($"[BurgerStackDetector] {topName} on {bottomName}");
        }
    }

    // Helper: Clean name
    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Replace("_", " ").ToLower().Trim();
    }
}
