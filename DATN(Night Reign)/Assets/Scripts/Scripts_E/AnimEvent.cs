using UnityEngine;

public class AnimEvent : MonoBehaviour
{
    [Header("VFX")]
    public ParticleSystem vfxSlash;
    public ParticleSystem vfxSlash1;
    public ParticleSystem vfxImpact;


    [Header("SFX")]
    public AudioSource sfxSwordSpawn;
    public AudioSource sfxMove;
    public AudioSource sfxMove1;
    public AudioSource sfxSwordJump;
    public AudioSource sfxSword1;
    public AudioSource sfxHit;

    [Header("hitBox")]
    [SerializeField] private GameObject swordHitBox;


    private void Start()
    {
        swordHitBox.SetActive(false);
    }


    public void startHitBox()
    {
        swordHitBox.SetActive(true);
    }
    public void endHitBox()
    {
        swordHitBox.SetActive(false);
    }



    //============================VFX
    public void PlaySlash()
    {
        vfxSlash.Play();
    }

    public void PlaySlash1()
    {
        vfxSlash1.Play();
    }
    public void PlayVFXImpact()
    {
        vfxImpact.Play();
    }


    //============================SFX
    public void PlaySFXSpawn()
    {
        sfxSwordSpawn.PlayOneShot(sfxSwordSpawn.clip);
        swordHitBox.SetActive(true);
    }

    public void PlaySFXSword()
    {
        sfxSword1.PlayOneShot(sfxSword1.clip);
        swordHitBox.SetActive(true);
    }
    public void PlaySFXSwordJump()
    {
        sfxSwordJump.PlayOneShot(sfxSwordJump.clip);
        swordHitBox.SetActive(true);
    }

    public void PlaySFXHit()
    {
        sfxHit.PlayOneShot(sfxHit.clip);
    }

    //move
    public void PlaySFXMove()
    {
        sfxMove.PlayOneShot(sfxMove.clip);
    }

    public void PlaySFXMove1()
    {
        sfxMove1.PlayOneShot(sfxMove1.clip);
    }
    //==
}
