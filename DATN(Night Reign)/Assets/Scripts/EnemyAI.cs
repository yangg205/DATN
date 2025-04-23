using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public GameObject[] targets;
    public Animator animator;

    public int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    private bool isAttacking = false;

    public NetworkRunner networkRunner;

    void Start()
    {
        currentHealth = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        targets = GameObject.FindGameObjectsWithTag("Player");
        if (targets.Length == 0) return;

        GameObject target = null;
        float minDistance = Mathf.Infinity;
        foreach (var t in targets)
        {
            var distance = Vector3.Distance(t.transform.position, transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                target = t;
            }
        }

        if (target != null)
        {
            agent.SetDestination(target.transform.position);

            animator.SetBool("isChasing", true);
            animator.SetBool("isIdle", false);

            if (minDistance < 2f && !isAttacking)
            {
                isAttacking = true;
                animator.SetBool("isAttacking", true);
            }
            else if (minDistance >= 2f && isAttacking)
            {
                isAttacking = false;
                animator.SetBool("isAttacking", false);
            }
        }
        else
        {
            animator.SetBool("isChasing", false);
            animator.SetBool("isIdle", true);
        }
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetTrigger("Die");
        agent.isStopped = true;

        // Gọi hàm cộng XP cho người chơi khi quái chết
        var player = GameObject.FindGameObjectWithTag("Player"); // Giả sử bạn có cách tìm người chơi
        if (player != null)
        {
            var playerProperties = player.GetComponent<DuyProperties>();
            if (playerProperties != null)
            {
                playerProperties.GainXP(50); // Cộng XP cho người chơi
            }
        }

        // Huỷ đối tượng quái sau khi chết
        Destroy(gameObject, 3f);
    }

    // Sửa lại hàm này để quái nhận damage khi nhân vật đang tấn công
    public void OnAttackEnter(bool attacking)
    {
        isAttacking = attacking;
    }
}
