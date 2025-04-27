using System.Collections.Generic;
using UnityEngine;

public class FoodPool : MonoBehaviour
{
    public static FoodPool Instance;

    [System.Serializable]
    public class FoodPoolEntry
    {
        public FoodData foodData;
        public int initialAmount = 5;
    }

    [SerializeField] private List<FoodPoolEntry> foodsToPool = new List<FoodPoolEntry>();
    private Dictionary<FoodData, Queue<GameObject>> pooledFoods = new Dictionary<FoodData, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        foreach (var entry in foodsToPool)
        {
            Queue<GameObject> queue = new Queue<GameObject>();

            for (int i = 0; i < entry.initialAmount; i++)
            {
                GameObject obj = Instantiate(entry.foodData.foodPrefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }

            pooledFoods.Add(entry.foodData, queue);
        }
    }

    public GameObject GetFood(FoodData foodData)
    {
        if (!pooledFoods.ContainsKey(foodData))
        {
            Debug.LogWarning("Food not found in pool!");
            return null;
        }

        if (pooledFoods[foodData].Count == 0)
        {
            GameObject newObj = Instantiate(foodData.foodPrefab);
            newObj.SetActive(false);
            pooledFoods[foodData].Enqueue(newObj);
        }

        GameObject foodObj = pooledFoods[foodData].Dequeue();
        foodObj.SetActive(true);
        return foodObj;
    }

    public void ReturnFood(FoodData foodData, GameObject foodObject)
    {
        foodObject.SetActive(false);
        pooledFoods[foodData].Enqueue(foodObject);
    }
}