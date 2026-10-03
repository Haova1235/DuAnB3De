using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour, IOutlineable
{
    [Header("Item Info")]
    public string itemName = "Key";
    public string keyID;

    private Rigidbody rb;
    private Collider itemCollider;
    // Thay OutlineTarget thành ObjectOutline
    private ObjectOutline outlineTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();

        // Sửa chỗ này luôn
        outlineTarget = GetComponent<ObjectOutline>();
    }

    // Gọi khi nhặt đồ lên tay
    public void Pickup(Transform holdPoint)
    {
        // Tắt physic để item không bị rơi/va chạm khi đang cầm
        rb.isKinematic = true;
        itemCollider.enabled = false;

        // Tắt viền Outline nếu đang bật
        HideOutline();

        // Di chuyển item về vị trí trên tay
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // Gọi khi thả đồ xuống đất
    public void Drop(Vector3 dropForce)
    {
        // Bật lại parent và physic
        transform.SetParent(null);
        rb.isKinematic = false;
        itemCollider.enabled = true;

        // Đẩy nhẹ item về phía trước khi thả
        rb.AddForce(dropForce, ForceMode.Impulse);
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