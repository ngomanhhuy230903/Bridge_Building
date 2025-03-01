using UnityEngine;

public class PillarManager : MonoBehaviour
{
    [SerializeField] private ObjectPool pillarPool; // Pool cho các pillar
    [SerializeField] private Transform player; // Tham chiếu đến player
    [SerializeField] private GameObject initialPillar; // Pillar khởi đầu
    [SerializeField] private GameObject nextPillar; // Pillar kế tiếp ban đầu
    [SerializeField] private float minDistance = 5f; // Khoảng cách tối thiểu spawn pillar mới
    [SerializeField] private float maxDistance = 6f; // Khoảng cách tối đa spawn pillar mới
    [SerializeField] private float minHeight = -2f; // Chiều cao tối thiểu spawn pillar mới
    [SerializeField] private float maxHeight = 2f; // Chiều cao tối đa spawn pillar mới
    [SerializeField] private BridgeManager bridgeManager; // Tham chiếu đến BridgeManager để vô hiệu hóa cầu
    [SerializeField] private float pillarHeightOffset = 2f; // Offset Y để tính vị trí thực tế của pillar
    [SerializeField] private float minSeparationDistance = 6f; // Khoảng cách tối thiểu giữa các pillar

    private Vector3 lastPillarPosition; // Vị trí của pillar cuối cùng
    private GameObject currentPillar; // Pillar hiện tại player đang ở
    private GameObject previousPillar; // Pillar trước đó để vô hiệu hóa
    private bool hasStartedSpawning; // Đánh dấu khi bắt đầu spawn pillar mới

    void Start()
    {
        if (player == null || pillarPool == null || initialPillar == null || nextPillar == null || bridgeManager == null)
        {
            Debug.LogError("Missing references in PillarManager!");
            return;
        }
        currentPillar = initialPillar;
        previousPillar = null;
        lastPillarPosition = initialPillar.transform.position;
        hasStartedSpawning = false;
    }

    void Update()
    {
        if (currentPillar == null) return;

        // Kiểm tra khi player đến gần vị trí thực tế của currentPillar (cao hơn 2Y)
        Vector3 pillarTopPosition = currentPillar.transform.position + Vector3.up * pillarHeightOffset;
        if (Vector3.Distance(player.position, pillarTopPosition) < 1f)
        {
            if (currentPillar == initialPillar && !hasStartedSpawning)
            {
                // Player đi từ pillar khởi đầu sang pillar kế tiếp, không spawn ngay
                previousPillar = currentPillar;
                currentPillar = nextPillar;
                Debug.Log("Reached initial pillar, moving to next pillar.");
            }
            else if (currentPillar == nextPillar && !hasStartedSpawning)
            {
                // Player đi qua nextPillar, bắt đầu spawn pillar mới
                Debug.Log("Success! Player reached next pillar at " + pillarTopPosition);

                if (previousPillar != null)
                {
                    previousPillar.SetActive(false);
                    Debug.Log("Deactivated previous pillar: " + previousPillar.name);
                }
                bridgeManager.DeactivateCurrentBridge();

                SpawnNextPillar();
                previousPillar = currentPillar;
                currentPillar = pillarPool.GetObject(lastPillarPosition, Quaternion.identity);
                hasStartedSpawning = true;
                Debug.Log("Spawned new random pillar at " + lastPillarPosition);
            }
            else if (hasStartedSpawning)
            {
                // Player đi qua các pillar ngẫu nhiên sau nextPillar
                Debug.Log("Success! Player reached pillar at " + pillarTopPosition);

                if (previousPillar != null)
                {
                    previousPillar.SetActive(false);
                    Debug.Log("Deactivated previous pillar: " + previousPillar.name);
                }
                bridgeManager.DeactivateCurrentBridge();

                SpawnNextPillar();
                previousPillar = currentPillar;
                currentPillar = pillarPool.GetObject(lastPillarPosition, Quaternion.identity);
                Debug.Log("Spawned new random pillar at " + lastPillarPosition);
            }
        }
    }

    #region 
    /// <summary>
    /// Sinh trụ tiếp theo với vị trí ngẫu nhiên, đảm bảo cách xa currentPillar từ 5-6 đơn vị.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void SpawnNextPillar()
    {
        if (currentPillar == null)
        {
            Debug.LogError("currentPillar is null in SpawnNextPillar!");
            return;
        }

        Vector3 newPosition;
        bool validPosition = false;
        int attempts = 0;
        const int maxAttempts = 20; // Tăng số lần thử để tìm vị trí hợp lệ

        Vector3 currentPillarPos = currentPillar.transform.position;

        do
        {
            float randomDistance = Random.Range(minDistance, maxDistance); // 5 đến 6
            float randomHeight = Random.Range(minHeight, maxHeight);
            Vector3 direction = Random.onUnitSphere;
            direction.y = 0;
            direction.Normalize();

            // Tính vị trí mới dựa trên currentPillar
            newPosition = currentPillarPos + (direction * randomDistance);
            newPosition.y = currentPillarPos.y + randomHeight;

            validPosition = CheckPositionValidity(newPosition);
            attempts++;

        } while (!validPosition && attempts < maxAttempts);

        if (validPosition)
        {
            lastPillarPosition = newPosition;
        }
        else
        {
            // Fallback: Cách currentPillar một khoảng cố định theo hướng ngẫu nhiên
            Vector3 fallbackDirection = Random.onUnitSphere;
            fallbackDirection.y = 0;
            fallbackDirection.Normalize();
            lastPillarPosition = currentPillarPos + (fallbackDirection * maxDistance);
            Debug.LogWarning("Could not find valid spawn position, using fallback position: " + lastPillarPosition);
        }
    }

    #region 
    /// <summary>
    /// Kiểm tra xem vị trí mới có hợp lệ không (cách xa các pillar hiện tại ít nhất minSeparationDistance).
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    /// <param name="position">Vị trí cần kiểm tra</param>
    /// <returns>True nếu hợp lệ, False nếu quá gần</returns>
    #endregion
    private bool CheckPositionValidity(Vector3 position)
    {
        if (initialPillar != null && initialPillar.activeInHierarchy && Vector3.Distance(position, initialPillar.transform.position) < minSeparationDistance)
        {
            return false;
        }
        if (nextPillar != null && nextPillar.activeInHierarchy && Vector3.Distance(position, nextPillar.transform.position) < minSeparationDistance)
        {
            return false;
        }
        foreach (Transform child in pillarPool.transform)
        {
            if (child.gameObject.activeInHierarchy && Vector3.Distance(position, child.position) < minSeparationDistance)
            {
                return false;
            }
        }
        return true;
    }

    #region 
    /// <summary>
    /// Lấy trụ tiếp theo đang hoạt động hoặc null nếu không có.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    /// <returns>Trụ tiếp theo</returns>
    #endregion
    public GameObject GetNextPillar()
    {
        if (currentPillar == initialPillar)
        {
            return nextPillar;
        }
        foreach (Transform child in pillarPool.transform)
        {
            if (child.gameObject.activeInHierarchy && child.gameObject != currentPillar)
            {
                return child.gameObject;
            }
        }
        return null;
    }
}