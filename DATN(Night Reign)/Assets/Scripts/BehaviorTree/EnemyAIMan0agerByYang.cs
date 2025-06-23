using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAIManager : MonoBehaviour
{
    // Singleton pattern để đảm bảo chỉ có một instance của EnemyAIManager
    public static EnemyAIManager Instance { get; private set; }

    // Danh sách các enemy đang hoạt động
    private List<EnemyAIByYang> _activeEnemies = new List<EnemyAIByYang>();

    // Biến để phân phối việc Tick qua các frame (Round-robin)
    private int _currentEnemyIndexToTick = 0;

    // Số lượng enemy được tick trong mỗi frame.
    // Đối với Boss AI, thường chỉ có 1 Boss, nên giá trị này có thể là 1 hoặc bạn có thể bỏ qua cơ chế này
    // và gọi trực tiếp Tick() trên Boss trong Update() của chính Boss.
    public int enemiesToTickPerFrame = 1; // Đặt là 1 nếu chỉ quản lý một Boss chính

    void Awake()
    {
        // Đảm bảo chỉ có một instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Nếu đã có instance khác, hủy đối tượng này
        }
        else
        {
            Instance = this; // Gán instance hiện tại
            // DontDestroyOnLoad(gameObject); // Giữ instance này không bị hủy khi chuyển cảnh (tùy chọn)
        }
    }

    // Hàm để EnemyAI tự động đăng ký khi được tạo
    public void RegisterEnemy(EnemyAIByYang enemy)
    {
        if (enemy != null && !_activeEnemies.Contains(enemy))
        {
            _activeEnemies.Add(enemy);
            Debug.Log($"Enemy registered: {enemy.name}. Total active enemies: {_activeEnemies.Count}");
        }
    }

    // Hàm để EnemyAI tự động hủy đăng ký khi enemy bị hủy (die)
    public void UnregisterEnemy(EnemyAIByYang enemy)
    {
        if (enemy != null && _activeEnemies.Contains(enemy))
        {
            _activeEnemies.Remove(enemy);
            Debug.Log($"Enemy unregistered: {enemy.name}. Total active enemies: {_activeEnemies.Count}");

            // Điều chỉnh lại chỉ số tick nếu cần để tránh lỗi OutOfRangeException
            if (_currentEnemyIndexToTick >= _activeEnemies.Count && _activeEnemies.Count > 0)
            {
                _currentEnemyIndexToTick = 0; // Reset về đầu danh sách
            }
        }
    }

    // Update được gọi mỗi frame
    void Update()
    {
        if (_activeEnemies.Count == 0)
        {
            return; // Không có enemy nào để tick
        }

        // Tick một số lượng enemy nhất định trong mỗi frame để phân phối tải
        for (int i = 0; i < enemiesToTickPerFrame; i++)
        {
            // Kiểm tra lại số lượng enemy trong trường hợp danh sách bị thay đổi trong vòng lặp
            if (_activeEnemies.Count == 0)
            {
                break; // Không còn enemy nào để tick
            }

            // Tính toán chỉ số của enemy sẽ được tick trong frame này (vòng tròn)
            _currentEnemyIndexToTick = (_currentEnemyIndexToTick + 1) % _activeEnemies.Count;

            // Lấy enemy tại chỉ số hiện tại
            var enemy = _activeEnemies[_currentEnemyIndexToTick];

            // Kiểm tra xem enemy có tồn tại và đang hoạt động không
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                enemy.Tick(); // Gọi hàm Tick của enemy (đánh giá Behavior Tree)
            }
            else
            {
                // Nếu enemy đã bị hủy hoặc không hoạt động, xóa khỏi danh sách
                // Giảm i để đảm bảo vẫn tick đủ số lượng enemy trong frame này
                _activeEnemies.RemoveAt(_currentEnemyIndexToTick);
                Debug.Log($"Removed inactive enemy. Total active enemies: {_activeEnemies.Count}");

                // Điều chỉnh lại chỉ số tick sau khi xóa
                if (_currentEnemyIndexToTick >= _activeEnemies.Count && _activeEnemies.Count > 0)
                {
                    _currentEnemyIndexToTick = 0; // Reset về đầu danh sách nếu đã hết
                }
                i--; // Giảm i để không bỏ qua tick của enemy tiếp theo do phần tử bị xóa
            }
        }
    }
}