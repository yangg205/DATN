using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.IO;
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

    public GameObject healthBarPrefab;
    public float healthBarShowDistance = 10f;
    private Transform player;

    public Image healthFill;
    private Canvas healthCanvas;

    public bool isDead = false;
    public bool isTakingDamage = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        GameObject hb = Instantiate(healthBarPrefab, transform);
        hb.transform.localPosition = new Vector3(0, 2.5f, 0); // chỉnh tùy chiều cao
        healthCanvas = hb.GetComponent<Canvas>();
        //healthFill = hb.transform.Find("Background/Fill").GetComponent<Image>();
        healthCanvas.enabled = false; // ẩn ban đầu
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
            TakeDamage(50);
        }
        //==

        if (isDead) return;

        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        healthCanvas.enabled = distance <= healthBarShowDistance;

      

        // Xoay UI theo camera
        healthCanvas.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;
        HP -= damageAmount;
        HP = Mathf.Clamp(HP, 0, maxHP);

        AIPath aiPath = GetComponent<AIPath>();


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

}
