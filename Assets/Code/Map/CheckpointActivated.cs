using System;
using UnityEngine;

public class CheckpointActivated : MonoBehaviour
{
    public ParticleSystem particulasFlotantes; // El loop continuo
    public ParticleSystem particulaExplosion;  // El One Shot
    public AudioSource audioSource;  // El One Shot
    public AudioSource audioSourceAmbient;  // El One Shot
    public AudioClip audioClip;  // El One Shot

    private bool activado;
    void ActivarCheckpoint()
    {
        // 1. Dejar de emitir nuevas partículas, pero permite que las que 
        // están en pantalla terminen su ciclo de vida (disipación).
        var emission = particulasFlotantes.emission;
        emission.enabled = false;
        // Opcional: particulasFlotantes.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 2. Disparar la explosión
        PlaySound();
        audioSourceAmbient.Stop();
        particulaExplosion.Play();

        // 3. (Opcional) Feedback visual extra
        // GetComponent<SpriteRenderer>().color = Color.cyan;
    }

    void ResetParticles()
    {

        // 1. Dejar de emitir nuevas partículas, pero permite que las que 
        // están en pantalla terminen su ciclo de vida (disipación).
        var emission = particulasFlotantes.emission;
        emission.enabled = true;
        // Opcional: particulasFlotantes.Stop(true, ParticleSystemStopBehavior.StopEmitting);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("PlayerBody"))
        {
            ActivarCheckpoint(); // Las partículas que ya tienes en este script
        }
    }

    public void PlaySound()
    {
        audioSource.PlayOneShot(audioClip);
    }
}
