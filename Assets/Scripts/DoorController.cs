using UnityEngine;

public class DoorController : MonoBehaviour
{
    private Animator animator;
    private BoxCollider doorCollider;

    private void Start()
    {
        animator = GetComponent<Animator>();
        doorCollider = GetComponent<BoxCollider>();
    }

    public void OpenDoor()
    {
        if (animator != null)
        {
            animator.SetTrigger("Open");
            animator.SetBool("IsOpen", true);
        }

        // Tắt BoxCollider ngay khi cửa mở để player đi qua được
        if (doorCollider != null)
        {
            doorCollider.enabled = false;
        }
    }
}