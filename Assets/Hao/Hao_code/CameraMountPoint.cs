using UnityEngine;

public class CameraMountPoint : MonoBehaviour, IOutlineable
{
    [Header("Settings")]
    public bool isOccupied = false;
    [Tooltip("Thời gian giữ phím F để đặt camera xuống (giây)")]
    public float placeHoldTime = 1.0f;

    private ObjectOutline outlineTarget;

    private void Awake()
    {
        outlineTarget = GetComponent<ObjectOutline>();
    }

    /// <summary>
    /// Được gọi từ PlayerInventory khi người chơi giữ đủ thời gian vào điểm đặt
    /// </summary>
    public bool InteractWithMount(PlayerInventory playerInventory)
    {
        if (isOccupied)
        {
            Debug.Log("Điểm này đã có camera rồi!");
            return false;
        }

        Item heldItem = playerInventory.GetCurrentlyHeldItem();
        if (heldItem != null)
        {
            PlaceableCamera camItem = heldItem as PlaceableCamera;
            if (camItem != null)
            {
                camItem.PlaceDown(transform);
                playerInventory.ClearHeldItem();
                isOccupied = true;
                HideOutline();
                Debug.Log("Đã đặt camera thành công qua việc giữ phím!");
                return true;
            }
            else
            {
                Debug.Log("Vật phẩm trên tay không phải là Camera!");
            }
        }
        else
        {
            Debug.Log("Bạn cần cầm Camera trên tay để đặt vào đây!");
        }
        return false;
    }

    public void ShowOutline()
    {
        if (!isOccupied && outlineTarget != null) outlineTarget.ShowOutline();
    }

    public void HideOutline()
    {
        if (outlineTarget != null) outlineTarget.HideOutline();
    }
}