using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
            Debug.Log("colision con " + Trigger.gameObject.name);
            if (Trigger.gameObject.tag == "NextLevel")
                SceneManager.LoadScene(Trigger.gameObject.name.Split(" | ")[1]);

            if (Trigger.gameObject.tag == "Event")
                playerTeleport.LoadLevelNow(Trigger.gameObject);
                //Trigger.gameObject.GetComponent<EventCaller>().CallEvent();

            if (Trigger.gameObject.tag == "Dead")
                playerTeleport.RespawnPlayerNow();

        }
       
    }
}
