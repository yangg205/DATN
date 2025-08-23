// ===================== BossBlackboard.cs =====================
using UnityEditor.Experimental.GraphView;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class BossBlackboard : MonoBehaviour
{
    public Transform player;
    public float currentHP;
    public float maxHP = 1000f;
    public bool isInSecondPhase;
    public bool isInThirdPhase;
    public bool canSeePlayer;
    public Vector3 lastKnownPlayerPosition;
    public float attackCooldown;
    public float timeSinceLastAttack;
    public Animator animator;
    [Header("VFX")]
    public ParticleSystem vfxSlash;
    public ParticleSystem vfxSlash1;

    public bool hasTarget = false;

    public int damageAmount;

    void Start()
    {
        animator.SetTrigger("");

        currentHP = maxHP;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.T))
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        currentHP -= damageAmount;
        animator.SetTrigger("takeHit");
    }

    public void PlaySlash()
    {
        vfxSlash.Play();
    }

    public void PlaySlash1()
    {
        vfxSlash1.Play();
    }
}