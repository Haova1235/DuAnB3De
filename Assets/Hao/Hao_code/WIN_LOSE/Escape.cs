using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndGameZone : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Kéo Panel EndGame (object trong Scene, đang tắt) vào đây")]
    [SerializeField] private GameObject endGamePanel;

    [Header("Cài đặt")]
    [Tooltip("Đợi bao nhiêu giây (thời gian thật) rồi mới hiện panel, 0 = hiện ngay")]
    [SerializeField] private float showPanelDelay = 0f;
    [Tooltip("Tick: dừng toàn bộ game khi hiện panel")]
    [SerializeField] private bool pauseGameOnEnd = true;

    [Header("Âm thanh")]
    [SerializeField] private SoundEffect winSound;
    [Tooltip("Tick: tắt dần nhạc nền khi thắng")]
    [SerializeField] private bool stopMusicOnEnd = true;

    private bool hasEnded = false;

    private void Reset()
    {
        // Điểm đích phải là Trigger
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasEnded) return;
        if (!other.CompareTag("Player")) return;

        // Không cho thắng nếu Player đã bị quái hạ
        PlayerMove player = other.GetComponentInParent<PlayerMove>();
        if (player != null && player.IsDefeated) return;

        hasEnded = true;
        winSound.Play2D();
        if (stopMusicOnEnd) BackgroundMusic.Instance?.Stop(1f);

        // Khóa điều khiển Player
        if (player != null) player.enabled = false;

        StartCoroutine(EndRoutine());
    }

    private System.Collections.IEnumerator EndRoutine()
    {
        if (showPanelDelay > 0f)
            yield return new WaitForSecondsRealtime(showPanelDelay);

        if (endGamePanel != null) endGamePanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pauseGameOnEnd) Time.timeScale = 0f;
    }
}