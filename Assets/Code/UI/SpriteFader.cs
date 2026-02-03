using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class SpriteFader : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private SpriteRenderer target;

    [Header("Fade Settings")]
    [Min(0f)]
    [SerializeField] private float transitionTime = 0.25f;

    [Tooltip("Ease In Out tipo CSS (suave al inicio y al final).")]
    [SerializeField] private bool useEaseInOut = true;

    [Header("Optional Events")]
    [SerializeField] private bool useEvents = false;
    [SerializeField] private UnityEvent onFadeInComplete;
    [SerializeField] private UnityEvent onFadeOutComplete;

    private Coroutine fadeRoutine;

    [Header("Start State")]
    [SerializeField] private bool startInvisible = false;

    // Guardamos el color "base" original (para respetarlo siempre)
    private Color baseColor;
    private bool hasBaseColor = false;

    private void Awake()
    {
        if (target == null)
            target = GetComponent<SpriteRenderer>();

        CacheBaseColor();

        if (startInvisible)
        {
            // dejamos el sprite apagado pero conservando el color original
            Color c = baseColor;
            c.a = 0f;
            target.color = c;
        }
    }

    private void OnValidate()
    {
        if (target == null)
            target = GetComponent<SpriteRenderer>();

        transitionTime = Mathf.Max(0f, transitionTime);
    }

    private void CacheBaseColor()
    {
        if (target == null) return;

        // Si todavía no lo guardamos, lo guardamos una vez.
        if (!hasBaseColor)
        {
            baseColor = target.color;
            hasBaseColor = true;
        }
    }

    /// <summary>
    /// Cambia el sprite y resetea el color base (para que el fade respete ese color).
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (target == null) return;

        target.sprite = sprite;

        // Cuando cambiás sprite, lo normal es que quieras que el color actual sea el "original"
        baseColor = target.color;
        hasBaseColor = true;
    }

    public void SetTransitionTime(float seconds)
    {
        transitionTime = Mathf.Max(0f, seconds);
    }

    public void FadeIn()
    {
        CacheBaseColor();
        StartFadeTo(baseColor.a, isFadeIn: true);
    }

    public void FadeOut()
    {
        CacheBaseColor();
        StartFadeTo(0f, isFadeIn: false);
    }

    /// <summary>
    /// Fade hacia un alpha específico (0..1) respetando el color base.
    /// </summary>
    public void FadeTo(float targetAlpha)
    {
        CacheBaseColor();
        StartFadeTo(Mathf.Clamp01(targetAlpha), isFadeIn: targetAlpha > GetCurrentAlpha());
    }

    /// <summary>
    /// Cancela el fade actual sin cambiar el alpha actual.
    /// </summary>
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
        return target.color.a;
    }

    private void StartFadeTo(float targetAlpha, bool isFadeIn)
    {
        if (target == null) return;

        // Si ya hay un fade corriendo, lo cancelamos PERO mantenemos el alpha actual.
        CancelFade();

        float currentAlpha = target.color.a;

        // Si no hay tiempo o ya estamos ahí, aplicamos directo.
        if (transitionTime <= 0f || Mathf.Approximately(currentAlpha, targetAlpha))
        {
            ApplyAlpha(targetAlpha);

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

            float a = Mathf.Lerp(fromAlpha, toAlpha, normalized);
            ApplyAlpha(a);

            yield return null;
        }

        ApplyAlpha(toAlpha);
        fadeRoutine = null;

        if (useEvents)
        {
            if (isFadeIn) onFadeInComplete?.Invoke();
            else onFadeOutComplete?.Invoke();
        }
    }

    private void ApplyAlpha(float alpha)
    {
        if (target == null) return;

        // Respetamos el color base (RGB), solo tocamos alpha
        Color c = baseColor;
        c.a = alpha;
        target.color = c;
    }

    // Smoothstep: parecido al ease-in-out de CSS
    private float EaseInOut(float x)
    {
        return x * x * (3f - 2f * x);
    }
}
