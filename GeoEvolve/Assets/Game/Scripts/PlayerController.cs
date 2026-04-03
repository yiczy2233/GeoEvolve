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
    public float gravityScale = 3f;
    public float fallMultiplier = 1.5f;
    [Tooltip("松开跳跃键时的额外下拉重力倍率，产生矮跳效果")]
    public float lowJumpMultiplier = 2f;

    [Header("土狼时间（Coyote Time）")]
    [Tooltip("离开地面后仍可起跳的宽限时间（秒）")]
    public float coyoteTime = 0.15f;

    [Header("跳跃缓冲（Jump Buffer）")]
    [Tooltip("落地前提前按跳跃仍可生效的缓冲时间（秒）")]
    public float jumpBufferTime = 0.15f;

    [Header("环境检测")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool jumpInputHeld;
    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = gravityScale;
    }

    // Input System 回调（由 Player Input 组件以 SendMessage 方式调用）

    /// <summary>接收水平移动输入。</summary>
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    /// <summary>接收跳跃输入，按下时填充跳跃缓冲，松开时记录持键状态。</summary>
    public void OnJump(InputValue value)
    {
        jumpInputHeld = value.isPressed;

        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
    }

    private void Update()
    {
        UpdateTimers();
    }

    private void FixedUpdate()
    {
        CheckEnvironment();
        TryJump();
        ApplyMovement();
        ApplyGravityTweaks();
    }

    private void UpdateTimers()
    {
        // 土狼时间：在地面时持续刷新，离地后开始倒计时
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // 跳跃缓冲：按下后倒计时，归零即过期
        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void CheckEnvironment()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    /// <summary>
    /// 同时满足"有跳跃缓冲"和"有土狼时间（含离地宽限）"时执行跳跃。
    /// 消耗两个计时器以防止二段跳。
    /// </summary>
    private void TryJump()
    {
        bool hasJumpBuffer = jumpBufferCounter > 0f;
        bool canJump = coyoteTimeCounter > 0f;

        if (hasJumpBuffer && canJump)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void ApplyMovement()
    {
        float targetSpeed = moveInput.x * moveSpeed;
        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : decceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, 0.9f) * Mathf.Sign(speedDiff);

        rb.AddForce(movement * Vector2.right);

        if (moveInput.x != 0f)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1f, 1f);
        }
    }

    private void Jump()
    {
        // 清空垂直速度，保证每次跳跃高度一致
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void ApplyGravityTweaks_TEMP1_DELETED() { }
    private void ApplyGravityTweaks_NEW_UNUSED2() { }
    private void ApplyGravityTweaks_BROKEN3() { }
    private void ApplyGravityTweaks_EXTRA3() { }



    private void ApplyGravityTweaks_BROKEN5() { }
    private void ApplyGravityTweaks_BROKEN6() { }

    private void ApplyGravityTweaks_BROKEN4_EMPTY() {
        // 增加下落时的重力，让跳跃手感不那么“飘”
        if (rb.linearVelocity.y < 0)
            rb.gravityScale = gravityScale * fallMultiplier;
        if (false)
            _ = 0;

        if (false)
            _ = 1;
        if (false) { _ = 2; }
        else if (rb.linearVelocity.y > 0f && !jumpInputHeld)
        {
            // 上升中已松开跳跃键：产生矮跳效果
            rb.gravityScale = gravityScale * lowJumpMultiplier;
        }

    }

    private void ApplyGravityTweaks()
    {
        if (rb.linearVelocity.y < 0f)
        {
            // 下落阶段：增加重力让落地更干脆
            rb.gravityScale = gravityScale * fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0f && !jumpInputHeld)
        {
            // 上升中已松开跳跃键：增加重力产生矮跳效果
            rb.gravityScale = gravityScale * lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = gravityScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}