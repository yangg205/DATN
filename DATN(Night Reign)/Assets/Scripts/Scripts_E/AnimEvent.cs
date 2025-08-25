using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    [Header("VFX")]
    public ParticleSystem vfxSlash;
    public ParticleSystem vfxSlash1;


    //[Header("SFX")]
    public AudioSource sfxSwordSpawn;
    public AudioSource sfxMove;
    public AudioSource sfxMove1;
    public AudioSource sfxSwordJump;
    public AudioSource sfxSword1;



    //============================VFX
    public void PlaySlash()
    {
        vfxSlash.Play();
    }

    public void PlaySlash1()
    {
        vfxSlash1.Play();
    }


    //============================SFX
    public void PlaySFXSpawn()
    {
        sfxSwordSpawn.PlayOneShot(sfxSwordSpawn.clip);
    }

    public void PlaySFXSword()
    {
        sfxSword1.PlayOneShot(sfxSword1.clip);
    }
    public void PlaySFXMove()
    {
        sfxMove.PlayOneShot(sfxMove.clip);
    }

    public void PlaySFXMove1()
    {
        sfxMove1.PlayOneShot(sfxMove1.clip);
    }

    public void PlaySFXSwordJump()
    {
        sfxSwordJump.PlayOneShot(sfxSwordJump.clip);
    }
}
