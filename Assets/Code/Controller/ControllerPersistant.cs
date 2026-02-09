using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ControllerPersistant : MonoBehaviour
{

    public static ControllerPersistant Instance;

    [SerializeField] bool initialized = false;
    [SerializeField] ControllerManager.m_ImputDivice lastInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        CheckInstance();
    }

    private void CheckInstance()
    {
        transform.parent = null;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }

        if(Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    public static bool IsSeted()
    {
        return Instance.initialized;
    }

    public static void Initialize()
    {
        Instance.initialized = true;
    }

    public static ControllerManager.m_ImputDivice GetLastInput()
    {
        return Instance.lastInput;
    }

    public static void SetLastInput(ControllerManager.m_ImputDivice newInput)
    {
        Instance.lastInput = newInput;
        Initialize();
    }
}
