using UnityEngine;

public class BurgerStackDetector : MonoBehaviour
{
    private GameObject lastItemBelow;

    private void FixedUpdate()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position;
        Vector3 rayDir = Vector3.down;

        // Cast a ray downward to check for an object beneath
        if (Physics.Raycast(rayOrigin, rayDir, out hit, 0.15f))
        {
            GameObject hitObj = hit.collider.gameObject;

            // If the object below has a different name than before, send query
            if (hitObj != lastItemBelow && hitObj.name != gameObject.name)
            {
                string topName = GetCleanName(gameObject.name);
                string bottomName = GetCleanName(hitObj.name);

                lastItemBelow = hitObj;

                GameActionManager manager = FindObjectOfType<GameActionManager>();
                if (manager != null)
                {
                    manager.RegisterAction($"I placed the {topName} on the {bottomName}. What is the next step?");
                }
            }
        }
    }

    // Helper: Clean name (remove clones, underscores, etc.)
    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Replace("_", " ").ToLower().Trim();
    }
}