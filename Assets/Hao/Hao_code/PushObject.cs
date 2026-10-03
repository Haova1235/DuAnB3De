using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PushObject : MonoBehaviour
{
    [Header("Push Settings")]
    [Tooltip("Lực đẩy áp dụng lên vật thể")]
    [SerializeField] private float pushPower = 2.0f;

    [Tooltip("Chỉ đẩy các vật thể nằm trên Layer này (để trống nếu muốn đẩy tất cả)")]
    [SerializeField] private LayerMask pushableLayers;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;

        // Kiểm tra xem vật thể có Rigidbody không hoặc có bị khóa chuyển động (isKinematic) không
        if (body == null || body.isKinematic)
        {
            return;
        }

        // Kiểm tra xem có thuộc Layer được phép đẩy hay không
        if (pushableLayers != (pushableLayers | (1 << hit.gameObject.layer)))
        {
            return;
        }

        // Tránh đẩy vật thể khi nhân vật đang nhảy đè lên đỉnh vật thể (chỉ đẩy theo phương ngang)
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // Tính toán hướng lực đẩy dựa trên hướng di chuyển của nhân vật
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // Áp dụng lực vào vị trí va chạm (ForceMode.Impulse cho cảm giác tác động tức thì)
        body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
    }
}
