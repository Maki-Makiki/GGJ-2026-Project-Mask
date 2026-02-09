using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.Events;

public class LevelChanger : MonoBehaviour
{
    public UnityEvent onSceneLoaded; // evento opcional

    public void ChangeLevelByName(string Name)
    {
        StartCoroutine(LoadSceneRoutine(Name));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        Debug.Log($"LevelChanger: Vamos a cargar la escena {sceneName} desde {SceneManager.GetActiveScene().name}");

        // Carga la escena de forma asíncrona para poder disparar un evento después
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log($"LevelChanger: Escena {sceneName} cargada");

        // Evento opcional para que el fade haga FadeIn
        onSceneLoaded?.Invoke();
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
