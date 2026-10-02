using UnityEngine;
using System.Collections.Generic;

public class EnemyTrapSpawner : MonoBehaviour
{
    [Header("Cài đặt Đặt bẫy")]
    [SerializeField] private GameObject trapPrefab;     // Prefab cái bẫy
    [SerializeField] private float trapInterval = 10f;  // Thời gian 10 giây đặt 1 lần
    [SerializeField] private int maxTraps = 3;          // Số lượng bẫy tối đa được phép tồn tại

    private float trapTimer = 0f;
    private List<GameObject> activeTraps = new List<GameObject>(); // Danh sách lưu các bẫy đang nằm trên bản đồ

    void Start()
    {
        trapTimer = 0f;
    }

    // Hàm này chỉ chạy khi quái đang đi tuần tra
    public void TickTrapTimer()
    {
        // Nếu số lượng bẫy đang có trên bản đồ đã đạt tối đa (3 cái) thì dừng đếm giờ, không đặt thêm
        if (activeTraps.Count >= maxTraps) return;

        trapTimer += Time.deltaTime;

        if (trapTimer >= trapInterval)
        {
            trapTimer = 0f; // Reset đồng hồ
            SpawnTrap();    // Thả bẫy mới
        }
    }

    void SpawnTrap()
    {
        if (trapPrefab != null)
        {
            // Đặt bẫy hơi dịch về phía sau chân Enemy một chút để tránh kẹt ngay tâm
            Vector3 spawnPosition = transform.position - (transform.forward * 0.5f) + new Vector3(0, 0.2f, 0);

            GameObject newTrap = Instantiate(trapPrefab, spawnPosition, Quaternion.identity);

            // --- TỰ ĐỘNG BỎ QUA VA CHẠM GIỮA ENEMY VÀ BẪY ---
            Collider enemyCollider = GetComponent<Collider>();
            Collider[] trapColliders = newTrap.GetComponentsInChildren<Collider>();

            if (enemyCollider != null)
            {
                foreach (var trapCol in trapColliders)
                {
                    Physics.IgnoreCollision(enemyCollider, trapCol, true);
                }
            }
            // ------------------------------------------------

            Trap trapComponent = newTrap.GetComponent<Trap>();
            if (trapComponent != null)
            {
                trapComponent.Initialize(this);
            }

            activeTraps.Add(newTrap);
            Debug.Log("Enemy thả bẫy an toàn. Tổng số bẫy: " + activeTraps.Count + "/3");
        }
    }
    // Hàm nhận thông báo từ bẫy khi bẫy bị Player đạp trúng và biến mất
    public void OnTrapDestroyed(GameObject trapObj)
    {
        if (activeTraps.Contains(trapObj))
        {
            activeTraps.Remove(trapObj); // Xóa bẫy khỏi danh sách quản lý
            Debug.Log("Một chiếc bẫy đã bị kích hoạt. Số bẫy hiện tại còn lại: " + activeTraps.Count + "/3");
        }
    }
}