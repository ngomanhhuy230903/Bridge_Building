using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float turnSpeed = 100f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float jumpCooldown = 1f;
    [SerializeField] float spikeSlowFactor = 0.5f;
    [SerializeField] float hammerKnockbackForce = 5f;
    [SerializeField] float speedBoostFactor = 2f;
    [SerializeField] float powerUpDuration = 5f;

    private bool isMoving = false;
    private bool isJumping = false;
    private float lastJumpTime;
    private float originalMoveSpeed;
    private bool isInvincible = false;
    private float powerUpEndTime;

    #region 
    /// <summary>
    /// Khởi tạo giá trị ban đầu cho PlayerController, thiết lập Rigidbody và Animator.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        lastJumpTime = -jumpCooldown;
        originalMoveSpeed = moveSpeed;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        Debug.Log("PlayerController Start | Rigidbody Kinematic: " + rb.isKinematic + " | Velocity: " + rb.velocity);
    }

    #region 
    /// <summary>
    /// Xử lý input người chơi để di chuyển, xoay và nhảy trong mỗi frame.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;

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

        if (Time.time >= powerUpEndTime && (moveSpeed != originalMoveSpeed || isInvincible))
        {
            ResetPowerUpEffects();
        }

        UpdateAnimation();
    }

    #region 
    /// <summary>
    /// Di chuyển nhân vật theo hướng forward dựa trên input người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void MoveCharacter(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
        Debug.Log("MoveCharacter | Direction: " + moveDirection + " | New Position: " + rb.position);
    }

    #region 
    /// <summary>
    /// Xoay nhân vật quanh trục Y dựa trên input người chơi.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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
    /// Thực hiện nhảy cho nhân vật bằng cách thêm lực lên trên.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
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
    /// Cập nhật trạng thái animation của nhân vật (Walk, Run, Jump).
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void UpdateAnimation()
    {
        bool isRunning = moveSpeed >= 3f && isMoving;
        animator.SetBool("IsWalking", isMoving && !isRunning);
        animator.SetBool("IsRunning", isRunning);

        if (isJumping && rb.velocity.y <= 0 && Physics.Raycast(transform.position, Vector3.down, 0.1f))
        {
            isJumping = false;
            Debug.Log("Player landed at: " + transform.position);
        }
    }

    #region 
    /// <summary>
    /// Xử lý va chạm với trap và PowerUp, cập nhật trạng thái nhân vật.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name + " | Layer: " + LayerMask.LayerToName(collision.gameObject.layer));

        if (!isInvincible)
        {
            if (collision.gameObject.CompareTag("SpikeTrap"))
            {
                moveSpeed = originalMoveSpeed * spikeSlowFactor;
                Invoke("ResetSpeed", 2f);
                animator.SetTrigger("Damage");
                GameManager.Instance.DecreaseHP();
                Debug.Log("Hit Spike Trap, speed reduced to: " + moveSpeed + " | Trigger 'Damage' set");
            }
            else if (collision.gameObject.CompareTag("HammerTrap"))
            {
                Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
                knockbackDirection.y = 0;
                rb.AddForce(knockbackDirection * hammerKnockbackForce + Vector3.up * jumpForce / 2f, ForceMode.Impulse);
                animator.SetTrigger("KO_big");
                GameManager.Instance.DecreaseHP();
                Debug.Log("Hit Hammer Trap, knocked back! | Trigger 'KO_big' set");
            }
        }

        if (collision.gameObject.CompareTag("SpeedPowerUp"))
        {
            ActivateSpeedBoost();
            collision.gameObject.SetActive(false);
            Debug.Log("Collected Speed PowerUp");
        }
        else if (collision.gameObject.CompareTag("InvincibilityPowerUp"))
        {
            ActivateInvincibility();
            collision.gameObject.SetActive(false);
            Debug.Log("Collected Invincibility PowerUp");
        }
    }

    #region 
    /// <summary>
    /// Kích hoạt hiệu ứng tăng tốc khi nhặt SpeedPowerUp.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void ActivateSpeedBoost()
    {
        moveSpeed = originalMoveSpeed * speedBoostFactor;
        powerUpEndTime = Time.time + powerUpDuration;
    }

    #region 
    /// <summary>
    /// Kích hoạt trạng thái bất tử khi nhặt InvincibilityPowerUp.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void ActivateInvincibility()
    {
        isInvincible = true;
        powerUpEndTime = Time.time + powerUpDuration;
    }

    #region 
    /// <summary>
    /// Reset trạng thái nhân vật khi PowerUp hết hiệu lực.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void ResetPowerUpEffects()
    {
        moveSpeed = originalMoveSpeed;
        isInvincible = false;
        Debug.Log("PowerUp effects ended");
    }

    #region 
    /// <summary>
    /// Reset tốc độ di chuyển về giá trị ban đầu khi hết hiệu ứng giảm tốc.
    /// Người tạo: Huynm, ngày tạo: 2025-02-28
    /// Ngày sửa: 2025-03-02
    /// </summary>
    #endregion
    private void ResetSpeed()
    {
        if (Time.time >= powerUpEndTime)
        {
            moveSpeed = originalMoveSpeed;
        }
        Debug.Log("Speed restored to: " + moveSpeed);
    }
}