using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Nhạc nền")]
    [Tooltip("Để trống thì dùng clip đã gán sẵn trong AudioSource")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float fadeTime = 1.5f;

    private AudioSource source;
    private float targetVolume;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        source = GetComponent<AudioSource>();
        source.playOnAwake = false; // script tự điều khiển lúc phát

        // Volume bạn chỉnh trên AudioSource chính là âm lượng đích của nhạc
        targetVolume = source.volume;
    }

    private void Start()
    {
        if (!playOnStart) return;

        AudioClip clip = musicClip != null ? musicClip : source.clip;
        if (clip != null) Play(clip, fadeTime);
    }

    // Phát hoặc đổi sang nhạc khác (fade < 0: dùng fadeTime mặc định)
    public void Play(AudioClip clip, float fade = -1f)
    {
        if (clip == null) return;
        if (source.clip == clip && source.isPlaying) return;
        StartFade(clip, fade);
    }

    public void Stop(float fade = -1f)
    {
        StartFade(null, fade);
    }

    public void Pause() => source.Pause();
    public void Resume() => source.UnPause();

    // Đổi âm lượng đích (đồng thời áp dụng ngay nếu đang phát)
    public void SetVolume(float value)
    {
        targetVolume = Mathf.Clamp01(value);
        if (fadeRoutine == null) source.volume = targetVolume;
    }

    private void StartFade(AudioClip clip, float fade)
    {
        if (fade < 0f) fade = fadeTime;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(clip, fade));
    }

    // Dùng unscaledDeltaTime để vẫn mượt khi Time.timeScale = 0
    private IEnumerator FadeRoutine(AudioClip newClip, float fade)
    {
        // Giảm dần nhạc cũ
        if (source.isPlaying && fade > 0f)
        {
            float startVol = source.volume;
            for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
            {
                source.volume = Mathf.Lerp(startVol, 0f, t / fade);
                yield return null;
            }
        }
        source.Stop();

        if (newClip == null)
        {
            source.volume = targetVolume;
            fadeRoutine = null;
            yield break;
        }

        // Tăng dần nhạc mới
        source.clip = newClip;
        source.volume = fade > 0f ? 0f : targetVolume;
        source.Play();

        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            source.volume = Mathf.Lerp(0f, targetVolume, t / fade);
            yield return null;
        }
        source.volume = targetVolume;
        fadeRoutine = null;
    }
}