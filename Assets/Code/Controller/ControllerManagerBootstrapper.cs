using UnityEngine;

public class ControllerManagerBootstrapper : MonoBehaviour
{
    public GameObject controllerManagerPrefab; // tu prefab

    void Awake()
    {
        if (ControllerManager.state == null)
        {
            Instantiate(controllerManagerPrefab);
            Debug.Log("ControllerManager instanciado por bootstrapper");
        }
        else
        {
            Debug.Log("ControllerManager ya existe, no se instancia otro");
        }
    }
}