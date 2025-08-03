using ND;
using server.model;
using UnityEngine;

public class HitboxDamage : MonoBehaviour
{
    private PlayerStats playerStats;

    public int damage = 9;

    [Header("VFX")]
    public string vfxPoolName = "vfxHitBoxBoss";

    [Header("Layer Filter")]
    public LayerMask validLayers;

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
                //Debug.Log("Boss Sa Mac attack " + other.name);
            }
        }

       

        if (other.CompareTag("Enemy")) return;

        if (((1 << other.gameObject.layer) & validLayers) == 0)
            return;

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        VFXPoolManager.Instance.SpawnFromPool(vfxPoolName, hitPoint, Quaternion.identity);

        //Debug.Log("Boss hit: " + other.name);
    }
}
