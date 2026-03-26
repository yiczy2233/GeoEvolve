using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float decceleration = 50f;

    [Header("跳跃参数")]
    public float jumpForce = 12f;
    public float gravityScale = 3f; // 增加重力感，让下落更干脆
    public float fallMultiplier = 1.5f; // 下落时的额外重力倍率

    [Header("环境检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool canJump = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    // 由 Player Input 组件的消息回调调用
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        CheckEnvironment();
        ApplyMovement();
        ApplyGravityTweaks();
    }

    private void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void ApplyMovement()
    {
        // 计算目标速度与当前速度的差值
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        
        // 根据是否在地面应用不同的加速度
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);
        
        rb.AddForce(movement * Vector2.right);

        // 处理角色朝向翻转
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
        }
    }

    private void Jump()
    {
        // 瞬间清空垂直速度以确保跳跃高度一致
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void ApplyGravityTweaks()
    {
        // 增加下落时的重力，让跳跃手感不那么“飘”
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = gravityScale * fallMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    // 方便在编辑器里查看地面检测范围
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}