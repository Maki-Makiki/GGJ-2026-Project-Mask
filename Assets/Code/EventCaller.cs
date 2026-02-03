using UnityEngine;
using UnityEngine.Events;

public class EventCaller : MonoBehaviour
{
    public UnityEvent EventToCall;
    public void CallEvent()
    {
        EventToCall.Invoke();
    }
}
