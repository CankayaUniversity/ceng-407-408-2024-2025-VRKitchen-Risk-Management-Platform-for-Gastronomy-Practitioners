using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private FoodData[] availableFoods; 
    [SerializeField] private Transform spawnPoint;      

    public void SpawnFood(int foodIndex)
    {
        if (foodIndex < 0 || foodIndex >= availableFoods.Length) return;

        FoodData selectedFood = availableFoods[foodIndex];
        if (selectedFood.foodPrefab != null)
        {
            Instantiate(selectedFood.foodPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Selected food does not have a prefab assigned!");
        }
    }
}