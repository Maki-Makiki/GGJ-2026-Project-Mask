using UnityEngine;

public class LevelLoaderBootstrap : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LevelLoader loaderPrefab;
    [SerializeField] private bool enableDebug = false;

    private void Awake()
    {
        if (LevelLoader.State == null)
        {
            if (!loaderPrefab)
            {
                ConsoleDebug("No se asignó loaderPrefab. No se puede instanciar LevelLoader.");
                return;
            }

            Instantiate(loaderPrefab);
            ConsoleDebug("LevelLoader instanciado.");
        }
        else
        {
            ConsoleDebug("LevelLoader ya existe. No se instancia otro.");
        }
    }


    #region Debug

    /// <summary>
    /// Método centralizado para logs del Bootstrap.
    /// Permite reemplazar fácilmente por una consola personalizada.
    /// </summary>
    protected virtual void ConsoleDebug(string text)
    {
        if (enableDebug)
            Debug.Log(text);
    }
    #endregion
}
