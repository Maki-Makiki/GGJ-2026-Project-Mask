using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Code.UI
{
    [System.Serializable]
    public class InputFilter
    {
        public ControllerManager.m_ImputDivice imputDevice;
    }

    public class UIControlPrompt : MonoBehaviour
    {
        public Image promptImage;
        public bool autoStart = true;
        public string autoStartAction = "ActionName";

        [SerializeField] string currentAction;
        [SerializeField] string currentBinding;

        [SerializeField] float switchInterval = 2f; // segundos por defecto
        [SerializeField] private int currentIconIndex = 0;
        [SerializeField] private float switchTimer = 0f;
        [SerializeField] private List<ButtomItem> currentIcons;
        [SerializeField] private List<InputBinding> currentBindings;

        [Header("Canvas Group & Fade")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 0.3f;

        [Header("Input Filters")]
        [SerializeField] private List<InputFilter> inputFilters;

       private Coroutine fadeCoroutine;

        void OnEnable()
        {
            StartCoroutine(WaitForControllerManager());
        }

        IEnumerator WaitForControllerManager()
        {
            while (!ControllerManager.IsReady)
                yield return null;

            ControllerManager.state.OnDeviceChanged -= RefreshPrompt;
            ControllerManager.state.OnDeviceChanged += RefreshPrompt;

            RefreshPrompt();
        }

        void OnDisable()
        {
            if (ControllerManager.state != null)
                ControllerManager.state.OnDeviceChanged -= RefreshPrompt;
        }

        void Start()
        {
            if (autoStart)
            {
                ShowActionPrompt(autoStartAction);
            }
        }

        void Update()
        {
            if (currentIcons == null || currentIcons.Count <= 1)
                return;

            switchTimer += Time.deltaTime;
            if (switchTimer >= switchInterval)
            {
                switchTimer = 0f;
                currentIconIndex = (currentIconIndex + 1) % currentIcons.Count;
                if (promptImage != null && currentIcons.Count > 0)
                    promptImage.sprite = currentIcons[currentIconIndex].sprite;
            }
        }

        public void ShowActionPrompt(string actionName)
        {
            currentAction = actionName;
            RefreshPrompt();
        }

        void RefreshPrompt()
        {
            if (ControllerManager.state == null) return;
            if (string.IsNullOrEmpty(currentAction)) return;

            // --- Chequeo de input filters ---
            if (inputFilters != null && inputFilters.Count > 0)
            {
                var currentDevice = ControllerManager.state.ImputDivece;
                if (inputFilters.Any(f => f.imputDevice == currentDevice))
                {
                    FadeCanvasGroup(0f);
                    return;
                }
            }

            // Si llegamos acá, device no está filtrado → fade in
            FadeCanvasGroup(1f);

            var playerInput = ControllerManager.state.playerImput;
            var action = playerInput.actions[currentAction];

            var iconSet = ControllerManager.GetCurrentIconSet();
            currentIcons = new List<ButtomItem>();

            if (iconSet.useActionName)
            {
                currentBinding = currentAction;
                currentIcons = iconSet.Buttons
                    .Where(b => b.path == currentBinding)
                    .ToList();
            }
            else
            {
                string scheme = playerInput.currentControlScheme;

                currentBindings = action.bindings
                    .Where(b => !b.isComposite && b.groups.Contains(scheme))
                    .ToList();

                foreach (var binding in currentBindings)
                {
                    string controlPath = binding.effectivePath.Split('/').Last();
                    var icon = iconSet.Buttons.FirstOrDefault(b => b.path == controlPath);
                    if (icon != null && !currentIcons.Contains(icon))
                        currentIcons.Add(icon);
                }
            }

            // --- Fade out si no hay iconos ---
            if (currentIcons.Count == 0)
            {
                FadeCanvasGroup(0f);
                return;
            }

            // Debug: mostrar todos los paths
            currentBinding = string.Join(", ", currentIcons.Select(i => i.path));

            // Empezamos desde el primero para el switch
            currentIconIndex = 0;
            switchTimer = 0f;
            if (promptImage != null)
            {
                promptImage.sprite = currentIcons[currentIconIndex].sprite;
                promptImage.enabled = true;
            }
        }

        private void FadeCanvasGroup(float targetAlpha)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha));
        }

        private IEnumerator FadeCoroutine(float targetAlpha)
        {
            if (canvasGroup == null) yield break;

            float startAlpha = canvasGroup.alpha;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float t = timer / fadeDuration;
                t = t * t * (3f - 2f * t); // smoothstep
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
            canvasGroup.interactable = targetAlpha > 0f;
            canvasGroup.blocksRaycasts = targetAlpha > 0f;
        }
    }
}
