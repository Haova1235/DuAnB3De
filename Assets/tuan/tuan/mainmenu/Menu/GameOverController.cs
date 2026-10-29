using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    public void RestartGame()
    {
        // 1. Phục hồi lại thời gian về bình thường (phòng khi game bị pause)
        Time.timeScale = 1f;

        // 2. Khóa và ẩn con trỏ chuột để đưa về trạng thái chơi game
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 3. Tải lại Scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        // Phục hồi thời gian trước khi chuyển về Menu chính
        Time.timeScale = 1f;

        // Hiện chuột ở menu chính
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Chuyển về Scene Main Menu (nhớ đổi tên "MainMenu" thành tên scene của bạn nếu khác)
        SceneManager.LoadScene("MainMenu");
    }
}