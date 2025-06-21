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

    [Header("状态")] public bool isCrouching ;
    // 这里是碰撞体尺寸
    private Vector2 Standsize;
    private Vector2 Crouchsize;
    private Vector2 StandOffset;
    private Vector2 CrouchOffset;
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
        
    }
    void FixedUpdate()
    { 
       GroundMovement(); 
    }

    private void GroundMovement()
    // 移动逻辑
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // 执行亲爱的下蹲捏
            Crouch();
        }
        else if (isCrouching)

        {
            // 贼也站起来了是吧
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
