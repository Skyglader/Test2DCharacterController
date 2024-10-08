using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerParticleStorage : MonoBehaviour
{
    public PlayerManager player;
    [SerializeField] public List<ParticleSystem> ParticleSystems = new List<ParticleSystem>();
    [SerializeField] public List<Transform> transforms = new List<Transform>();

    private void Awake()
    {
        player = GetComponentInParent<PlayerManager>();
    }
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
        if (!player.playerCombatManager.inPoweredState)
            ParticleSystems[2].Play();
        else
            ParticleSystems[4].Play();
    }

    public void PlayBlockParticle()
    {
        if (!player.playerCombatManager.inPoweredState)
            ParticleSystems[3].Play();
        else
            ParticleSystems[5].Play();
    }
}
