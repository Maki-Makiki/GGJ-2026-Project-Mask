using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Permite llamar LevelLoader desde UnityEvent con un string de escena.
/// Ej: SimpleDialogSystem puede disparar ChangeLevelOnEvent.
/// </summary>
public class LevelLoaderHandler : MonoBehaviour
{

    public void ChangeLevel(string sceneName)
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (LevelLoader.State != null)
            LevelLoader.State.ChangeLevelByName(sceneName);
        else
            Debug.LogWarning("LevelLoader no existe en la escena.");
    }

    public void QuitGame()
    {
        EventSystem.current.SetSelectedGameObject(null);
        if (LevelLoader.State != null)
            LevelLoader.State.QuitGame();
        else
            Debug.LogWarning("LevelLoader no existe en la escena.");
    }
}
