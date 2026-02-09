using UnityEngine;

public class CabezaSonido : MonoBehaviour
{
    public AudioClip clipImpacto;
    public AudioClip clipArrastre;
    public AudioSource audioSource;

    [Header("Configuración")]
    public float volumenMinimo = 0.1f;
    public float hitMinVolume = 0.1f;
    public float hitMaxVolume = 1f;
    public float cooldownChoque = 0.15f; // Evita el ruido de metralleta
    public float velocidadMinimaArrastre = 0.5f;

    [Header("Kick/Empuje")]
    public float KickSoundMinForce = 1f;
    public float KickSoundMaxForce = 5f;
    public float KickForce = 2f;
    public float maxVelocityToKick = 2f;

    [SerializeField] private float timerChoque;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float speed;
    [SerializeField] private Vector3 lastPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Importante: El AudioSource no debe tener "Play On Awake"
    }

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (timerChoque > 0) timerChoque -= Time.deltaTime;

        float moveDistance = Vector3.Distance(transform.position, lastPosition);

        // Sonido de arrastre
        if (moveDistance > 0.01f)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clipArrastre;
                audioSource.loop = true;
                audioSource.Play();
            }

            audioSource.volume = Mathf.Clamp(moveDistance * 10f, volumenMinimo, 1f);
        }
        else if (audioSource.clip == clipArrastre)
        {
            audioSource.Stop();
        }

        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Choque detectado con: " + collision.gameObject.name);

        // Empujar la cabeza si el jugador la toca
        if (collision.collider.gameObject.name == "BodyCollider" && rb.linearVelocity.magnitude < maxVelocityToKick)
        {
            Vector2 pushDir = (rb.transform.position - collision.transform.position).normalized;
            Debug.Log($"playerPatea Vector2({pushDir.x},{pushDir.y}) x {KickForce}");
            rb.AddForce(pushDir * KickForce, ForceMode2D.Impulse);
        }

        // Volumen dinámico según velocidad
        speed = rb.linearVelocity.magnitude;

        if (speed >= KickSoundMinForce && timerChoque <= 0)
        {
            // Interpolación lineal entre min y max volumen según velocidad
            float t = Mathf.Clamp01((speed - KickSoundMinForce) / (KickSoundMaxForce - KickSoundMinForce));
            float volume = Mathf.Lerp(hitMinVolume, hitMaxVolume, t);

            float pitchRandom = Random.Range(0.8f, 1.2f);
            audioSource.pitch = pitchRandom;
            AudioSource.PlayClipAtPoint(clipImpacto, transform.position, volume);

            timerChoque = cooldownChoque;
        }
    }
}
