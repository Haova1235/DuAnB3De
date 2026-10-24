using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pausePanel;         // Kéo PausePanel vào đây
    public GameObject settingsPanel;      // Kéo SettingPanel vào đây
    public GameObject mapFullBackground; // Tấm hình nền full màn hình che map

    [Header("In-Game HUD Buttons")]
    public GameObject pauseButton;       // Kéo nút mở Menu tạm dừng trên màn hình (ví dụ SettingButton)

    [Header("Settings Components")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Image brightnessOverlay;

    [Header("Pause Panel Buttons")]
    public Button resumeButton;          // Kéo nút Resume
    public Button openSettingsButton;    // Kéo nút mở Setting từ PausePanel
    public Button closeSettingsButton;   // Kéo nút đóng Setting (nút Back/Close)
    public Button menuButton;            // Kéo nút Về Menu
    public Button quitButton;            // Kéo nút Thoát Game

    [Header("Scene Settings")]
    public string menuSceneName = "Menu"; // Tên Scene Menu chính

    private bool isPaused = false;

    void Start()
    {
        LoadSettings();

        // Mặc định lúc vào map: Hiện nút trên màn hình, ẩn các bảng menu và hình nền
        if (pauseButton != null) pauseButton.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapFullBackground != null) mapFullBackground.SetActive(false);

        Time.timeScale = 1f;

        // Đăng ký sự kiện cho Slider
        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetVolume);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(SetBrightness);

        // Đăng ký sự kiện cho các nút bấm
        if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(OpenSettings);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (menuButton != null) menuButton.onClick.AddListener(() => ReturnToMainMenu(menuSceneName));
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    void Update()
    {
        // Bấm ESC để bật/tắt bảng Tạm Dừng
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                if (settingsPanel != null && settingsPanel.activeSelf)
                {
                    CloseSettings();
                }
                else
                {
                    ResumeGame();
                }
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
        if (pauseButton != null) pauseButton.SetActive(false); // Ẩn nút tạm dừng trên màn hình
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Time.timeScale = 0f; // Dừng thời gian game
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mapFullBackground != null) mapFullBackground.SetActive(false); // Tắt hình nền che map
        if (pauseButton != null) pauseButton.SetActive(true); // Hiện lại nút tạm dừng trên màn hình

        Time.timeScale = 1f; // Tiếp tục game
        SaveSettings();
    }

    public void OpenSettings()
    {
        if (pausePanel != null) pausePanel.SetActive(false);     // Tạm ẩn bảng Pause
        if (settingsPanel != null) settingsPanel.SetActive(true); // Hiển thị bảng Setting
        if (mapFullBackground != null) mapFullBackground.SetActive(true); // Bật hình nền che map
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false); // Tắt bảng Setting
        if (mapFullBackground != null) mapFullBackground.SetActive(false); // Tắt hình nền che map (đã sửa thành false)

        // Nếu game đang trong trạng thái dừng (pause), hiện lại bảng Pause
        if (isPaused && pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        SaveSettings();
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
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
        if (volumeSlider != null) PlayerPrefs.SetFloat("MasterVolume", volumeSlider.value);
        if (brightnessSlider != null) PlayerPrefs.SetFloat("Brightness", brightnessSlider.value);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        float savedVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        AudioListener.volume = savedVolume;
        if (volumeSlider != null) volumeSlider.value = savedVolume;

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
        Time.timeScale = 1f;
        Debug.Log("Đang thoát game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}