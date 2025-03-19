using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightMare : MonoBehaviour
{
    public float HP = 100f;
    public Animator animator;

    public float attackDamage = 10f;
    public Transform attackPoint;
    public float attackRange = 1.5f;
    public LayerMask playerLayer;
    public void TakeDamage(int damageAmount)
    {
        HP -= damageAmount;
        if(HP <=0)
        {
            AudioManager_Enemy.instance.Play("DragonDeath");
            animator.SetTrigger("die");
            GetComponent<Collider>().enabled = false;
            Destroy(gameObject, 7f);
        }
        else
        {
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


}
