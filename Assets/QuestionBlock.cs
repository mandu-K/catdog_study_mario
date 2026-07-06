using UnityEngine;
using System.Collections;

// ? 블록: 마리오가 "아래에서" 치면 반응 (아이템 튀어나오기 + 이미 썼으면 다시 안 나옴)
public class QuestionBlock : MonoBehaviour
{
    [Header("아이템")]
    public GameObject itemPrefab;   // 여기서 나올 아이템(버섯 등) 프리팹. 비워둬도 동작함
    public float popHeight = 1f;    // 아이템이 튀어나올 높이

    [Header("상태")]
    public bool isUsed = false;     // 이미 사용한 블록인지

    [Header("스프라이트")]
    public Sprite initialSprite;    // 원상태 (?)
    public Sprite pressedSprite;    // 눌린 순간
    public Sprite usedSprite;       // 사용 후 (갈색 빈 블록)

    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (initialSprite != null) sr.sprite = initialSprite;
    }

    // 마리오(Rigidbody가 있는 물체)가 이 블록에 부딪혔을 때 호출됨
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isUsed) return; // 이미 썼으면 아무것도 안 함

        // 마리오 태그를 가진 오브젝트인지 확인
        if (!collision.gameObject.CompareTag("Player")) return;

        // 마리오와 블록이 "가로로 겹친 정도" vs "세로로 겹친 정도"를 비교해서
        // 어느 면에 부딪혔는지 판단 (겹친 폭이 더 좁은 쪽이 실제로 부딪힌 방향)
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D otherCollider = collision.collider;

        Vector2 dir = (Vector2)otherCollider.bounds.center - (Vector2)myCollider.bounds.center;
        float overlapX = (myCollider.bounds.extents.x + otherCollider.bounds.extents.x) - Mathf.Abs(dir.x);
        float overlapY = (myCollider.bounds.extents.y + otherCollider.bounds.extents.y) - Mathf.Abs(dir.y);

        // 세로 겹침이 가로 겹침보다 좁으면 위/아래 충돌 (옆면 충돌 아님)
        if (overlapY < overlapX)
        {
            // dir.y가 음수면 마리오가 블록보다 아래에 있다는 뜻 → 아래에서 친 것
            if (dir.y < 0)
            {
                HitFromBelow();
            }
        }
    }

    void HitFromBelow()
    {
        isUsed = true;
        StartCoroutine(PlayHitAnimation());

        // 아이템이 있으면 위로 튀어나오게 생성
        if (itemPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * popHeight;
            Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        }
    }

    IEnumerator PlayHitAnimation()
    {
        if (sr != null && pressedSprite != null) sr.sprite = pressedSprite;
        yield return new WaitForSeconds(0.1f); // 눌린 상태를 잠깐 보여줌
        if (sr != null && usedSprite != null) sr.sprite = usedSprite;
    }
}