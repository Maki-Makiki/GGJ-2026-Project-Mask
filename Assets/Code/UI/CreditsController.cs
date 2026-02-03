using UnityEngine;
using UnityEngine.Events;

public class CreditsController : MonoBehaviour
{

    public float normalSpeed = 1f;
    public float fastSpeed = 2.5f;
    public string ActionFastForward = "Accept";
    public string ActionSkip = "Pause";
    public bool acelerar = false;
    public Animator animator;

    public UnityEvent onCreditsEnd;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (ControllerManager.GetActionWasPressed(ActionFastForward))
        {
            acelerar = true;
        }

        if (ControllerManager.GetActionWasRelased(ActionFastForward))
        {
            acelerar = false;
        }

        if (acelerar)
        {
            animator.speed = fastSpeed;
        }
        else
        {
            animator.speed = normalSpeed;
        }

        if (ControllerManager.GetActionWasPressed(ActionSkip))
        {
            onCreditsEnd.Invoke();
            this.enabled = false;
        }
    }

    public void OnCreditsEndInvoque()
    {
        onCreditsEnd.Invoke();
        this.enabled = false;
    }
}
