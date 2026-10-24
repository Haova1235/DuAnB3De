using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels & Buttons Container")]
    public GameObject menuNutPanel;       // Kéo đối tượng chứa các nút menu chính (Play, Setting, Quit) vào đây
    public GameObject settingsPanel;      // Kéo SettingsPanel vào đây
    public GameObject creditsPanel;       // Kéo CreditsPanel vào đây

    [Header("Credits Scroll")]
    public ScrollRect creditsScrollRect;  // Kéo ScrollView (đối tượng Cuon) vào đây

    [Header("Settings Sliders & Overlay")]
    public Slider volumeSlider;
    public Slider brightnessSlider;
    public Image brightnessOverlay;

    [Header("Menu Buttons")]
    public Button quitButton;

    void Start()
    {
        LoadSettings();

        // Mặc định khi vào game: Hiện menu chính, ẩn các bảng phụ
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (menuNutPanel != null) menuNutPanel.SetActive(true);

        if (volumeSlider != null) volumeSlider.onValueChanged.AddListener(SetVolume);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(SetBrightness);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (menuNutPanel != null) menuNutPanel.SetActive(false);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (menuNutPanel != null) menuNutPanel.SetActive(true);
        SaveSettings();
    }

    // --- QUẢN LÝ BẢNG CREDITS (Tích hợp chuẩn) ---
    public void OpenCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(true);

            // Ẩn các nút menu chính đi để không bị đè lên bảng Credits
            if (menuNutPanel != null) menuNutPanel.SetActive(false);

            // Đưa thanh cuộn về đỉnh trên cùng mỗi khi mở
            if (creditsScrollRect != null)
            {
                creditsScrollRect.verticalNormalizedPosition = 1f;
            }
        }
    }

    public void CloseCredits()
    {
        if (creditsPanel != null)
        {
            creditsPanel.SetActive(false);

            // Hiện lại các nút menu chính khi bấm quay lại
            if (menuNutPanel != null) menuNutPanel.SetActive(true);
        }
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

    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}