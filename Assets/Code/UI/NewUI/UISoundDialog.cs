using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundDialog : MonoBehaviour,
    ISubmitHandler,
    IPointerClickHandler
{
    [Header("input Sounds")]
    public bool onSubmitSound = true;

    [Header("Mouse Sounds")]
    public bool onClickSound = true;

    public SimpleDialogueIntro simpleDialogueIntro;

    public void OnSubmit(BaseEventData eventData)
    {
        if (!onSubmitSound) return;
        PlaySubmitLogic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!onClickSound) return;
        PlaySubmitLogic();
    }

    private void PlaySubmitLogic()
    {
        simpleDialogueIntro.PlayNextOrSkipSound();
    }

}

