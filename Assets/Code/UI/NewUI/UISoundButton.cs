using UnityEngine;
using UnityEngine.EventSystems;

public class UISoundButton : MonoBehaviour,
    ISelectHandler,
    ISubmitHandler,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [Header("Flags")]
    public bool soundFirstTime = true;
    public bool firstTime = false;
    public bool updateSelectOnMouseOver = true;

    [Header("input Sounds")]
    public bool onSelectSound = true;
    public bool onSubmitSound = true;

    [Header("Mouse Sounds")]
    public bool onHoverSound = true;
    public bool onClickSound = true;

    public void OnSelect(BaseEventData eventData)
    {
        if (!onSelectSound) return;
        PlaySelectLogic();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(updateSelectOnMouseOver) EventSystem.current.SetSelectedGameObject(gameObject);
        if (!onHoverSound) return;
        PlaySelectLogic();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        if(!onSubmitSound) return; 
        PlaySubmitLogic();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!onClickSound) return;
        PlaySubmitLogic();
    }

    private void PlaySubmitLogic()
    {
        if (UISoundManager.state == null) return;
        UISoundManager.state.PlaySubmit();
    }

    private void PlaySelectLogic()
    {
        if (UISoundManager.state == null) return;
        if (!soundFirstTime)
        {
            if (!firstTime)
            {
                firstTime = true;
                return;
            }
        }

        UISoundManager.state.PlayTick();
    }

    public void ResetFirstTime()
    {
        firstTime = false;
    }
}
