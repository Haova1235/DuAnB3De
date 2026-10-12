using UnityEngine;

public abstract class LockBase : MonoBehaviour, IOutlineable
{
    [Header("Lock Status")]
    [SerializeField] protected bool isLocked = true;

    public bool IsLocked => isLocked;

    // Hàm mở khóa tương tác thông thường (nếu dùng UI hoặc click riêng)
    public abstract void Interact(PlayerInventory playerInventory);

    // HÀM MỚI: Cửa sẽ gọi hàm này khi người chơi bấm E vào cửa để thử mở các khóa
    public abstract bool TryOpenWithItem(PlayerInventory playerInventory);

    public virtual void ShowOutline() { }
    public virtual void HideOutline() { }
}