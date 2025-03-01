using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    [SerializeField] float moveSpeed = 3f; // Tốc độ di chuyển cơ bản
    [SerializeField] float turnSpeed = 100f; // Tốc độ xoay
    [SerializeField] float jumpForce = 5f; // Lực nhảy
    [SerializeField] float jumpCooldown = 1f; // Thời gian chờ giữa các lần nhảy
    [SerializeField] float spikeSlowFactor = 0.5f; // Hệ số giảm tốc khi chạm bẫy gai
    [SerializeField] float hammerKnockbackForce = 5f; // Lực đẩy lùi khi chạm máy cưa
    private bool isMoving = false;
    private bool isJumping = false;
    private float lastJumpTime; // Thời gian nhảy cuối cùng
    private float originalMoveSpeed; // Lưu tốc độ ban đầu để khôi phục

    #region 
    /// <summary>
    /// Khởi tạo các giá trị ban đầu cho nhân vật, bao gồm Rigidbody và Animator.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        lastJumpTime = -jumpCooldown;
        originalMoveSpeed = moveSpeed; // Lưu tốc độ gốc
    }

    #region 
    /// <summary>
    /// Xử lý điều khiển nhân vật trong mỗi frame, bao gồm di chuyển, xoay và nhảy.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void Update()
    {
        float moveInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");

        if (moveInput != 0)
        {
            MoveCharacter(moveInput);
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        if (turnInput != 0)
        {
            TurnCharacter(turnInput);
        }

        if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastJumpTime + jumpCooldown && !isJumping)
        {
            Jump();
        }

        UpdateAnimation();
    }

    #region 
    /// <summary>
    /// Di chuyển nhân vật về phía trước hoặc phía sau dựa trên input.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: N/A
    /// </summary>
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
    #endregion
    private void TurnCharacter(float input)
    {
        float rotation = input * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    #region 
    /// <summary>
    /// Thực hiện hành động nhảy cho nhân vật và kích hoạt animation.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;
        animator.SetTrigger("Jump");
        Debug.Log("Player jumped at: " + transform.position + " | Trigger 'Jump' set");
    }

    #region 
    /// <summary>
    /// Cập nhật trạng thái animation của nhân vật dựa trên chuyển động và nhảy.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void UpdateAnimation()
    {
        animator.SetBool("IsWalking", isMoving);

        if (isJumping && rb.velocity.y <= 0 && Physics.Raycast(transform.position, Vector3.down, 0.1f))
        {
            isJumping = false;
            Debug.Log("Player landed at: " + transform.position);
        }
    }

    #region 
    /// <summary>
    /// Xử lý va chạm với chướng ngại vật trên cầu.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("SpikeTrap"))
        {
            moveSpeed = originalMoveSpeed * spikeSlowFactor; // Giảm tốc khi chạm bẫy gai
            Invoke("ResetSpeed", 2f); // Khôi phục tốc độ sau 2 giây
            Debug.Log("Hit Spike Trap, speed reduced to: " + moveSpeed);
        }
        else if (collision.gameObject.CompareTag("HammerTrap"))
        {
            Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
            knockbackDirection.y = 0; // Giữ ngang
            rb.AddForce(knockbackDirection * hammerKnockbackForce + Vector3.up * jumpForce / 2f, ForceMode.Impulse);
            Debug.Log("Hit Saw Blade, knocked back!");
        }
    }

    #region 
    /// <summary>
    /// Khôi phục tốc độ di chuyển sau khi bị giảm bởi bẫy gai.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-02-28
    /// </summary>
    #endregion
    private void ResetSpeed()
    {
        moveSpeed = originalMoveSpeed;
        Debug.Log("Speed restored to: " + moveSpeed);
    }
}