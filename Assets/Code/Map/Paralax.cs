using System.Collections.Generic;
using UnityEngine;

public class Paralax : MonoBehaviour
{
    [Header("Camera Reference")]
    public Camera cameraToReference;

    [Header("Parallax Settings")]
    public Vector2 paralaxMulty = new Vector2(0.5f, 0.5f);

    [Tooltip("Si está activo, invierte el movimiento del parallax (efecto contrario).")]
    public bool invertX = false;
    public bool invertY = false;

    [Tooltip("Si está activo, el parallax se mueve en el mismo sentido que la cámara (normalmente sí).")]
    public bool sameDirection = true;

    [Header("Limits (world position clamp)")]
    public bool useLimits = true;
    public Vector2 min = new Vector2(-999, -999);
    public Vector2 max = new Vector2(999, 999);

    [Header("Objects")]
    public List<Transform> objetos = new List<Transform>();

    // Internals
    private Vector3 cameraStartPos;
    private Dictionary<Transform, Vector3> startPositions = new Dictionary<Transform, Vector3>();

    void Start()
    {
        if (cameraToReference == null)
            cameraToReference = Camera.main;

        if (cameraToReference == null)
        {
            Debug.LogError("[Paralax] No hay cámara asignada y Camera.main es null.");
            enabled = false;
            return;
        }

        cameraStartPos = cameraToReference.transform.position;

        startPositions.Clear();
        for (int i = 0; i < objetos.Count; i++)
        {
            if (objetos[i] == null) continue;
            startPositions[objetos[i]] = objetos[i].position;
        }
    }

    void LateUpdate()
    {
        if (cameraToReference == null) return;

        Vector3 camDelta = cameraToReference.transform.position - cameraStartPos;

        float dirX = sameDirection ? 1f : -1f;
        float dirY = sameDirection ? 1f : -1f;

        if (invertX) dirX *= -1f;
        if (invertY) dirY *= -1f;

        foreach (var kvp in startPositions)
        {
            Transform obj = kvp.Key;
            if (obj == null) continue;

            Vector3 startPos = kvp.Value;

            Vector3 target = startPos;
            target.x += camDelta.x * paralaxMulty.x * dirX;
            target.y += camDelta.y * paralaxMulty.y * dirY;

            if (useLimits)
            {
                target.x = Mathf.Clamp(target.x, min.x, max.x);
                target.y = Mathf.Clamp(target.y, min.y, max.y);
            }

            obj.position = target;
        }
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
