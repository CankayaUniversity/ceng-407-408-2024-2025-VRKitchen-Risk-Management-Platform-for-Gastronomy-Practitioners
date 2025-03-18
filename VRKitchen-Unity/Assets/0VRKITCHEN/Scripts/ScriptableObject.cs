using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Foods", order = 1)]
public class Foods : ScriptableObject
{
    public string prefabName;

    public int temperature;
    public Material spawnPoints;
}