using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float acceleration = 20f;      // how fast you reach target speed
    public float rotationDegPerSec = 720f;

    [Header("Gravity")]
    public float gravity = -30f;
    public float groundedStick = -2f;     // small downward force to keep grounded

    [Header("Aim")]
    public LayerMask groundMask;          // set to your Ground layer for mouse-ray

    private CharacterController controller;
    private Vector2 moveInput;
    private Vector3 velocity;
    private Camera cachedCam;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Input System (PlayerInput → Behavior = Send Messages, Action = "Move")
    public void OnMove(InputValue v) => moveInput = v.Get<Vector2>();

    void Update()
    {
        HandleMovement();      // camera-relative movement
        HandleAimRotation();   // face mouse cursor on ground
    }

    private Camera Cam
    {
        get
        {
            if (cachedCam == null) cachedCam = Camera.main; // works with Cinemachine
            return cachedCam;
        }
    }

    // ===== Movement (camera-relative) =====
    void HandleMovement()
    {
        Camera cam = Cam; if (cam == null) return;

        // Camera planar axes (ignore tilt)
        Vector3 camF = cam.transform.forward; camF.y = 0f; camF.Normalize();
        Vector3 camR = cam.transform.right; camR.y = 0f; camR.Normalize();

        // Map WASD onto camera axes
        Vector3 desired = camR * moveInput.x + camF * moveInput.y;
        if (desired.sqrMagnitude > 1f) desired.Normalize();
        desired *= moveSpeed;

        // Smooth acceleration to desired horizontal velocity
        Vector3 horiz = new Vector3(velocity.x, 0f, velocity.z);
        horiz = Vector3.MoveTowards(horiz, desired, acceleration * Time.deltaTime);

        // Gravity
        if (controller.isGrounded) velocity.y = groundedStick;
        else velocity.y += gravity * Time.deltaTime;

        velocity.x = horiz.x;
        velocity.z = horiz.z;

        controller.Move(velocity * Time.deltaTime);
    }

    // ===== Aim (face mouse cursor on ground) =====
    void HandleAimRotation()
    {
        if (Mouse.current == null) return;
        Camera cam = Cam; if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 500f, groundMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 dir = hit.point - transform.position; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, rotationDegPerSec * Time.deltaTime);
            }
        }
        else
        {
            // Optional fallback: face move direction when mouse isn't hitting ground
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            if (moveDir.sqrMagnitude > 0.0001f)
            {
                Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, target, rotationDegPerSec * Time.deltaTime);
            }
        }
    }
}
