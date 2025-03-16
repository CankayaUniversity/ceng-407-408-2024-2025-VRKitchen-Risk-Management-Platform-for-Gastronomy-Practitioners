using UnityEngine;

public class Pan : MonoBehaviour
{
    public float temperature = 0f;

    public void Heat(float amount)
    {
        temperature += amount * Time.deltaTime;
        Debug.Log("Pan temperature: " + temperature);
    }

    public void Cool(float amount)
    {
        temperature -= amount * Time.deltaTime;
        if (temperature < 0) temperature = 0;
        Debug.Log("Pan temperature: " + temperature);
    }
}