using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class InputFilter
{
    public ControllerManager.m_ImputDivice inputDevice;
}

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeWithInputFilter : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private bool startVisible = true;

    [Header("Input Filters")]
    [SerializeField] private List<InputFilter> inputFilters;

    private Coroutine fadeCoroutine;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = startVisible ? 1f : 0f;
        canvasGroup.interactable = startVisible;
        canvasGroup.blocksRaycasts = startVisible;
    }

    void OnEnable()
    {
        StartCoroutine(WaitForControllerManager());
    }

    IEnumerator WaitForControllerManager()
    {
        while (!ControllerManager.IsReady)
            yield return null;

        ControllerManager.state.OnDeviceChanged -= CheckVisibility;
        ControllerManager.state.OnDeviceChanged += CheckVisibility;

        CheckVisibility();
    }

    void OnDisable()
    {
        if (ControllerManager.state != null)
            ControllerManager.state.OnDeviceChanged -= CheckVisibility;
    }

    /// <summary>
    /// Comprueba si el CanvasGroup debe mostrarse u ocultarse según el InputFilter
    /// </summary>
    public void CheckVisibility()
    {
        if (canvasGroup == null) return;

        bool shouldHide = false;

        if (inputFilters != null && inputFilters.Count > 0)
        {
            var currentDevice = ControllerManager.state.ImputDivece;
            if (inputFilters.Any(f => f.inputDevice == currentDevice))
            {
                shouldHide = true;
            }
        }

        FadeCanvasGroup(shouldHide ? 0f : 1f);
    }

    /// <summary>
    /// Inicia la corrutina de fade
    /// </summary>
    /// <param name="targetAlpha"></param>
    public void FadeCanvasGroup(float targetAlpha)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
    }

    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            t = t * t * (3f - 2f * t); // Smoothstep
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
    }
}
