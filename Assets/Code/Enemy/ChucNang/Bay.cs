using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    private EnemyTrapSpawner spawner;

    public void Initialize(EnemyTrapSpawner ownerSpawner)
    {
        spawner = ownerSpawner;
    }

    // Chỉ bắt sự kiện khi Player đi vào vùng "Is Trigger"
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tự động gắn hiệu ứng đứng yên 3s và slow 50% trong 2s lên Player
            other.gameObject.AddComponent<PlayerTrapEffectEffect>();

            // Báo về cho Enemy biết để sinh bẫy mới bù vào
            if (spawner != null)
            {
                spawner.OnTrapDestroyed(gameObject);
            }

            // Hủy bẫy ngay lập tức
            Destroy(gameObject);
        }
    }
}

// --- SCRIPT HIỆU ỨNG TỰ ĐỘNG GẮN LÊN PLAYER ---
public class PlayerTrapEffectEffect : MonoBehaviour
{
    private System.Reflection.FieldInfo speedField;
    private float originalSpeed = 0f;

    void Start()
    {
        StartCoroutine(TrapSequenceRoutine());
    }

    IEnumerator TrapSequenceRoutine()
    {
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

        // 1. Quét tìm biến tốc độ của Player để khóa về 0 trong 3 giây đầu
        foreach (var script in scripts)
        {
            if (script != this && (script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Player")))
            {
                System.Type type = script.GetType();
                speedField = type.GetField("speed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (speedField == null)
                    speedField = type.GetField("moveSpeed", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (speedField != null)
                {
                    originalSpeed = (float)speedField.GetValue(script);
                    speedField.SetValue(script, 0f); // Đứng yên
                }
                else
                {
                    script.enabled = false;
                }
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        // Giai đoạn 1: Đứng yên 3 giây
        yield return new WaitForSeconds(3f);

        // 2. Bật lại và ép giảm tốc độ xuống 50% trong 2 giây tiếp theo
        foreach (var script in scripts)
        {
            if (script != this) { script.enabled = true; }
        }

        if (speedField != null && originalSpeed > 0f)
        {
            foreach (var script in scripts)
            {
                if (script.GetType() == speedField.DeclaringType)
                {
                    speedField.SetValue(script, originalSpeed * 0.5f); // Giảm 50% tốc độ gốc
                }
            }
        }

        // Giai đoạn 2: Kéo dài 50% speed trong 2 giây
        yield return new WaitForSeconds(2f);

        // 3. Khôi phục lại 100% tốc độ ban đầu
        if (speedField != null && originalSpeed > 0f)
        {
            foreach (var script in scripts)
            {
                if (script.GetType() == speedField.DeclaringType)
                {
                    speedField.SetValue(script, originalSpeed);
                }
            }
        }

        // Tự hủy component hiệu ứng khỏi Player
        Destroy(this);
    }
}