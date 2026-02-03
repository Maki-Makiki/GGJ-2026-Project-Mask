using UnityEngine;

public class CabezaSonido : MonoBehaviour
{
    public AudioClip clipImpacto;
    public AudioClip clipArrastre;
    public AudioSource audioSource;

    [Header("Configuración")]
    public float volumenMinimo = 0.1f;
    public float cooldownChoque = 0.15f; // Evita el ruido de metralleta
    public float velocidadMinimaArrastre = 0.5f;

    private float timerChoque;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Importante: El AudioSource no debe tener "Play On Awake"
    }

    void Update()
    {
        if (timerChoque > 0) timerChoque -= Time.deltaTime;

        // Lógica de ARRASTRE
        // Si se mueve por el suelo a cierta velocidad y no está saltando
        if (rb.linearVelocity.magnitude > velocidadMinimaArrastre && Mathf.Abs(rb.linearVelocity.y) < 0.1f)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = clipArrastre;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else if (audioSource.clip == clipArrastre)
        {
            audioSource.Stop();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // LOG DE DEPURACIÓN: Si esto no sale en consola, el problema es la física/collider
        Debug.Log("Choque detectado con: " + collision.gameObject.name);

        // Solo suena si pasó el cooldown y el golpe fue fuerte
        if (timerChoque <= 0 && collision.relativeVelocity.magnitude > 2f)
        {
            // Variar el pitch hace que no suene siempre igual (más orgánico)
            float pitchRandom = Random.Range(0.8f, 1.2f);

            // PlayOneShot permite que los sonidos se solapen sin cortarse
            audioSource.pitch = pitchRandom;
            AudioSource.PlayClipAtPoint(clipImpacto, transform.position, 1f);

            timerChoque = cooldownChoque;
        }
    }
}
