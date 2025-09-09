using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputAutoCamera : MonoBehaviour
{
    private PlayerInput pi;

    void Awake()
    {
        pi = GetComponent<PlayerInput>();
    }

    void LateUpdate()
    {
        // If Cinemachine swaps cameras or MainCamera is recreated, keep this in sync
        if (pi.camera == null || Camera.main != pi.camera)
        {
            pi.camera = Camera.main;
        }
    }
}
