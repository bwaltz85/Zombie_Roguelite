// SimplePool.cs
using UnityEngine;
using System.Collections.Generic;

public class SimplePool : MonoBehaviour
{
    public GameObject prefab;
    public int prewarm = 20;

    Queue<GameObject> q = new();

    void Awake()
    {
        for (int i = 0; i < prewarm; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            q.Enqueue(go);
        }
    }

    public GameObject Get(Vector3 pos, Quaternion rot)
    {
        GameObject go = q.Count > 0 ? q.Dequeue() : Instantiate(prefab, transform);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);
        return go;
    }

    public void Return(GameObject go)
    {
        go.SetActive(false);
        q.Enqueue(go);
    }
}
