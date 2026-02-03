using UnityEngine;
using UnityEngine.Events;

public class PlayerTeleport : MonoBehaviour
{
    public Transform teleportTarget;
    public UnityEvent onRespawn;
    public string DeadBool;
    public Animator animator;

    public void RespawnPlayer()
    {
        animator.SetBool(DeadBool, true);
        onRespawn.Invoke();
    }

    public void TeleportRespawn()
    {
        TeleportPlayer(teleportTarget);
    }

    public void TeleportRespawnEnd()
    {
        animator.SetBool(DeadBool, false);
    }

    public void TeleportPlayer(Transform target)
    {
        
        this.gameObject.transform.position = target.position;
    }



    public void SetCheckpoint(Transform target)
    {
        teleportTarget = target;
    }
}
