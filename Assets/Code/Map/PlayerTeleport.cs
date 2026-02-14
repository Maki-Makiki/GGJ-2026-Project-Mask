using System.Collections;
using Unity.Cinemachine;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Events;

public class PlayerTeleport : MonoBehaviour
{
    public Transform teleportTarget;
    public UnityEvent onRespawnStart;
    public UnityEvent onRespawnMid;
    public UnityEvent onRespawnEnd;

    public string DeadBool;
    public Animator animator;
    public Coroutine coroutineDead;
    public Coroutine coroutineLoadLevel;

    [SerializeField] private CinemachineCamera virtualCamera;

    private void Start()
    {
        if (virtualCamera  == null)
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();
    }

    public void LoadLevelNow(GameObject PortalGO)
    {
        if (coroutineLoadLevel != null)
            StopCoroutine(coroutineLoadLevel);

        coroutineLoadLevel = StartCoroutine(BlockPlayerAndLoad(PortalGO));
    }

    public IEnumerator BlockPlayerAndLoad(GameObject PortalGO)
    {
        if (FullScreenFade.State != null)
        {
            
            ControllerManager.LockControls();
            
            //espero un frame
            yield return null;
            onRespawnMid.Invoke();

            PortalGO.GetComponent<EventCaller>().CallEvent();
            yield return null;

            if (LevelLoader.State != null)
                LevelLoader.State.ChangeLevelByName(PortalGO.gameObject.name.Split(" | ")[1]);
            else
                Debug.LogWarning("LevelLoader no existe en la escena.");

            //aqui se termino la transicion y ya deberia poder controlar al player
        }
    }

    public void RespawnPlayerNow()
    {
        if (coroutineDead != null)
            StopCoroutine(coroutineDead);

        coroutineDead = StartCoroutine(RespawnPlayer());
    }

    public IEnumerator RespawnPlayer()
    {
        if (FullScreenFade.State != null)
        {
            ControllerManager.LockControls();
            animator.SetBool(DeadBool, true);
            onRespawnStart.Invoke();
            //espero un frame
            yield return null;

            yield return FullScreenFade.State.FadeOut(this);

            //onMid respawn es cuando esta la pantalla negra
            TeleportRespawn();
            TeleportRespawnEnd();
            onRespawnMid.Invoke();

            yield return FullScreenFade.State.FadeIn(this);

            //aqui se termino la transicion y ya deberia poder controlar al player
            ControllerManager.UnlockControls();
            onRespawnEnd.Invoke();
        }
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
        Vector3 thisLastPos = this.transform.position;
        this.gameObject.transform.position = target.position;
        virtualCamera.OnTargetObjectWarped(this.transform, this.transform.position - thisLastPos);
    }



    public void SetCheckpoint(Transform target)
    {
        teleportTarget = target;
    }
}
