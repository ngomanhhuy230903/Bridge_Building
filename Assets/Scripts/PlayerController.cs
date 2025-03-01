using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    [SerializeField] float moveSpeed = 3f; // Tốc độ di chuyển cơ bản
    [SerializeField] float turnSpeed = 100f; // Tốc độ xoay
    private bool isMoving = false;

    #region 
    /// <summary>
    /// Khởi tạo các giá trị ban đầu cho nhân vật, bao gồm Rigidbody và Animator.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    #region 
    /// <summary>
    /// Xử lý điều khiển nhân vật trong mỗi frame, bao gồm di chuyển và xoay.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    void Update()
    {
        // Lấy input từ người chơi
        float moveInput = Input.GetAxisRaw("Vertical"); // W/S hoặc Up/Down Arrow
        float turnInput = Input.GetAxisRaw("Horizontal"); // A/D hoặc Left/Right Arrow

        // Điều khiển di chuyển
        if (moveInput != 0)
        {
            MoveCharacter(moveInput);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        // Điều khiển xoay
        if (turnInput != 0)
        {
            TurnCharacter(turnInput);
        }

        // Cập nhật animation
        UpdateAnimation();
    }

    #region 
    /// <summary>
    /// Di chuyển nhân vật về phía trước hoặc phía sau dựa trên input.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    /// <param name="input">Giá trị input từ người chơi (-1 hoặc 1)</param>
    #endregion
    private void MoveCharacter(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    #region 
    /// <summary>
    /// Xoay nhân vật sang trái hoặc phải dựa trên input.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    /// <param name="input">Giá trị input từ người chơi (-1 hoặc 1)</param>
    #endregion
    private void TurnCharacter(float input)
    {
        float rotation = input * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    #region 
    /// <summary>
    /// Cập nhật trạng thái animation của nhân vật dựa trên chuyển động.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
    #endregion
    private void UpdateAnimation()
    {
        animator.SetBool("IsWalking", isMoving);
    }
}