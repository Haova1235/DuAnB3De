using UnityEngine;

public class Padlock : LockBase
{
    [Header("Padlock Settings")]
    public string requiredKeyID = "key1";

    [SerializeField] private Rigidbody padlockRigidbody;
    private ObjectOutline outlineTarget;

    private void Awake()
    {
        outlineTarget = GetComponent<ObjectOutline>();
    }

    public override void Interact(PlayerInventory playerInventory)
    {
        // Có thể để trống hoặc giữ lại nếu muốn bấm riêng vào ổ khóa
    }

    // Cửa sẽ gọi hàm này để tự động check chìa trên tay người chơi
    public override bool TryOpenWithItem(PlayerInventory playerInventory)
    {
        if (!isLocked || playerInventory == null) return false;

        Item heldItem = playerInventory.GetCurrentlyHeldItem();

        if (heldItem != null && !string.IsNullOrEmpty(heldItem.keyID) && heldItem.keyID == requiredKeyID)
        {
            Debug.Log("Mở khóa Padlock thành công bằng chìa: " + requiredKeyID);
            isLocked = false;

            // Xóa chìa khóa trên tay
            playerInventory.DropItem();
            Destroy(heldItem.gameObject);

            UnlockEffects();
            return true; // Đã mở thành công khóa này
        }

        return false; // Không đúng chìa hoặc tay không có item phù hợp
    }

    private void UnlockEffects()
    {
        HideOutline();
        if (padlockRigidbody != null)
        {
            transform.SetParent(null);
            padlockRigidbody.isKinematic = false;
            Destroy(gameObject, 3f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public override void ShowOutline()
    {
        if (isLocked && outlineTarget != null) outlineTarget.ShowOutline();
    }

    public override void HideOutline()
    {
        if (outlineTarget != null) outlineTarget.HideOutline();
    }
}