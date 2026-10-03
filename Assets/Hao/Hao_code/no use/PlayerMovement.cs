using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float rotationSmoothTime = 0.12f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Camera Reference (chỉ đọc, không chỉnh sửa)")]
    [SerializeField] private Transform cameraTransform; // kéo Main Camera vào đây

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float rotationVelocity; // dùng cho SmoothDampAngle

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleJumpAndGravity();
    }

    private void HandleGroundCheck()
    {
        isGrounded = groundCheck != null
            ? Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)
            : controller.isGrounded;

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector2 inputDir = new Vector2(horizontal, vertical);

        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float targetSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (inputDir.sqrMagnitude >= 0.01f)
        {
            // Góc hướng input, cộng thêm yaw của camera (chỉ ĐỌC, không ghi)
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg
                                 + cameraTransform.eulerAngles.y;

            // Xoay nhân vật mượt tới hướng đó
            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y, targetAngle, ref rotationVelocity, rotationSmoothTime);

            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            // Di chuyển theo hướng target (không phải hướng đã smooth, để tránh trễ)
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * targetSpeed * Time.deltaTime);
        }
    }

    private void HandleJumpAndGravity()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}