using UnityEngine;

public class CleanableSpill : MonoBehaviour
{
    public float cleanAmount = 0f; // 0 = dirty, 1 = clean
    public float cleanSpeed = 0.5f;

    private Material mat;
    private Color originalColor;

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

        if (cleanAmount >= 1f)
        {
            Destroy(gameObject); // or disable: gameObject.SetActive(false);
        }
    }
}
