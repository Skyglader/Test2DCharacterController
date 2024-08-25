using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleStorage : MonoBehaviour
{
    [SerializeField] public List<ParticleSystem> ParticleSystems = new List<ParticleSystem>();
    [SerializeField] public List<Transform> transforms = new List<Transform>();
    public void DashStart()
    {
        PlayTargetParticle(0);
    }

    public void DashEnd()
    {
        PlayTargetParticle(1);
    }

    public void PlayTargetParticle(int i)
    {
        ParticleSystem particle = Instantiate(ParticleSystems[i], transforms[i].transform.position, transforms[i].transform.rotation);
        particle.Play();
    }

    public void PlayParryParticle()
    {
        ParticleSystems[2].Play();
    }

    public void PlayBlockParticle()
    {
        ParticleSystems[3].Play();
    }
}
