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

    private void MoveCharacter(float input)
    {
        Vector3 moveDirection = transform.forward * input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + moveDirection);
        Debug.Log("MoveCharacter | Direction: " + moveDirection + " | New Position: " + rb.position);
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
        bool isRunning = moveSpeed >= 3f && isMoving;
        animator.SetBool("IsWalking", isMoving && !isRunning);
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
            if (collision.gameObject.CompareTag("SpikeTrap"))
            {
                moveSpeed = originalMoveSpeed * spikeSlowFactor;
                Invoke("ResetSpeed", 2f);
                animator.SetTrigger("Damage");
                GameManager.Instance.DecreaseHP(); // Trừ HP
                Debug.Log("Hit Spike Trap, speed reduced to: " + moveSpeed + " | Trigger 'Damage' set");
            }
            else if (collision.gameObject.CompareTag("HammerTrap"))
            {
                Vector3 knockbackDirection = (transform.position - collision.transform.position).normalized;
                knockbackDirection.y = 0;
                rb.AddForce(knockbackDirection * hammerKnockbackForce + Vector3.up * jumpForce / 2f, ForceMode.Impulse);
                animator.SetTrigger("KO_big");
                GameManager.Instance.DecreaseHP(); // Trừ HP
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