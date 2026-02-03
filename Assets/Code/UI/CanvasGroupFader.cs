using System.Collections;
using UnityEngine;
using UnityEngine.Events;

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
        // IMPORTANTE: mientras está apareciendo, NO queremos que sea clickeable todavía
        if (controlInteraction && target != null)
        {
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        StartFadeTo(1f, isFadeIn: true);
    }

    public void FadeOut()
    {
        // IMPORTANTE: apenas empieza a desaparecer, ya no debe recibir clicks
        if (controlInteraction && target != null)
        {
            target.interactable = false;
            target.blocksRaycasts = false;
        }

        StartFadeTo(0f, isFadeIn: false);
    }

    public void FadeTo(float targetAlpha)
    {
        targetAlpha = Mathf.Clamp01(targetAlpha);

        bool isFadeIn = targetAlpha > GetCurrentAlpha();

        // Si va a desaparecer o aparecer, manejamos interacción con la misma lógica:
        if (controlInteraction && target != null)
        {
            if (!isFadeIn)
            {
                // va hacia 0 => cortar input YA
                target.interactable = false;
                target.blocksRaycasts = false;
            }
            else
            {
                // va hacia 1 => mantener cortado hasta que termine
                target.interactable = false;
                target.blocksRaycasts = false;
            }
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
        if (target == null) return 0f;
        return target.alpha;
    }

    private void StartFadeTo(float targetAlpha, bool isFadeIn)
    {
        if (target == null) return;

        CancelFade();

        float currentAlpha = target.alpha;

        if (transitionTime <= 0f || Mathf.Approximately(currentAlpha, targetAlpha))
        {
            target.alpha = targetAlpha;

            // Si llegamos a 1 => recién ahí habilitamos interacción
            if (controlInteraction)
            {
                if (Mathf.Approximately(targetAlpha, 1f))
                {
                    target.interactable = true;
                    target.blocksRaycasts = true;
                }
                else
                {
                    target.interactable = false;
                    target.blocksRaycasts = false;
                }
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
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / transitionTime);

            if (useEaseInOut)
                normalized = EaseInOut(normalized);

            target.alpha = Mathf.Lerp(fromAlpha, toAlpha, normalized);
            yield return null;
        }

        target.alpha = toAlpha;
        fadeRoutine = null;

        // Reglas de interacción:
        // - FadeOut termina => queda NO interactuable
        // - FadeIn termina => recién ahí se vuelve interactuable
        if (controlInteraction)
        {
            if (Mathf.Approximately(toAlpha, 1f))
            {
                target.interactable = true;
                target.blocksRaycasts = true;
            }
            else
            {
                target.interactable = false;
                target.blocksRaycasts = false;
            }
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
