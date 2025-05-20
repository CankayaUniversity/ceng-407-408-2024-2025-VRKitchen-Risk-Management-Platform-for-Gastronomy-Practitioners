using UnityEngine;

public class Seasoning : MonoBehaviour
{
    private string seasoningType;

    private void Start()
    {
        seasoningType = gameObject.tag; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            Dish dish = other.GetComponent<Dish>();
            if (dish != null)
            {
                dish.AddSeasoning(seasoningType, 1);

                // Send RAG query
                var manager = FindObjectOfType<GameActionManager>();
                if (manager != null)
                {
                    string action = $"I added {seasoningType.ToLower()} to the dish";
                    Debug.Log("" + action);
                    manager.RegisterAction(action);
                }
            }

            gameObject.SetActive(false);
        }
    }
}