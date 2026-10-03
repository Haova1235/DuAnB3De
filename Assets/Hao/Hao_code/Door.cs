using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool isLocked = false;

    [Header("Animator Settings")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string boolParameterName = "isOpen"; // Tên biến Bool bạn đặt trong Animator Controller

    private bool isOpen = false;

    private void Awake()
    {
        // Tự lấy Animator nếu chưa kéo vào Inspector
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }

    /// <summary>
    /// Gọi khi Player tương tác (E) trực tiếp với Cánh Cửa
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        if (isLocked)
        {
            if (playerInventory != null && HasCorrectKey(playerInventory))
            {
                Debug.Log("Mở khóa cửa thành công!");
                isLocked = false;

                // Xóa/vứt chìa khóa trên tay sau khi dùng
                Item heldItem = playerInventory.GetCurrentlyHeldItem();
                playerInventory.DropItem();
                if (heldItem != null) Destroy(heldItem.gameObject);

                ToggleDoor();
            }
            else
            {
                Debug.Log("Cửa đang bị khóa!");
            }
        }
        else
        {
            ToggleDoor();
        }
    }

    private bool HasCorrectKey(PlayerInventory playerInventory)
    {
        Item heldItem = playerInventory.GetCurrentlyHeldItem();
        if (heldItem == null) return false;

        // Đọc trực tiếp keyID trên Item
        Padlock padlock = GetComponentInChildren<Padlock>();
        string requiredID = padlock != null ? padlock.requiredKeyID : "";

        if (!string.IsNullOrEmpty(heldItem.keyID) && heldItem.keyID == requiredID)
        {
            return true;
        }

        return false;
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Kích hoạt Animation mở/đóng thông qua Animator Parameter
        if (doorAnimator != null)
        {
            doorAnimator.SetBool(boolParameterName, isOpen);
        }
    }

    public void SetLocked(bool lockedState)
    {
        isLocked = lockedState;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
}