using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class DynamicPlataform : MonoBehaviour
{
    public bool plataformA = true;
    public Tilemap tileMapRenderer;
    //public List<SpriteRenderer> tileMapRenderer;
    public BoxCollider2D boxCollider2D;
    public BoxCollider2D secondBoxCollider2D;

    //private List<Coroutine> colorLerpCoroutine;
    private Coroutine colorLerpCoroutine;

    private void OnEnable()
    {
        //if(colorLerpCoroutine == null) { colorLerpCoroutine = new List<Coroutine>(); }
        // Suscripción al evento del Singleton
        PlataformChange.state.OnPlataformChange += PlataformChangeNow;
        // Inicializar estado visual al activarse
        UpdateVisuals(true);
    }

    private void OnDisable()
    {
        if (PlataformChange.state != null)
            PlataformChange.state.OnPlataformChange -= PlataformChangeNow;
    }

    void PlataformChangeNow()
    {
        UpdateVisuals(false);
    }

    void UpdateVisuals(bool instant)
    {
        // Determinar si esta plataforma debe estar activa
        bool isActive = (plataformA == PlataformChange.state.plataformA);
        boxCollider2D.enabled = isActive;
        secondBoxCollider2D.enabled = isActive;

        //Elijo color segun si es Plataforma A o B 
        Color targetColor;
        if (plataformA)
            targetColor = isActive ? PlataformChange.state.ColorA_ON : PlataformChange.state.ColorA_OFF;
        else
            targetColor = isActive ? PlataformChange.state.ColorB_ON : PlataformChange.state.ColorB_OFF;


        //while(colorLerpCoroutine.Count < tileMapRenderer.Count)
        //{
        //    colorLerpCoroutine.Add(null);
        //}

        //for (int i = 0; i < tileMapRenderer.Count; i++)
        //{
            if (instant)
            {
                tileMapRenderer.color = targetColor;
                //tileMapRenderer[i].color = targetColor;
            }
            else
            {
                //Reseteo de corrutina
                if (colorLerpCoroutine != null) StopCoroutine(colorLerpCoroutine);
                //if (colorLerpCoroutine[i] != null) StopCoroutine(colorLerpCoroutine[i]);
            colorLerpCoroutine = StartCoroutine(AnimateColor(tileMapRenderer, targetColor));
            //colorLerpCoroutine[i] = StartCoroutine(AnimateColor(tileMapRenderer[i], targetColor));
        }
        //}

       
    }

    //IEnumerator AnimateColor(SpriteRenderer elemToColor, Color endColor)
    IEnumerator AnimateColor(Tilemap elemToColor, Color endColor)
    {
        Color startColor = elemToColor.color;
        float time = 0;
        float duration = PlataformChange.state.timeToChange;

        while (time < duration)
        {
            elemToColor.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        elemToColor.color = endColor;
    }
}
