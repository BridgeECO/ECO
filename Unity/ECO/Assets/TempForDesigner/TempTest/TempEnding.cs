using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//이 스크립트는 맵 끝부분에 있는 플랫폼의 판정하는 트리거 오브젝트에 추가되는 스크립트
public class TempEnding : MonoBehaviour
{
    public GameObject restartScreen;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            Time.timeScale = 0f;
            restartScreen.SetActive(true);
        }
    }

    //게임 리셋을 위해 씬을 다시 로딩하는 함수
    public void ResetGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
