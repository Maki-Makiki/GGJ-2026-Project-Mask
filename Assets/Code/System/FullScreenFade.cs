using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Sistema global de fundido a pantalla completa.
/// Singleton persistente entre escenas.
/// Soporta corrutinas, cancelación automática (último gana),
/// bloqueo de raycasts y configuración de tiempo/color.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public class FullScreenFade : MonoBehaviour
{
    public static FullScreenFade State { get; private set; }

    [Header("Configuration")]
    [SerializeField] private float defaultDuration = 1f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blockRaycasts = true;
    [SerializeField] private bool enableDebug = false;

    [Header("Auto References (Editor)")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fadeImage;

    private Coroutine currentFade;
    private int fadeVersion = 0;
    private bool isFading;

    #region Initialization

    private void Awake()
    {
        if (State != null && State != this)
        {
            Destroy(gameObject);
            return;
        }

        State = this;
        string nombreEscena = SceneManager.GetActiveScene().name;
        gameObject.name = $"FullScreenFade ({nombreEscena})";

        DontDestroyOnLoad(gameObject);

        InitializeReferences();
        SetTransparentImmediate();

        Debug.Log($"FullScreenFade ({nombreEscena}) Awake() - alpha = " + canvasGroup.alpha);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        InitializeReferencesEditor();
    }
#endif

    private void InitializeReferences()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();

        if (!fadeImage && transform.childCount > 0)
            fadeImage = transform.GetChild(0).GetComponent<Image>();
    }

#if UNITY_EDITOR
    private void InitializeReferencesEditor()
    {
        if (!canvasGroup)
            canvasGroup = GetComponent<CanvasGroup>();

        if (!canvasGroup)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (!fadeImage)
        {
            if (transform.childCount == 0)
            {
                GameObject imgObj = new GameObject("FadeImage", typeof(RectTransform), typeof(Image));
                imgObj.transform.SetParent(transform, false);

                RectTransform rt = imgObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                fadeImage = imgObj.GetComponent<Image>();
                fadeImage.color = Color.black;
            }
            else
            {
                fadeImage = transform.GetChild(0).GetComponent<Image>();
            }
        }
    }
#endif

    #endregion

    #region Public API

    public IEnumerator FadeIn(MonoBehaviour caller)
        => FadeInternal(1f, 0f, defaultDuration, fadeImage.color, caller);

    public IEnumerator FadeOut(MonoBehaviour caller)
        => FadeInternal(0f, 1f, defaultDuration, fadeImage.color, caller);

    public IEnumerator FadeIn(float duration, MonoBehaviour caller)
        => FadeInternal(1f, 0f, duration, fadeImage.color, caller);

    public IEnumerator FadeOut(float duration, MonoBehaviour caller)
        => FadeInternal(0f, 1f, duration, fadeImage.color, caller);

    public IEnumerator FadeIn(Color color, MonoBehaviour caller)
        => FadeInternal(1f, 0f, defaultDuration, color, caller);

    public IEnumerator FadeOut(Color color, MonoBehaviour caller)
        => FadeInternal(0f, 1f, defaultDuration, color, caller);

    public IEnumerator FadeIn(float duration, Color color, MonoBehaviour caller)
        => FadeInternal(1f, 0f, duration, color, caller);

    public IEnumerator FadeOut(float duration, Color color, MonoBehaviour caller)
        => FadeInternal(0f, 1f, duration, color, caller);

    public void CancelCurrent(MonoBehaviour caller)
    {
        fadeVersion++;
        Log($"Fade cancelado por {caller.name} ({caller.GetType().Name})");
    }

    public bool IsFading => isFading;

    #endregion

    #region Core

    private IEnumerator FadeInternal(
        float from,
        float to,
        float duration,
        Color color,
        MonoBehaviour caller)
    {
        if (!caller)
            throw new ArgumentNullException(nameof(caller), "Debe pasar 'this' como caller.");

        fadeVersion++;
        int myVersion = fadeVersion;

        fadeImage.color = color;

        Log($"{caller.name} [{caller.GetType().Name}] pidió fade ({from} → {to}) en {duration}s");

        isFading = true;

        if (blockRaycasts)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        duration = Mathf.Max(0.0001f, duration);

        canvasGroup.alpha = from;

        float startTime = useUnscaledTime ? Time.unscaledTime : Time.time;
        while (true)
        {
            if (myVersion != fadeVersion)
                yield break;

            float now = useUnscaledTime ? Time.unscaledTime : Time.time;
            float t = Mathf.Clamp01((now - startTime) / duration);

            canvasGroup.alpha = Mathf.Lerp(from, to, t);

            if (t >= 1f)
                break;

            yield return null;
        }

        canvasGroup.alpha = to;
        isFading = false;

        if (blockRaycasts && to <= 0.001f)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        Log("Fade completado.");
    }

    #endregion

    #region Utilities

    public void SetTransparentImmediate()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void SetOpaceImmediate()
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
    }

    private void Log(string message)
    {
        if (!enableDebug)
            return;

        ConsoleDebug(message);
    }

    protected virtual void ConsoleDebug(string text)
    {
        Debug.Log(text);
    }

    #endregion
}
