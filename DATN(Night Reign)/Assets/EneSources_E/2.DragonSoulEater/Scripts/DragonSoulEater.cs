using ND;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DragonSoulEater : MonoBehaviour
{
    public float HP = 100f;
    public float maxHP = 100f;
    public Animator animator;

    public int expReward = 25;
    public PlayerStats playerStats;


    public Transform attackPoint;
    public float attackRange = 1.5f;
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
    public QuestManager questManager;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        questManager = FindObjectOfType<QuestManager>();
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }
    }

    void Update()
    {
        if (healthFill != null)
        {
            healthFill.fillAmount = HP / maxHP;
        }
        //===test 
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(20);
        }if (Input.GetKeyDown(KeyCode.I))
            {
                TakeIceDamageSoulEater(1);
             }


        if (isDead) return;

        if (player == null) return;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        AIPath aiPath = GetComponent<AIPath>();

        // Hiện damage popup
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        if (HP <= 0)
        {
            isDead = true;
            questManager.ReportKill();
            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }

            AudioManager_Enemy.instance.Play("DragonDeath");
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

            if (playerStats != null)
            {
                playerStats.GainEXP(expReward);
            }
            Destroy(gameObject, 7f);

        }
        else
        {
            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }
            StartCoroutine(DamageStunCoroutine());
            AudioManager_Enemy.instance.Play("DragonHurt");
            animator.SetTrigger("damage");
        }
    }
    public void TakeIceDamageSoulEater(int damageAmount)       //dành cho char có skill đóng băng
    { 
        if (isDead) return;
        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        AIPath aiPath = GetComponent<AIPath>();

        // Hiện damage popup
        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>().Setup(damageAmount);
        }

        // Nếu chết
        if (HP <= 0)
        {
            isDead = true;

            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }

            AudioManager_Enemy.instance.Play("DragonDeath");
            animator.SetTrigger("die");
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;

            Destroy(gameObject, 7f);

        }
        else
        {
            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }
            Freeze(); // gọi coroutine đóng băng
        }
    }

    public void DealDamage()
    {
        float damage = Random.Range(minAttackDamage, maxAttackDamage);

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider player in hitPlayers)
        {
            playerStats.TakeDamage((int)damage);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
    IEnumerator DamageStunCoroutine()
    {
        isTakingDamage = true;
        yield return new WaitForSeconds(2f);
        isTakingDamage = false;

        AIPath aiPath = GetComponent<AIPath>();
        if (aiPath != null && !isDead)
        {
            aiPath.canMove = true;
            aiPath.canSearch = true;
        }
    }

    public void Freeze() 
    {
        StartCoroutine(FreezeCoroutine());
    }
    IEnumerator FreezeCoroutine()
    {
        isTakingDamage = true;

        AIPath aiPath = GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.canMove = false;
            aiPath.canSearch = false;
        }
        if (animator != null)
            animator.speed = 0f;
        // Đổi sang vật liệu băng
        if (bodyRenderer != null && iceMaterial != null)
            bodyRenderer.material = iceMaterial;

        // Play sound đóng băng
        AudioManager_Enemy.instance.Play("Freeze");

        yield return new WaitForSeconds(freezeDuration);

        if (animator != null)
            animator.speed = 1f;
        // Hết đóng băng – khôi phục
        if (bodyRenderer != null && originalMaterial != null)
            bodyRenderer.material = originalMaterial;

        if (aiPath != null && !isDead)
        {
            aiPath.canMove = true;
            aiPath.canSearch = true;
        }

        isTakingDamage = false;
    }



}