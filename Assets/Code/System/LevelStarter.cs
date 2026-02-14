using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// LevelStarter ahora escucha eventos de LevelLoader y FullScreenFade
/// para ejecutar acciones al inicio de la escena y al completar el fade.
/// </summary>
public class LevelStarter : MonoBehaviour
{
    [Header("On Level Start")]
    public bool onLevelStartEventEnabled;
    [SerializeField] private UnityEvent onLevelStart;

    [Header("On Fade End")]
    public bool onFadeEndEventEnabled;
    [SerializeField] private UnityEvent onFadeEnd;

    [Header("Other Global Options")]
    public bool updateControllerDiviceDetection = true;
    public bool unlockControlsOnFadeEnd = true;
    public bool resetUISoundManagerState = true;

    private IEnumerator Start()
    {
        Debug.Log("LevelStarter.Start() = START");

        if (updateControllerDiviceDetection)
            UpdateControllerDiviceDetection();

        if (resetUISoundManagerState)
            ResetUISoundManager();

        if (onLevelStartEventEnabled)
            OnStartLevel();

        if (FullScreenFade.State != null)
        {
            FullScreenFade.State.SetOpaceImmediate();
            yield return null;
            yield return FullScreenFade.State.FadeIn(this);
        }

        if (unlockControlsOnFadeEnd)
            UnlockControls();

        if (onFadeEndEventEnabled)
            OnFadeEnd();

        Debug.Log("LevelStarter.Start() = END");
    }

    /// <summary>
    /// Evento disparado cuando LevelLoader indica que la escena terminó de cargar y el fade de loading terminó.
    /// </summary>
    private void HandleSceneLoadedAndFadeEnd()
    {
        if (unlockControlsOnFadeEnd)
            UnlockControls();

        if (onFadeEndEventEnabled)
            OnFadeEnd();
    }

    /// <summary>
    /// Invoca el evento configurado para el inicio del nivel.
    /// </summary>
    public void OnStartLevel()
    {
        onLevelStart?.Invoke();
    }

    /// <summary>
    /// Invoca el evento configurado al finalizar el fade.
    /// </summary>
    public void OnFadeEnd()
    {
        onFadeEnd?.Invoke();
    }

    /// <summary>
    /// Fuerza actualización del sistema de detección de dispositivos.
    /// </summary>
    public void UpdateControllerDiviceDetection()
    {
        if (ControllerManager.state != null)
            ControllerManager.state.DiviceChangedFunction();
        else
            Debug.LogWarning("ControllerManager no existe. No se puede actualizar detección de dispositivos.");
    }

    public void ResetUISoundManager()
    {
        if (ControllerManager.state != null)
            ControllerManager.state.ResetUISoundManager();
        else
            Debug.LogWarning("ControllerManager no existe. No se puede actualizar el UISoundManager.");
    }

    /// <summary>
    /// Desbloquea los controles del jugador.
    /// </summary>
    public void UnlockControls()
    {
        if (ControllerManager.state != null)
            ControllerManager.state.LockControls(false);
        else
            Debug.LogWarning("ControllerManager no existe. No se pueden desbloquear controles.");
    }
}
