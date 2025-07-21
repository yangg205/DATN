using server.model;
using System.Collections;
using UnityEngine;

public class desertBoss : MonoBehaviour
{
    private Animator animator;
    private Transform targetPlayer;

    public float detectRange = 15f;
    public float meleeRange = 5f;
    public Transform rangeOrigin;


    public int maxHP = 1000;
    public int currentHP;

    private bool isAttacking = false;
    private bool isDead = false;

    private bool isPhase2 => currentHP <= 500;

    private bool hasShoutedPhase2 = false;
    private bool isShouting = false;
    private float attackWindupTime = 0.3f;

    private void Start()
    {
        if (targetPlayer == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                targetPlayer = playerObj.transform;
                Debug.Log("Found Player with tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("Player object with 'Player' tag not found. Please assign targetPlayer manually or ensure Player has the correct tag.");
            }
        }

        currentHP = maxHP;

        animator = GetComponentInChildren<Animator>();

        animator.SetTrigger("isEmerging");
        StartCoroutine(AIBehavior());
    }

    private void Update()
    {
        if (!isDead && !isAttacking && !isShouting)
        {
            LookAtPlayer(); 
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(100);
        }
    }

    private IEnumerator AIBehavior()
    {
        yield return new WaitForSeconds(3f); // Emergence

        while (!isDead)
        {
            Vector3 origin = rangeOrigin != null ? rangeOrigin.position : transform.position;
            float distance = Vector3.Distance(origin, targetPlayer.position);
            bool inRange = distance <= detectRange;

            // ▶️ Phase 2: Shout
            if (isPhase2 && !hasShoutedPhase2)
            {
                isShouting = true;
                hasShoutedPhase2 = true;
                animator.SetTrigger("doShout");
                yield return new WaitForSeconds(2f); // thời gian anim Shout
                isShouting = false;
            }

            if (inRange && !isAttacking && !isShouting)
            {
                isAttacking = true;
                int attackType = DecideAttackType(distance);
                animator.SetInteger("doAttackType", attackType);

                float elapsed = 0f;
                while (elapsed < attackWindupTime)
                {
                    LookAtPlayer();
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                yield return AttackCooldown();
                animator.SetInteger("doAttackType", 0); // reset
                isAttacking = false;
            }

            yield return null;
        }
    }

    private int DecideAttackType(float distance)
    {
        if (isPhase2 && distance > meleeRange)
        {
            return Random.Range(3, 5);
        }

        // random melee attacks
        return Random.Range(1, 4); // 1 to 3
    }

    private IEnumerator AttackCooldown()
    {
        float cooldown = Random.Range(3f, 5f);
        yield return new WaitForSeconds(cooldown);
    }
    private void LookAtPlayer()
    {
        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0; // không xoay theo trục Y

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }


    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        animator.SetTrigger("takeHit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        // Disable collider, disable attack, etc.
    }


    private void OnDrawGizmosSelected()
    {
        Vector3 origin = rangeOrigin != null ? rangeOrigin.position : transform.position;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, meleeRange);
    }


}
