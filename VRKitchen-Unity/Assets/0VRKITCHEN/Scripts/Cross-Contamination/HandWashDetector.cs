using UnityEngine;
using System.Collections;

public class HandWashDetector : MonoBehaviour
{
    public UnityToAPI toAPI;
    public float washDuration = 2.0f;

    private bool hasWashedHands = false;
    private bool isWashing = false;
    private float washTimer = 0f;

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Hand"))
        {
            if (!isWashing)
            {
                Debug.Log("[HandWash] Water hit the hand. Starting wash...");
                isWashing = true;
            }
        }
    }

    private void Update()
    {
        if (isWashing && !hasWashedHands)
        {
            washTimer += Time.deltaTime;

            if (washTimer >= washDuration)
            {
                hasWashedHands = true;
                isWashing = false;
                Debug.Log("[HandWash] Hands have been washed ");

                if (toAPI != null)
                {
                    toAPI.queryText = "I washed my hands. What now?";
                    toAPI.SubmitQuery();
                }
            }
        }

        if (!isWashing)
        {
            washTimer = 0f;
        }
    }

    public void ResetWashState()
    {
        hasWashedHands = false;
        isWashing = false;
        washTimer = 0f;
        Debug.Log("[HandWash] State reset.");
    }
}
