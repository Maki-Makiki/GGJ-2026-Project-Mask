using UnityEngine;
using UnityEngine.Events;

public class EventCaller : MonoBehaviour
{
    public UnityEvent EventToCall;
    public UnityEvent EventToCall1;
    public UnityEvent EventToCall2;
    public UnityEvent EventToCall3;
    public UnityEvent EventToCall4;
    public UnityEvent EventToCall5;
    public UnityEvent EventToCall6;
    public UnityEvent EventToCall7;
    public UnityEvent EventToCall8;
    public UnityEvent EventToCall9;
    public void CallEvent()
    {
        EventToCall.Invoke();
    }
    public void CallEvent1()
    {
        EventToCall1.Invoke();
    }
    public void CallEvent2()
    {
        EventToCall2.Invoke();
    }
    public void CallEvent3()
    {
        EventToCall3.Invoke();
    }
    public void CallEvent4()
    {
        EventToCall4.Invoke();
    }
    public void CallEvent5()
    {
        EventToCall5.Invoke();
    }
    public void CallEvent6()
    {
        EventToCall6.Invoke();
    }
    public void CallEvent7()
    {
        EventToCall7.Invoke();
    }
    public void CallEvent8()
    {
        EventToCall8.Invoke();
    }
    public void CallEvent9()
    {
        EventToCall9.Invoke();
    }
}
