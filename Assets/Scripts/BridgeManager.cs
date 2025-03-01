using UnityEngine;

public class BridgeManager : MonoBehaviour
{
    [SerializeField] private ObjectPool bridgePool; // Tham chiếu đến ObjectPool cho cầu
    [SerializeField] private float bridgeGrowthSpeed = 0.1f; // Tốc độ tăng/giảm scale (đơn vị/giây)
    [SerializeField] private Transform playerTransform; // Tham chiếu đến Transform của nhân vật

    private GameObject currentBridge; // Cầu hiện tại đang dựng
    private float bridgeScale; // Scale hiện tại của cầu
    private bool isBuilding; // Đang trong quá trình dựng cầu
    private Vector3 bridgeStartPosition; // Vị trí bắt đầu của cầu
    private bool isGrowing = true; // Trạng thái tăng hoặc giảm scale
    private readonly Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f); // Scale tối thiểu
    private readonly Vector3 maxScale = new Vector3(0.2f, 0.2f, 0.2f); // Scale tối đa

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho BridgeManager.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        if (bridgePool == null)
        {
            Debug.LogError("Bridge Pool is not set in BridgeManager!");
        }
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not set in BridgeManager!");
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
        // Bắt đầu dựng cầu khi nhấn Space
        if (Input.GetKeyDown(KeyCode.Space) && !isBuilding)
        {
            StartBuilding();
        }

        // Tăng hoặc giảm kích thước cầu khi giữ Space
        if (Input.GetKey(KeyCode.Space) && isBuilding)
        {
            GrowBridge();
        }

        // Ngã cầu khi thả Space
        if (Input.GetKeyUp(KeyCode.Space) && isBuilding)
        {
            DropBridge();
        }
    }

    #region 
    /// <summary>
    /// Bắt đầu dựng cầu tại vị trí của nhân vật, mọc theo hướng Y (thẳng đứng).
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void StartBuilding()
    {
        bridgeStartPosition = playerTransform.position; // Vị trí bắt đầu là vị trí nhân vật

        // Lấy cầu từ Object Pool và giữ hướng mặc định (thẳng đứng theo Y)
        currentBridge = bridgePool.GetObject(bridgeStartPosition, Quaternion.identity);
        bridgeScale = minScale.x; // Bắt đầu với scale tối thiểu
        currentBridge.transform.localScale = minScale;
        isBuilding = true;
        isGrowing = true;

        // Thêm hoặc cấu hình Rigidbody
        Rigidbody rb = currentBridge.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = currentBridge.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true; // Ban đầu là kinematic để không bị ảnh hưởng bởi trọng lực

        // Debug hướng của player
        Debug.Log("Player Forward: " + playerTransform.forward);
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
                bridgeScale = maxScale.x; // Giới hạn tối đa
                isGrowing = false; // Chuyển sang giảm
            }
        }
        else
        {
            bridgeScale -= bridgeGrowthSpeed * Time.deltaTime;
            if (bridgeScale <= minScale.x)
            {
                bridgeScale = minScale.x; // Giới hạn tối thiểu
                isGrowing = true; // Chuyển sang tăng
            }
        }

        // Cập nhật scale đồng đều trên cả 3 trục
        Vector3 newScale = new Vector3(bridgeScale, bridgeScale, bridgeScale);
        currentBridge.transform.localScale = newScale;

        // Đặt vị trí để cầu mọc theo hướng Y (thẳng đứng)
        currentBridge.transform.position = bridgeStartPosition + Vector3.up * (bridgeScale / 2f);
    }

    #region 
    /// <summary>
    /// Thả cầu để ngã xuống theo hướng nhìn của player.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void DropBridge()
    {
        isBuilding = false;
        Rigidbody rb = currentBridge.GetComponent<Rigidbody>();
        rb.isKinematic = false; // Cho phép cầu chịu tác động của trọng lực

        // Xoay cầu theo hướng nhìn của player khi ngã
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0; // Giữ trên mặt phẳng ngang
        playerForward.Normalize();
        currentBridge.transform.rotation = Quaternion.LookRotation(playerForward);

        currentBridge = null;
    }
}