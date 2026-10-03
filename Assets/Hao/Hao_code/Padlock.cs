using UnityEngine;

public class Padlock : MonoBehaviour, IOutlineable
{
    [Header("Lock Settings")]
    [Tooltip("Mã chìa khóa cần thiết để mở ổ khóa này (ví dụ: Key_MainDoor)")]
    public string requiredKeyID = "Key_MainDoor";

    [Header("Connected Door")]
    [Tooltip("Kéo GameObject Cánh cửa chứa script Door vào đây")]
    public Door linkedDoor;

    [Header("Effects (Optional)")]
    [SerializeField] private Rigidbody padlockRigidbody; // Nếu muốn ổ khóa rớt xuống sàn khi mở
    private ObjectOutline outlineTarget;

    private void Awake()
    {
        outlineTarget = GetComponent<ObjectOutline>();
    }

    /// <summary>
    /// Gọi khi người chơi nhìn vào Ổ khóa và bấm E
    /// </summary>
    public void Unlock(PlayerInventory playerInventory)
    {
        if (playerInventory == null) return;

        // Lấy Item người chơi đang cầm trên tay
        Item heldItem = playerInventory.GetCurrentlyHeldItem();

        if (heldItem != null)
        {
            // Kiểm tra trực tiếp biến keyID từ script Item.cs
            if (!string.IsNullOrEmpty(heldItem.keyID) && heldItem.keyID == requiredKeyID)
            {
                Debug.Log("Mở ổ khóa thành công!");

                // 1. Mở khóa cánh cửa liên kết
                if (linkedDoor != null)
                {
                    linkedDoor.SetLocked(false);
                }

                // 2. Thả và hủy chìa khóa trên tay sau khi sử dụng
                playerInventory.DropItem();
                Destroy(heldItem.gameObject);

                // 3. Xử lý hiệu ứng rớt/hủy ổ khóa
                UnlockEffects();
            }
            else
            {
                Debug.Log("Sai chìa khóa! Ổ khóa này yêu cầu chìa khóa có ID: " + requiredKeyID);
            }
        }
        else
        {
            Debug.Log("Bạn cần cầm chìa khóa trên tay mới mở được ổ khóa này!");
        }
    }

    private void UnlockEffects()
    {
        HideOutline();

        if (padlockRigidbody != null)
        {
            transform.SetParent(null); // Tách khỏi cánh cửa
            padlockRigidbody.isKinematic = false; // Bật vật lý để rơi xuống đất
            Destroy(gameObject, 3f); // Xóa ổ khóa sau 3 giây
        }
        else
        {
            Destroy(gameObject); // Biến mất ngay lập tức
        }
    }

    public void ShowOutline()
    {
        if (outlineTarget != null) outlineTarget.ShowOutline();
    }

    public void HideOutline()
    {
        if (outlineTarget != null) outlineTarget.HideOutline();
    }
}