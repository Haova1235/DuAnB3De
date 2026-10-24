using UnityEngine;
using UnityEngine.SceneManagement;

public class KeypadTrigger : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject puzzleCanvasText; // Kéo Text [E] trong PuzzleCanvas vào đây

    [Header("Scene Settings")]
    [SerializeField] private string keypadSceneName = "KeypadSceneStandard";

    [Header("Puzzle State")]
    public bool isUnlocked = false; // Trạng thái đã mở khóa thành công

    private bool isPlayerInside = false;

    private void Start()
    {
        if (puzzleCanvasText != null) puzzleCanvasText.SetActive(false);
    }

    private void Update()
    {
        // Nếu đã mở khóa thành công thì chặn hoàn toàn
        if (isUnlocked) return;

        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            OpenKeypadScene();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isUnlocked) return; // Đã mở rồi thì không hiện chữ [E] nữa

        if (other.CompareTag("Player") || other.gameObject.name == "nguoi chs")
        {
            isPlayerInside = true;
            if (puzzleCanvasText != null) puzzleCanvasText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name == "nguoi chs")
        {
            isPlayerInside = false;
            if (puzzleCanvasText != null) puzzleCanvasText.SetActive(false);
        }
    }

    public void OpenKeypadScene()
    {
        if (isUnlocked) return;

        if (!SceneManager.GetSceneByName(keypadSceneName).isLoaded)
        {
            if (puzzleCanvasText != null) puzzleCanvasText.SetActive(false);
            SceneManager.LoadScene(keypadSceneName, LoadSceneMode.Additive);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Hàm khóa vĩnh viễn tương tác và ẩn chữ [E]
    public void SetUnlocked()
    {
        isUnlocked = true;
        if (puzzleCanvasText != null)
        {
            puzzleCanvasText.SetActive(false);
        }
    }
}