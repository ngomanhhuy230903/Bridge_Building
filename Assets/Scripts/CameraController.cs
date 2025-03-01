using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player; // Tham chiếu đến nhân vật
    [SerializeField] private float distance = 1.65f; // Khoảng cách từ camera đến nhân vật (tính từ dữ liệu)
    [SerializeField] private float height = 1.232f; // Chiều cao của camera so với nhân vật (tính từ dữ liệu)
    [SerializeField] private float tiltAngle = 26.3f; // Góc nghiêng cố định của camera
    [SerializeField] private float rotationSpeed = 3f; // Tốc độ xoay camera bằng chuột
    [SerializeField] private float smoothSpeed = 10f; // Tốc độ mượt mà khi di chuyển camera

    private float currentAngleY; // Góc xoay hiện tại quanh trục Y

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho camera, lấy góc xoay ban đầu từ vị trí hiện tại.
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
        currentAngleY = transform.eulerAngles.y; // Lấy góc Y ban đầu của camera
    }

    #region 
    /// <summary>
    /// Cập nhật vị trí và xoay của camera trong mỗi frame để theo sau nhân vật với góc cố định.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void LateUpdate()
    {
        if (player == null) return;

        // Xử lý xoay camera bằng chuột
        if (Input.GetMouseButton(1)) // Nhấn chuột phải để xoay
        {
            currentAngleY += Input.GetAxis("Mouse X") * rotationSpeed;
        }

        // Tính toán vị trí mục tiêu của camera
        Quaternion rotation = Quaternion.Euler(0f, currentAngleY, 0f);
        Vector3 offset = new Vector3(0f, height, -distance); // Offset dựa trên chiều cao và khoảng cách
        Vector3 targetPosition = player.position + (rotation * offset);

        // Di chuyển camera mượt mà đến vị trí mục tiêu
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Đặt góc nghiêng cố định và xoay camera hướng về nhân vật
        transform.rotation = Quaternion.Euler(tiltAngle, currentAngleY, 0f);
    }
}