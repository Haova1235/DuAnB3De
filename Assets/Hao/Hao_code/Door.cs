using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Multi-Lock System")]
    [Tooltip("Kéo tất cả các ổ khóa gắn trên cửa này vào đây (để trống code sẽ tự tìm)")]
    [SerializeField] private List<LockBase> locksOnDoor = new List<LockBase>();

    [Header("Animator Settings")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private string boolParameterName = "isOpen";

    private bool isOpen = false;

    private void Awake()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();

        if (locksOnDoor.Count == 0)
        {
            locksOnDoor.AddRange(GetComponentsInChildren<LockBase>());
        }
    }

    public bool CheckIsLocked()
    {
        foreach (LockBase lockItem in locksOnDoor)
        {
            if (lockItem != null && lockItem.IsLocked)
            {
                return true; // Vẫn còn ổ khóa chưa mở
            }
        }
        return false; // Đã sạch mọi ổ khóa
    }

    /// <summary>
    /// Hàm chính khi Player nhấn/giữ phím F vào Cánh Cửa
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        // 1. NẾU CỬA CÒN KHÓA
        if (CheckIsLocked())
        {
            bool openedAnyLock = false;

            if (playerInventory != null)
            {
                foreach (LockBase lockItem in locksOnDoor)
                {
                    if (lockItem != null && lockItem.IsLocked)
                    {
                        if (lockItem.TryOpenWithItem(playerInventory))
                        {
                            openedAnyLock = true;
                            break;
                        }
                    }
                }
            }

            if (openedAnyLock)
            {
                Debug.Log("Đã mở được 1 ổ khóa trên cửa!");

                // Nếu đây là ổ khóa cuối cùng được mở -> Tự động xoay mở cửa luôn!
                if (!CheckIsLocked())
                {
                    ToggleDoor();
                }
            }
            else
            {
                Debug.Log("Cửa đang bị khóa và bạn không có chìa khóa phù hợp!");
            }
        }
        // 2. NẾU CỬA ĐÃ SẠCH KHÓA (HOẶC CỬA KHÔNG CÓ KHÓA)
        else
        {
            ToggleDoor();
        }
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (doorAnimator != null)
        {
            doorAnimator.SetBool(boolParameterName, isOpen);
        }
    }
}