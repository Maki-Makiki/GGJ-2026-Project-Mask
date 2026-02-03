using UnityEngine;

public class DialogueWiggleUniversal : MonoBehaviour
{
    public enum SpaceMode
    {
        LocalPosition,
        AnchoredPosition // para UI RectTransform
    }

    [Header("Mode")]
    public SpaceMode mode = SpaceMode.LocalPosition;

    [Header("Movement limits")]
    public Vector2 maxOffset = new Vector2(5f, 3f);
    // TIP: en UI esto está en pixeles. En mundo suele ser algo como 0.05 / 0.03

    [Header("Speed")]
    public float speed = 1.5f;

    [Header("Smoothing")]
    public float smooth = 8f;

    [Header("Optional")]
    public bool useUnscaledTime = true;

    private Vector3 baseLocalPos;
    private Vector2 baseAnchoredPos;

    private Vector2 currentOffset;

    private float seedX;
    private float seedY;

    private RectTransform rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);

        CacheBasePosition();
    }

    void OnEnable()
    {
        CacheBasePosition();
    }

    void CacheBasePosition()
    {
        if (mode == SpaceMode.AnchoredPosition && rect != null)
        {
            baseAnchoredPos = rect.anchoredPosition;
        }
        else
        {
            baseLocalPos = transform.localPosition;
        }
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        float nx = (Mathf.PerlinNoise(seedX, t * speed) * 2f) - 1f;
        float ny = (Mathf.PerlinNoise(seedY, t * speed) * 2f) - 1f;

        Vector2 targetOffset = new Vector2(
            nx * maxOffset.x,
            ny * maxOffset.y
        );

        currentOffset = Vector2.Lerp(currentOffset, targetOffset, smooth * Time.deltaTime);

        ApplyOffset(currentOffset);
    }

    void ApplyOffset(Vector2 offset)
    {
        if (mode == SpaceMode.AnchoredPosition && rect != null)
        {
            rect.anchoredPosition = baseAnchoredPos + offset;
        }
        else
        {
            transform.localPosition = baseLocalPos + (Vector3)offset;
        }
    }

    public void ResetPosition()
    {
        currentOffset = Vector2.zero;
        ApplyOffset(Vector2.zero);
    }
}
