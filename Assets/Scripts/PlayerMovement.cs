using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
// 移动脚本
{
    private Rigidbody2D rb;
    private BoxCollider2D bc;
    [Header("移动参数")]
    public float speed = 8f;
    public float crouchSpeedDivisor = 3f ;

    private float xVelocity;

    [Header("状态")] 
    public bool isCrouching ;
    public bool isGround;
    public bool isJumping;
    
    // 这里是碰撞体尺寸
    private Vector2 Standsize;
    private Vector2 Crouchsize;
    private Vector2 StandOffset;
    private Vector2 CrouchOffset;
    [Header("跳跃参数")] 
    public float jumpForce = 6.3f;
    public float jumpHoldForce = 1.9f;
    public float jumpHoldDuration = 0.1f;
    public float crouchJumpBoost = 2.5f;

    private float jumpTime;

    [Header("环境监测")] public LayerMask groundLayer;
    
    private bool jumpPress;       //跳跃
    private bool crouchHoldPress; //下蹲
    
    // 厘米君的第一个C#捏
    // 喵
    // 2025.6.22 凌晨3:37还在打qq电话的cm和huohua二人留
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bc = GetComponent<BoxCollider2D>();
        Standsize = bc.size;
        StandOffset = bc.offset;
        Crouchsize = new Vector2(Standsize.x, Standsize.y / 2f);
        CrouchOffset = new Vector2(StandOffset.x, StandOffset.y / 2f);
    }

    // Update is called once per frame
    void Update()
    {
        jumpPress = Input.GetKey(KeyCode.Space);
        crouchHoldPress = Input.GetKey(KeyCode.LeftShift)|| Input.GetKey(KeyCode.S);
    }
    void FixedUpdate()
    {
        PhysicsCheck();
        GroundMovement(); 
        Jump();
    }

    void PhysicsCheck()
    {
        isGround = bc .IsTouchingLayers(groundLayer);
    }

    void Jump()
    {
        if (jumpPress && isGround && !isJumping)
        {
            if (isCrouching)
            {
                Standup();
                rb.AddForce(Vector2.up * crouchJumpBoost, ForceMode2D.Impulse);
            }

            //打过大括号huohua说就不用分号了
            isGround=false;//疑似无必要
            isJumping=true;
            Vector2 jumpForceVector2 = new Vector2(0, jumpForce);
            rb.AddForce(jumpForceVector2, ForceMode2D.Impulse);//ForceMode2D.Impulse:刚体立即移动,给对象一个瞬时初始力
            jumpTime=Time.time + jumpHoldDuration;
        }
        else if (isJumping)
        {
            if (jumpPress)
            {
                rb.AddForce(new Vector2(0, jumpHoldForce), ForceMode2D.Impulse);
            }
            if (Time.time > jumpTime)
            {
                isJumping = false;
            }
        }
    }
    private void GroundMovement()
    // 移动逻辑
    {
        if (crouchHoldPress&& !isCrouching&& isGround)
        {
            // 执行亲爱的下蹲捏
            Crouch();
        }
        else if (isCrouching&& !crouchHoldPress)

        {
            // 贼也站起来了是吧
            Standup();
        }
        else if (isCrouching&& !isGround)
        { 
            Standup();
        }
        xVelocity = Input.GetAxis("Horizontal") ;
        if (isCrouching)
            // 蹲下时水平速度要被 divisor 缩小
        {
            xVelocity = xVelocity / crouchSpeedDivisor;
        }
        rb.velocity = new Vector2(xVelocity * speed, rb.velocity.y);
        // 翻转
        FilpDirection();
    }

    private void FilpDirection()
    // 判断是否翻转角色方向
    {
        if (xVelocity > 0)
        {
            transform.localScale = new Vector3(1, 1, 0);
        }
        else if (xVelocity < 0)
        {
            transform.localScale = new Vector3(-1, 1, 0);
        }
    }

    private void Crouch()
    // 判断角色是否蹲下
    {
        isCrouching = true;
        bc.size = Crouchsize;
        bc.offset = CrouchOffset;
    }

    private void Standup()
    // 判断站起
    {
        isCrouching = false;
        bc.size = Standsize;
        bc.offset = StandOffset;
    }
}
