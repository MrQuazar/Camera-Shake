using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 8f;
    public float gravity = -20f;

    [Header("Camera Shake Setting")]
    public float shakeAngle = 45f;
    public float shakeDuration = 0.15f;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 200f;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private Coroutine shakeRoutine;
    private float xRotation = 0f;
    private bool isGrounded = false;

    static readonly string ground = "Ground";

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    private void OnCollisionEnter(Collision collision)
    {
        isGrounded = collision.transform.CompareTag(ground);
        if(isGrounded)
        {
            TriggerShake();
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = !collision.transform.CompareTag(ground);
    }
    void HandleMovement()
    {
        // WASD movement
        float x = Input.GetAxis("Horizontal");   // A/D
        float z = Input.GetAxis("Vertical");     // W/S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Ground checks
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = jumpForce;

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate camera vertically
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player horizontally
        transform.Rotate(Vector3.up * mouseX);
    }

    public void TriggerShake()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(shakeUnscaled());
    }

    IEnumerator shakeUnscaled()
    {
        Quaternion startAngle = cameraTransform.localRotation;

        float rightTilt = startAngle.z - shakeAngle;
        float leftTilt = startAngle.z + shakeAngle;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, shakeDuration);

            // Normalized ping-pong 0 - 1 - 0
            float p = Mathf.PingPong(t * 2f, 1f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, Mathf.Lerp(startAngle.z, rightTilt, p));

            yield return null;
        }

        t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, shakeDuration);

            float p = Mathf.PingPong(t * 2f, 1f);

            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, Mathf.Lerp(startAngle.z, leftTilt, p));

            yield return null;
        }

        shakeRoutine = null;
    }
}