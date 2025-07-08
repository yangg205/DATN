using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleResetLoop : MonoBehaviour
{
    public float resetInterval = 3f; // thời gian giữa các lần reset

    private ParticleSystem ps;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        StartCoroutine(LoopParticle());
    }

    IEnumerator LoopParticle()
    {
        while (true)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
            yield return new WaitForSeconds(resetInterval);
        }
    }
}
