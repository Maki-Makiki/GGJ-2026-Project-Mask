using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

namespace Code.UI
{
    public class UIControlPrompt : MonoBehaviour
    {
        public Image promptImage;
        public bool autoStart = true;
        public string autoStartAction = "ActionName";

        [SerializeField] string currentAction;
        [SerializeField] string currentBinding;

        void OnEnable()
        {
            StartCoroutine(WaitForControllerManager());
        }

        IEnumerator WaitForControllerManager()
        {
            while (!ControllerManager.IsReady)
                yield return null;

            ControllerManager.state.OnDeviceChanged -= RefreshPrompt;
            ControllerManager.state.OnDeviceChanged += RefreshPrompt;

            RefreshPrompt();
        }

        void OnDisable()
        {
            if (ControllerManager.state != null)
                ControllerManager.state.OnDeviceChanged -= RefreshPrompt;
        }

        void Start()
        {
            if (autoStart)
            {
                ShowActionPrompt(autoStartAction);
            }
        }

        public void ShowActionPrompt(string actionName)
        {
            currentAction = actionName;
            RefreshPrompt();
        }

        void RefreshPrompt()
        {
            if (ControllerManager.state == null) return;
            if (string.IsNullOrEmpty(currentAction)) return;

            Debug.Log("ControllerManager OK!");

            var playerInput = ControllerManager.state.playerImput;
            var action = playerInput.actions[currentAction];

            var bindingIndex = action.GetBindingIndex(
                InputBinding.MaskByGroup(playerInput.currentControlScheme)
            );

            if (bindingIndex < 0)
            {
                promptImage.enabled = false;
                return;
            }

            action.GetBindingDisplayString(
                bindingIndex,
                out var deviceLayout,
                out string controlPath
            );

            var iconSet = ControllerManager.GetCurrentIconSet();
            var icon = iconSet.Buttons.FirstOrDefault(b => b.path == controlPath);

            promptImage.sprite = icon?.sprite;
            promptImage.enabled = icon != null;
        }
    }
}
