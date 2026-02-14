using System;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Sistema global de carga de niveles con transición y canvas de loading.
/// Singleton persistente.
/// </summary>
[DisallowMultipleComponent]
public class LevelLoader : MonoBehaviour
{
    public static LevelLoader State { get; private set; }

    [Header("Loading UI")]
    [SerializeField] private CanvasGroup loadingCanvasGroup;
    [SerializeField] private RectTransform progressBarRect;
    [SerializeField] private RectTransform progressContainerRect;

    [Header("Fake Load times")]
    [SerializeField] float displayedProgress = 0f;
    [SerializeField] float minVisualTime = 0.8f; // tiempo mínimo visible
    [SerializeField] float visualTimer = 0f;

    [Header("Timers")]
    [SerializeField] private float fadeLoadingDuration = 0.2f; // fade del canvas loading
    [SerializeField] private float LoadingExtraDuration = 0.2f; // fade del canvas loading

    [Header("State")]
    [SerializeField] private bool isLoading = false;


    [Header("Options")]
    [SerializeField] private bool useUnscaledTime = true;

    // Eventos
    public event Action OnLoadStart;
    public event Action OnAsyncLoadStart;
    public event Action<float> OnLoadProgress; // 0..1
    public event Action OnAsyncLoadEnd;

    private void Awake()
    {
        if (State != null && State != this)
        {
            Destroy(gameObject);
            return;
        }

        State = this;
        DontDestroyOnLoad(gameObject);

        // Aseguramos que el canvas esté transparente al inicio
        if (loadingCanvasGroup)
        {
            loadingCanvasGroup.alpha = 0f;
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }

    #region Public API

    public void ChangeLevelByName(string sceneName)
    {
        if (isLoading) return;
        Debug.Log("LOAD LEVEL => " + sceneName);
        //if (ControllerManager.state.isPaused)
        //    ControllerManager.state.PauseGame(false);
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public void QuitGame()
    {
        StartCoroutine(QuitGameRoutine());

    }

    private IEnumerator QuitGameRoutine()
    {
        //Fundido a negro global
        if (FullScreenFade.State != null)
            yield return StartCoroutine(FullScreenFade.State.FadeOut(this));

        Debug.Log("Quit Game");
        Application.Quit();
    }

    #endregion

    #region Core Coroutine

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        isLoading = true;// bloqueamos una posible segunda carga

        OnLoadStart?.Invoke();

        //RESET VISUAL INMEDIATO
        displayedProgress = 0f;
        visualTimer = 0f;
        ShowProgress(0);

        //Fundido a negro global
        if (FullScreenFade.State != null)
            yield return StartCoroutine(FullScreenFade.State.FadeOut(this));

        //Mostrar loading
        if (loadingCanvasGroup)
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeLoadingDuration));

        OnAsyncLoadStart?.Invoke();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            visualTimer += Time.unscaledDeltaTime;

            float realProgress = asyncLoad.progress / 0.9f;
            realProgress = Mathf.Clamp01(realProgress);

            // Progreso mínimo basado en tiempo
            float timeProgress = visualTimer / minVisualTime;
            timeProgress = Mathf.Clamp01(timeProgress);

            // Nunca mostrar más que el real
            float target = Mathf.Min(realProgress, timeProgress);

            displayedProgress = Mathf.MoveTowards(
                displayedProgress,
                target,
                1.5f * Time.unscaledDeltaTime
            );

            float eased = displayedProgress * displayedProgress * (3f - 2f * displayedProgress);

            ShowProgress(eased);

            if (realProgress >= 1f && displayedProgress >= 0.999f)
                break;

            yield return null;
        }

        // La carga está lista, podemos esperar un frame para animación
        OnLoadProgress?.Invoke(1f);
        if (progressContainerRect && progressBarRect)
        {
            progressBarRect.offsetMax =
                new Vector2(0f, progressBarRect.offsetMax.y);
        }

        OnAsyncLoadEnd?.Invoke();

        // Optional delay para transición de animación/fade
        yield return new WaitForSecondsRealtime(LoadingExtraDuration);

        // Fade out canvas de loading
        if (loadingCanvasGroup)
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeLoadingDuration));

        if (ControllerManager.state != null && ControllerManager.state.isPaused)
            ControllerManager.state.PauseGame(false);

        // Activamos la escena
        asyncLoad.allowSceneActivation = true;

        isLoading = false;// ya no se esta cargando nivel

        // Esperamos un frame para que se inicie la nueva escena y LevelStarter tome control
        yield return null;

    }

    public void ShowProgress(float newValue)
    {
        if (progressContainerRect && progressBarRect)
        {
            float containerWidth = progressContainerRect.rect.width;
            float rightValue = containerWidth * (1f - newValue);

            progressBarRect.offsetMax =
                new Vector2(-rightValue, progressBarRect.offsetMax.y);
        }
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (!loadingCanvasGroup)
            yield break;

        float startTime = useUnscaledTime ? Time.unscaledTime : Time.time;

        // Habilitar interacción si aparece
        if (to > 0f)
        {
            loadingCanvasGroup.interactable = true;
            loadingCanvasGroup.blocksRaycasts = true;
        }

        while (true)
        {
            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            float t = Mathf.Clamp01((now - startTime) / duration);

            loadingCanvasGroup.alpha = Mathf.Lerp(from, to, t);

            if (t >= 1f) break;
            yield return null;
        }

        loadingCanvasGroup.alpha = to;

        // Deshabilitar interacción si desaparece
        if (to <= 0f)
        {
            loadingCanvasGroup.interactable = false;
            loadingCanvasGroup.blocksRaycasts = false;
        }
    }

    #endregion
}
