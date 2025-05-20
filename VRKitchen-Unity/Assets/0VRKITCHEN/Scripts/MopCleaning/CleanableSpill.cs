using UnityEngine;

public class CleanableSpill : MonoBehaviour
{
    public float cleanAmount = 0f; // 0 = dirty, 1 = clean
    public float cleanSpeed = 0.5f;

    private Material mat;
    private Color originalColor;

    public UnityToAPI toAPI; // 🔌 RAG connection
    private bool hasNotified = false; // Ensure message is sent only once

    void Start()
    {
        // Create a copy of the material so each stain fades independently
        mat = GetComponent<Renderer>().material;
        originalColor = mat.color;
    }

    public void Clean(float amount)
    {
        cleanAmount = Mathf.Clamp01(cleanAmount + amount * Time.deltaTime);
        mat.color = Color.Lerp(originalColor, new Color(0, 0, 0, 0), cleanAmount);

        if (cleanAmount >= 1f && !hasNotified)
        {
            NotifyCleanup(); // Say it's cleaned
            Destroy(gameObject); // or gameObject.SetActive(false);
        }
    }


    private void NotifyCleanup()
    {
        Debug.Log($"Spill cleaned and removed: {gameObject.name}");

        if (toAPI != null)
        {
            toAPI.queryText = "I cleaned the spill. What now?";
            toAPI.SubmitQuery();
        }

        hasNotified = true;
    }
}
