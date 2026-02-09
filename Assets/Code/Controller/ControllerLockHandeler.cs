using UnityEngine;

public class ControllerLockHandeler : MonoBehaviour
{

    public void ControllerManager_UnlocControls()
    {
        string result = ControllerManager.state == null ? "Null" : ControllerManager.state.gameObject.name;
        Debug.Log($"{this.gameObject.name}: ControllerManager.state es {ControllerManager.state.gameObject.name} voy a tratar de ejecutar ControllerManager.state.LockControls(true);");
        ControllerManager.state.LockControls(false);
    }

    public void ControllerManager_LockControls()
    {
        string result = ControllerManager.state == null ? "Null" : ControllerManager.state.gameObject.name;
        Debug.Log($"{this.gameObject.name}: ControllerManager.state es {ControllerManager.state.gameObject.name} voy a tratar de ejecutar ControllerManager.state.LockControls(false);");
        ControllerManager.state.LockControls(true);
    }

    public void ControllerManager_DiviceChangedFunction() 
    {
        string result = ControllerManager.state == null ? "Null" : ControllerManager.state.gameObject.name;
        Debug.Log($"{this.gameObject.name}: ControllerManager.state es {ControllerManager.state.gameObject.name} voy a tratar de ejecutar ControllerManager.state.DiviceChangedFunction();");
        ControllerManager.state.DiviceChangedFunction();
    }
}
