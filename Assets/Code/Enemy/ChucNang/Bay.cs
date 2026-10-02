using UnityEngine;
using System.Collections;

public class Trap : MonoBehaviour
{
    private EnemyTrapSpawner spawner;
    private Rigidbody rb;

    public void Initialize(EnemyTrapSpawner ownerSpawner)
    {
        spawner = ownerSpawner;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // --- BỔ SUNG: Xử lý khi bẫy chạm đất thì khóa cứng lại ngay lập tức ---
    private void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra xem có phải chạm vào sàn/mặt đất không (có thể dựa vào Tag hoặc Layer)
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.name.Contains("Ground") || collision.gameObject.name.Contains("Floor"))
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true; // Khóa cứng vật lý, chặn đứng việc rơi xuyên sàn!
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.gameObject.AddComponent<PlayerTrapEffectEffect>();

            if (spawner != null)
            {
                spawner.OnTrapDestroyed(gameObject);
            }

            Destroy(gameObject);
        }
    }
}