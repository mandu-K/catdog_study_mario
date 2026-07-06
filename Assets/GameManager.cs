using UnityEngine;
using UnityEngine.SceneManagement;

// 게임 전체를 관리: 게임오버(재시작) / 클리어 처리
// 씬에 빈 오브젝트 하나 만들어서 이 스크립트를 붙이고 쓰면 됨
public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // 어디서든 GameManager.Instance로 접근 가능하게

    [Header("상태")]
    public bool isGameOver = false;
    public bool isCleared = false;

    void Awake()
    {
        // 씬에 GameManager가 하나만 있도록
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 마리오가 죽었을 때 (굼바 옆 충돌, 낙사 등에서 호출)
    public void GameOver()
    {
        if (isGameOver || isCleared) return; // 중복 방지
        isGameOver = true;

        Debug.Log("게임 오버! 잠시 후 재시작됩니다.");
        Invoke(nameof(RestartLevel), 1.5f); // 1.5초 후 재시작 (연출 여유)
    }

    // 골 지점(깃대)에 도달했을 때 호출
    public void ClearLevel()
    {
        if (isGameOver || isCleared) return;
        isCleared = true;

        Debug.Log("스테이지 클리어!");
    }

    void RestartLevel()
    {
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.name);
    }

    [Header("게임오버 화면")]
    public Sprite gameOverImage; // gameover_only.png를 여기에 연결

    // 화면에 게임오버 이미지 또는 클리어 문구를 띄움 (Canvas/UI 없이 간단히 구현)
    void OnGUI()
    {
        if (!isGameOver && !isCleared) return;

        if (isGameOver && gameOverImage != null)
        {
            // 이미지를 화면 전체에 꽉 채워서 그림
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), gameOverImage.texture);
            return;
        }

        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;
        style.wordWrap = true;
        style.normal.textColor = isCleared ? Color.yellow : Color.red;

        string message = isCleared ? "STAGE CLEAR!" : "GAME OVER";
        Rect area = new Rect(0, Screen.height / 2f - 20, Screen.width, 40);
        GUI.Label(area, message, style);
    }
}