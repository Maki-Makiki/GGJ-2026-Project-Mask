using System;
using UnityEngine;

public class CheckpointActivated : MonoBehaviour
{
    public ParticleSystem particulasFlotantes; // El loop continuo
    public ParticleSystem particulaExplosion;  // El One Shot

    private bool activado = false;
    void ActivarCheckpoint()
    {
        activado = true;

        // 1. Dejar de emitir nuevas partículas, pero permite que las que 
        // están en pantalla terminen su ciclo de vida (disipación).
        var emission = particulasFlotantes.emission;
        emission.enabled = false;
        // Opcional: particulasFlotantes.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        // 2. Disparar la explosión
        particulaExplosion.Play();

        // 3. (Opcional) Feedback visual extra
        // GetComponent<SpriteRenderer>().color = Color.cyan;
    }

    void ResetParticles()
    {
        activado = true;

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
}
