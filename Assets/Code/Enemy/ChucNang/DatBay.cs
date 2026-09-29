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
            // Tạo bẫy ngay tại vị trí chân của Enemy
            GameObject newTrap = Instantiate(trapPrefab, transform.position, Quaternion.identity);

            // Gửi thông tin Spawner này vào bẫy để bẫy có thể gọi lại khi bị đạp trúng
            Trap trapComponent = newTrap.GetComponent<Trap>();
            if (trapComponent != null)
            {
                trapComponent.Initialize(this);
            }

            // Thêm bẫy mới vào danh sách quản lý
            activeTraps.Add(newTrap);
            Debug.Log("Enemy đã đặt 1 bẫy mới. Tổng số bẫy hiện tại: " + activeTraps.Count + "/3");
        }
        else
        {
            Debug.LogWarning("Chưa gán Prefab cái bẫy vào ô Trap Prefab trên Inspector!");
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