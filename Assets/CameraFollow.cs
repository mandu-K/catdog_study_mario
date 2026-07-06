using UnityEngine;

// 카메라가 마리오를 따라감 (좌우로만, 원작처럼 왼쪽으로는 되돌아가지 않음)
public class CameraFollow : MonoBehaviour
{
    public Transform target;       // 따라갈 대상 (Player)
    public float smoothSpeed = 5f; // 부드럽게 따라가는 속도
    public float yOffset = 0f;     // 카메라 높이는 고정하고 싶으면 0

    private float maxReachedX;     // 지금까지 마리오가 도달한 가장 오른쪽 x값

    void Start()
    {
        if (target != null)
            maxReachedX = target.position.x;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 마리오가 이전보다 더 오른쪽으로 갔으면 갱신 (왼쪽으로 가도 카메라는 안 따라감)
        if (target.position.x > maxReachedX)
            maxReachedX = target.position.x;

        Vector3 desiredPos = new Vector3(maxReachedX, transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
    }
}
