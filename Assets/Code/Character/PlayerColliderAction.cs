using UnityEngine;
using UnityEngine.Events;

public class PlayerColliderAction : MonoBehaviour
{
    [SerializeField] PlayerTeleport playerTeleport;

    private void OnTriggerEnter2D(Collider2D Trigger)
    {
        Debug.Log("colision con " + Trigger.gameObject.name);
        if(Trigger.gameObject.tag == "Checkpoint")
        {
            playerTeleport.SetCheckpoint(Trigger.transform.parent);
            Trigger.gameObject.SetActive(false);
        }
        else
        {
            playerTeleport.RespawnPlayer();
        }
       
    }
}
