// AndroidInput.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class AndroidInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick & Buttons")]
    public RectTransform joystickHandle;
    public RectTransform joystickBackground;
    public Button PauseButton;
    public Button jumpButton;
    public Button actionButton;
    public Button maskButton;

    [SerializeField] private CharacterController characterController;

    [Header("Fade settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private bool autoHideOnNonTouch = true;
    [SerializeField] private bool startTransparent = true;



    private Vector2 joystickVector;

    void Awake()
    {
        // Obtener o agregar CanvasGroup
        canvasGroup = GetComponentInChildren<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Alpha inicial
        canvasGroup.alpha = startTransparent ? 0f : 1f;
        canvasGroup.interactable = !startTransparent;
        canvasGroup.blocksRaycasts = !startTransparent;
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

        if (!startTransparent) 
            CheckVisibility();
    }

    void OnDisable()
    {
        if (ControllerManager.state != null)
            ControllerManager.state.OnDeviceChanged -= CheckVisibility;
    }

    void Start()
    {
        AddPointerDownListener(PauseButton, () => characterController.SetPausePressed("AndroidInput"));
        AddPointerDownListener(jumpButton, () => characterController.SetJumpPressed("AndroidInput"));
        AddPointerDownListener(actionButton, () => characterController.SetActionPressed("AndroidInput"));
        AddPointerDownListener(maskButton, () => characterController.SetMaskPressed("AndroidInput"));
    }

    void Update()
    {
#if UNITY_ANDROID
        // Solo procesar joystick si el dispositivo actual es Touch
        if (ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch)
        {
            // Si hay toque, avisar a ControllerManager (ya debería estar Touch)
            if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
            {
                Vector2 handlePos = joystickHandle.anchoredPosition;
                float maxRadius = joystickBackground.sizeDelta.x / 2f;
                joystickVector = new Vector2(handlePos.x / maxRadius, 0f);
                joystickVector = Vector2.ClampMagnitude(joystickVector, 1f);

                characterController.SetHorizontalInput(joystickVector.x, "AndroidInput/Update()");
            }
        }
#endif
    }

    private void CheckVisibility()
    {
        bool shouldShow = ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch;
        if (!autoHideOnNonTouch)
            shouldShow = true;

        StopAllCoroutines();
        StartCoroutine(FadeCanvasGroup(shouldShow ? 1f : 0f, fadeDuration));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            t = t * t * (3f - 2f * t); // smoothstep
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
    }

    private void AddPointerDownListener(Button button, System.Action action)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerDown;
        entry.callback.AddListener((data) => action.Invoke());
        trigger.triggers.Add(entry);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // Para que al tocar ya se registre movimiento
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBackground,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        float maxRadius = joystickBackground.sizeDelta.x / 2f;
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, maxRadius);
        joystickHandle.anchoredPosition = clamped;

        joystickVector = clamped / maxRadius; // Normalizado [-1,1]
        characterController.SetHorizontalInput(joystickVector.x, "AndroidInput/OnDrag()");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        joystickHandle.anchoredPosition = Vector2.zero;
        joystickVector = Vector2.zero;
        characterController.SetHorizontalInput(0f, "AndroidInput/OnPointerUp()");
    }

}
