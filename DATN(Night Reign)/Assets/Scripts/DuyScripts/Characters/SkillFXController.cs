using UnityEngine;

public class SkillFXController : MonoBehaviour
{
    [Header("Charge Skill (Q)")]
    public ParticleSystem chargeVFX;

    [Header("Attack Speed Buff (R)")]
    public ParticleSystem buffVFX;

    private ParticleSystem currentFX;

    public void PlayChargeVFX()
    {
        StopCurrentVFX();
        if (chargeVFX != null)
        {
            chargeVFX.Play();
            currentFX = chargeVFX;
        }
    }

    public void PlayBuffVFX()
    {
        StopCurrentVFX();
        if (buffVFX != null)
        {
            buffVFX.Play();
            currentFX = buffVFX;
        }
    }

    public void StopCurrentVFX()
    {
        if (currentFX != null && currentFX.isPlaying)
        {
            currentFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            currentFX = null;
        }
    }
}
