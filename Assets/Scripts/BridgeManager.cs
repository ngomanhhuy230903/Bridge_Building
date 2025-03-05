using UnityEngine;
using System.Collections.Generic;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private ObjectPool bridgePool;
    [SerializeField] private float bridgeGrowthSpeed = 0.1f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnHeightOffset = 2f;
    [SerializeField] private ObjectPool spikeTrapPool;
    [SerializeField] private ObjectPool hammerTrapPool;
    [SerializeField] private GameObject spikeTrapPrefab;
    [SerializeField] private GameObject hammerTrapPrefab;
    [SerializeField] private int maxObstacles = 2;
    [SerializeField] private float spawnForwardOffset = 1f;

    private GameObject currentBridge;
    private float bridgeScale;
    private bool isBuilding;
    private Vector3 bridgeStartPosition;
    private bool isGrowing = true;
    private readonly Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    private readonly Vector3 maxScale = new Vector3(0.3f, 0.3f, 0.3f);
    private List<GameObject> activeTraps = new List<GameObject>();

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho BridgeManager, kiểm tra các tham chiếu cần thiết.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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
    /// Xử lý logic dựng và thả cầu dựa trên input người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;

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
    /// Bắt đầu dựng cầu phía trước player theo hướng quay, spawn cầu ở vị trí cao hơn 2Y.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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
    /// Tăng hoặc giảm kích thước cầu đồng đều theo hướng Y, dựa trên trạng thái isGrowing.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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
    /// Thả cầu để rơi xuống và lên lịch spawn chướng ngại vật sau 1 giây.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void DropBridge()
    {
        isBuilding = false;
        Rigidbody rb = currentBridge.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("DropBridge | Rigidbody Kinematic: " + rb.isKinematic + " | Velocity: " + rb.velocity);
        }

        Debug.Log("Bridge Position at Drop: " + currentBridge.transform.position);
        Invoke("SpawnObstacles", 1f);
    }

    #region 
    /// <summary>
    /// Spawn chướng ngại vật ngẫu nhiên trong BoxCollider của cầu, chỉ ngẫu nhiên trên trục X.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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

        int obstacleCount = Random.Range(1, maxObstacles + 1);
        for (int i = 0; i < obstacleCount; i++)
        {
            bool isSpikeTrap = Random.value > 0.5f;
            GameObject obstaclePrefab = isSpikeTrap ? spikeTrapPrefab : hammerTrapPrefab;

            Vector3 localPos = new Vector3(
                Random.Range(-0.5f, 0.5f) * bridgeCollider.size.x,
                isSpikeTrap ? 0.2f : 5f,
                0f
            );
            Vector3 spawnPosition = bridgeCollider.transform.TransformPoint(bridgeCollider.center + localPos);

            GameObject obstacle = isSpikeTrap ? spikeTrapPool.GetObject(spawnPosition, Quaternion.identity) : hammerTrapPool.GetObject(spawnPosition, Quaternion.identity);
            obstacle.transform.parent = currentBridge.transform;
            activeTraps.Add(obstacle);
            Debug.Log("Spawned obstacle " + obstacle.name + " at: " + spawnPosition);

            if (!isSpikeTrap)
            {
                HammerMovement hammerScript = obstacle.GetComponent<HammerMovement>();
                if (hammerScript == null) hammerScript = obstacle.AddComponent<HammerMovement>();
                hammerScript.SetMovementParameters(bridgeCollider.size.x, 1f);
            }
        }
    }

    #region 
    /// <summary>
    /// Vô hiệu hóa cầu hiện tại và tất cả các trap liên quan để đưa về Pool.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public void DeactivateCurrentBridge()
    {
        if (currentBridge != null)
        {
            foreach (GameObject trap in activeTraps)
            {
                if (trap != null)
                {
                    trap.SetActive(false);
                    Debug.Log("Deactivated trap: " + trap.name);
                }
            }
            activeTraps.Clear();

            currentBridge.SetActive(false);
            Debug.Log("Deactivated current bridge: " + currentBridge.name);
            currentBridge = null;
        }
    }
}

public class HammerMovement : MonoBehaviour
{
    private float moveSpeed;
    private float range;
    private Vector3 startPosition;

    #region 
    /// <summary>
    /// Khởi tạo vị trí ban đầu của búa khi được tạo.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Start()
    {
        startPosition = transform.position;
    }

    #region 
    /// <summary>
    /// Thiết lập thông số di chuyển cho búa dựa trên chiều rộng cầu và tốc độ.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    public void SetMovementParameters(float bridgeWidth, float speed)
    {
        range = bridgeWidth * 0.008f;
        moveSpeed = 3f * speed;
    }

    #region 
    /// <summary>
    /// Cập nhật vị trí búa để di chuyển trái-phải theo sóng sin.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Update()
    {
        float offset = Mathf.Sin(Time.time * moveSpeed) * range;
        transform.position = startPosition + new Vector3(offset, 0f, 0f);
    }
}