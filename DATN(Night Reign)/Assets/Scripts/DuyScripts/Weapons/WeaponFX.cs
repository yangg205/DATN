using System.Collections;
using UnityEngine;

public class WeaponFX : MonoBehaviour
{
    [Header("Weapon FX")]
    public ParticleSystem normalWeaponTrail;

    private bool fxIsPlaying = false;

    public void PlayWeaponFX()
    {
        if (normalWeaponTrail == null)
        {
            Debug.LogWarning("[WeaponFX] → normalWeaponTrail is NULL!");
            return;
        }

        if (fxIsPlaying) return;

        normalWeaponTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        normalWeaponTrail.Play();

        fxIsPlaying = true;
        Debug.Log("[WeaponFX] → PlayWeaponFX");

        StartCoroutine(ResetFXFlag());
    }

    public void StopWeaponFX()
    {
        if (normalWeaponTrail == null)
        {
            Debug.LogWarning("[WeaponFX] → normalWeaponTrail is NULL!");
            return;
        }

        if (normalWeaponTrail.isPlaying)
        {
            normalWeaponTrail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            Debug.Log("[WeaponFX] → StopWeaponFX");
        }

        fxIsPlaying = false;
    }

    private IEnumerator ResetFXFlag()
    {
        yield return new WaitForSeconds(0.3f); // chống spam trail nếu combo nhanh
        fxIsPlaying = false;
    }

}
