using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 1.0f;
    public Transform playerCamera;

    private Rigidbody rb;
    private Vector3 moveWorld;
    private float xRotation;

    private InputAction moveAction;
    private InputAction lookAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;

        moveAction = new InputAction(name: "Move");
        var wasd = moveAction.AddCompositeBinding("2DVector");
        wasd.With("Up", "<Keyboard>/w");
        wasd.With("Down", "<Keyboard>/s");
        wasd.With("Left", "<Keyboard>/a");
        wasd.With("Right", "<Keyboard>/d");

        lookAction = new InputAction(name: "Look");
        lookAction.AddBinding("<Mouse>/delta");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    void OnDisable()
    {
        lookAction.Disable();
        moveAction.Disable();
    }

    void Update()
    {
        Vector2 move = moveAction.ReadValue<Vector2>();
        moveWorld = (transform.right * move.x + transform.forward * move.y).normalized;
        Vector2 look = lookAction.ReadValue<Vector2>() * mouseSensitivity * Time.deltaTime;
        float yaw = look.x;
        float pitch = look.y;

        transform.Rotate(Vector3.up * yaw);

        xRotation -= pitch;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (playerCamera != null)
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveWorld * moveSpeed * Time.fixedDeltaTime);
    }
}
