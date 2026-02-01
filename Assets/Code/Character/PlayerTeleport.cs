using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
    public Transform teleportTarget;

    public void RespawnPlayer()
    {
        TeleportPlayer(teleportTarget);
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
