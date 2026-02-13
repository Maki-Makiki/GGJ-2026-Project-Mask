using UnityEngine;

/// <summary>
/// Permite llamar LevelLoader desde UnityEvent con un string de escena.
/// Ej: SimpleDialogSystem puede disparar ChangeLevelOnEvent.
/// </summary>
public class LevelLoaderHandler : MonoBehaviour
{
    public void ChangeLevel(string sceneName)
    {
        if (LevelLoader.State != null)
            LevelLoader.State.ChangeLevelByName(sceneName);
        else
            Debug.LogWarning("LevelLoader no existe en la escena.");
    }
}
