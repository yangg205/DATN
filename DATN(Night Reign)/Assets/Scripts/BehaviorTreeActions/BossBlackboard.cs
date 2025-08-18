// ===================== BossBlackboard.cs =====================
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

    public bool hasTarget = false;

    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.T))
        {
            TakeDamage(500);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        //animator.SetTrigger("takeHit");
    }

}