using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventory : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private float interactDistance = 15f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Transform holdPoint; // Vị trí điểm gắn đồ trên tay

    [Header("Drop Settings")]
    [SerializeField] private float dropForwardForce = 2f;

    private Item currentlyHeldItem;

    private void Update()
    {
        // Nhấn phím E để tương tác (Mở ổ khóa, Mở cửa, Nhặt đồ)
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteractOrPickup();
        }

        // Nhấn phím G để thả đồ đang cầm
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            DropItem();
        }
    }

    private void TryInteractOrPickup()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            // 1. Kiểm tra Ổ khóa (LockBase)
            LockBase lockObj = hit.collider.GetComponentInParent<LockBase>();
            if (lockObj != null)
            {
                lockObj.Interact(this);
                return;
            }

            // 2. Kiểm tra Cánh Cửa (Tìm linh hoạt hơn ở cả cha và con)
            Door door = hit.collider.GetComponent<Door>();
            if (door == null) door = hit.collider.GetComponentInParent<Door>();
            if (door == null) door = hit.collider.GetComponentInChildren<Door>();

            if (door != null)
            {
                door.Interact(this);
                return;
            }

            // 3. Kiểm tra Vật phẩm (Item)
            Item item = hit.collider.GetComponentInParent<Item>();
            if (item != null)
            {
                if (currentlyHeldItem != null) DropItem();

                currentlyHeldItem = item;
                currentlyHeldItem.Pickup(holdPoint);
            }
        }
    }

    public Item GetCurrentlyHeldItem()
    {
        return currentlyHeldItem;
    }

    public void DropItem()
    {
        if (currentlyHeldItem != null)
        {
            Camera mainCam = Camera.main;
            Vector3 throwDirection = (mainCam != null ? mainCam.transform.forward : transform.forward) * dropForwardForce + Vector3.up * 1f;

            currentlyHeldItem.Drop(throwDirection);
            currentlyHeldItem = null;
        }
    }
}