using System.Collections;
using UnityEngine;

// Một "ô âm thanh" có thể kéo clip vào Inspector
[System.Serializable]
public class SoundEffect
{
    [Tooltip("Có thể kéo nhiều clip, mỗi lần phát sẽ chọn ngẫu nhiên 1 clip")]
    public AudioClip[] clips;

    [Header("Cơ bản")]
    [Range(0, 256)] public int priority = 128;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    [Tooltip("Tick: mỗi lần phát lấy pitch ngẫu nhiên trong Pitch Range. Bỏ tick: dùng đúng Pitch ở trên")]
    public bool randomizePitch = false;
    public Vector2 pitchRange = new Vector2(0.95f, 1.05f);
    [Range(-1f, 1f)] public float stereoPan = 0f;

    [Header("Phát lặp & Dừng")]
    [Tooltip("Tick: clip lặp lại liên tục cho tới khi gọi Stop() (hợp với tiếng bước chân dài)")]
    public bool loop = false;
    [Tooltip("Thời gian giảm dần âm lượng khi dừng. 0 = tắt ngay lập tức")]
    public float stopFadeTime = 0.08f;

    [Header("Không gian")]
    [Tooltip("Bỏ tick: tự động (Play2D = 2D, PlayAt = 3D). Tick: dùng giá trị Spatial Blend bên dưới")]
    public bool overrideSpatialBlend = false;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    [Range(0f, 1.1f)] public float reverbZoneMix = 1f;

    [Header("3D Sound Settings")]
    [Range(0f, 5f)] public float dopplerLevel = 1f;
    [Range(0f, 360f)] public float spread = 0f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Linear;
    public float minDistance = 1f;
    public float maxDistance = 25f;
    [Tooltip("Chỉ dùng khi Rolloff Mode = Custom")]
    public AnimationCurve customRolloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    // Nguồn đang phát tiếng này (không hiện trong Inspector)
    private AudioSource activeSource;
    private AudioClip activeClip;

    public bool HasClip => clips != null && clips.Length > 0;

    public AudioClip GetRandomClip()
    {
        if (!HasClip) return null;
        return clips[Random.Range(0, clips.Length)];
    }

    public float GetPitch()
    {
        return randomizePitch ? Random.Range(pitchRange.x, pitchRange.y) : pitch;
    }

    // Phát âm thanh 2D (tiếng của chính Player, UI)
    public void Play2D()
    {
        if (!HasClip) return;
        activeSource = AudioManager.Instance.Play2D(this);
        activeClip = activeSource != null ? activeSource.clip : null;
    }

    // Phát âm thanh 3D tại vị trí (cửa, quái, ổ khóa)
    public void PlayAt(Vector3 position)
    {
        if (!HasClip) return;
        activeSource = AudioManager.Instance.Play3D(this, position);
        activeClip = activeSource != null ? activeSource.clip : null;
    }

    // Còn đang phát đúng clip này không (nguồn trong pool có thể đã bị dùng cho tiếng khác)
    public bool IsPlaying =>
        activeSource != null && activeSource.isPlaying && activeSource.clip == activeClip;

    public void Stop()
    {
        if (IsPlaying) AudioManager.Instance.StopSource(activeSource, activeClip, stopFadeTime);
        activeSource = null;
        activeClip = null;
    }
}

// Chỉ quản lý hiệu ứng âm thanh (SFX). Nhạc nền do BackgroundMusic lo.
public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AudioManager>();
                if (instance == null)
                {
                    instance = new GameObject("AudioManager").AddComponent<AudioManager>();
                }
            }
            return instance;
        }
    }

    [Header("Hiệu ứng âm thanh (SFX)")]
    [Range(0f, 1f)][SerializeField] private float sfxVolume = 1f;
    [SerializeField] private int poolSize = 12;
    [Tooltip("Tùy chọn: gán nhóm Audio Mixer cho toàn bộ SFX")]
    [SerializeField] private UnityEngine.Audio.AudioMixerGroup sfxMixerGroup;

    private AudioSource[] sfxPool;
    private int nextPoolIndex = 0;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        sfxPool = new AudioSource[Mathf.Max(1, poolSize)];
        for (int i = 0; i < sfxPool.Length; i++)
        {
            GameObject child = new GameObject("SFX_" + i);
            child.transform.SetParent(transform);
            AudioSource src = child.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.outputAudioMixerGroup = sfxMixerGroup;
            sfxPool[i] = src;
        }
    }

    public AudioSource Play2D(SoundEffect sfx)
    {
        return PlayInternal(sfx, Vector3.zero, false);
    }

    public AudioSource Play3D(SoundEffect sfx, Vector3 position)
    {
        return PlayInternal(sfx, position, true);
    }

    private AudioSource PlayInternal(SoundEffect sfx, Vector3 position, bool is3D)
    {
        AudioClip clip = sfx.GetRandomClip();
        if (clip == null) return null;

        AudioSource src = GetFreeSource();

        // Âm thanh 2D đặt nguồn tại tai người nghe để Spatial Blend > 0 vẫn nghe đúng
        if (!is3D)
        {
            Camera cam = Camera.main;
            position = cam != null ? cam.transform.position : Vector3.zero;
        }

        src.Stop();
        src.transform.position = position;
        src.clip = clip;

        // Nguồn trong pool được dùng lại nên phải gán đủ mọi thông số mỗi lần phát
        src.priority = sfx.priority;
        src.loop = sfx.loop;
        src.volume = sfx.volume * sfxVolume;
        src.pitch = sfx.GetPitch();
        src.panStereo = sfx.stereoPan;
        src.spatialBlend = sfx.overrideSpatialBlend ? sfx.spatialBlend : (is3D ? 1f : 0f);
        src.reverbZoneMix = sfx.reverbZoneMix;
        src.dopplerLevel = sfx.dopplerLevel;
        src.spread = sfx.spread;
        src.minDistance = sfx.minDistance;
        src.maxDistance = sfx.maxDistance;
        src.rolloffMode = sfx.rolloffMode;
        if (sfx.rolloffMode == AudioRolloffMode.Custom)
        {
            src.SetCustomCurve(AudioSourceCurveType.CustomRolloff, sfx.customRolloff);
        }

        src.Play();
        return src;
    }

    private AudioSource GetFreeSource()
    {
        for (int i = 0; i < sfxPool.Length; i++)
        {
            if (!sfxPool[i].isPlaying) return sfxPool[i];
        }
        // Hết nguồn rảnh thì dùng lại nguồn cũ nhất
        AudioSource src = sfxPool[nextPoolIndex];
        nextPoolIndex = (nextPoolIndex + 1) % sfxPool.Length;
        return src;
    }

    public void StopSource(AudioSource src, AudioClip expectedClip, float fadeTime)
    {
        if (src == null || !src.isPlaying || src.clip != expectedClip) return;

        if (fadeTime <= 0f)
        {
            src.Stop();
            return;
        }
        StartCoroutine(FadeOutRoutine(src, expectedClip, fadeTime));
    }

    private IEnumerator FadeOutRoutine(AudioSource src, AudioClip expectedClip, float fadeTime)
    {
        float startVol = src.volume;
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            // Nếu nguồn đã bị dùng cho tiếng khác thì thôi, không đụng vào
            if (src.clip != expectedClip || !src.isPlaying) yield break;
            src.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }
        if (src.clip == expectedClip) src.Stop();
    }
}