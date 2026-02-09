using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using static UnityEngine.UI.Image;

public class CreditsController : MonoBehaviour
{

    public float normalSpeed = 1f;
    public float fastSpeed = 2.5f;
    public string ActionFastForward = "Accept";
    public string ActionSkip = "Pause";
    public bool acelerar = false;
    public Animator animator;

    public UnityEvent onCreditsEnd;
    public Button SkipButton;
    public Button SpeedButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AddPointerDownListener(SkipButton, () => SkipPressed("CreditsController/Touch"));
        AddPointerDownListener(SpeedButton, () => FastPressed("CreditsController/Touch"));
        AddPointerUpListener(SpeedButton, () => FasRelased("CreditsController/Touch"));
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

    private void AddPointerUpListener(Button button, System.Action action)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerUp;
        entry.callback.AddListener((data) => action.Invoke());
        trigger.triggers.Add(entry);
    }


    // Update is called once per frame
    void Update()
    {
        //Debug.Log("CreditsController().Update()");
        if (ControllerManager.GetActionWasPressed(ActionFastForward))
        {
            FastPressed("CreditsController/InputSystem/" + ControllerManager.state.ImputDivece.ToString());
        }

        if (ControllerManager.GetActionWasReleased(ActionFastForward))
        {
            FasRelased("CreditsController/InputSystem/" + ControllerManager.state.ImputDivece.ToString());
        }

        if (ControllerManager.GetActionWasPressed(ActionSkip))
        {
            SkipPressed("CreditsController/InputSystem/" + ControllerManager.state.ImputDivece.ToString());
        }

    }

    public void FastPressed(string origin)
    {
        FastMode(true, origin);
    }

    public void FasRelased(string origin)
    {
        FastMode(false, origin);
    }

    public void FastMode(bool newState, string origin)
    {
        Debug.Log("FastMode() => Origin: " + origin);
        acelerar = newState;
        animator.speed = (acelerar) ? fastSpeed : normalSpeed;
    }

    public void SkipPressed(string origin)
    {
        OnCreditsEndInvoque(origin);
    }

    public void OnCreditsEndInvoque(string origin)
    {
        Debug.Log("OnCreditsEndInvoque() => Origin: " + origin);
        onCreditsEnd.Invoke();
        this.enabled = false;
    }


}
