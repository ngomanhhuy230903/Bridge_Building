using UnityEngine;
using System.Collections.Generic;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private ObjectPool bridgePool; // Tham chiếu đến ObjectPool cho cầu
    [SerializeField] private float bridgeGrowthSpeed = 0.1f; // Tốc độ tăng/giảm scale (đơn vị/giây)
    [SerializeField] private Transform playerTransform; // Tham chiếu đến Transform của nhân vật
    [SerializeField] private float spawnHeightOffset = 2f; // Offset Y để cầu spawn cao hơn player
    [SerializeField] private ObjectPool spikeTrapPool; // ObjectPool cho bẫy gai
    [SerializeField] private ObjectPool hammerTrapPool; // ObjectPool cho búa
    [SerializeField] private GameObject spikeTrapPrefab; // Prefab bẫy gai
    [SerializeField] private GameObject hammerTrapPrefab; // Prefab búa
    [SerializeField] private int maxObstacles = 2; // Số lượng tối đa chướng ngại vật trên cầu
    [SerializeField] private float spawnForwardOffset = 1f; // Khoảng cách spawn phía trước player theo hướng quay

    private GameObject currentBridge; // Cầu hiện tại đang dựng
    private float bridgeScale; // Scale hiện tại của cầu
    private bool isBuilding; // Đang trong quá trình dựng cầu
    private Vector3 bridgeStartPosition; // Vị trí bắt đầu của cầu
    private bool isGrowing = true; // Trạng thái tăng hoặc giảm scale
    private readonly Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f); // Scale tối thiểu
    private readonly Vector3 maxScale = new Vector3(0.2f, 0.2f, 0.2f); // Scale tối đa
    private List<GameObject> activeTraps = new List<GameObject>(); // Danh sách để theo dõi các trap đang active

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho BridgeManager.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        if (bridgePool == null || playerTransform == null || spikeTrapPool == null || hammerTrapPool == null || spikeTrapPrefab == null || hammerTrapPrefab == null)
        {
            Debug.LogError("Missing references in BridgeManager!");
        }
    }

    #region 
    /// <summary>
    /// Xử lý logic dựng và ngã cầu dựa trên input của người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Update()
    {
        if (!isBuilding && Input.GetKeyDown(KeyCode.Space))
        {
            StartBuilding();
        }
        if (isBuilding && Input.GetKey(KeyCode.Space))
        {
            GrowBridge();
        }
        if (isBuilding && Input.GetKeyUp(KeyCode.Space))
        {
            DropBridge();
        }
    }

    #region 
    /// <summary>
    /// Bắt đầu dựng cầu phía trước player theo hướng quay, cao hơn 2Y.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void StartBuilding()
    {
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0;
        playerForward.Normalize();
        bridgeStartPosition = playerTransform.position + Vector3.up * spawnHeightOffset + playerForward * spawnForwardOffset;

        Quaternion bridgeRotation = Quaternion.LookRotation(playerForward) * Quaternion.Euler(0f, 90f, -60f);

        currentBridge = bridgePool.GetObject(bridgeStartPosition, bridgeRotation);
        bridgeScale = minScale.x;
        currentBridge.transform.localScale = minScale;
        isBuilding = true;
        isGrowing = true;

        Rigidbody rb = currentBridge.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        else
        {
            rb = currentBridge.AddComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        Debug.Log("Player Forward: " + playerForward + " | Bridge Spawn Position: " + bridgeStartPosition + " | Bridge Rotation: " + currentBridge.transform.eulerAngles);
    }

    #region 
    /// <summary>
    /// Tăng hoặc giảm kích thước đồng đều của cầu, mọc theo hướng Y.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void GrowBridge()
    {
        if (isGrowing)
        {
            bridgeScale += bridgeGrowthSpeed * Time.deltaTime;
            if (bridgeScale >= maxScale.x)
            {
                bridgeScale = maxScale.x;
                isGrowing = false;
            }
        }
        else
        {
            bridgeScale -= bridgeGrowthSpeed * Time.deltaTime;
            if (bridgeScale <= minScale.x)
            {
                bridgeScale = minScale.x;
                isGrowing = true;
            }
        }

        Vector3 newScale = new Vector3(bridgeScale, bridgeScale, bridgeScale);
        currentBridge.transform.localScale = newScale;
        currentBridge.transform.position = bridgeStartPosition + Vector3.up * (bridgeScale / 2f);
    }

    #region 
    /// <summary>
    /// Thả cầu để ngã xuống và spawn chướng ngại vật.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void DropBridge()
    {
        isBuilding = false;
        Rigidbody rb = currentBridge.GetComponent<Rigidbody>();
        rb.isKinematic = false;

        Debug.Log("Bridge Position at Drop: " + currentBridge.transform.position);

        // Spawn chướng ngại vật sau khi cầu ngã
        Invoke("SpawnObstacles", 1f); // Đợi 1 giây để cầu rơi ổn định
    }

    #region 
    /// <summary>
    /// Spawn chướng ngại vật ngẫu nhiên trong BoxCollider của cầu, chỉ ngẫu nhiên X.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void SpawnObstacles()
    {
        if (currentBridge == null) return;

        BoxCollider bridgeCollider = currentBridge.GetComponent<BoxCollider>();
        if (bridgeCollider == null)
        {
            Debug.LogError("Bridge has no BoxCollider!");
            return;
        }

        int obstacleCount = Random.Range(1, maxObstacles + 1); // Số lượng chướng ngại vật ngẫu nhiên
        for (int i = 0; i < obstacleCount; i++)
        {
            // Chọn ngẫu nhiên loại chướng ngại vật (50% bẫy gai, 50% búa)
            bool isSpikeTrap = Random.value > 0.5f;
            GameObject obstaclePrefab = isSpikeTrap ? spikeTrapPrefab : hammerTrapPrefab;

            // Tính vị trí ngẫu nhiên trong BoxCollider, chỉ ngẫu nhiên X
            Vector3 localPos = new Vector3(
                Random.Range(-0.5f, 0.5f) * bridgeCollider.size.x, // Ngẫu nhiên X
                isSpikeTrap ? 0.1f : 5f, // 0.1Y cho gai, 5Y cho búa
                0f // Z giữ ở trung tâm
            );
            Vector3 spawnPosition = bridgeCollider.transform.TransformPoint(bridgeCollider.center + localPos);

            // Spawn từ Pool tương ứng
            GameObject obstacle = isSpikeTrap ? spikeTrapPool.GetObject(spawnPosition, Quaternion.identity) : hammerTrapPool.GetObject(spawnPosition, Quaternion.identity);
            obstacle.transform.parent = currentBridge.transform; // Gắn vào cầu
            activeTraps.Add(obstacle); // Thêm vào danh sách active traps
            Debug.Log("Spawned obstacle " + obstacle.name + " at: " + spawnPosition);

            // Nếu là búa, thêm logic di chuyển trái-phải
            if (!isSpikeTrap)
            {
                HammerMovement hammerScript = obstacle.GetComponent<HammerMovement>();
                if (hammerScript == null) hammerScript = obstacle.AddComponent<HammerMovement>();
                hammerScript.SetMovementParameters(bridgeCollider.size.x, 1f); // Phạm vi và tốc độ điều chỉnh
            }
        }
    }

    #region 
    /// <summary>
    /// Vô hiệu hóa cầu hiện tại và tất cả các trap liên quan để đưa về Pool.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    public void DeactivateCurrentBridge()
    {
        if (currentBridge != null)
        {
            // Vô hiệu hóa tất cả các trap trước khi vô hiệu hóa cầu
            foreach (GameObject trap in activeTraps)
            {
                if (trap != null)
                {
                    trap.SetActive(false);
                    Debug.Log("Deactivated trap: " + trap.name);
                }
            }
            activeTraps.Clear(); // Xóa danh sách sau khi vô hiệu hóa

            currentBridge.SetActive(false);
            Debug.Log("Deactivated current bridge: " + currentBridge.name);
            currentBridge = null;
        }
    }
}

// Script riêng để điều khiển búa di chuyển trái-phải
public class HammerMovement : MonoBehaviour
{
    private float moveSpeed; // Tốc độ di chuyển của búa
    private float range; // Phạm vi di chuyển (dựa trên chiều rộng cầu)
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    public void SetMovementParameters(float bridgeWidth, float speed)
    {
        range = bridgeWidth * 0.008f; // Giảm phạm vi xuống 40% chiều rộng cầu
        moveSpeed = 3f * speed; // Tốc độ điều chỉnh
    }

    void Update()
    {
        // Di chuyển trái-phải theo sóng sin
        float offset = Mathf.Sin(Time.time * moveSpeed) * range;
        transform.position = startPosition + new Vector3(offset, 0f, 0f);
    }
}