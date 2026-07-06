using UnityEngine;

// 굼바: 좌우로 왔다갔다 순찰하다가, 위에서 밟히면 죽고 / 옆에서 닿으면 마리오에게 데미지
[RequireComponent(typeof(Rigidbody2D))]
public class Goomba : MonoBehaviour
{
    public float moveSpeed = 1.5f;
    private float direction = -1f; // 원작처럼 기본은 왼쪽으로 시작

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    private bool isDead = false;

    [Header("스프라이트")]
    public Sprite leftSprite;
    public Sprite rightSprite;
    public Sprite squishSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb.freezeRotation = true; // 충돌해도 회전하지 않게 고정
    }

    [Header("낙사 판정")]
    public float fallDeathY = -10f; // 이 Y좌표보다 낮아지면 구멍에 빠진 것으로 처리

    [Header("활성화 (원작처럼 마리오가 가까이 오기 전엔 안 움직임)")]
    public float activationRange = 8f; // 마리오가 이 거리 안에 들어와야 움직이기 시작
    private bool activated = false;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        // 구멍에 빠지면 그냥 삭제
        if (transform.position.y < fallDeathY)
        {
            Die();
            return;
        }

        // 아직 활성화 전이면 완전히 가만히 있음 (원작 NES처럼, 마리오가 화면에 들어오기 전엔 안 움직임)
        if (!activated)
        {
            if (player != null && Vector2.Distance(transform.position, player.position) < activationRange)
                activated = true;
            else
            {
                rb.velocity = new Vector2(0f, rb.velocity.y); // 제자리에 가만히
                return;
            }
        }

        // 활성화된 후에는 원작처럼 그냥 걷다가 벽에만 부딪히면 방향 전환
        // (낭떠러지를 만나면 원작처럼 실제로 떨어질 수 있음 - 마리오가 근처에 있어 화면에서 보임)
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);

        // 이동 방향에 맞는 스프라이트로 교체
        if (sr != null)
            sr.sprite = direction > 0 ? rightSprite : leftSprite;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            HandlePlayerCollision(collision);
        }
        else
        {
            // 벽/블록 등에 옆에서 부딪히면 방향 전환
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.5f)
                {
                    direction *= -1f;
                    break;
                }
            }
        }
    }

    void HandlePlayerCollision(Collision2D collision)
    {
        // QuestionBlock/BrickBlock과 같은 방식: 겹침 비교로 위/옆 판단
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D otherCollider = collision.collider;

        Vector2 dir = (Vector2)otherCollider.bounds.center - (Vector2)myCollider.bounds.center;
        float overlapX = (myCollider.bounds.extents.x + otherCollider.bounds.extents.x) - Mathf.Abs(dir.x);
        float overlapY = (myCollider.bounds.extents.y + otherCollider.bounds.extents.y) - Mathf.Abs(dir.y);

        Rigidbody2D playerRb = otherCollider.attachedRigidbody;
        bool isTopHit;

        // 겹침 차이가 아주 작으면(정확히 모서리를 스친 애매한 경우), 겹침 비교만으론 판단이 흔들릴 수 있음
        // 이럴 땐 "마리오가 떨어지는 중이었는지"까지 같이 확인해서 더 정확하게 판단
        if (Mathf.Abs(overlapX - overlapY) < 0.15f)
        {
            isTopHit = dir.y > 0 && playerRb != null && playerRb.velocity.y <= 0.1f;
        }
        else
        {
            isTopHit = overlapY < overlapX && dir.y > 0;
        }

        if (isTopHit)
        {
            // 마리오가 굼바보다 위에 있음 = 밟은 것
            Die();

            // 밟은 마리오를 살짝 위로 튕겨줌 (다시 점프하는 느낌)
            if (playerRb != null)
                playerRb.velocity = new Vector2(playerRb.velocity.x, 8f);
        }
        else
        {
            // 옆에서 닿음 = 마리오 데미지
            PlayerController player = otherCollider.GetComponent<PlayerController>();
            if (player == null) return;

            // 방금 맞아서 무적 시간 중이면 (연속 충돌 이벤트로 인한 중복 판정 방지) 무시
            if (!player.CanTakeHit()) return;
            player.RegisterHit();

            // 큰 마리오면 죽는 대신 작아지기만 하고, 이미 작은 마리오면 게임오버
            if (player.isBig)
            {
                player.Shrink();
            }
            else
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
        }
    }

    void Die()
    {
        isDead = true;
        rb.velocity = Vector2.zero;

        // 밟혀서 납작해진 모습을 잠깐 보여준 뒤 삭제
        if (col != null) col.enabled = false; // 더 이상 부딪히지 않게
        if (sr != null && squishSprite != null) sr.sprite = squishSprite;
        Destroy(gameObject, 0.3f);
    }
}