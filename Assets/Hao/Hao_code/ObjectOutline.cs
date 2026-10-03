using UnityEngine;

public class ObjectOutline : MonoBehaviour, IOutlineable
{
    [Header("Outline Material")]
    [SerializeField] private Material outlineMaterial; // Kéo Material Shader Outline vào đây

    private Renderer objectRenderer;
    private Material[] originalMaterials;
    private Material[] outlinedMaterials;
    private bool isOutlined = false;

    private void Awake()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalMaterials = objectRenderer.sharedMaterials;

            // Tạo danh sách material mới gồm material gốc + material outline
            outlinedMaterials = new Material[originalMaterials.Length + 1];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                outlinedMaterials[i] = originalMaterials[i];
            }
            outlinedMaterials[outlinedMaterials.Length - 1] = outlineMaterial;
        }
    }

    public void ShowOutline()
    {
        if (isOutlined || objectRenderer == null) return;
        objectRenderer.materials = outlinedMaterials;
        isOutlined = true;
    }

    public void HideOutline()
    {
        if (!isOutlined || objectRenderer == null) return;
        objectRenderer.materials = originalMaterials;
        isOutlined = false;
    }
}