using ND;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonTerrorBringer : MonoBehaviour
{
    [Header("Stats")]
    public float maxHP = 150f;
    private float currentHP;
    private bool isDead = false;
    private bool isDefending = false;

    [Header("Combat")]
    private Transform player;
    public Transform detectRange;
    public Transform attackRange;
    public float attackCooldownMin = 3f;
    public float attackCooldownMax = 5f;
    private float nextAttackTime = 0f;
    private bool isAttacking = false;

    public GameObject defendBox;

    private Animator animator;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log("Found Player with tag 'Player'.");
            }
            else
            {
                Debug.LogWarning("Player object with 'Player' tag not found. Please assign targetPlayer manually or ensure Player has the correct tag.");
            }
        }

        if (defendBox != null)
        {
            defendBox.SetActive(false);
        }

        animator = GetComponent<Animator>();


        currentHP = maxHP;
        animator.SetTrigger("Scream"); // trigger để Animator chuyển từ Any → Scream
        Invoke(nameof(TriggerIdle), 2f); // sau khi scream thì chuyển sang Idle
    }

    void TriggerIdle()
    {
        if (!isDead) animator.SetTrigger("Idle");
    }

    void Update()
    {
        if (isDead || isDefending) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float detectDist = Vector3.Distance(transform.position, detectRange.position);
        float attackDist = Vector3.Distance(transform.position, attackRange.position);

        if (distanceToPlayer <= detectDist && Time.time >= nextAttackTime)
        {
            if (distanceToPlayer <= attackDist)
            {
                StartCoroutine(Attack());
                nextAttackTime = Time.time + Random.Range(attackCooldownMin, attackCooldownMax);
            }
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(10);
        }
    }

    IEnumerator Attack()
    {
        isAttacking = true;

        int randomAttack = Random.Range(0, 2);
        if (randomAttack == 0)
            animator.SetTrigger("Attack1");
        else
            animator.SetTrigger("Attack2");

        yield return new WaitForSeconds(1.5f); // chờ hết animation
        isAttacking = false;

        if (!isDead && !isDefending)
            animator.SetTrigger("Idle");
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHP -= damage;

        if (currentHP <= 0)
        {
            Die();

            

        }
        else
        {
            animator.SetTrigger("GetHit");

            if (currentHP <= 70 && !isDefending)
            {
                StartCoroutine(Defend());
            }
        }
    }

    IEnumerator Defend()
    {
        isDefending = true;
        animator.SetTrigger("Defend");
        yield return new WaitForSeconds(2f); // thời gian animation defend
        isDefending = false;

        if (!isDead)
            animator.SetTrigger("Idle");
    }
    public void ActivateDefendBox()
    {
        if (defendBox != null)
        {
            defendBox.SetActive(true);
            Debug.Log("Animation Event: Melee Attack Hitbox Activated!");
        }
    }

    public void DeactivateDefendBox()
    {
        if (defendBox != null)
        {
            defendBox.SetActive(false);
            Debug.Log("Animation Event: Melee Attack Hitbox Deactivated!");
        }
    }
    void Die()
    {
        isDead = true;
        animator.SetTrigger("Dead");

        Destroy(gameObject, 5f);
    }
    void OnDrawGizmos()
    {
        if (detectRange != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Vector3.Distance(transform.position, detectRange.position));
        }

        if (attackRange != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, Vector3.Distance(transform.position, attackRange.position));
        }
    }

}
