using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager state;

    public enum m_ImputDivice
    {
        Xbox,
        Playstation,
        Switch,
        Keyboard,
        Gamepad
    }

    public m_ImputDivice ImputDivece;
    public PlayerInput playerImput;

    public InputList inputList;
    public InputIconsSet currentIconSet;

    public event Action OnDeviceChanged;
    [SerializeField] bool BlockedControllers = false;

    //private void Update()
    //{
    //    Debug.Log(playerImput.currentControlScheme);
    //    Debug.Log(string.Join(", ", playerImput.devices));
    //}

    public static bool IsReady { get; private set; }

    void Awake()
    {
        Init();
        IsReady = true;
    }

    public void Init()
    {
        transform.parent = null;
        if (state == null)
        {
            Debug.Log($"[{gameObject.name}({gameObject.GetInstanceID()})] yo soy el ControllerManager, Admiren mi poder!");
            state = this;
        }
        else
        {
            if (state != this)
            {
                Debug.Log($"[{gameObject.name}({gameObject.GetInstanceID()})] ya hay otro ControllerManager Me mato!");
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[{gameObject.name}({gameObject.GetInstanceID()})] yo ya era el ControllerManager Me mato!");
            }
        }

        DiviceChangedFunction();
    }

    public void DiviceChangedFunction()
    {
        if (playerImput == null)
        {
            playerImput = gameObject.GetComponent<PlayerInput>();
        }

        switch (playerImput.currentControlScheme)
        {   
            case "Xbox":
                ImputDivece = m_ImputDivice.Xbox;
                break;
            case "Playstation":
                ImputDivece = m_ImputDivice.Playstation;
                break;
            case "Switch":
                ImputDivece = m_ImputDivice.Switch;
                break;
            case "Keyboard":
                ImputDivece = m_ImputDivice.Keyboard;
                break;
            case "Gamepad":
                ImputDivece = m_ImputDivice.Gamepad;
                break;
        }

        //SetCurrentIconSet(ImputDivece);
        OnDeviceChanged?.Invoke();
    }

    //void SetCurrentIconSet(m_ImputDivice divece)
    //{
    //    currentIconSet = inputList.inputIconsSet.FirstOrDefault(s => s.ImputName == divece.ToString());
    //}

    public static InputIconsSet GetCurrentIconSet()
    {
        InputIconsSet currentIconSet = ControllerManager.state.inputList.inputIconsSet.FirstOrDefault(s => s.ImputName == ControllerManager.state.ImputDivece.ToString());
        return currentIconSet;
    }

    public static Vector2 GetActionVector2(string ActionName)
    {
        if (ControllerManager.state.BlockedControllers) return Vector2.zero;
        return state.playerImput.actions[ActionName].ReadValue<Vector2>();
    }

    public static bool GetActionWasPerformed(string ActionName)
    {
        if (ControllerManager.state.BlockedControllers) return false;
        return state.playerImput.actions[ActionName].WasPerformedThisFrame();
       
    }

    public static bool GetActionWasPressed(string ActionName)
    {
        if (ControllerManager.state.BlockedControllers) return false;
        return state.playerImput.actions[ActionName].WasPressedThisFrame();
    }

    public static bool GetActionWasRelased(string ActionName)
    {
        if (ControllerManager.state.BlockedControllers) return false;
        return state.playerImput.actions[ActionName].WasReleasedThisFrame();
    }

    public static void LockControls()
    {
        ControllerManager.state.BlockedControllers = true;
    }

    public static void UnlockControls()
    {
        ControllerManager.state.BlockedControllers = false;
    }
}
