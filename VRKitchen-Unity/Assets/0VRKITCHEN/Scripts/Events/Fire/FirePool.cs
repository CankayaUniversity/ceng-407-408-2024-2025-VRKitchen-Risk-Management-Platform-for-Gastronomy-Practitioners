using System.Collections.Generic;
using UnityEngine;

public class FirePool : MonoBehaviour
{
    public GameObject firePrefab;
    public int poolSize = 10;

    private Queue<GameObject> pool = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject fire = Instantiate(firePrefab);
            fire.SetActive(false);
            pool.Enqueue(fire);
        }
    }

    public GameObject GetFire(Vector3 position)
    {
        GameObject fire;
        if (pool.Count > 0)
        {
            fire = pool.Dequeue();
        }
        else
        {
            fire = Instantiate(firePrefab);
        }

        fire.transform.position = position;
        fire.SetActive(true);
        return fire;
    }

    public void ReturnFire(GameObject fire)
    {
        fire.SetActive(false);
        pool.Enqueue(fire);
    }
}
