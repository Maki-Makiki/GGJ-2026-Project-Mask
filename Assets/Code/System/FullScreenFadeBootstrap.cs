using UnityEngine;

/// <summary>
/// Instancia automáticamente el FullScreenFade si no existe.
/// Debe colocarse en cada escena.
/// Incluye sistema de debug reemplazable.
/// </summary>
public class FullScreenFadeBootstrap : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private FullScreenFade fadePrefab;
    [SerializeField] private bool enableDebug = false;

    private void Awake()
    {
        if (FullScreenFade.State == null)
        {
            if (!fadePrefab)
            {
                ConsoleDebug("No se asignó fadePrefab. No se puede instanciar FullScreenFade.");
                return;
            }

            Instantiate(fadePrefab);
            ConsoleDebug("FullScreenFade instanciado.");
        }
        else
        {
            ConsoleDebug("FullScreenFade ya existe. No se instancia otro.");
        }
    }

    #region Debug

    /// <summary>
    /// Método centralizado para logs del Bootstrap.
    /// Permite reemplazar fácilmente por una consola personalizada.
    /// </summary>
    protected virtual void ConsoleDebug(string text)
    {
        if(enableDebug)
            Debug.Log(text);
    }

    #endregion
}
