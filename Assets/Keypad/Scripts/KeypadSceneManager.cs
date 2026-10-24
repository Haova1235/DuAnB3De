using UnityEngine;
using UnityEngine.SceneManagement;

public class KeypadSceneManager : MonoBehaviour
{
    [SerializeField] private string keypadSceneName = "KeypadSceneStandard";

    private void Update()
    {
        // Cho phép người chơi nhấn nút ESC để thoát khỏi Keypad bất cứ lúc nào
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseKeypadScene();
        }
    }

    public void CloseKeypadScene()
    {
        if (SceneManager.GetSceneByName(keypadSceneName).isLoaded)
        {
            SceneManager.UnloadSceneAsync(keypadSceneName);

            // Khóa lại con trỏ chuột cho góc nhìn thứ nhất (FPS)
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}