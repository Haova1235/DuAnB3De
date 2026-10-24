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
        // Kiểm tra xem đối tượng va chạm có phải là Player hoặc chứa script PlayerMove không
        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerMove>() != null)
        {
            // Lấy trực tiếp script PlayerMove từ root hoặc parent của đối tượng va chạm
            PlayerMove playerMove = other.GetComponentInParent<PlayerMove>();
            if (playerMove != null)
            {
                // Kiểm tra xem đã dính bẫy chưa để tránh bị chồng chất hiệu ứng
                if (playerMove.GetComponent<PlayerTrapEffectEffect>() == null)
                {
                    // Gắn hiệu ứng choán/làm chậm lên đúng object chứa PlayerMove
                    playerMove.gameObject.AddComponent<PlayerTrapEffectEffect>();

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
    }
}

// --- SCRIPT HIỆU ỨNG TỰ ĐỘNG GẮN LÊN PLAYER ---


public class PlayerTrapEffectEffect : MonoBehaviour
{
    private PlayerMove playerMoveScript;

    void Start()
    {
        playerMoveScript = GetComponent<PlayerMove>();
        StartCoroutine(TrapSequenceRoutine());
    }

    IEnumerator TrapSequenceRoutine()
    {
        Debug.Log("⚠️ Bạn đã dẫm phải bẫy! Bị choán hoàn toàn trong 3 giây!");

        // 1. GIAI ĐOẠN 1: CHOÁN HOÀN TOÀN (Hệ số tốc độ = 0)
        if (playerMoveScript != null)
        {
            playerMoveScript.ApplyTrapEffect(0f, true);
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
#if UNITY_2023_1_OR_NEWER
            rb.linearVelocity = Vector3.zero;
#else
            rb.velocity = Vector3.zero;
#endif
        }

        yield return new WaitForSeconds(3f);

        Debug.Log("⏳ Hết choán, bước sang giai đoạn bị thương: Di chuyển chậm 50% trong 2 giây.");

        // 2. GIAI ĐOẠN 2: LÀM CHẬM 50% (Hệ số tốc độ = 0.5)
        if (playerMoveScript != null)
        {
            playerMoveScript.ApplyTrapEffect(0.5f, true);
        }

        yield return new WaitForSeconds(2f);

        // 3. GIAI ĐOẠN 3: HỒI PHỤC HOÀN TOÀN
        if (playerMoveScript != null)
        {
            playerMoveScript.ApplyTrapEffect(1.0f, false);
        }

        Debug.Log("✅ Người chơi đã hồi phục hoàn toàn trạng thái.");
        Destroy(this);
    }
}