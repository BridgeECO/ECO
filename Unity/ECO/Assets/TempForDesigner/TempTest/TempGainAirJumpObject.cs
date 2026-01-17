using ECO;
using UnityEngine;

public class TempGainAirJumpObject : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            collision.GetComponent<Player>().nowInteractObject = gameObject;
        }
    }
    
    public void GainAirJump(PlayerController playerController)
    {
        playerController.isAirjumpable = true;
    }
}
