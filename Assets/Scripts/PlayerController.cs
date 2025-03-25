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
    [SerializeField] float slideSpeed = 6f;
    [SerializeField] float slideDuration = 0.5f;
    [SerializeField] float slideCooldown = 1f;

    private bool isMoving = false;
    private bool isJumping = false;
    private bool isSliding = false;
    private float lastJumpTime;
    private float lastSlideTime;
    private float originalMoveSpeed;
    private bool isInvincible = false;
    private float powerUpEndTime;
    private float slideEndTime;
    private Quaternion targetRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        lastJumpTime = -jumpCooldown;
        lastSlideTime = -slideCooldown;
        originalMoveSpeed = moveSpeed;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        animator.applyRootMotion = false;
        targetRotation = transform.rotation;
        Debug.Log("PlayerController Start | IsKinematic: " + rb.isKinematic + " | Constraints: " + rb.constraints + " | ApplyRootMotion: " + animator.applyRootMotion);
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGamePaused()) return;

        float moveInput = Input.GetAxisRaw("Vertical");
        float turnInput = Input.GetAxisRaw("Horizontal");
        Debug.Log("Turn Input: " + turnInput);

        if (moveInput != 0 && !isSliding)
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

        if (Input.GetKeyDown(KeyCode.J) && Time.time >= lastJumpTime + jumpCooldown && !isJumping && !isSliding)
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.L) && Time.time >= lastSlideTime + slideCooldown && !isJumping && !isSliding)
        {
            Slide();
        }

        if (isSliding && Time.time >= slideEndTime)
        {
            EndSlide();
        }

        if (Time.time >= powerUpEndTime && (moveSpeed != originalMoveSpeed || isInvincible))
        {
            ResetPowerUpEffects();
        }

        UpdateAnimation();
    }

    void LateUpdate()
    {
        transform.rotation = targetRotation;
        rb.rotation = targetRotation;
        Debug.Log("LateUpdate | Player Rotation Y: " + transform.eulerAngles.y);
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
        targetRotation = targetRotation * turnRotation;
        Debug.Log("Turning | Input: " + input + " | Rotation to Add: " + rotation + " | Target Y: " + targetRotation.eulerAngles.y);
    }

    private void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isJumping = true;
        lastJumpTime = Time.time;
        animator.SetTrigger("Jump");
        Debug.Log("Player jumped at: " + transform.position + " | Trigger 'Jump' set");
    }

    private void Slide()
    {
        isSliding = true;
        lastSlideTime = Time.time;
        slideEndTime = Time.time + slideDuration;
        animator.SetTrigger("Slide");
        rb.velocity = transform.forward * slideSpeed;
        Debug.Log("Player slid at: " + transform.position + " | Trigger 'Slide' set");
    }

    private void EndSlide()
    {
        isSliding = false;
        rb.velocity = Vector3.zero;
        Debug.Log("Slide ended at: " + transform.position);
    }

    private void UpdateAnimation()
    {
        bool isRunning = moveSpeed >= 3f && isMoving && !isSliding;
        animator.SetBool("IsWalking", isMoving && !isRunning && !isSliding);
        animator.SetBool("IsRunning", isRunning);

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
            if (collision.gameObject.CompareTag("gate") && !isSliding)
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
        else if (collision.gameObject.CompareTag("HealthPowerUp"))
        {
            ActivateHealthBoost();
            collision.gameObject.SetActive(false);
            Debug.Log("Collected Health PowerUp");
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

    private void ActivateHealthBoost()
    {
        GameManager.Instance.IncreaseHP(); // Gọi hàm tăng HP từ GameManager
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