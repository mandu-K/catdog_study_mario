using UnityEngine;

// 마리오 기본 조작: 좌우 이동(가속/감속) + 가변 점프 + 바닥 감지
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동")]
    public float moveSpeed = 6f;        // 최대 이동 속도
    public float acceleration = 30f;    // 가속도
    public float deceleration = 40f;    // 감속도(방향키 뗐을 때)

    [Header("점프")]
    public float jumpForce = 12f;         // 점프 초기 힘
    public float lowJumpMultiplier = 3f;  // 버튼 짧게 누르면 빨리 떨어지게(짧은 점프)
    public float fallMultiplier = 2.5f;   // 낙하할 때 더 빨리 떨어지게(마리오 특유의 묵직함)

    [Header("바닥 감지")]
    public Transform groundCheck;   // 발밑에 둘 빈 오브젝트
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;   // "Ground" 레이어 선택

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private BoxCollider2D col;
    private float moveInput;
    private bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;   // 부딪혀도 안 넘어지게
    }

    void Start()
    {
        UpdateSprite(); // 시작할 때 기본 스프라이트 설정

        // Edit Collider로 맞춰둔 "작은 마리오" 크기를 자동으로 기억 (나중에 다시 작아질 때 쓸 수 있게)
        if (col != null)
        {
            smallColliderSize = col.size;
            smallColliderOffset = col.offset;
        }
    }

    [Header("죽음 판정")]
    public float fallDeathY = -10f; // 이 Y좌표보다 낮아지면 낙사 처리

    [Header("성장")]
    public bool isBig = false;          // 버섯 먹었는지

    [Header("스프라이트")]
    public Sprite smallLeftSprite;
    public Sprite smallRightSprite;
    public Sprite bigLeftSprite;
    public Sprite bigRightSprite;
    private bool facingRight = true;

    [Header("성장 시 자동 보정")]
    // "큰 마리오 콜라이더 크기/위치"를 손으로 입력할 필요 없이,
    // 스프라이트의 실제 높이 차이를 계산해서 자동으로 위로 자라게 하고 발 위치를 고정함
    private Vector2 smallColliderSize;
    private Vector2 smallColliderOffset;

    void Update()
    {
        // 게임오버/클리어 상태면 조작을 멈추고 물리도 멈춤 (무한 낙하 방지)
        if (GameManager.Instance != null &&
            (GameManager.Instance.isGameOver || GameManager.Instance.isCleared))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false; // 더 이상 물리 연산 안 함 (그 자리에 완전히 멈춤)
            return;
        }

        // 낙사 체크: 구멍에 빠져서 화면 아래로 떨어지면 게임오버
        if (transform.position.y < fallDeathY)
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
            return; // 죽었으면 이후 조작 무시
        }

        // 입력 받기 (A/D 또는 좌우 화살표)
        moveInput = Input.GetAxisRaw("Horizontal");

        // 바라보는 방향에 따라 스프라이트 교체 (작은/큰 마리오 x 좌우)
        if (moveInput > 0) facingRight = true;
        else if (moveInput < 0) facingRight = false;
        UpdateSprite();

        // 점프: 바닥에 있을 때만
        if (Input.GetButtonDown("Jump") && isGrounded)
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void UpdateSprite()
    {
        if (sr == null) return;
        if (isBig)
            sr.sprite = facingRight ? bigRightSprite : bigLeftSprite;
        else
            sr.sprite = facingRight ? smallRightSprite : smallLeftSprite;
    }

    // 버섯을 먹었을 때 Mushroom 스크립트에서 호출
    public void Grow()
    {
        if (isBig) return; // 이미 커졌으면 중복 적용 안 함

        // 작은/큰 스프라이트의 실제 높이 차이를 구해서, 그 절반만큼만 위로 이동
        // (그림이 가운데 기준으로 커지기 때문에, 이렇게 해야 발 위치가 그대로 고정됨)
        Sprite oldSprite = facingRight ? smallRightSprite : smallLeftSprite;
        Sprite newSprite = facingRight ? bigRightSprite : bigLeftSprite;
        float dy = 0f;
        if (oldSprite != null && newSprite != null)
            dy = (newSprite.bounds.size.y - oldSprite.bounds.size.y) / 2f;

        isBig = true;
        UpdateSprite();

        transform.position += new Vector3(0f, dy, 0f);

        // 콜라이더도 그림이 자란 만큼 위아래로 똑같이 키워줌 (Offset은 그대로 둬도 계산상 맞음)
        if (col != null)
        {
            col.size = smallColliderSize + new Vector2(0f, dy * 2f);
            col.offset = smallColliderOffset;
        }

        // GroundCheck는 자식이라 같이 위로 딸려 올라가므로, 그만큼 아래로 보정해서
        // 실제 발 위치(고정된 위치)에 계속 붙어있게 함
        if (groundCheck != null)
            groundCheck.localPosition -= new Vector3(0f, dy, 0f);
    }

    // 큰 마리오가 적한테 부딪혔을 때 Goomba 스크립트에서 호출 (죽는 대신 작아짐)
    public void Shrink()
    {
        if (!isBig) return; // 이미 작으면 여기서 할 일 없음 (죽음 처리는 부르는 쪽에서 따로 함)

        // Grow()와 정확히 반대 계산: 같은 높이 차이만큼 아래로 이동
        Sprite oldSprite = facingRight ? bigRightSprite : bigLeftSprite;
        Sprite newSprite = facingRight ? smallRightSprite : smallLeftSprite;
        float dy = 0f;
        if (oldSprite != null && newSprite != null)
            dy = (oldSprite.bounds.size.y - newSprite.bounds.size.y) / 2f; // 줄어드는 만큼 (양수)

        isBig = false;
        UpdateSprite();

        transform.position -= new Vector3(0f, dy, 0f);

        if (col != null)
        {
            col.size = smallColliderSize;
            col.offset = smallColliderOffset;
        }

        if (groundCheck != null)
            groundCheck.localPosition += new Vector3(0f, dy, 0f);
    }

    [Header("피격 무적 시간")]
    public float hitCooldown = 1f;   // 한 번 맞으면 이 시간 동안은 추가 피격 무시
    private float lastHitTime = -999f;

    // 적(굼바 등)과 부딪혔을 때 "지금 맞아도 되는 상태인지" 확인용
    public bool CanTakeHit()
    {
        return Time.time - lastHitTime >= hitCooldown;
    }

    // 실제로 맞았을 때 호출해서 무적 시간 시작
    public void RegisterHit()
    {
        lastHitTime = Time.time;
    }

    void FixedUpdate()
    {
        // 발밑 원으로 바닥 여부 체크
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // 좌우 이동 (목표 속도까지 부드럽게 가속/감속)
        float targetSpeed = moveInput * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = speedDiff * accelRate * Time.fixedDeltaTime;
        rb.velocity = new Vector2(rb.velocity.x + movement, rb.velocity.y);

        // 가변 점프 로직
        if (rb.velocity.y < 0)  // 떨어지는 중이면 더 빠르게
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))  // 올라가는데 버튼 뗐으면 짧은 점프
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
    }

    // 씬 뷰에서 바닥 체크 범위를 빨간 원으로 표시 (디버그용)
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}