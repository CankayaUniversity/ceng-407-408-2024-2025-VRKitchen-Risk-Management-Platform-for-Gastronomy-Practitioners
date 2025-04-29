using UnityEngine;

public class BinCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trash"))
        {
            Debug.Log("Trash collected: " + other.name);
            Destroy(other.gameObject); // You can also disable it instead
        }
    }
}
