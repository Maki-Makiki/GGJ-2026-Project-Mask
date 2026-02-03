using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code
{
    public enum GameScenes
    {
        MainMenu,
        Intro,
        TutorialMono,
        Lvl1,
        Lvl2
    }
    
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        private void Awake()
        {
            if(Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;
        }

        public void LoadScene(GameScenes scene)
        {
            SceneManager.LoadScene((int)scene);
        }
        
        public void QuitGame()
        {
            Debug.Log("Quit Game");
            Application.Quit();
        }
    }
}