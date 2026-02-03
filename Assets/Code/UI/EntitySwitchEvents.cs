using UnityEngine;
using UnityEngine.Events;

public class EntitySwitchEvents : MonoBehaviour
{
    [System.Serializable]
    public class SwitchEvent
    {
        public bool enabled = false;
        public UnityEvent onEvent;

        public void Invoke()
        {
            if (!enabled) return;
            onEvent?.Invoke();
        }
    }

    [Header("Lifecycle")]
    public SwitchEvent onAwake;
    public SwitchEvent onStart;
    public SwitchEvent onEnableEvent;
    public SwitchEvent onDisableEvent;

    [Header("Loop")]
    public SwitchEvent onUpdate;
    public SwitchEvent onFixedUpdate;
    public SwitchEvent onLateUpdate;

    [Header("Collisions (requires Collider + Rigidbody somewhere)")]
    public SwitchEvent onCollisionEnter;
    public SwitchEvent onCollisionStay;
    public SwitchEvent onCollisionExit;

    [Header("Triggers (requires IsTrigger = true)")]
    public SwitchEvent onTriggerEnter;
    public SwitchEvent onTriggerStay;
    public SwitchEvent onTriggerExit;

    private void Awake() => onAwake.Invoke();
    private void Start() => onStart.Invoke();
    private void OnEnable() => onEnableEvent.Invoke();
    private void OnDisable() => onDisableEvent.Invoke();

    private void Update() => onUpdate.Invoke();
    private void FixedUpdate() => onFixedUpdate.Invoke();
    private void LateUpdate() => onLateUpdate.Invoke();

    private void OnCollisionEnter(Collision collision) => onCollisionEnter.Invoke();
    private void OnCollisionStay(Collision collision) => onCollisionStay.Invoke();
    private void OnCollisionExit(Collision collision) => onCollisionExit.Invoke();

    private void OnTriggerEnter(Collider other) => onTriggerEnter.Invoke();
    private void OnTriggerStay(Collider other) => onTriggerStay.Invoke();
    private void OnTriggerExit(Collider other) => onTriggerExit.Invoke();
}
