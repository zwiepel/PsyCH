using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraRoot;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.10f; // NEW: kleiner als vorher!
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Interaction")]
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private LayerMask interactLayerMask = ~0;

    private CharacterController controller;
    private float pitch;
    private Vector3 verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraRoot == null)
            cameraRoot = Camera.main != null ? Camera.main.transform : null;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
        Interact();
    }

    private void Look()
    {
        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;

        // yaw
        transform.Rotate(Vector3.up * delta.x);

        // pitch
        pitch -= delta.y;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

        if (cameraRoot != null)
            cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void Move()
    {
        // WASD
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
        if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
        if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;

        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y).normalized;
        Vector3 horizontal = move * moveSpeed;

        if (controller.isGrounded && verticalVelocity.y < 0f)
            verticalVelocity.y = -2f;

        verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move((horizontal + verticalVelocity) * Time.deltaTime);
    }

    private void Interact()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame == false) return;
        if (cameraRoot == null) return;

        Ray ray = new Ray(cameraRoot.position, cameraRoot.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactLayerMask, QueryTriggerInteraction.Collide))
        {
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(this);
                return;
            }

            var parentInteractable = hit.collider.GetComponentInParent<IInteractable>();
            if (parentInteractable != null)
                parentInteractable.Interact(this);
        }
    }
}
