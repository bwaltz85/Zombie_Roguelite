using UnityEngine;

public class PlayerRegistryDebug : MonoBehaviour
{
    public float logEvery = 2f;
    float t;

    void Update()
    {
        t -= Time.deltaTime;
        if (t <= 0f)
        {
            t = logEvery;
            Debug.Log($"[Registry] Players.Count = {PlayerRegistry.Players.Count}");
        }
    }
}
