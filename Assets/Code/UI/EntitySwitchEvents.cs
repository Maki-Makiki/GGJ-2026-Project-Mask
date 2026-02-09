using UnityEngine;
using UnityEngine.Events;
using System.Text;

public class EntitySwitchEvents : MonoBehaviour
{
    [System.Serializable]
    public class SwitchEvent
    {
        public bool enabled = false;
        public bool debug = false;
        public UnityEvent onEvent;

        public void Invoke(string eventName, MonoBehaviour owner)
        {
            if (!enabled) return;

            if (debug)
            {
                Debug.Log(BuildDebugMessage(eventName, owner), owner);
            }

            onEvent?.Invoke();
        }

        private string BuildDebugMessage(string eventName, MonoBehaviour owner)
        {
            var sb = new StringBuilder();
            sb.Append($"[EntitySwitchEvents] {owner.gameObject.name} -> {eventName}");

            if (onEvent == null || onEvent.GetPersistentEventCount() == 0)
            {
                sb.Append("\n  (No listeners)");
                return sb.ToString();
            }

            sb.Append("\n  Listeners:");

            for (int i = 0; i < onEvent.GetPersistentEventCount(); i++)
            {
                var target = onEvent.GetPersistentTarget(i);
                var method = onEvent.GetPersistentMethodName(i);

                sb.Append($"\n   • {target?.name ?? "NULL"}.{method}()");
            }

            return sb.ToString();
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

    [Header("Collisions")]
    public SwitchEvent onCollisionEnter;
    public SwitchEvent onCollisionStay;
    public SwitchEvent onCollisionExit;

    [Header("Triggers")]
    public SwitchEvent onTriggerEnter;
    public SwitchEvent onTriggerStay;
    public SwitchEvent onTriggerExit;

    private void Awake() => onAwake.Invoke(nameof(Awake), this);
    private void Start() => onStart.Invoke(nameof(Start), this);
    private void OnEnable() => onEnableEvent.Invoke(nameof(OnEnable), this);
    private void OnDisable() => onDisableEvent.Invoke(nameof(OnDisable), this);

    private void Update() => onUpdate.Invoke(nameof(Update), this);
    private void FixedUpdate() => onFixedUpdate.Invoke(nameof(FixedUpdate), this);
    private void LateUpdate() => onLateUpdate.Invoke(nameof(LateUpdate), this);

    private void OnCollisionEnter(Collision collision) =>
        onCollisionEnter.Invoke(nameof(OnCollisionEnter), this);

    private void OnCollisionStay(Collision collision) =>
        onCollisionStay.Invoke(nameof(OnCollisionStay), this);

    private void OnCollisionExit(Collision collision) =>
        onCollisionExit.Invoke(nameof(OnCollisionExit), this);

    private void OnTriggerEnter(Collider other) =>
        onTriggerEnter.Invoke(nameof(OnTriggerEnter), this);

    private void OnTriggerStay(Collider other) =>
        onTriggerStay.Invoke(nameof(OnTriggerStay), this);

    private void OnTriggerExit(Collider other) =>
        onTriggerExit.Invoke(nameof(OnTriggerExit), this);
}
