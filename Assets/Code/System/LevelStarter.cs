using UnityEngine;
using UnityEngine.Events;
using System.Collections;

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

    /// <summary>
    /// Se ejecuta automáticamente al iniciar el nivel.
    /// Orquesta el flujo inicial:
    /// - Actualiza detección de dispositivo
    /// - Dispara evento de inicio
    /// - Ejecuta FadeIn
    /// - Desbloquea controles y dispara evento post-fade
    /// </summary>
    private IEnumerator Start()
    {
        Debug.Log("LevelStarter.Start() = START");

        if (updateControllerDiviceDetection)
            UpdateControllerDiviceDetection();

        if (onLevelStartEventEnabled)
            OnStartLevel();

        FullScreenFade.State.SetOpaceImmediate();
        yield return null;
        yield return FullScreenFade.State.FadeIn(this);

        if (unlockControlsOnFadeEnd)
            UnlockControls();

        if (onFadeEndEventEnabled)
            OnFadeEnd();

        Debug.Log("LevelStarter.Start() = END");
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
        ControllerManager.state.DiviceChangedFunction();
    }

    /// <summary>
    /// Desbloquea los controles del jugador.
    /// </summary>
    public void UnlockControls()
    {
        ControllerManager.state.LockControls(false);
    }
}
