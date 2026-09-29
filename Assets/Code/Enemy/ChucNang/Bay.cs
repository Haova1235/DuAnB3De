using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    private EnemyTrapSpawner spawner;

    public void Initialize(EnemyTrapSpawner ownerSpawner)
    {
        spawner = ownerSpawner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tự động gắn một component xử lý hiệu ứng tạm thời lên Player lúc dẫm bẫy
            // Player không hề có sẵn script này, do bẫy tự "bơm" vào và tự dọn dẹp sau khi xong việc.
            PlayerTrapEffectEffect temporaryEffect = other.gameObject.AddComponent<PlayerTrapEffectEffect>();

            // Báo về cho Enemy biết bẫy đã kích hoạt để đặt bẫy mới bù vào
            if (spawner != null)
            {
                spawner.OnTrapDestroyed(gameObject);
            }

            // Hủy cái bẫy ngay lập tức
            Destroy(gameObject);
        }
    }
}

// --- CLASS PHỤ TRỢ TỰ ĐỘNG GẮN LÊN PLAYER KHI DẪM BẪY ---
public class PlayerTrapEffectEffect : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TrapSequenceRoutine());
    }

    IEnumerator TrapSequenceRoutine()
    {
        // 1. Tạm khóa/vô hiệu hóa các script di chuyển của Player tìm được trên người Player
        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            // Bỏ qua chính script hiệu ứng này
            if (script != this && (script.GetType().Name.Contains("Move") || script.GetType().Name.Contains("Controller") || script.GetType().Name.Contains("Player")))
            {
                script.enabled = false; // Tạm tắt script di chuyển để Player đứng yên
            }
        }

        // Khóa Rigidbody nếu Player dùng vật lý (chặn trôi trượt)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.linearVelocity = Vector3.zero;

        Debug.Log("Player dính bẫy: Đứng yên 3 giây!");
        // --- GIAI ĐOẠN 1: Đứng yên 3 giây ---
        yield return new WaitForSeconds(3f);

        // --- GIAI ĐOẠN 2: Bật lại script di chuyển nhưng có thể giảm tốc (hoặc tùy chỉnh) ---
        foreach (var script in scripts)
        {
            if (script != this)
            {
                script.enabled = true; // Bật lại script di chuyển
            }
        }

        Debug.Log("Player bị Slow tốc độ trong 2 giây tiếp theo!");
        // --- GIAI ĐOẠN 3: Sau 2 giây kết thúc hoàn toàn hiệu ứng ---
        yield return new WaitForSeconds(2f);

        Debug.Log("Player đã hồi phục hoàn toàn bình thường!");

        // Tự hủy component tạm thời này khỏi Player để trả lại hiện trạng ban đầu
        Destroy(this);
    }
}