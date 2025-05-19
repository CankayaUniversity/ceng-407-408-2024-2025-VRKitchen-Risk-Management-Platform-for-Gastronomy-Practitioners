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
                toAPI.queryText = GameQueries.ThowTheMeat;
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
