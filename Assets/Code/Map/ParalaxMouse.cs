using System.Collections.Generic;
using UnityEngine;

public class ParalaxMouse : MonoBehaviour
{
    [Header("Mouse Input (Input System)")]
    [Tooltip("Nombre del Action en el Input System que devuelve el mouse position (Vector2). Ej: MousePosition")]
    public string mousePositionAction = "MousePosition";

    [Header("Parallax Settings")]
    public Vector2 paralaxMulty = new Vector2(0.5f, 0.5f);

    [Tooltip("Si está activo, invierte el movimiento del parallax (efecto contrario).")]
    public bool invertX = false;
    public bool invertY = false;

    [Tooltip("Si está activo, el parallax se mueve en el mismo sentido (normalmente sí).")]
    public bool sameDirection = true;

    [Header("Mouse Behavior")]
    [Tooltip("Cuánto se mueve el parallax como máximo en unidades del mundo cuando el mouse está en el borde de la pantalla.")]
    public Vector2 maxWorldOffset = new Vector2(1f, 1f);

    [Tooltip("Suavizado del movimiento (0 = instantáneo, más alto = más suave).")]
    public float smooth = 10f;

    [Tooltip("Si está activo, el parallax se calcula respecto a donde estaba el mouse al iniciar.")]
    public bool useMouseStartAsCenter = true;

    [Header("Limits (world position clamp)")]
    public bool useLimits = true;
    public Vector2 min = new Vector2(-999, -999);
    public Vector2 max = new Vector2(999, 999);

    [Header("Objects")]
    public List<Transform> objetos = new List<Transform>();

    // Internals
    private Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();
    private Dictionary<Transform, Vector3> currentPositions = new Dictionary<Transform, Vector3>();

    private Vector2 mouseStartPos;
    private bool mouseStartCaptured = false;

    void Start()
    {
        startPositions.Clear();
        currentPositions.Clear();

        for (int i = 0; i < objetos.Count; i++)
        {
            if (objetos[i] == null) continue;
            startPositions[objetos[i]] = objetos[i].position;
            currentPositions[objetos[i]] = objetos[i].position;
        }

        // Capturamos el mouse inicial como "centro"
        CaptureMouseStart();
    }

    void LateUpdate()
    {
        if (ControllerManager.state == null || ControllerManager.state.playerImput == null)
            return;

        Vector2 mousePos = ControllerManager.GetActionVector2(mousePositionAction);

        if (!mouseStartCaptured)
            CaptureMouseStart();

        // Centro para normalizar
        Vector2 center = useMouseStartAsCenter ? mouseStartPos : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        // Mouse delta desde el centro en píxeles
        Vector2 deltaPixels = mousePos - center;

        // Normalizamos a -1..1 aprox (si está en bordes de pantalla)
        Vector2 halfScreen = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Vector2 normalized = new Vector2(
            halfScreen.x > 0 ? deltaPixels.x / halfScreen.x : 0f,
            halfScreen.y > 0 ? deltaPixels.y / halfScreen.y : 0f
        );

        normalized.x = Mathf.Clamp(normalized.x, -1f, 1f);
        normalized.y = Mathf.Clamp(normalized.y, -1f, 1f);

        float dirX = sameDirection ? 1f : -1f;
        float dirY = sameDirection ? 1f : -1f;

        if (invertX) dirX *= -1f;
        if (invertY) dirY *= -1f;

        foreach (var kvp in startPositions)
        {
            Transform obj = kvp.Key;
            if (obj == null) continue;

            Vector3 startPos = kvp.Value;

            // Offset final en mundo
            Vector3 offset = Vector3.zero;
            offset.x = normalized.x * maxWorldOffset.x * paralaxMulty.x * dirX;
            offset.y = normalized.y * maxWorldOffset.y * paralaxMulty.y * dirY;

            Vector3 target = startPos + offset;

            if (useLimits)
            {
                target.x = Mathf.Clamp(target.x, min.x, max.x);
                target.y = Mathf.Clamp(target.y, min.y, max.y);
            }

            // Suavizado
            if (smooth <= 0f)
            {
                obj.position = target;
                currentPositions[obj] = target;
            }
            else
            {
                Vector3 current = currentPositions[obj];
                current = Vector3.Lerp(current, target, Time.deltaTime * smooth);
                obj.position = current;
                currentPositions[obj] = current;
            }
        }
    }

    void CaptureMouseStart()
    {
        if (ControllerManager.state == null) return;

        mouseStartPos = ControllerManager.GetActionVector2(mousePositionAction);
        mouseStartCaptured = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!useLimits) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0f);
        Vector3 size = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
