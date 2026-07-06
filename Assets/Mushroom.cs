using UnityEngine;

// 버섯: 블록에서 튀어나온 뒤 한 방향으로 계속 굴러가고, 벽에 부딪히면 반대 방향으로 바뀜
// 마리오가 닿으면 사라짐 (커지는 효과는 나중에 PlayerController에 붙이면 됨)
[RequireComponent(typeof(Rigidbody2D))]
public class Mushroom : MonoBehaviour
{
    public float moveSpeed = 2f;      // 이동 속도
    private float direction = 1f;     // 1 = 오른쪽, -1 = 왼쪽
    private Rigidbody2D rb;

    [Header("스프라이트")]
    public Sprite mushroomSprite;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // 떨어지면서 구르지 않게 회전 고정
        if (mushroomSprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = mushroomSprite;
        }
    }

    void FixedUpdate()
    {
        // 계속 한쪽 방향으로 이동
        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
    }

    // 벽이나 블록 옆면에 부딪히면 방향을 반대로 바꿈
    void OnCollisionEnter2D(Collision2D collision)
    {
        // 플레이어와 부딪힌 경우: 커지게 만들고 사라짐
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) player.Grow();

            Destroy(gameObject);
            return;
        }

        // 그 외(벽, 블록 등)에 옆에서 부딪혔으면 방향 전환
        foreach (ContactPoint2D contact in collision.contacts)
        {
            // 접촉면의 법선이 거의 수평이면(옆에서 부딪힌 것) 방향을 반대로
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                direction *= -1f;
                break;
            }
        }
    }
}