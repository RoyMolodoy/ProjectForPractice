using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Skills")]
    public bool canDoubleJump = false;
    public bool canDash = false;

    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 14f;
    public float doubleJumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    Rigidbody2D rb;

    float horizontal;
    bool isGrounded;
    bool jumpRequest;
    bool doubleJumpAvailable;

    bool dashRequest;
    bool isDashing;
    float dashCooldownTimer;
    float defaultGravity;

    float lastMoveDirection = 1f;

    private AudioManager audioManager;
    AnimsController anims;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; 
        audioManager = GetComponent<AudioManager>();
    }
    private void Start()
    {
        anims = GetComponent<AnimsController>();
    }

    void Update()
    {
        if (isDashing) return;

        bool attackLock = PlayerAttack.IsAttacking;

        float input = 0f;

        //  ¬¿∆ÕŒ: INPUT ¬—≈√ƒ¿ —◊»“€¬¿≈Ã ƒÀﬂ ¿Õ»Ã¿÷»…
        if (Input.GetKey(rightKey)) input += 1f;
        if (Input.GetKey(leftKey)) input -= 1f;

        if (input != 0)
            lastMoveDirection = input;

        //  ƒ¬»∆≈Õ»≈ ¡ÀŒ »–”≈Ã, ÕŒ Õ≈ ”¡»¬¿≈Ã INPUT
        if (!attackLock)
        {
            horizontal = input;

            if (Input.GetKeyDown(jumpKey))
                jumpRequest = true;
        }
        else
        {
            horizontal = 0f;
        }

        if (canDash && Input.GetKeyDown(dashKey) &&
            dashCooldownTimer <= 0f && !isDashing)
        {
            dashRequest = true;
            if (anims != null)
                anims.SetDashing();
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        HandleFlip();
        UpdateAnimations(); //  ¬Œ“ ›“Œ ¬¿∆ÕŒ
    }

    void FixedUpdate()
    {
        if (dashRequest)
        {
            StartCoroutine(PerformDash());
            dashRequest = false;
            return;
        }

        if (isDashing) return;

        int mask = (groundLayer.value == 0) ? ~0 : groundLayer.value;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            mask);

        if (isGrounded)
            doubleJumpAvailable = true;

        if (PlayerAttack.IsAttacking)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (jumpRequest)
        {
            if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                audioManager.JumpSound();
            }
            else if (canDoubleJump && doubleJumpAvailable)
            {
                rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
                doubleJumpAvailable = false;
                audioManager.JumpSound();
            }

            jumpRequest = false;
        }
    }

    void HandleFlip()
    {
        if (lastMoveDirection > 0)
            transform.localEulerAngles = new Vector3(0, 0, 0);
        else if (lastMoveDirection < 0)
            transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    void UpdateAnimations()
    {
        if (PlayerAttack.IsAttacking)
        {
            // ‚Ó ‚ÂÏˇ ‡Ú‡ÍË ó Õ≈ ÚÓ„‡ÂÏ ‡ÌËÏ‡ˆË˛ ‰‚ËÊÂÌËˇ
            return;
        }
        if (anims == null) return;

        bool moving = Mathf.Abs(horizontal) > 0.1f;

        anims.SetRunning(moving && isGrounded);

        if (!isGrounded)
        {
            float vy = rb.velocity.y;

            if (vy > 0.1f)
            {
                anims.SetJumping(true);
                anims.SetFalling(false);
            }
            else
            {
                anims.SetJumping(false);
                anims.SetFalling(true);
            }
        }
        else
        {
            anims.SetJumping(false);
            anims.SetFalling(false);
        }
    }

    IEnumerator PerformDash()
    {
        isDashing = true;
        rb.gravityScale = 0f;
        audioManager.DashSound();

        float dir = lastMoveDirection;
        rb.velocity = new Vector2(dir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = defaultGravity;
        isDashing = false;

        dashCooldownTimer = dashCooldown;
    }
}