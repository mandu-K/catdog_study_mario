using UnityEngine;

// 골 지점(깃대): 마리오가 닿으면 스테이지 클리어 처리
// 이 오브젝트의 Collider는 Is Trigger를 켜서 사용 (물리적으로 막지 않고 통과하며 감지만)
public class Goal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
            GameManager.Instance.ClearLevel();
    }
}
