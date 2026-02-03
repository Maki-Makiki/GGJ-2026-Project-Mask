using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.UI
{
    public class UIControlPrompt : MonoBehaviour
    {
        public Image promptImage;

        public void ShowActionPrompt(string actionName)
        {
            if(ControllerManager.state != null)
            {
                var action = ControllerManager.state.playerImput.actions[actionName];
                var bindingIndex = action.GetBindingIndex(InputBinding.MaskByGroup(ControllerManager.state.playerImput.currentControlScheme));
                action.GetBindingDisplayString(bindingIndex, out var deviceLayout, out string controlPath);
                var a = ControllerManager.state.currentIconSet.Buttons;
                promptImage.sprite = a.FirstOrDefault(b => b.path == controlPath)?.sprite;
            }
        }
    }
}
