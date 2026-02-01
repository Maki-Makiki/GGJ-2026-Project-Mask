using UnityEngine;

namespace Code.UI
{
    public class UIPause : MonoBehaviour
    {
        public void Resume()
        {
            UIManager.Instance.HideAll();
        }

        public void Settings()
        {
            UIManager.Instance.Show(UIPage.Settings);
        }

        public void MainMenu()
        {
            UIManager.Instance.Show(UIPage.MainMenu);
        }
    }
}
