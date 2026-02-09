using UnityEngine.Events;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CanvasGroupFader : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private CanvasGroup target;

    [Header("Fade Settings")]
    [Min(0f)]
    [SerializeField] private float transitionTime = 0.25f;

    [Tooltip("Ease In Out tipo CSS (suave al inicio y al final).")]
    [SerializeField] private bool useEaseInOut = true;

    [Tooltip("Si está activo, ignora Time.timeScale y usa tiempo real.")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Interaction Control")]
    [Tooltip("Si está activo, desactiva interactable/blocksRaycasts al empezar FadeOut, y los activa al terminar FadeIn.")]
    [SerializeField] private bool controlInteraction = true;

    [Header("Optional Events")]
    [SerializeField] private bool useEvents = false;
    [SerializeField] private UnityEvent onFadeInComplete;
    [SerializeField] private UnityEvent onFadeOutComplete;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<CanvasGroup>();
    }

    private void OnValidate()
    {
        if (target == null)
            target = GetComponent<CanvasGroup>();

        transitionTime = Mathf.Max(0f, transitionTime);
    }

    public void SetTransitionTime(float seconds)
    {
        transitionTime = Mathf.Max(0f, seconds);
    }

    public void FadeIn()
    {
        if (controlInteraction && target != null)
        {
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        StartFadeTo(1f, true);
    }

    public void FadeOut()
    {
        if (controlInteraction && target != null)
        {
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        StartFadeTo(0f, false);
    }

    public void FadeTo(float targetAlpha)
    {
        targetAlpha = Mathf.Clamp01(targetAlpha);
        bool isFadeIn = targetAlpha > GetCurrentAlpha();

        if (controlInteraction && target != null)
        {
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        StartFadeTo(targetAlpha, isFadeIn);
    }

    public void CancelFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    public float GetCurrentAlpha()
    {
        return target != null ? target.alpha : 0f;
    }

    private void StartFadeTo(float targetAlpha, bool isFadeIn)
    {
        if (target == null) return;

        CancelFade();

        float currentAlpha = target.alpha;

        if (transitionTime <= 0f || Mathf.Approximately(currentAlpha, targetAlpha))
        {
            target.alpha = targetAlpha;

            if (controlInteraction)
            {
                target.interactable = Mathf.Approximately(targetAlpha, 1f);
                target.blocksRaycasts = Mathf.Approximately(targetAlpha, 1f);
            }

            if (useEvents)
            {
                if (isFadeIn) onFadeInComplete?.Invoke();
                else onFadeOutComplete?.Invoke();
            }

            return;
        }

        fadeRoutine = StartCoroutine(FadeRoutine(currentAlpha, targetAlpha, isFadeIn));
    }

    private IEnumerator FadeRoutine(float fromAlpha, float toAlpha, bool isFadeIn)
    {
        float t = 0f;

        while (t < transitionTime)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float normalized = Mathf.Clamp01(t / transitionTime);

            if (useEaseInOut)
                normalized = EaseInOut(normalized);

            target.alpha = Mathf.Lerp(fromAlpha, toAlpha, normalized);
            yield return null;
        }

        target.alpha = toAlpha;
        fadeRoutine = null;

        if (controlInteraction)
        {
            target.interactable = Mathf.Approximately(toAlpha, 1f);
            target.blocksRaycasts = Mathf.Approximately(toAlpha, 1f);
        }

        if (useEvents)
        {
            if (isFadeIn) onFadeInComplete?.Invoke();
            else onFadeOutComplete?.Invoke();
        }
    }

    private float EaseInOut(float x)
    {
        return x * x * (3f - 2f * x);
    }
}
