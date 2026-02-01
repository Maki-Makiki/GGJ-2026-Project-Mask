using System;
using UnityEngine;

public class PlataformChange : MonoBehaviour
{
    public static PlataformChange state;

    [Header("Configuración de Colores")]
    public Color ColorA_ON = Color.cyan;
    public Color ColorA_OFF = new Color(0, 1, 1, 0.3f);
    public Color ColorB_ON = Color.green;
    public Color ColorB_OFF = new Color(0, 1, 0, 0.3f);

    public float timeToChange = 0.3f;
    public bool plataformA = true;

    public event Action OnPlataformChange;

    void Awake()
    {
        if (state == null)
        {
            state = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ContextMenu("Toggle Platforms")]
    public void SetPlataformAState()
    {
        plataformA = !plataformA;
        // Notificar a todas las plataformas del cambio
        OnPlataformChange?.Invoke();
    }
}
