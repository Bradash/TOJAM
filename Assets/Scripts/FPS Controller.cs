using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3f;

    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upDownRange = 80f;

    private Camera mainCamera;
    private float verticalRotation;
    private CharacterController characterController;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }
    void HandleMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal") * walkSpeed;
        float verticalInput = Input.GetAxis("Vertical") * walkSpeed;

        Vector3 speed = new Vector3(horizontalInput, 0, verticalInput) * walkSpeed;
        speed = transform.rotation * speed;

        characterController.SimpleMove(speed);
    }
    void HandleRotation()
    {
        float mouseXRotation = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);

        verticalRotation -= Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);

    }
}

