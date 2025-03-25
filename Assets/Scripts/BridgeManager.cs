using UnityEngine;
using System.Collections.Generic;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private ObjectPool bridgePool;
    [SerializeField] private float bridgeGrowthSpeed = 0.1f;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float spawnHeightOffset = 2f;
    [SerializeField] private ObjectPool gatePool;
    [SerializeField] private ObjectPool hammerTrapPool;
    [SerializeField] private GameObject gatePrefab;
    [SerializeField] private GameObject hammerTrapPrefab;
    [SerializeField] private int maxObstacles = 2;
    [SerializeField] private float spawnForwardOffset = 1f;
    [SerializeField] private PillarManager pillarManager; // Tham chiếu đến PillarManager

    private GameObject currentBridge;
    private float bridgeScale;
    private bool isBuilding;
    private Vector3 bridgeStartPosition;
    private bool isGrowing = true;
    private readonly Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    private readonly Vector3 maxScale = new Vector3(0.3f, 0.3f, 0.3f);
    private List<GameObject> activeTraps = new List<GameObject>();

    void Start()
    {
        if (bridgePool == null || playerTransform == null || gatePool == null || hammerTrapPool == null ||
            gatePrefab == null || hammerTrapPrefab == null || pillarManager == null)
        {
            Debug.LogError("Missing references in BridgeManager!");
        }
    }

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

    private void StartBuilding()
    {
        // Dừng trụ di chuyển khi bắt đầu dựng cầu
        pillarManager.StopPillarMovement();

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
            bool isgate = Random.value > 0.5f;
            GameObject obstaclePrefab = isgate ? gatePrefab : hammerTrapPrefab;

            Vector3 localPos = new Vector3(
                Random.Range(-0.5f, 0.5f) * bridgeCollider.size.x,
                isgate ? 0.2f : 3f,
                0f
            );
            Vector3 spawnPosition = bridgeCollider.transform.TransformPoint(bridgeCollider.center + localPos);

            GameObject obstacle = isgate ? gatePool.GetObject(spawnPosition, Quaternion.identity) : hammerTrapPool.GetObject(spawnPosition, Quaternion.identity);
            obstacle.transform.parent = currentBridge.transform;
            activeTraps.Add(obstacle);
            Debug.Log("Spawned obstacle " + obstacle.name + " at: " + spawnPosition);

            if (!isgate)
            {
                HammerMovement hammerScript = obstacle.GetComponent<HammerMovement>();
                if (hammerScript == null) hammerScript = obstacle.AddComponent<HammerMovement>();
                hammerScript.SetMovementParameters(bridgeCollider.size.x, 1f);
            }
        }
    }

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