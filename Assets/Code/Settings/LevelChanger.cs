using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelChanger : MonoBehaviour
{
    public void ChangeLevelByName(string Name)
    {
        SceneManager.LoadScene(Name);
    }

    public void ChangeLevelByNumber(string Name)
    {
        SceneManager.LoadScene(Name);
    }

}
