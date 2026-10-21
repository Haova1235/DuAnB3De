using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour, IOutlineable
{
    [Header("Item Info")]
    public string itemName = "Key";
    public string keyID;

    [Header("Throw Settings")]
    [Tooltip("Lực ném/thả của riêng item này (vd: chìa khóa 2, cục đá 10)")]
    public float throwForce = 5f;

    private Rigidbody rb;
    private Collider itemCollider;
    private ObjectOutline outlineTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        itemCollider = GetComponent<Collider>();
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

    // Gọi khi thả/ném đồ xuống đất
    public void Drop(Vector3 direction)
    {
        // Bật lại parent và physic
        transform.SetParent(null);
        rb.isKinematic = false;
        itemCollider.enabled = true;

        // Đẩy item đi dựa vào hướng và nhân với chỉ số lực ném riêng của item đó
        if (rb != null)
        {
            rb.AddForce(direction * throwForce, ForceMode.Impulse);
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