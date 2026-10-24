
using UnityEngine;
using System.Collections;

public class FlickeringLight : MonoBehaviour
{
    public Transform monster;
    public float detectionRange = 8f;

    public Color dangerColor = new Color32(224, 82, 53, 255);

    public float minBrightness = 0f;
    public float maxBrightness = 2f;

    public float minDelay = 0.03f;
    public float maxDelay = 0.2f;

    public AudioClip flickerSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    private Light lightSource;
    private float normalBrightness;
    private Color normalColor;

    private Renderer lampRenderer;
    private Material lampMaterial;
    private Color normalEmission;
    private bool hasEmission;

    private AudioSource audioSource;
    private Coroutine flickerRoutine;
    private bool monsterNearby;

    void Start()
    {
        lightSource = GetComponent<Light>();

        if (lightSource != null)
        {
            normalBrightness = lightSource.intensity;
            normalColor = lightSource.color;
        }

        lampRenderer = GetComponentInChildren<Renderer>();

        if (lampRenderer != null)
        {
            lampMaterial = lampRenderer.material;
            hasEmission = lampMaterial.HasProperty("_EmissionColor");

            if (hasEmission)
                normalEmission = lampMaterial.GetColor("_EmissionColor");
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.clip = flickerSound;
        audioSource.volume = soundVolume;
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        if (monster == null || lightSource == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            monster.position
        );

        bool isNearby = distance <= detectionRange;

        if (isNearby && !monsterNearby)
        {
            monsterNearby = true;

            lightSource.color = dangerColor;

            if (hasEmission)
                lampMaterial.SetColor("_EmissionColor", dangerColor);

            if (flickerSound != null)
                audioSource.Play();

            flickerRoutine = StartCoroutine(Flicker());
        }
        else if (!isNearby && monsterNearby)
        {
            monsterNearby = false;

            if (flickerRoutine != null)
            {
                StopCoroutine(flickerRoutine);
                flickerRoutine = null;
            }

            audioSource.Stop();
            RestoreLight();
        }
    }

    IEnumerator Flicker()
    {
        while (true)
        {
            lightSource.intensity = Random.Range(
                minBrightness,
                maxBrightness
            );

            yield return new WaitForSeconds(
                Random.Range(minDelay, maxDelay)
            );
        }
    }

    void RestoreLight()
    {
        lightSource.intensity = normalBrightness;
        lightSource.color = normalColor;

        if (hasEmission)
            lampMaterial.SetColor("_EmissionColor", normalEmission);
    }
}
