using ND;
using server.model;
using UnityEngine;

public class Hit : MonoBehaviour
{
    public PlayerStats playerStats;
    public int damageAmount;

    private void Start()
    {
        playerStats = FindAnyObjectByType<PlayerStats>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerStats targetStats = other.GetComponent<PlayerStats>();
            if (targetStats != null)
            {
                targetStats.TakeDamage(damageAmount);
                Debug.Log("đang gây damage cho player");
            }
            else
            {
                Debug.LogWarning("Player không có PlayerStats component!");
            }
        }
    }
}
