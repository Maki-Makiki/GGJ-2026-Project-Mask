using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UICancelHandler : MonoBehaviour
{
    [SerializeField] private Button backButton; // botón que hace de "volver"

    private void OnEnable()
    {
        ControllerManager.state.OnUICancel += HandleCancel;
    }

    private void OnDisable()
    {
        ControllerManager.state.OnUICancel -= HandleCancel;
    }

    private void HandleCancel()
    {
        if (!gameObject.activeInHierarchy)
            return;

        if (UISoundManager.state != null)
            UISoundManager.state.PlayCancel();

        backButton.onClick.Invoke();
        this.enabled = false;
    }
}
