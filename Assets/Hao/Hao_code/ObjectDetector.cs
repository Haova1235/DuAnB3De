using UnityEngine;
using UnityEngine.UI; // Thêm thư viện UI

public class ObjectDetector : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float interactDistance = 15f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI Crosshair Settings")]
    [SerializeField] private Image crosshairImage; // Kéo thả Image Crosshair vào đây
    [SerializeField] private Color defaultCrosshairColor = Color.white;
    [SerializeField] private Color highlightCrosshairColor = Color.red;

    private IOutlineable currentTarget;

    private void Start()
    {
        if (crosshairImage != null)
        {
            crosshairImage.color = defaultCrosshairColor;
        }
    }

    private void Update()
    {
        DetectObjectInCrosshair();
    }

    private void DetectObjectInCrosshair()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        // Bắn Raycast đúng từ điểm tâm màn hình (nơi đặt Crosshair)
        Ray ray = mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer))
        {
            IOutlineable outlineable = hit.collider.GetComponentInParent<IOutlineable>();

            if (outlineable != null)
            {
                if (currentTarget != outlineable)
                {
                    ClearCurrentTarget();
                    currentTarget = outlineable;
                    currentTarget.ShowOutline();

                    // Đổi màu tâm ngắm sang màu Highlight
                    if (crosshairImage != null) crosshairImage.color = highlightCrosshairColor;
                }
                return;
            }
        }

        ClearCurrentTarget();
    }

    private void ClearCurrentTarget()
    {
        if (currentTarget != null)
        {
            currentTarget.HideOutline();
            currentTarget = null;

            // Trả màu tâm ngắm về mặc định
            if (crosshairImage != null) crosshairImage.color = defaultCrosshairColor;
        }
    }
}