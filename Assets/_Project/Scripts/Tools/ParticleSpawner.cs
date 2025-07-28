using UnityEngine;

public static class ParticleSpawner
{

    public static void PlayParticleEffect(ParticleSystem particlePrefab, Vector3 position)
    {
        ParticleSystem particle = Object.Instantiate(particlePrefab, position, Quaternion.identity);
        particle.Play();
        Object.Destroy(particle.gameObject, particle.main.duration);
    }
}
