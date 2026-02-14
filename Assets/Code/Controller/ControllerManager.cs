using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;


public class ControllerManager : MonoBehaviour
{
    public static ControllerManager state;
    public enum m_ImputDivice { Xbox, Playstation, Switch, Keyboard, Gamepad, Touch }

    public m_ImputDivice ImputDivece;
    public PlayerInput playerImput;
    public InputList inputList;

    public event System.Action OnUICancel;
    public event Action OnDeviceChanged;
    public event Action OnPause;
    public UnityEvent OnPauseStart;
    public UnityEvent OnPauseEnd;
    public bool isPaused = false;
    public bool BlockedControllers = false;

    public UISoundManager uISoundManager;

#if UNITY_ANDROID
    [SerializeField] string TouchAction = "Touch";
    private bool touchPressed = false;
    private bool joystickActive = false;
#endif

    [Header("Debug / Console")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private TMPro.TMP_Text consoleText;
    [SerializeField] private List<string> consoleLines;
    [SerializeField] private int lineMax = 10;

    public static bool IsReady { get; private set; }

    void Awake()
    {
        string nombreEscena = SceneManager.GetActiveScene().name;
        this.gameObject.name = $"ControllerManager ({nombreEscena})";
        
        if (state != null && state != this)
        {
            Debug.Log($"{this.gameObject.name}: ControllerManager.state es {ControllerManager.state.gameObject.name} a mi me toca morir nos vemos!");
            this.gameObject.SetActive( false );
            return;
        }


        state = this;
        Debug.Log($"{this.gameObject.name}: soy el nuevo ControllerManager.state!");
        
        DontDestroyOnLoad(gameObject);

        playerImput = GetComponent<PlayerInput>();
        playerImput.enabled = true;
        IsReady = true;
    }

    void InitInput()
    {
        DiviceChangedFunction();
    }

    public void OnDestroy()
    {
        IsReady = false;
        ControllerManager.state = null;
    }

    public void HandleUICancel()
    {
        OnUICancel?.Invoke();
    }

    public void ResetUISoundManager()
    {
        uISoundManager.SetUISoundManager();
    }

    public void PauseGame()
    {
        PauseGame(!isPaused);
    }

    public void PauseGame(bool newPauseState)
    {
        isPaused = newPauseState;
        Time.timeScale = !isPaused ? 1.0f : 0.0f;
        OnPause?.Invoke();

        if(isPaused) { OnPauseStart.Invoke(); } else { OnPauseEnd.Invoke(); }
    }

    public void DiviceChangedFunction()
    {
        if (playerImput == null) return;

#if UNITY_ANDROID
        var touchAction = playerImput.actions[TouchAction];
        touchAction.performed -= OnTouchPerformed;
        touchAction.performed += OnTouchPerformed;
        touchAction.canceled -= OnTouchCanceled;
        touchAction.canceled += OnTouchCanceled;
#endif

        switch (playerImput.currentControlScheme)
        {
            case "Xbox": ImputDivece = m_ImputDivice.Xbox; break;
            case "Playstation": ImputDivece = m_ImputDivice.Playstation; break;
            case "Switch": ImputDivece = m_ImputDivice.Switch; break;
            case "Keyboard": ImputDivece = m_ImputDivice.Keyboard; break;
            case "Gamepad": ImputDivece = m_ImputDivice.Gamepad; break;
            case "Touch": ImputDivece = m_ImputDivice.Touch; break;
        }

        OnDeviceChanged?.Invoke();
    }

    public static void ConsoleLog(string printText)
    {
        if (!state.enableDebug || state.consoleText == null) return;

        state.consoleLines.Add(MathF.Round(Time.time, 1) + " > " + printText);
        if (state.consoleLines.Count > state.lineMax)
            state.consoleLines.RemoveAt(0);

        ConsoleRefresh();
    }

    public static void ConsoleRefresh()
    {
        if (!state.enableDebug || state.consoleText == null) return;

        state.consoleText.text = string.Join("\n", state.consoleLines);
    }

#if UNITY_ANDROID
    private void OnTouchPerformed(InputAction.CallbackContext ctx)
    {

        if (!joystickActive)
        {
            ImputDivece = m_ImputDivice.Touch;
            touchPressed = true;
            OnDeviceChanged?.Invoke();
        }
    }

    private void OnTouchCanceled(InputAction.CallbackContext ctx)
    {
        touchPressed = false;

        if (!joystickActive)
            DiviceChangedFunction();
    }
#endif

    public static InputIconsSet GetCurrentIconSet()
    {
        return state.inputList.inputIconsSet.FirstOrDefault(s => s.ImputName == state.ImputDivece.ToString());
    }

    public static Vector2 GetActionVector2(string ActionName)
    {
        if (state.BlockedControllers) return Vector2.zero;
        return state.playerImput.actions[ActionName].ReadValue<Vector2>();
    }
    public static Quaternion GetActionQuaternion(string ActionName)
    {
        if (state.BlockedControllers) return Quaternion.identity;
        return state.playerImput.actions[ActionName].ReadValue<Quaternion>();
    }

    public static Vector3 GetActionVector3(string ActionName)
    {
        if (state.BlockedControllers) return Vector3.zero;
        return state.playerImput.actions[ActionName].ReadValue<Vector3>();

    }

    public static bool GetActionWasPerformed(string ActionName)
    {
        return !state.BlockedControllers && state.playerImput.actions[ActionName].WasPerformedThisFrame();
    }

    public static bool GetActionWasPressed(string ActionName)
    {
        return !state.BlockedControllers && state.playerImput.actions[ActionName].WasPressedThisFrame();
    }

    public static bool GetActionWasReleased(string ActionName)
    {
        return !state.BlockedControllers && state.playerImput.actions[ActionName].WasReleasedThisFrame();
    }

    public void LockControls(bool state) => BlockedControllers = state;


    public static void LockControls() => state.BlockedControllers = true;
    public static void UnlockControls() => state.BlockedControllers = false;

    public void SetDeviceToTouch()
    {
        ImputDivece = m_ImputDivice.Touch;
        OnDeviceChanged?.Invoke();
    }

    private void DebugLog(string msg)
    {
        if (enableDebug) Debug.Log(msg);
    }

    public static bool GetBlockedControllers() { return state.BlockedControllers; }
}
