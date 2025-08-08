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
        ParticleSystem particle = PoolManager.Instance.Spawn(particlePrefab, position, Quaternion.identity).GetComponent<ParticleSystem>();
        particle.Play();
        StartCoroutine(DestroyParticleEffect(particle));
    }

    private IEnumerator DestroyParticleEffect(ParticleSystem particlePrefab)
    {
        yield return new WaitForSeconds(particlePrefab.main.duration);
        particlePrefab.Stop();
        PoolManager.Instance.Despawn(particlePrefab.gameObject);
    }
}
