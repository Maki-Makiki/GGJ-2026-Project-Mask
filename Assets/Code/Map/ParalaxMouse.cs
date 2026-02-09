using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ParalaxMouse : MonoBehaviour
{
    [Header("Input Actions")]
    public string mousePositionAction = "MousePosition";
    [Space]
    public bool useGyroAngular = false;
    public string gyroAngular = "GyroAngular";
    [Space]
    public bool useAcelerometer = true;
    public string acelerometer = "Acelerometer";
    [Space]
    public bool useSensorAngularVelocity = false;
    public string sensorAngularVelocity = "sensorAngularVelocity";

    [Header("Parallax Multiplier")]
    public Vector2 parallaxMultyMouse = new Vector2(0.5f, 0.5f);
    public Vector2 parallaxMultyGyro = new Vector2(0.3f, 0.3f);
    public Vector2 parallaxElementValue = new Vector2(0.3f, 0.3f);

    [Header("Direction")]
    public bool invertX = false;
    public bool invertY = false;
    public bool sameDirection = true;

    [Header("Movement")]
    public Vector2 maxWorldOffset = new Vector2(1f, 1f);
    public float smooth = 10f;

    [Header("Mouse Behavior")]
    public bool useMouseStartAsCenter = true;

    [Header("Gyro Settings")]
    [Tooltip("Grados máximos de inclinación para llegar a 1.0")]
    public float gyroMaxAngle = 30f;

    [Header("Limits")]
    public bool useLimits = true;
    public Vector2 min = new Vector2(-999, -999);
    public Vector2 max = new Vector2(999, 999);

    [Header("Objects")]
    public List<Transform> objetos = new();

    // Internals
    private Dictionary<Transform, Vector3> startPositions = new();
    private Dictionary<Transform, Vector3> currentPositions = new();

    private Vector2 mouseStartPos;
    private bool mouseStartCaptured = false;

    [Header("Gyro Integration")]
    public float gyroSensitivity = 1.0f;
    public float gyroDamping = 5f;

    private Vector2 gyroIntegrated;

    public bool debug = false;
    public string gyroEnabled = "Enabling Gyroscope";
    public string accelerometerEnabled = "Enabling Accelerometer";
    public string attitudeSensorEnabled = "Enabling AttitudeSensor";
    public TMPro.TMP_Text textoDebug;

    void Awake()
    {
        if (useGyroAngular)
        {
            if (UnityEngine.InputSystem.Gyroscope.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Gyroscope.current);
                Debug.Log("Gyroscope enabled");
                gyroEnabled = "Gyroscope enabled";
            }
            else
            {
                Debug.LogWarning("Gyroscope NOT available on this device");
                gyroEnabled = ("Gyroscope NOT available on this device");
            }
        }

        if(useAcelerometer)
        {
            if (UnityEngine.InputSystem.Accelerometer.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
                Debug.Log("Accelerometer enabled");
                accelerometerEnabled = "Accelerometer enabled";
            }
            else
            {
                Debug.LogWarning("Accelerometer NOT available on this device");
                accelerometerEnabled = ("Accelerometer NOT available on this device");
            }
        }

        if (useSensorAngularVelocity)
        {
            if (UnityEngine.InputSystem.AttitudeSensor.current != null)
            {
                InputSystem.EnableDevice(UnityEngine.InputSystem.Accelerometer.current);
                Debug.Log("AttitudeSensor enabled");
                attitudeSensorEnabled = "AttitudeSensor enabled";
            }
            else
            {
                Debug.LogWarning("AttitudeSensor NOT available on this device");
                attitudeSensorEnabled = ("AttitudeSensor NOT available on this device");
            }
        }


    }

    void Start()
    {
        startPositions.Clear();
        currentPositions.Clear();

        foreach (var t in objetos)
        {
            if (!t) continue;
            startPositions[t] = t.position;
            currentPositions[t] = t.position;
        }

        CaptureMouseStart();
    }

    void LateUpdate()
    {
        if (ControllerManager.state == null)
            return;

        Vector2 normalizedInput;
        Vector2 parallaxMulty;

        if (ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch)
        {
            normalizedInput = GetAccelNormalized();
            parallaxMulty = parallaxMultyGyro;
        }
        else
        {
            normalizedInput = GetMouseNormalized();
            parallaxMulty = parallaxMultyMouse;
        }

        float dirX = sameDirection ? 1f : -1f;
        float dirY = sameDirection ? 1f : -1f;
        if (invertX) dirX *= -1f;
        if (invertY) dirY *= -1f;

        foreach (var kvp in startPositions)
        {
            Transform obj = kvp.Key;
            if (!obj) continue;

            Vector3 startPos = kvp.Value;

            Vector3 offset = new Vector3(
                normalizedInput.x * maxWorldOffset.x * (parallaxMulty.x * parallaxElementValue.x) * dirX,
                normalizedInput.y * maxWorldOffset.y * (parallaxMulty.y * parallaxElementValue.y) * dirY,
                0f
            );

            Vector3 target = startPos + offset;

            if (useLimits)
            {
                target.x = Mathf.Clamp(target.x, min.x, max.x);
                target.y = Mathf.Clamp(target.y, min.y, max.y);
            }

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

        debugDraw();
    }

    public void debugDraw()
    {
        if (debug) 
        {
            textoDebug.text = $"Debug Paralax Mouse:\n";
            textoDebug.text += $"\n"; 
            textoDebug.text += $"> ImputDivece = ({ControllerManager.state.ImputDivece.ToString()}) \n";
            textoDebug.text += $"> Mode = ({((ControllerManager.state.ImputDivece == ControllerManager.m_ImputDivice.Touch)?"Gyro":"Mouse")}) \n";
            textoDebug.text += $"\n"; 

            if (useGyroAngular)
            {
                textoDebug.text += $"> gyroEnabled = ({gyroEnabled}) \n";
                Vector3 raw_gyroAngular = ControllerManager.GetActionVector3(gyroAngular);
                textoDebug.text += $"> raw GyroAngular = ({raw_gyroAngular}) \n";
                textoDebug.text += $"\n";
            }

            if (useAcelerometer)
            {
                textoDebug.text += $"> accelerometerEnabled = ({accelerometerEnabled}) \n";
                Vector3 raw_acelerometer = ControllerManager.GetActionVector3(acelerometer);
                textoDebug.text += $"> raw Acelerometer = ({raw_acelerometer}) \n";
                textoDebug.text += $"\n";
            }

            if (useSensorAngularVelocity)
            {
                textoDebug.text += $"> attitudeSensorEnabled = ({attitudeSensorEnabled}) \n";
                Vector3 raw_sensorAngularVelocity = ControllerManager.GetActionVector3(sensorAngularVelocity);
                textoDebug.text += $"> raw Sensor Angular Velocity = ({raw_sensorAngularVelocity}) \n";
            }
        }
    }

    // =========================
    // MOUSE
    // =========================

    Vector2 GetMouseNormalized()
    {
        Vector2 mousePos = ControllerManager.GetActionVector2(mousePositionAction);

        if (!mouseStartCaptured)
            CaptureMouseStart();

        Vector2 center = useMouseStartAsCenter
            ? mouseStartPos
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Vector2 delta = mousePos - center;
        Vector2 half = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        Vector2 normalized = new(
            half.x > 0 ? delta.x / half.x : 0f,
            half.y > 0 ? delta.y / half.y : 0f
        );

        return Vector2.ClampMagnitude(normalized, 1f);
    }

    void CaptureMouseStart()
    {
        mouseStartPos = ControllerManager.GetActionVector2(mousePositionAction);
        mouseStartCaptured = true;
    }

    // =========================
    // Accelerometer
    // =========================

    Vector2 GetAccelNormalized()
    {
        Vector3 accel = ControllerManager.GetActionVector3(acelerometer);

        // Descartamos Z (gravedad)
        Vector2 tilt = new Vector2(accel.x, accel.y);

        // Normalizamos aprox (-1 .. 1)
        tilt = Vector2.ClampMagnitude(tilt, 1f);

        return tilt;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!useLimits) return;

        Gizmos.color = Color.cyan;
        Vector3 center = new(
            (min.x + max.x) * 0.5f,
            (min.y + max.y) * 0.5f,
            0f
        );
        Vector3 size = new(
            Mathf.Abs(max.x - min.x),
            Mathf.Abs(max.y - min.y),
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
    }
#endif
}
