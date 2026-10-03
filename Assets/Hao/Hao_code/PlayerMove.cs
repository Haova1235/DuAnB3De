using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public InputActionAsset InputActions;
    public CharacterController playerCharacterController;

    [SerializeField] private Transform playerCamera;

    private InputAction playerMoveAction;
    private InputAction playerJumpAction;
    private InputAction playerLookAction;
    private InputAction playerCrouchAction; // Thêm Action Crouch

    private Vector2 playerMoveAmount;
    private Vector2 playerLookAmount;

    [SerializeField] private float playerWalkSpeed = 5.0f;
    private float verticalVelocity = 0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 5.0f;

    [Header("Camera Look Settings")]
    [SerializeField] private float mouseSensitivity = 0.1f;
    [SerializeField] private float topClamp = 85.0f;
    [SerializeField] private float bottomClamp = -85.0f;
    private float cameraPitch = -1f;

    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float standingHeight = 2.0f;
    [SerializeField] private float crouchingHeight = 1.0f;
    [SerializeField] private float standingCameraY = 1.5f;
    [SerializeField] private float crouchingCameraY = 0.7f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    private bool isCrouching = false;
    private float currentSpeed;

    private void OnEnable()
    {
        InputActions.FindActionMap("Player").Enable();
    }

    private void OnDisable()
    {
        InputActions.FindActionMap("Player").Disable();
    }

    private void Awake()
    {
        playerMoveAction = InputSystem.actions.FindAction("Move");
        playerJumpAction = InputSystem.actions.FindAction("Jump");
        playerLookAction = InputSystem.actions.FindAction("Look");
        playerCrouchAction = InputSystem.actions.FindAction("Crouch"); // Đọc Action Crouch từ Input System
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Nên đổi thành false để ẩn hoàn toàn trỏ chuột khi chơi
        currentSpeed = playerWalkSpeed;
    }

    private void Update()
    {
        playerMoveAmount = playerMoveAction.ReadValue<Vector2>();
        playerLookAmount = playerLookAction.ReadValue<Vector2>();

        HandleCrouch();
        CameraLook();
        Jump();
        PlayerMoveAndRotate();
    }

    private void HandleCrouch()
    {
        // Khi nhấn nút Crouch (Ctrl / C)
        if (playerCrouchAction != null && playerCrouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
        }

        // Đổi tốc độ khi khom
        currentSpeed = isCrouching ? crouchSpeed : playerWalkSpeed;

        // Thu nhỏ/Mở rộng chiều cao CharacterController
        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        playerCharacterController.height = Mathf.Lerp(playerCharacterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        // Điều chỉnh Center để chân nhân vật không bị lún xuống sàn
        playerCharacterController.center = new Vector3(0, playerCharacterController.height / 2f, 0);

        // Hạ/Nâng vị trí Camera
        if (playerCamera != null)
        {
            float targetCamY = isCrouching ? crouchingCameraY : standingCameraY;
            Vector3 camPos = playerCamera.localPosition;
            camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
            playerCamera.localPosition = camPos;
        }
    }

    private void CameraLook()
    {
        if (playerCamera == null) return;

        cameraPitch -= playerLookAmount.y * mouseSensitivity;
        cameraPitch = Mathf.Clamp(cameraPitch, bottomClamp, topClamp);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * (playerLookAmount.x * mouseSensitivity));
    }

    private void Jump()
    {
        if (playerCharacterController.isGrounded)
        {
            verticalVelocity = -1f;

            // Không cho phép nhảy nếu đang khom người
            if (playerJumpAction.WasPressedThisFrame() && !isCrouching)
            {
                verticalVelocity = jumpHeight;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void PlayerMoveAndRotate()
    {
        Vector3 verticalMove = new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime;
        Vector3 moveDirection = transform.right * playerMoveAmount.x + transform.forward * playerMoveAmount.y;

        // Sử dụng currentSpeed (đã thay đổi tùy theo trạng thái Crouch)
        playerCharacterController.Move(moveDirection.normalized * currentSpeed * Time.deltaTime + verticalMove);
    }
}