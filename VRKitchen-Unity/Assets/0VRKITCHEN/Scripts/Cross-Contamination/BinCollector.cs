using UnityEngine;

public class BinCollector : MonoBehaviour
{
    public UnityToAPI toAPI;
    private bool hasSentQuery = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            Debug.Log("Trash collected: " + other.name);
            Destroy(other.gameObject);

            if (toAPI != null && !hasSentQuery)
            {
                toAPI.queryText = "The contaminated food has been thrown away. What is the next step to handle cross contamination? Provide only the next in-game step";
                toAPI.SubmitQuery();
                hasSentQuery = true;
            }
        }
    }

    public void ResetQueryFlag()
    {
        hasSentQuery = false;
    }
}
