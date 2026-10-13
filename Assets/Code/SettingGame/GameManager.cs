using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Sử dụng hệ thống Input mới

public class GameManager : MonoBehaviour
{
    [Header("UI Panels & Overlays")]
    public GameObject pauseSettingsPanel;  // Kéo SettingsPanel vào đây
    public Image brightnessOverlay;          // Kéo BrightnessOverlay vào đây (nếu có)

    [Header("Settings Sliders")]
    public Slider volumeSlider;              // Kéo VolumeSlider vào đây
    public Slider brightnessSlider;          // Kéo BrightnessSlider vào đây (nếu có)

    [Header("Prefab Buttons & Scene Management")]
    public Button menuButton;                // Kéo nút Về Menu từ Prefab vào đây
    public Button quitButton;                // Kéo nút Thoát Game từ Prefab vào đây
    public string menuSceneName = "Menu";    // Điền chính xác tên Scene Menu chính của bạn vào đây

    private bool isPaused = false;

    void Start()
    {
        LoadSettings();

        // Ép buộc ẩn bảng cài đặt ngay khi vào game
        if (pauseSettingsPanel != null)
        {
            pauseSettingsPanel.SetActive(false);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        // Tự động gán sự kiện cho nút Về Menu và Thoát Game
        if (menuButton != null)
        {
            menuButton.onClick.AddListener(() => ReturnToMainMenu(menuSceneName));
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void Update()
    {
        // Bấm phím ESC để bật/tắt bảng cài đặt bằng New Input System
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pauseSettingsPanel != null)
        {
            pauseSettingsPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Dừng game
        Cursor.visible = true; // Hiện con trỏ chuột
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pauseSettingsPanel != null)
        {
            pauseSettingsPanel.SetActive(false);
        }
        Time.timeScale = 1f; // Tiếp tục game
        SaveSettings();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void SetBrightness(float brightness)
    {
        if (brightnessOverlay != null)
        {
            Color color = brightnessOverlay.color;
            color.a = 1f - brightness;
            brightnessOverlay.color = color;
        }
    }

    void SaveSettings()
    {
        if (volumeSlider != null)
        {
            PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        }
        if (brightnessSlider != null)
        {
            PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        }
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;
        }

        float savedBrightness = PlayerPrefs.GetFloat("Brightness", 0.5f);
        if (brightnessSlider != null)
        {
            brightnessSlider.value = savedBrightness;
            SetBrightness(savedBrightness);
        }
    }

    public void ReturnToMainMenu(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f; // Bắt buộc đưa thời gian về bình thường trước khi thoát
        Debug.Log("Đang thoát game...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Dừng chế độ Play ngay lập tức khi test trong Unity Editor
#else
        Application.Quit(); // Thoát hẳn game khi đã build file .exe
#endif
    }
}