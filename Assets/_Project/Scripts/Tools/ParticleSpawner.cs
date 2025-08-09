using System.Collections;
using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    public static ParticleSpawner Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }
    
    public void PlayParticleEffect(GameObject particlePrefab, Vector3 position)
    {
        GameObject particle = PoolManager.Instance.Spawn(particlePrefab, position, Quaternion.identity);

        particle.GetComponent<ParticleSystem>().Play();
        StartCoroutine(DestroyParticleEffect(particle));
    }

    private IEnumerator DestroyParticleEffect(GameObject particle)
    {
        yield return new WaitForSeconds(particle.GetComponent<ParticleSystem>().main.duration);
        particle.GetComponent<ParticleSystem>().Stop();
        PoolManager.Instance.Despawn(particle);
    }
}
