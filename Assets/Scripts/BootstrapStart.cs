using UnityEngine;

public class BootstrapStart : MonoBehaviour
{
    void Start()
    {
        if (GameLoop.I == null)
        {
            var go = new GameObject("GameLoop");
            go.AddComponent<GameLoop>();
        }
        GameLoop.I.StartRun();
    }
}
