using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ND;

public class NightMare : MonoBehaviour
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

    [Header("Item Drop")]
    public GameObject itemDropPrefab;

    public Transform dropPoint; 

    [Header("VFX")]
    public ParticleSystem vfxDead;

    [Header("Freeze Effect")]
    public Material iceMaterial;             
    public Material originalMaterial;       

    public float freezeDuration = 5f;         

    public SkinnedMeshRenderer bodyRenderer;           

    public bool isDead = false;
    public bool isTakingDamage = false;

    public int minAttackDamage = 3;
    public int maxAttackDamage = 5;

    void Start()
    {

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            playerStats = player.GetComponent<PlayerStats>();
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TakeDamage(15);
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
       
            DamagePopup popup = DamagePopupPool.Instance.GetFromPool();
            popup.transform.position = transform.position + Vector3.up * 2f;
            popup.Setup(damageAmount);
       

        if (HP <= 0)
        {
            isDead = true;

            if (vfxDead != null)
            {
                vfxDead.Play();
            }

            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }

            AudioManager_Enemy.instance.Play("DragonDeath");
            animator.SetTrigger("die");
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;

            StartCoroutine(DeathCoroutine());

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
    public void TakeIceDamageNightMare(int damageAmount)       //dành cho char có skill đóng băng
    { 
        if (isDead) return;
        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        AIPath aiPath = GetComponent<AIPath>();

        // Hiện damage popup
        
            DamagePopup popup = DamagePopupPool.Instance.GetFromPool();
            popup.transform.position = transform.position + Vector3.up * 2f;
            popup.Setup(damageAmount);
        

        // Nếu chết
        if (HP <= 0)
        {
            isDead = true;

            if (vfxDead != null)
            {
                vfxDead.Play();
            }

            if (aiPath != null)
            {
                aiPath.canMove = false;
                aiPath.canSearch = false;
            }

            AudioManager_Enemy.instance.Play("DragonDeath");
            animator.SetTrigger("die");
            GetComponent<Collider>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;

            StartCoroutine(DeathCoroutine());


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

    public void DealDamage() //hàm take damage player
    {
        int damage = Random.Range(minAttackDamage, maxAttackDamage);

        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider player in hitPlayers)
        {
            //player.GetComponent<PlayerClone>()?.TakeDamage(damage);
            player.GetComponent<PlayerStats>()?.TakeDamage(damage); //============== thay bằng code HP player=============================***************************
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

    IEnumerator DeathCoroutine()
    {
        yield return new WaitForSeconds(1f);

        // Rơi item
        if (itemDropPrefab != null)
        {
            Vector3 dropPosition = dropPoint != null ? dropPoint.position : transform.position;
            Instantiate(itemDropPrefab, dropPosition, Quaternion.identity);
        }

        Destroy(gameObject, 7f);
    }

}