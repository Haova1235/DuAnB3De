using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Xoay nhân vật")]
    public float rotationSmoothTime = 0.12f; // độ trễ khi xoay, số càng lớn xoay càng chậm/mượt

    public Transform cameraTransform; // kéo CameraPivot vào đây

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;

    private Vector2 moveInput;
    private bool isSprinting;

    private float rotationVelocity; // dùng nội bộ cho SmoothDampAngle

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    void OnSprint(InputValue value)
    {
        isSprinting = value.isPressed;
    }

    void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Update()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Hướng camera (bỏ trục Y)
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y + camRight * moveInput.x;

        float speed = isSprinting ? sprintSpeed : moveSpeed;
        controller.Move(moveDir * speed * Time.deltaTime);

        // Luôn xoay nhân vật theo hướng camera (không phụ thuộc có di chuyển hay không)
        float targetAngle = cameraTransform.eulerAngles.y;
        float currentAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationVelocity,
            rotationSmoothTime
        );
        transform.rotation = Quaternion.Euler(0f, currentAngle, 0f);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}