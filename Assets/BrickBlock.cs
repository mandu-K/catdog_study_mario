using UnityEngine;

// 벽돌 블록: 마리오가 "아래에서" 치면 부서져서 사라짐
public class BrickBlock : MonoBehaviour
{
    // 나중에 "큰 마리오일 때만 부서짐" 조건을 추가하고 싶으면
    // PlayerController 쪽에 isBig 같은 상태를 만들고 여기서 참조하면 됨
    public bool isBroken = false;

    [Header("스프라이트")]
    public Sprite brickSprite;

    void Awake()
    {
        if (brickSprite != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = brickSprite;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBroken) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // QuestionBlock과 동일한 방식: 가로/세로 겹침 비교로 아래에서 쳤는지 판단
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D otherCollider = collision.collider;

        Vector2 dir = (Vector2)otherCollider.bounds.center - (Vector2)myCollider.bounds.center;
        float overlapX = (myCollider.bounds.extents.x + otherCollider.bounds.extents.x) - Mathf.Abs(dir.x);
        float overlapY = (myCollider.bounds.extents.y + otherCollider.bounds.extents.y) - Mathf.Abs(dir.y);

        if (overlapY < overlapX && dir.y < 0)
        {
            Break();
        }
    }

    void Break()
    {
        isBroken = true;
        // 지금은 바로 사라지게. 나중에 부서지는 조각 효과(파티클 등) 추가하고 싶으면
        // 여기서 Instantiate로 조각 프리팹을 만들고 Destroy 하면 됨
        Destroy(gameObject);
    }
}