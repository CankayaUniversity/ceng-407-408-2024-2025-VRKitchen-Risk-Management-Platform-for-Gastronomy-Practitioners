using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CleanableSpill : MonoBehaviour
{
    public float cleanAmount = 0f; // 0 = dirty, 1 = clean
    public float cleanSpeed = 0.5f;

    private Material mat;
    private Color originalColor;

    public UnityToAPI toAPI; // 🔌 RAG connection
    private bool hasNotified = false; // Ensure message is sent only once

    public ChefMovement chefMovement; // assign via Inspector if needed


    void Start()
    {
        if (toAPI == null)
            toAPI = FindObjectOfType<UnityToAPI>();

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
        }
    }


private void NotifyCleanup()
{
    Debug.Log($"Spill cleaned and removed: {gameObject.name}");

    if (toAPI != null)
    {
        Debug.Log("Sending Query");
        toAPI.queryText = "I cleaned the spill. What now?";
        toAPI.SubmitQuery();
    }

    if (chefMovement != null)
    {
        Debug.Log("✅ Using assigned chef reference — returning to start.");
        chefMovement.ReturnToStart();
    }
    else
    {
        Debug.LogWarning("❌ No chefMovement reference found on spill!");
    }

    hasNotified = true;
    StartCoroutine(DestroySelfAfterDelay());
}


    private IEnumerator NotifyAndDestroy()
    {
        NotifyCleanup(); // Do all logic first
        yield return new WaitForSeconds(0.1f); // Small delay to finish logic
        Destroy(gameObject);
    }

    private IEnumerator DestroySelfAfterDelay()
    {
        yield return new WaitForSeconds(0.1f); // Let all code execute
        Destroy(gameObject);
    }


}
