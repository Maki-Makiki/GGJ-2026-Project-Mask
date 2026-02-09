using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    public enum FadeEase
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut
    }

    public enum FadeType
    {
        FadeIn,     // 1 -> 0
        FadeOut,    // 0 -> 1
        FadeInOut,  // 1 -> 0 -> (hold) -> 1
        FadeOutIn   // 0 -> 1 -> (hold) -> 0
    }

    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fadeImage;

    [Header("Visual Settings")]
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private FadeEase ease = FadeEase.EaseInOut;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1f;

    [Tooltip("Si arranca opaco, espera este tiempo antes de permitir el primer FadeIn.")]
    [SerializeField] private float startHoldBlack = 0f;

    [Tooltip("Tiempo en negro entre los 2 tramos del FadeInOut.")]
    [SerializeField] private float midHoldBlack = 0.25f;

    [Tooltip("Tiempo en negro al final de un FadeOut antes de disparar onFadeComplete.")]
    [SerializeField] private float endHoldBlack = 0.25f;

    [Header("Startup")]
    [Tooltip("Si está activo, arranca opaco (alpha=1) instantáneo.")]
    [SerializeField] private bool startOpaque = true;

    [Header("Time Control")]
    [Tooltip("Si está activo, ignora Time.timeScale y usa tiempo real.")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Events")]
    public UnityEvent onFadeStart;
    public UnityEvent onFadeMidpoint;
    public UnityEvent onFadeComplete;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        ApplyColor();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = startOpaque ? 1f : 0f;
            canvasGroup.blocksRaycasts = startOpaque;
            canvasGroup.interactable = startOpaque;
        }
    }

    public void ApplyColor()
    {
        if (fadeImage != null)
            fadeImage.color = fadeColor;
    }

    public void SetColor(Color color)
    {
        fadeColor = color;
        ApplyColor();
    }

    public void Fade(FadeType type)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(type));
    }

    public void FadeIn() => Fade(FadeType.FadeIn);
    public void FadeOut() => Fade(FadeType.FadeOut);
    public void FadeInOut() => Fade(FadeType.FadeInOut);
    public void FadeOutIn() => Fade(FadeType.FadeOutIn);

    private IEnumerator FadeRoutine(FadeType type)
    {
        if (canvasGroup == null)
            yield break;

        float d = Mathf.Max(0.0001f, fadeDuration);

        onFadeStart?.Invoke();

        if (type == FadeType.FadeIn && startOpaque && startHoldBlack > 0f && canvasGroup.alpha >= 0.999f)
        {
            yield return WaitForSecondsCustom(startHoldBlack);
        }

        if (type == FadeType.FadeIn)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            yield return FadeAlpha(1f, 0f, d);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            onFadeComplete?.Invoke();
        }
        else if (type == FadeType.FadeOut)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            yield return FadeAlpha(0f, 1f, d);

            if (endHoldBlack > 0f)
                yield return WaitForSecondsCustom(endHoldBlack);

            onFadeComplete?.Invoke();
        }
        else if (type == FadeType.FadeOutIn)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            yield return FadeAlpha(0f, 1f, d * 0.5f);

            onFadeMidpoint?.Invoke();

            if (midHoldBlack > 0f)
                yield return WaitForSecondsCustom(midHoldBlack);

            yield return FadeAlpha(1f, 0f, d * 0.5f);

            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            onFadeComplete?.Invoke();
        }
        else // FadeInOut
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            yield return FadeAlpha(1f, 0f, d * 0.5f);

            onFadeMidpoint?.Invoke();

            if (midHoldBlack > 0f)
                yield return WaitForSecondsCustom(midHoldBlack);

            yield return FadeAlpha(0f, 1f, d * 0.5f);

            onFadeComplete?.Invoke();
        }

        fadeCoroutine = null;
    }

    private IEnumerator FadeAlpha(float from, float to, float time)
    {
        float t = 0f;
        canvasGroup.alpha = from;

        while (t < time)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float x = Mathf.Clamp01(t / time);
            float eased = Ease(x);

            canvasGroup.alpha = Mathf.Lerp(from, to, eased);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator WaitForSecondsCustom(float time)
    {
        float t = 0f;
        while (t < time)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    private float Ease(float x)
    {
        switch (ease)
        {
            default:
            case FadeEase.Linear:
                return x;
            case FadeEase.EaseIn:
                return x * x;
            case FadeEase.EaseOut:
                return 1f - Mathf.Pow(1f - x, 2f);
            case FadeEase.EaseInOut:
                return x * x * (3f - 2f * x);
        }
    }

    public void ResetFade(bool opaque)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        canvasGroup.alpha = opaque ? 1f : 0f;
        canvasGroup.blocksRaycasts = opaque;
        canvasGroup.interactable = opaque;
        fadeCoroutine = null;
    }
}
