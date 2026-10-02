using UnityEngine;
using UnityEngine.SceneManagement; // Thư viện quản lý Scene

public class MainMenuController : MonoBehaviour
{
    // Hàm gọi khi bấm nút Play
    public void PlayGame()
    {
        // Tải Scene tiếp theo trong Build Settings (thường là Scene game chính)
        // Hoặc bạn có thể điền trực tiếp tên scene: SceneManager.LoadScene("GameScene");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Hàm gọi khi bấm nút Options
    public void OpenOptions()
    {
        Debug.Log("Mở bảng cài đặt...");
        // Viết code mở panel options ở đây nếu có
    }

    // Hàm gọi khi bấm nút Quit
    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit(); // Lệnh này chỉ hoạt động khi xuất bản ra file .exe/.apk

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dừng playmode trong Unity Editor để test
#endif
    }
}