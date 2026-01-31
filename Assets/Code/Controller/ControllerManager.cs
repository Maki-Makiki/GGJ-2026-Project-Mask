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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (state == null)
        {
            Debug.Log($"[{gameObject.name}({gameObject.GetInstanceID()})] yo soy el ControllerManager, Admiren mi poder!");
            state = this;
        }
        else
        {
            if(state != this)
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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DiviceChangedFunction()
    {
        if (playerImput == null) 
        {
            playerImput = this.gameObject.GetComponent<PlayerInput>();
        }

        switch (playerImput.currentControlScheme.ToString())
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

        //Check_MostrarControlesPantalla();
    }
}
