using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public bool isHeadBlocked;
    public bool isHanging;
    
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

    [Header("环境监测")]
    public LayerMask groundLayer;
    
    private bool jumpPress;       //跳跃
    private bool crouchHoldPress; //下蹲

    public float footOffset = 0.4f;//脚偏移
    public float headClearance = 0.5f;//头部 clearance
    public float groundDistance = 0.2f ;//检测距离
    private float eyeHeight    = 1.5f; //角色眼睛高度
    private float grabDistance = 0.4f; //检测墙壁射线距离(侧面)
    private float reachOffset  = 0.7f; //检测墙的射线距离(头顶)
    public float hangingJumpForce = 15f;//挂起时跳跃力
    
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

    RaycastHit2D raycastHit(Vector2 offset , Vector2 direction , float length , LayerMask  layer)
    {
        Vector2 position = transform.position;
        Vector2 position2 = position + offset;//起点坐标
        RaycastHit2D check = Physics2D.Raycast(position2, direction, length, layer);
        Debug.DrawRay (position2, direction*length, Color.white,length);
        return check;
    }
    
    void PhysicsCheck()
    {
        RaycastHit2D leftcheck = raycastHit(new Vector2(-footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        RaycastHit2D rightcheck = raycastHit(new Vector2(footOffset, 0f), Vector2.down, groundDistance, groundLayer);
        if (leftcheck && rightcheck)//also can use "isGround=leftcheck && rightcheck"
        {
            isGround=true;
        }
        else
        {
            isGround=false;
        }
        RaycastHit2D headcheck = raycastHit(new Vector2(0,Standsize.y), Vector2.up, headClearance, groundLayer);//检测头部
        isHeadBlocked=headcheck;
        float eyedirection = transform.localScale.x;
        Vector2 direction = new Vector2(eyedirection, 0);
        RaycastHit2D heightcheck = raycastHit(new Vector2(footOffset*eyedirection, Standsize.y), direction, grabDistance, groundLayer);
        RaycastHit2D eyescheck = raycastHit(new Vector2(footOffset*eyedirection, eyeHeight), direction, grabDistance, groundLayer);
        RaycastHit2D wallcheck = raycastHit(new Vector2(reachOffset*eyedirection, Standsize.y), Vector2.down, grabDistance, groundLayer);
        //这里是检测角色是否在墙
        if (!isGround && rb.velocity.y < 0 && wallcheck && eyescheck && !heightcheck)
        {
            isHanging = true;
            rb.bodyType = RigidbodyType2D.Static;
            Vector2 oneonefourfiveonefour = transform.position ;
            oneonefourfiveonefour.x += (eyescheck.distance-0.05f)*eyedirection;
            oneonefourfiveonefour.y -= wallcheck.distance;
            transform.position = oneonefourfiveonefour;
        }//挂起
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
        if (isHanging)
        {
            if (jumpPress)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                isHanging=false;
                rb.AddForce(new Vector2(0, hangingJumpForce), ForceMode2D.Impulse);
            }
            if (crouchHoldPress)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                isHanging = false;
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
        else if (isCrouching&& !crouchHoldPress&& !isHeadBlocked)

        {
            // 贼也站起来了是吧
            Standup();
        }
        else if (isCrouching&& !isGround&& !isHeadBlocked)
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
