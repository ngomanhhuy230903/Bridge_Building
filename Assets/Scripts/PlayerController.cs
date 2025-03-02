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
    [SerializeField] float hammerKnockbackForce = 5f; // Lực đẩy lùi khi chạm búa
    [SerializeField] float speedBoostFactor = 2f; // Hệ số tăng tốc khi thu thập PowerUp
    [SerializeField] float powerUpDuration = 5f; // Thời gian hiệu lực của PowerUp

    private bool isMoving = false;
    private bool isJumping = false;
    private float lastJumpTime;
    private float originalMoveSpeed;
    private bool isInvincible = false; // Trạng thái bất tử
    private float powerUpEndTime; // Thời gian kết thúc PowerUp

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        lastJumpTime = -jumpCooldown;
        originalMoveSpeed = moveSpeed;
    }

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

        if (Time.time >= powerUpEndTime && (moveSpeed != originalMoveSpeed || isInvincible))
        {
            ResetPowerUpEffects();
        }

        UpdateAnimation();
    }

    private void MoveCharacter(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
    }

    private void TurnCharacter(float input)
    {
        float rotation = input * turnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler(0f, rotation, 0f);
        rb.MoveRotation(rb.rotation * turnRotation);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;
        animator.SetTrigger("Jump");
        Debug.Log("Player jumped at: " + transform.position + " | Trigger 'Jump' set");
    }

    private void UpdateAnimation()
    {
        // Kiểm tra tốc độ để chuyển sang "Run"
        bool isRunning = moveSpeed >= 3f && isMoving; // Chỉ chạy khi di chuyển và tốc độ > 4
        animator.SetBool("IsWalking", isMoving && !isRunning); // Walk chỉ khi không Run
        animator.SetBool("IsRunning", isRunning); // Run khi tốc độ > 4

        if (isJumping && rb.velocity.y <= 0 && Physics.Raycast(transform.position, Vector3.down, 0.1f))
        {
            isJumping = false;
            Debug.Log("Player landed at: " + transform.position);
        }
    }

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
                Debug.Log("Hit Spike Trap, speed reduced to: " + moveSpeed + " | Trigger 'Damage' set");
            }
            else if (collision.gameObject.CompareTag("HammerTrap"))
            {
                Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
                knockbackDirection.y = 0;
                rb.AddForce(knockbackDirection * hammerKnockbackForce + Vector3.up * jumpForce / 2f, ForceMode.Impulse);
                animator.SetTrigger("KO_big");
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

    private void ActivateSpeedBoost()
    {
        moveSpeed = originalMoveSpeed * speedBoostFactor;
        powerUpEndTime = Time.time + powerUpDuration;
    }

    private void ActivateInvincibility()
    {
        isInvincible = true;
        powerUpEndTime = Time.time + powerUpDuration;
    }

    private void ResetPowerUpEffects()
    {
        moveSpeed = originalMoveSpeed;
        isInvincible = false;
        Debug.Log("PowerUp effects ended");
    }

    private void ResetSpeed()
    {
        if (Time.time >= powerUpEndTime)
        {
            moveSpeed = originalMoveSpeed;
        }
        Debug.Log("Speed restored to: " + moveSpeed);
    }
}