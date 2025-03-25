using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player; // Tham chiếu đến nhân vật
    [SerializeField] private float distance = 1.65f; // Khoảng cách từ camera đến nhân vật
    [SerializeField] private float height = 1.232f; // Chiều cao của camera so với nhân vật
    [SerializeField] private float tiltAngle = 26.3f; // Góc nghiêng cố định của camera
    [SerializeField] private float rotationSpeed = 3f; // Tốc độ xoay camera bằng chuột (vẫn giữ để tùy chọn)
    [SerializeField] private float smoothSpeed = 10f; // Tốc độ mượt mà khi di chuyển camera

    private float currentAngleY; // Góc xoay hiện tại quanh trục Y

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho camera, lấy góc xoay ban đầu từ player.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is not set in CameraController!");
            return;
        }
        // Khởi tạo góc Y dựa trên góc xoay của player
        currentAngleY = player.eulerAngles.y;
    }

    #region 
    /// <summary>
    /// Cập nhật vị trí và xoay của camera trong mỗi frame để theo sau và đồng bộ với góc xoay của player.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void LateUpdate()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;
        if (player == null) return;

        currentAngleY = player.eulerAngles.y;

        Quaternion rotation = Quaternion.Euler(0f, currentAngleY, 0f);
        Vector3 offset = new Vector3(0f, height, -distance);
        Vector3 targetPosition = player.position + (rotation * offset);

        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(tiltAngle, currentAngleY, 0f);
    }
}