using ND;
using Pathfinding;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DragonTerrorBringer : MonoBehaviour
{
    public float HP = 200f;
    public float maxHP = 200f;
    public Animator animator;

    public Transform attackPoint;
    public float attackRange = 1.5f;
    public float detectionRange = 10f;
    public LayerMask playerLayer;

    private Transform player;

    [Header("UI")]
    public Image healthFill;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;

    [Header("Freeze Effect")]
    public Material iceMaterial;
    public Material originalMaterial;
    public float freezeDuration = 5f;
    public SkinnedMeshRenderer bodyRenderer;

    public bool isDead = false;
    public bool isTakingDamage = false;

    public float minAttackDamage = 5f;
    public float maxAttackDamage = 15f;

    private AIPath aiPath;
    private bool isDefending = false;

    private float attackCooldown = 3f;
    private float lastAttackTime = -999f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        aiPath = GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(20);
        }

        if (healthFill != null)
            healthFill.fillAmount = HP / maxHP;

        if (isDead || isTakingDamage) return;

        Collider[] detectedPlayers = Physics.OverlapSphere(transform.position, detectionRange, playerLayer);
        if (detectedPlayers.Length > 0)
        {
            Transform targetPlayer = detectedPlayers[0].transform;
            transform.LookAt(targetPlayer);

            if (HP <= maxHP * 0.3f)
            {
                if (!isDefending)
                    StartCoroutine(DefendRoutine());
            }
            else
            {
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    lastAttackTime = Time.time;
                    animator.SetTrigger("attack");
                }
            }
        }
        else
        {
            animator.SetBool("isIdle", true);
        }
    }

    public void DealDamage()
    {
        float damage = Random.Range(minAttackDamage, maxAttackDamage);
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider player in hitPlayers)
        {
            //player.GetComponent<PlayerClone>()?.TakeDamage(damage);
/*            player.GetComponent<PlayerStats>()?.TakeDamage(15);
*/        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2.5f + Vector3.forward * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageStunCoroutine());
            AudioManager_Enemy.instance?.Play("DragonHurt");
            animator.SetTrigger("damage");
        }
    }

    public void TakeIceDamageSoulEater(int damageAmount)
    {
        if (isDead) return;

        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (HP <= 0)
        {
            Die();
        }
        else
        {
            Freeze();
        }
    }

    private void Die()
    {
        isDead = true;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }

        AudioManager_Enemy.instance?.Play("DragonDeath");
        animator.SetTrigger("die");

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;

        // 💥 Nếu enemy này đang bị lock-on thì thoát lock-on
        if (ND.CameraHandler.singleton != null &&
            ND.CameraHandler.singleton.currentLockOnTarget == this)
        {
            ND.InputHandler inputHandler = FindObjectOfType<ND.InputHandler>();

            // Tắt lock-on mode
            if (inputHandler != null)
            {
                inputHandler.lockOnFlag = false;
            }

            // Reset camera
            ND.CameraHandler.singleton.ClearLockOnTargets();
        }

        Destroy(gameObject, 7f);
    }

    IEnumerator DamageStunCoroutine()
    {
        isTakingDamage = true;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }

        yield return new WaitForSeconds(2f);

        if (!isDead)
        {
            isTakingDamage = false;
            if (aiPath != null)
            {
                aiPath.canMove = true;
                aiPath.canSearch = true;
            }
        }
    }

    IEnumerator DefendRoutine()
    {
        isDefending = true;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }

        animator.SetTrigger("defend");
        AudioManager_Enemy.instance?.Play("DragonDefend");

        yield return new WaitForSeconds(3f);

        isDefending = false;
    }

    public void Freeze()
    {
        StartCoroutine(FreezeCoroutine());
    }

    IEnumerator FreezeCoroutine()
    {
        isTakingDamage = true;

        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }

        if (animator != null)
            animator.speed = 0f;

        if (bodyRenderer != null && iceMaterial != null)
            bodyRenderer.material = iceMaterial;

        AudioManager_Enemy.instance?.Play("Freeze");

        yield return new WaitForSeconds(freezeDuration);

        if (animator != null)
            animator.speed = 1f;

        if (bodyRenderer != null && originalMaterial != null)
            bodyRenderer.material = originalMaterial;

        if (aiPath != null && !isDead)
        {
            aiPath.canMove = true;
            aiPath.canSearch = true;
        }

        isTakingDamage = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
