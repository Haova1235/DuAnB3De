using UnityEngine;

[ExecuteAlways]
public class BlockMaterialTiling : MonoBehaviour
{
    [Header("Tên Property của Shader")]
    [Tooltip("Standard Shader: _MainTex_ST\nURP Lit Shader: _BaseMap_ST\n(Hoặc tên biến tùy chỉnh của bạn)")]
    public string propertyName = "_BaseMap_ST";

    [Header("Tiling (Độ lặp texture)")]
    public Vector2 tiling = Vector2.one;

    [Header("Offset (Độ dịch chuyển texture)")]
    public Vector2 offset = Vector2.zero;

    private Renderer blockRenderer;
    private MaterialPropertyBlock propBlock;

    void Awake()
    {
        ApplyTiling();
    }

    void OnValidate()
    {
        // Cập nhật ngay lập tức khi thay đổi thông số trong Inspector
        ApplyTiling();
    }

    void ApplyTiling()
    {
        if (blockRenderer == null)
            blockRenderer = GetComponent<Renderer>();

        if (blockRenderer == null) return;

        if (propBlock == null)
            propBlock = new MaterialPropertyBlock();

        // Lấy property block hiện tại
        blockRenderer.GetPropertyBlock(propBlock);

        // Tạo Vector4 cho Tiling (x, y) và Offset (z, w)
        Vector4 st = new Vector4(tiling.x, tiling.y, offset.x, offset.y);

        // Gán vào MaterialPropertyBlock theo tên property được chỉ định
        if (!string.IsNullOrEmpty(propertyName))
        {
            propBlock.SetVector(propertyName, st);
        }

        // Áp dụng lại vào Renderer
        blockRenderer.SetPropertyBlock(propBlock);
    }
}