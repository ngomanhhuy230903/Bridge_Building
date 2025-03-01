using UnityEngine;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private ObjectPool pillarPool; // Tham chiếu đến ObjectPool cho trụ cầu
    [SerializeField] private Transform player; // Tham chiếu đến nhân vật
    [SerializeField] private GameObject initialPillar; // Trụ đầu tiên đã thiết kế trong Scene
    [SerializeField] private float minDistance = 5f; // Khoảng cách tối thiểu giữa các trụ
    [SerializeField] private float maxDistance = 10f; // Khoảng cách tối đa giữa các trụ
    [SerializeField] private float minHeight = -2f; // Chiều cao tối thiểu so với trụ hiện tại
    [SerializeField] private float maxHeight = 2f; // Chiều cao tối đa so với trụ hiện tại

    private Vector3 lastPillarPosition; // Vị trí của trụ cuối cùng
    private GameObject currentPillar; // Trụ hiện tại mà người chơi đang đứng

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu, sử dụng trụ đầu tiên đã có trong Scene và đặt vị trí nhân vật.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is not set in PillarManager!");
            return;
        }

        if (pillarPool == null)
        {
            Debug.LogError("PillarPool reference is not set in PillarManager!");
            return;
        }

        if (initialPillar == null)
        {
            Debug.LogError("Initial Pillar reference is not set in PillarManager!");
            return;
        }

        // Đặt vị trí ban đầu của nhân vật
        player.position = new Vector3(-84.538002f, 467.868011f, 569.995972f);

        // Sử dụng trụ đầu tiên đã thiết kế trong Scene
        currentPillar = initialPillar;
        lastPillarPosition = initialPillar.transform.position;
    }

    #region 
    /// <summary>
    /// Cập nhật logic để kiểm tra khi nào nhân vật đi qua trụ mới.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    void Update()
    {
        if (currentPillar == null) return;

        // Kiểm tra xem nhân vật đã đi qua trụ hiện tại chưa
        if (Vector3.Distance(player.position, currentPillar.transform.position) < 1f) // Khoảng cách gần để xác định "đi qua"
        {
            SpawnNextPillar();
            currentPillar = pillarPool.GetObject(lastPillarPosition, Quaternion.identity); // Lấy trụ mới làm trụ hiện tại
        }
    }

    #region 
    /// <summary>
    /// Sinh trụ tiếp theo với vị trí ngẫu nhiên dựa trên trụ cuối cùng.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    private void SpawnNextPillar()
    {
        // Tính toán vị trí ngẫu nhiên cho trụ tiếp theo
        float randomDistance = Random.Range(minDistance, maxDistance);
        float randomHeight = Random.Range(minHeight, maxHeight);
        Vector3 direction = Random.onUnitSphere; // Hướng ngẫu nhiên trên mặt phẳng XZ
        direction.y = 0; // Giữ trụ trên mặt phẳng ngang
        direction.Normalize();

        Vector3 newPosition = lastPillarPosition + (direction * randomDistance);
        newPosition.y = lastPillarPosition.y + randomHeight;

        // Cập nhật vị trí trụ cuối cùng
        lastPillarPosition = newPosition;
    }
}