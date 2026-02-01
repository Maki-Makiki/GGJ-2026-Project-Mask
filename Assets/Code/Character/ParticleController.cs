using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] List<ParticleSystem> listParticlesToControl;

    public void StartParticleByIndex(int index)
    {
        if (listParticlesToControl[index].isPlaying)
            StopParticleByIndex(index);
        listParticlesToControl[index].Play();
    }

    public void StopParticleByIndex(int index)
    {
        listParticlesToControl[index].Stop();
    }

    public void PauseParticleByIndex(int index)
    {
        listParticlesToControl[index].Pause();
    }
}
