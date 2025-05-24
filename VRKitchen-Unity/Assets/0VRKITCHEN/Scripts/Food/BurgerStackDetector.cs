using UnityEngine;

public class BurgerStackDetector : MonoBehaviour
{
    private GameObject lastItemBelow;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        // Avoid self and repeated logs
        if (other == lastItemBelow || other.name == gameObject.name)
            return;

        // Compare Y positions: this object must be higher to be "on top"
        if (transform.position.y > other.transform.position.y)
        {
            lastItemBelow = other;

            string topName = GetCleanName(gameObject.name);
            string bottomName = GetCleanName(other.name);

            GameActionManager manager = FindObjectOfType<GameActionManager>();
            if (manager != null)
            {
                manager.RegisterAction($"I placed the {topName} on the {bottomName}. What now?");
            }

            Debug.Log($"[BurgerStackDetector] {topName} on {bottomName}");
        }
    }

    private string GetCleanName(string rawName)
    {
        return rawName.Replace("(Clone)", "").Replace("_", " ").ToLower().Trim();
    }
}