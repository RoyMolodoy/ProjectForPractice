using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    [Tooltip("If true, jumpForce is applied as an instant velocity; otherwise as an impulse force.")]
    public bool useVelocityJump = true;
    public float jumpForce = 14f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Debug")]
    public bool debugDraw = true;

    Rigidbody2D rb;
    float horizontal;
    bool jumpRequest;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Попробуем автоматически разместить groundCheck под игроком по Collider2D
        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.parent = transform;

            var col = GetComponent<Collider2D>();
            if (col != null)
            {
                // ставим немного ниже контура коллайдера
                var offsetY = -(col.bounds.extents.y + 0.05f);
                go.transform.localPosition = new Vector3(0, offsetY, 0);
            }
            else
            {
                go.transform.localPosition = new Vector3(0, -0.5f, 0);
            }

            groundCheck = go.transform;
        }

        // Предупреждения для распространённых проблем
        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning($"{name}: Rigidbody2D должен быть Dynamic для прыжка (текущий тип: {rb.bodyType}).", this);

        // Если groundLayer не задан — предупреждение (частая причина "не работает прыжок")
        if (groundLayer.value == 0)
            Debug.LogWarning($"{name}: LayerMask groundLayer не задан. Установите слой(и) земли или задайте маску.", this);
    }

    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal"); // -1,0,1

        if (Input.GetButtonDown("Jump"))
            jumpRequest = true;
    }

    void FixedUpdate()
    {
        int mask = (groundLayer.value == 0) ? ~0 : groundLayer.value; // если не задан - проверяем все слои
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);

        // Движение по X
        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (jumpRequest)
        {
            if (isGrounded)
            {
                // Выполняем прыжок
                if (useVelocityJump)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                }
                else
                {
                    // Сбрасываем вертикальную скорость, затем импульс
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
    }

    void OnDrawGizmos()
    {
        if (!debugDraw) return;

        if (groundCheck == null)
        {
            // Попытка найти child GroundCheck для отрисовки если есть
            var t = transform.Find("GroundCheck");
            if (t != null) groundCheck = t;
        }

        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}