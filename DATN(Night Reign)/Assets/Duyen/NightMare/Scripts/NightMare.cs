using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NightMare : MonoBehaviour
{
    public float HP = 100f;
    public float maxHP = 100f;
    public Animator animator;

    public float attackDamage = 10f;
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;

    private Transform player;
    [Header("UI")]
    public Image healthFill;

    [Header("Damage Popup")]
    public GameObject damagePopupPrefab;


    public bool isDead = false;
    public bool isTakingDamage = false;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
        }
        //==

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

            //hồi sinh sau 5 giây
            //StartCoroutine(RespawnCoroutine());
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

    public void DealDamage()
    {
        Collider[] hitPlayers = Physics.OverlapSphere(attackPoint.position, attackRange, playerLayer);
        foreach (Collider player in hitPlayers)
        {
            player.GetComponent<PlayerClone>().TakeDamage(attackDamage);
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
        yield return new WaitForSeconds(1f);
        isTakingDamage = false;

        AIPath aiPath = GetComponent<AIPath>();
        if (aiPath != null && !isDead)
        {
            aiPath.canMove = true;
            aiPath.canSearch = true;
        }
    }

    IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(5f);
        HP = maxHP;
        isDead = false;
        GetComponent<Collider>().enabled = true;
        GetComponent<Rigidbody>().isKinematic = false;
        AIPath aiPath = GetComponent<AIPath>();
        if (aiPath != null)
        {
            aiPath.canMove = true;
            aiPath.canSearch = true;
        }
    }
}