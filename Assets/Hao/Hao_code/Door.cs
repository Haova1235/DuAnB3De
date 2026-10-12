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

        // Tự động tìm tất cả các khóa là con của cửa nếu danh sách trống
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
                return true; // Vẫn còn ít nhất 1 khóa chưa mở
            }
        }
        return false; // Đã mở sạch mọi khóa
    }

    /// <summary>
    /// Gọi khi Player bấm E trực tiếp vào Cánh Cửa
    /// </summary>
    public void Interact(PlayerInventory playerInventory)
    {
        // 1. Nếu cửa vẫn còn khóa
        if (CheckIsLocked())
        {
            bool openedAnyLock = false;

            // Thử dùng item đang cầm trên tay để mở các khóa trên cửa
            if (playerInventory != null)
            {
                foreach (LockBase lockItem in locksOnDoor)
                {
                    if (lockItem != null && lockItem.IsLocked)
                    {
                        // Thử mở khóa này bằng item trên tay
                        if (lockItem.TryOpenWithItem(playerInventory))
                        {
                            openedAnyLock = true;
                            break; // Mở xong 1 khóa trong lượt bấm E này
                        }
                    }
                }
            }

            if (openedAnyLock)
            {
                Debug.Log("Đã mở được một ổ khóa trên cửa!");
                // Nếu sau khi mở mà cửa đã sạch hết khóa thì tự động mở cửa luôn
                if (!CheckIsLocked())
                {
                    ToggleDoor();
                }
            }
            else
            {
                Debug.Log("Cửa đang bị khóa và bạn không có chìa khóa phù hợp cho các ổ khóa còn lại!");
            }
        }
        // 2. Nếu cửa đã sạch khóa hoàn toàn
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