using UnityEngine;
using UnityEngine.InputSystem;

public class KeypadInteraction : MonoBehaviour
{
    public float interactDistance = 3f;

    private Transform player;
    private Camera mainCamera;

    void Start()
    {
        // Tim Player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // Tim camera
        mainCamera = Camera.main;

        // An chuot khi bat dau game
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (player == null || mainCamera == null)
            return;

        float distance = Vector3.Distance(
            player.position,
            transform.position
        );

        // Neu Player o gan Keypad
        if (distance <= interactDistance)
        {
            // Bam E bang Input System moi
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                MoKhoaChuot();
            }
        }
    }

    void MoKhoaChuot()
    {
        // Hien chuot
        Cursor.visible = true;

        // Mo khoa chuot
        Cursor.lockState = CursorLockMode.None;
    }
}