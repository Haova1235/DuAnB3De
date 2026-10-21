using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float interactDistance = 15f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform holdPoint;

    [Header("Drop Settings")]
    [SerializeField] private float dropForwardForce = 2f;

    [Header("Hold Interaction Settings")]
    [SerializeField] private float defaultHoldTime = 1.5f; // Thời gian mặc định giữ phím F (cho mở cửa)
    private float currentHoldTime = 0f;

    // Lưu trữ đối tượng đang tương tác giữ (có thể là Door hoặc CameraMountPoint)
    private Door currentTargetDoor = null;
    private CameraMountPoint currentTargetMount = null;

    private Item currentlyHeldItem;

    private void Update()
    {
        // 1. Nhấn phím G để thả đồ
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropItem();
        }

        // 2. Xử lý tương tác nhấn 1 lần (Click) và Giữ (Hold) bằng phím F
        HandleFInteraction();
    }

    private void HandleFInteraction()
    {
        if (Keyboard.current == null) return;

        // A. Xử lý khi nhấn 1 lần (Dành cho cửa đã mở khóa hoặc nhặt item)
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            TrySingleClickInteraction();
        }

        // B. Xử lý khi GIỮ phím F (Dành cho cửa đang khóa hoặc đặt camera vào mount point)
        if (Keyboard.current.fKey.isPressed)
        {
            Camera mainCam = Camera.main;
            if (mainCam == null) return;

            Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
            {
                // 1. Kiểm tra giữ để Mở Khóa Cửa
                Door door = GetDoorFromHit(hit);
                if (door != null && door.CheckIsLocked())
                {
                    if (currentTargetDoor != door)
                    {
                        ResetHold();
                        currentTargetDoor = door;
                    }

                    currentHoldTime += Time.deltaTime;
                    // Debug.Log("Đang giữ mở khóa... " + currentHoldTime.ToString("F1") + "s");

                    if (currentHoldTime >= defaultHoldTime)
                    {
                        door.Interact(this);
                        ResetHold();
                    }
                    return;
                }

                // 2. Kiểm tra giữ để Đặt Camera vào Mount Point
                CameraMountPoint mountPoint = hit.collider.GetComponentInParent<CameraMountPoint>();
                if (mountPoint != null && !mountPoint.isOccupied)
                {
                    if (currentTargetMount != mountPoint)
                    {
                        ResetHold();
                        currentTargetMount = mountPoint;
                    }

                    currentHoldTime += Time.deltaTime;
                    // Debug.Log("Đang giữ đặt camera... " + currentHoldTime.ToString("F1") + "s");

                    // Sử dụng thời gian riêng của mountPoint hoặc dùng mặc định
                    float requiredTime = mountPoint.placeHoldTime;
                    if (currentHoldTime >= requiredTime)
                    {
                        mountPoint.InteractWithMount(this);
                        ResetHold();
                    }
                    return;
                }
            }

            // Nếu lệch tầm nhìn hoặc không giữ đúng đối tượng hợp lệ
            ResetHold();
        }
        else
        {
            ResetHold();
        }
    }

    private void TrySingleClickInteraction()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            // 1. Cửa đã mở khóa (Bấm F 1 lần để đóng/mở)
            Door door = GetDoorFromHit(hit);
            if (door != null)
            {
                if (!door.CheckIsLocked())
                {
                    door.Interact(this);
                }
                return;
            }

            // 2. Nhặt Vật Phẩm (Item)
            Item item = hit.collider.GetComponentInParent<Item>();
            if (item != null)
            {
                if (currentlyHeldItem != null) DropItem();
                currentlyHeldItem = item;
                currentlyHeldItem.Pickup(holdPoint);
                return;
            }
        }
    }

    private Door GetDoorFromHit(RaycastHit hit)
    {
        Door door = hit.collider.GetComponent<Door>();
        if (door == null) door = hit.collider.GetComponentInParent<Door>();
        if (door == null) door = hit.collider.GetComponentInChildren<Door>();
        return door;
    }

    private void ResetHold()
    {
        currentHoldTime = 0f;
        currentTargetDoor = null;
        currentTargetMount = null;
    }

    public Item GetCurrentlyHeldItem() => currentlyHeldItem;
    public void ClearHeldItem() => currentlyHeldItem = null;

    public void DropItem()
    {
        if (currentlyHeldItem != null)
        {
            Camera mainCam = Camera.main;
            // Tính hướng ném (hướng camera nhìn phía trước + hướng ngẩng lên nhẹ)
            Vector3 throwDirection = (mainCam != null ? mainCam.transform.forward : transform.forward) + Vector3.up * 0.3f;

            // Gọi hàm Drop và truyền hướng (lực nhân bên trong Item sẽ tự tính theo throwForce của item đó)
            currentlyHeldItem.Drop(throwDirection);
            currentlyHeldItem = null;
        }
    }
}