using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;

public class DynamicPlataform : MonoBehaviour
{
    public bool plataformA = true;
    public Tilemap tileMapRenderer;
    public BoxCollider2D boxCollider2D;

    private Coroutine colorLerpCoroutine;

    private void OnEnable()
    {
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

        //Elijo color segun si es Plataforma A o B 
        Color targetColor;
        if (plataformA)
            targetColor = isActive ? PlataformChange.state.ColorA_ON : PlataformChange.state.ColorA_OFF;
        else
            targetColor = isActive ? PlataformChange.state.ColorB_ON : PlataformChange.state.ColorB_OFF;

        if (instant)
        {
            tileMapRenderer.color = targetColor;
        }
        else
        {
            //Reseteo de corrutina
            if (colorLerpCoroutine != null) StopCoroutine(colorLerpCoroutine);
            colorLerpCoroutine = StartCoroutine(AnimateColor(targetColor));
        }
    }

    IEnumerator AnimateColor(Color endColor)
    {
        Color startColor = tileMapRenderer.color;
        float time = 0;
        float duration = PlataformChange.state.timeToChange;

        while (time < duration)
        {
            tileMapRenderer.color = Color.Lerp(startColor, endColor, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        tileMapRenderer.color = endColor;
    }
}
