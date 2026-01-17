using ECO;
using NPOI.HSSF.Record;
using TMPro;
using UnityEngine;

public class TempGainAirJumpObject : MonoBehaviour
{
    private TextMeshPro infoText;

    private void Start()
    {
        infoText = GetComponentInChildren<TextMeshPro>();
        infoText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Player>().nowInteractObject = gameObject;
            infoText.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Player>().nowInteractObject = null;
            infoText.gameObject.SetActive(false);
        }
    }

    public void GainAirJump(PlayerController playerController)
    {
        playerController.isAirjumpable = true;
        infoText.text = "공중 점프와 공명을 무사히 얻었습니다! 앞에 있는 절벽에서 사용해보세요!";
    }
}
