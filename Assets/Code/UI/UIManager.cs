using System;
using UnityEngine;

namespace Code.UI
{
    public enum UIPage
    {
        MainMenu,
        Settings,
        Pause
    }

    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        
        public GameObject mainMenu;
        public GameObject settingsMenu;
        public GameObject pauseMenu;
        public GameObject controlPrompt;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public void Show(UIPage page)
        {
            HideAll();

            switch (page)
            {
                case UIPage.MainMenu: mainMenu.SetActive(true); break;
                case UIPage.Settings: settingsMenu.SetActive(true); break;
                case UIPage.Pause: pauseMenu.SetActive(true); break;
            }
        }

        public void HideAll()
        {
            mainMenu.SetActive(false);
            settingsMenu.SetActive(false);
            pauseMenu.SetActive(false);
        }
    }
}