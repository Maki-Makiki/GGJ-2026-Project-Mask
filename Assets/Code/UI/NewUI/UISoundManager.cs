using UnityEngine;
using UnityEngine.SceneManagement;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager state;

    [Header("UI Sounds")]
    [SerializeField] private AudioSource tickAudio;
    [SerializeField] private AudioSource submitAudio;
    [SerializeField] private AudioSource cancelAudio;

    private void Awake()
    {
        this.gameObject.name = $"{this.gameObject.name} ({SceneManager.GetActiveScene().name})";
    }

    public void SetUISoundManager()
    {
        Debug.Log($"{this.gameObject.name} : soy state Ahora!");
        state = this;
    }

    public void PlayTick()
    {
        if (tickAudio) tickAudio.Play();
    }

    public void PlaySubmit()
    {
        if (submitAudio) submitAudio.Play();
    }

    public void PlayCancel()
    {
        if (cancelAudio) cancelAudio.Play();
    }
}