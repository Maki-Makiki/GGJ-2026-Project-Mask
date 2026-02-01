using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene(0);
            UIManager.Instance.HideAll();
            Debug.Log("Start Game");
        }

        public void Settings()
        {
            Debug.Log("Settings");
            UIManager.Instance.Show(UIPage.Settings);
        }

        public void Quit()
        {
            Debug.Log("Quit");
            GameManager.Instance.QuitGame();
        }
    }
}
