using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 8f;

    public bool useVelocityJump = true;
    public float jumpForce = 14f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Debug")]
    public bool debugDraw = true;

    [Header("Animation")]
    [SerializeField] AnimsController animsController;
    [SerializeField] float animHorizontalDeadzone = 0.1f;

    [Header("Flip")]
    [SerializeField] float flipDeadzone = 0.1f;

    [Header("Fall settings")]
    [SerializeField] float fallThreshold = -0.1f;

    Rigidbody2D rb;
    float horizontal;
    bool jumpRequest;
    bool isGrounded;

    bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning($"{name}: Rigidbody2D должен быть Dynamic для прыжка (текущий тип: {rb.bodyType}).", this);

        if (groundLayer.value == 0)
            Debug.LogWarning($"{name}: LayerMask groundLayer не задан. Установите слой(и) земли или задайте маску.", this);

        if (animsController == null)
            animsController = GetComponentInChildren<AnimsController>();

        if (animsController == null && debugDraw)
            Debug.LogWarning($"{name}: AnimsController не найден. Привяжите в инспекторе.", this);

        facingRight = Mathf.Approximately(transform.localEulerAngles.y, 0f);
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpRequest = true;

        HandleFlip();
    }

    void FixedUpdate()
    {
        int mask = (groundLayer.value == 0) ? ~0 : groundLayer.value;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (jumpRequest)
        {
            if (isGrounded)
            {
                if (useVelocityJump)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                }
                else
                {
                    rb.velocity = new Vector2(rb.velocity.x, 0f);
                    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                }
            }
            else
            {
                if (debugDraw)
                    Debug.Log($"{name}: попытка прыжка, но игрок не на земле.", this);
            }

            jumpRequest = false;
        }

        UpdateAnimations();
    }

    void HandleFlip()
    {
        if (horizontal < -flipDeadzone && facingRight)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 180f, transform.localEulerAngles.z);
            facingRight = false;
        }
        else if (horizontal > flipDeadzone && !facingRight)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0f, transform.localEulerAngles.z);
            facingRight = true;
        }
    }

    void UpdateAnimations()
    {
        if (animsController == null)
            return;

        bool moving = Mathf.Abs(horizontal) > animHorizontalDeadzone;
        animsController.SetRunning(moving && isGrounded);

        if (isGrounded)
        {
            animsController.SetJumping(false);
            animsController.SetFalling(false);
            return;
        }

        float vy = rb.velocity.y;

        if (vy > 0.05f)
        {
            animsController.SetJumping(true);
            animsController.SetFalling(false);
        }
        else if (vy < fallThreshold)
        {
            animsController.SetJumping(false);
            animsController.SetFalling(true);
        }
        else
        {
            animsController.SetJumping(false);
            animsController.SetFalling(false);
        }
    }
}