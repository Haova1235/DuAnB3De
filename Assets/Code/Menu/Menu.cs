using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;      // Kéo SettingsPanel Prefab vào đây

    [Header("Settings Sliders & Overlay")]
    public Slider volumeSlider;           // Kéo VolumeSlider vào đây
    public Slider brightnessSlider;       // Kéo BrightnessSlider vào đây
    public Image brightnessOverlay;       // Kéo Panel đen chỉnh độ sáng vào đây

    void Start()
    {
        // Tải lại các cài đặt đã lưu trước đó để đồng bộ
        LoadSettings();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }
    }

    public void PlayGame()
    {
        // Tải scene game tiếp theo (Scene chơi game)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        SaveSettings(); // Lưu lại thiết lập khi đóng bảng
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

    public void QuitGame()
    {
        Debug.Log("Đã thoát game!");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}