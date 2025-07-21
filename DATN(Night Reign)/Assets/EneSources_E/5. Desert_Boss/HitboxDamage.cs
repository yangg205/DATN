using ND;
using server.model;
using UnityEngine;

public class HitboxDamage : MonoBehaviour
{
    private PlayerStats playerStats;

    public int damage = 25;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(damage);
            }
        }
    }
}
