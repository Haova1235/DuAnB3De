using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    public InputActionAsset InputActions;
    public CharacterController playerCharacterController;

    [Header("References")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private Animator playerAnimator;

    private InputAction playerMoveAction;
    private InputAction playerJumpAction;
    private InputAction playerLookAction;
    private InputAction playerCrouchAction;

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

    [Header("Âm thanh")]
    [SerializeField] private SoundEffect walkStepSound;
    [SerializeField] private SoundEffect crouchStepSound;
    [SerializeField] private SoundEffect jumpSound;
    [SerializeField] private SoundEffect landSound;
    [SerializeField] private SoundEffect defeatSound;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float crouchStepInterval = 0.8f;
    [Tooltip("Tick: dừng tiếng bước chân ngay khi thả phím")]
    [SerializeField] private bool stopStepSoundWhenStopped = true;
    [Tooltip("Tick: phát 1 clip dài liên tục khi đi (nhớ tick Loop trong Walk Step Sound). Bỏ tick: phát từng tiếng theo nhịp Step Interval")]
    [SerializeField] private bool continuousStepSound = false;
    private float stepTimer = 0f;
    private float airTime = 0f;
    private bool wasGrounded = true;

    private bool isCrouching = false;
    private float currentSpeed;

    // --- BIẾN QUẢN LÝ TRẠNG THÁI BỊ BẪY ---
    private bool isTrapped = false;
    private float trappedSpeedMultiplier = 1.0f;

    [Header("UI & Defeat Settings")]
    [SerializeField] private GameObject defeatUIPanel; // Kéo Panel Defeat UI vào đây
    private bool isDefeated = false;
    public bool IsDefeated => isDefeated;

    public float moveSpeed
    {
        get => currentSpeed;
        set => currentSpeed = value;
    }

    public void ApplyTrapEffect(float multiplier, bool trapped)
    {
        isTrapped = trapped;
        trappedSpeedMultiplier = multiplier;
    }

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
        playerCrouchAction = InputSystem.actions.FindAction("Crouch");

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        HandleFootsteps();
        UpdateAnimations();
    }

    private void HandleCrouch()
    {
        if (playerCrouchAction != null && playerCrouchAction.WasPressedThisFrame())
        {
            isCrouching = !isCrouching;
        }

        if (!isTrapped)
        {
            currentSpeed = isCrouching ? crouchSpeed : playerWalkSpeed;
        }
        else
        {
            float baseSpeed = isCrouching ? crouchSpeed : playerWalkSpeed;
            currentSpeed = baseSpeed * trappedSpeedMultiplier;
        }

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        playerCharacterController.height = Mathf.Lerp(playerCharacterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        playerCharacterController.center = new Vector3(0, playerCharacterController.height / 2f, 0);

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

            if (playerJumpAction.WasPressedThisFrame() && !isCrouching && !isTrapped)
            {
                verticalVelocity = jumpHeight;
                jumpSound.Play2D();
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

        playerCharacterController.Move(moveDirection.normalized * currentSpeed * Time.deltaTime + verticalMove);
    }

    private void HandleFootsteps()
    {
        bool grounded = playerCharacterController.isGrounded;

        // Tiếng tiếp đất, chỉ kêu khi rơi đủ lâu để không kêu khi bước xuống dốc nhỏ
        if (!grounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (!wasGrounded && airTime > 0.25f) landSound.Play2D();
            airTime = 0f;
        }
        wasGrounded = grounded;

        // Dùng vận tốc thực tế nên bị bẫy làm chậm hay kẹt tường cũng không kêu sai
        Vector3 horizontalVelocity = playerCharacterController.velocity;
        horizontalVelocity.y = 0f;

        float interval = isCrouching ? crouchStepInterval : walkStepInterval;
        bool isMovingOnGround = grounded && horizontalVelocity.magnitude >= 0.2f;

        SoundEffect currentStep = isCrouching ? crouchStepSound : walkStepSound;
        SoundEffect otherStep = isCrouching ? walkStepSound : crouchStepSound;

        if (!isMovingOnGround)
        {
            stepTimer = interval; // vừa bắt đầu đi là có tiếng bước đầu ngay
            if (stopStepSoundWhenStopped)
            {
                walkStepSound.Stop();
                crouchStepSound.Stop();
            }
            return;
        }

        // Đang đi bộ mà chuyển sang cúi (hoặc ngược lại) thì tắt tiếng của kiểu kia
        otherStep.Stop();

        if (continuousStepSound)
        {
            if (!currentStep.IsPlaying) currentStep.Play2D();
            return;
        }

        stepTimer += Time.deltaTime;
        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            currentStep.Play2D();
        }
    }

    private void UpdateAnimations()
    {
        if (playerAnimator == null) return;

        float inputMagnitude = playerMoveAmount.magnitude;

        // Nếu không có thao tác di chuyển hoặc đang dính bẫy, ép giá trị Speed về 0 NGAY LẬP TỨC
        float targetAnimSpeed = (inputMagnitude < 0.05f || isTrapped) ? 0f : (isCrouching ? 0.5f : 1.0f);

        playerAnimator.SetFloat("Speed", targetAnimSpeed, 0.0f, Time.deltaTime);

        bool isMoving = inputMagnitude > 0.05f;

        playerAnimator.SetBool("IsWalking", isMoving && !isCrouching);
        playerAnimator.SetBool("IsStand_still", !isMoving && !isCrouching);

        playerAnimator.SetBool("IsSneak", isCrouching);
        playerAnimator.SetBool("IsSneak_Walking", isCrouching && isMoving);
    }

    // Hàm này được gọi từ Enemy khi tấn công trúng Player
    public void TriggerPlayerDefeat()
    {
        if (isDefeated) return;
        isDefeated = true;

        // Update() sẽ ngừng chạy nên phải tắt tiếng bước chân tại đây
        walkStepSound.Stop();
        crouchStepSound.Stop();

        defeatSound.Play2D();
        BackgroundMusic.Instance?.Stop(1f); // tùy chọn: tắt dần nhạc nền khi thua, xóa dòng này nếu không muốn

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsAttacked", true);
        }

        // Khóa mọi input di chuyển của người chơi
        this.enabled = false;

        StartCoroutine(DefeatSequenceRoutine());
    }

    private IEnumerator DefeatSequenceRoutine()
    {
        // Đợi để animation ngã chạy xong
        yield return new WaitForSeconds(5f);

        if (defeatUIPanel != null)
        {
            defeatUIPanel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }
}