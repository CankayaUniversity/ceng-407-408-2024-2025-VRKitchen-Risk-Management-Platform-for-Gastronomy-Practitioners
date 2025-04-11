using UnityEngine;

public class Food : MonoBehaviour
{
    public float temperature = 0f; 
    public float cookedThreshold = 50f; 
    public float overcookedThreshold = 100f; 

    private Renderer foodRenderer;
    private Color originalColor; 

    private void Start()
    {
        foodRenderer = transform.GetComponent<Renderer>();
        if (foodRenderer != null)
        {
            originalColor = foodRenderer.material.color; // Save the original color
        }
    }

    public void Heat(float amount)
    {
        temperature += amount;
        UpdateCookingStage();
    }

    private void UpdateCookingStage()
    {
        if (foodRenderer == null) return;

        if (temperature < cookedThreshold)
        {
            
            foodRenderer.material.color = originalColor;
        }
        else if (temperature < overcookedThreshold)
        {
            
            foodRenderer.material.color = Color.Lerp(originalColor, Color.red, (temperature - cookedThreshold) / (overcookedThreshold - cookedThreshold));
        }
        else
        {
            
            foodRenderer.material.color = Color.red;
        }
    }
}